using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Scripts;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Pondrop.Service.Store.Application.Interfaces;
using Pondrop.Service.Store.Application.Models;
using Pondrop.Service.Store.Domain.Models;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace Pondrop.Service.Store.Infrastructure.CosmosDb;

public class CheckpointRepository<T> : ICheckpointRepository<T> where T : EventEntity, new()
{
    private const string SpUpsertCheckpoint = "spUpsertCheckpoint";

    private readonly string _containerName;

    private readonly IEventRepository _eventRepository;
    private readonly ILogger<CheckpointRepository<T>> _logger;
    private readonly CosmosConfiguration _config;

    private readonly CosmosClient _cosmosClient;
    private readonly SemaphoreSlim _connectSemaphore = new SemaphoreSlim(1, 1);

    private Database? _database;
    private Container? _container;

    public CheckpointRepository(
        IEventRepository eventRepository,
        IOptions<CosmosConfiguration> config,
        ILogger<CheckpointRepository<T>> logger)
    {
        var nameChars = typeof(T).Name
            .Replace("Entity", string.Empty)
            .Replace("Record", string.Empty)
            .ToCharArray();
        nameChars[0] = char.ToLower(nameChars[0]);
        _containerName = $"{new string(nameChars)}_checkpoint";

        _eventRepository = eventRepository;
        _logger = logger;
        _config = config.Value;

        _cosmosClient =
            new CosmosClient(
                _config.ConnectionString,
                new CosmosClientOptions()
                {
                    AllowBulkExecution = true,
                    ApplicationName = _config.ApplicationName,
                    SerializerOptions = new CosmosSerializationOptions() { PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase }
                });
    }

    public async Task<bool> IsConnectedAsync()
    {
        if (_container is not null)
            return true;

        await _connectSemaphore.WaitAsync();

        try
        {
            if (_container is null)
            {
                _database = await _cosmosClient.CreateDatabaseIfNotExistsAsync(_config.DatabaseName);
            }

            if (_database is not null && _container is null)
            {
                _container = await _database.CreateContainerIfNotExistsAsync(_containerName, "/id");
            }

            if (_container is not null)
            {
                var spDirInfo = new DirectoryInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"CosmosDb/StoredProcedures/Checkpoint"));
                await _container.EnsureStoreProcedures(spDirInfo);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
        }
        finally
        {
            _connectSemaphore.Release();
        }

        return _container is not null;
    }

    public async Task<int> RebuildAsync()
    {
        if (await IsConnectedAsync())
        {
            var streamType = EventEntity.GetStreamTypeName<T>();

            var allStreams = await _eventRepository.LoadStreamsByTypeAsync(streamType);

            foreach (var i in allStreams)
            {
                var entity = new T();
                entity.Apply(i.Value.Events);
                await _container!.UpsertItemAsync(entity);
            }

            return allStreams.Count;
        }

        return -1;
    }

    public async Task<T?> UpsertAsync(long expectedVersion, T entity)
    {
        if (await IsConnectedAsync())
        {
            var idString = entity.Id.ToString();

            var parameters = new dynamic[]
            {
                idString,
                expectedVersion,
                JsonConvert.SerializeObject(entity)
            };

            return await _container!.Scripts.ExecuteStoredProcedureAsync<T?>(SpUpsertCheckpoint,
                new PartitionKey(idString), parameters);
        }

        return default;
    }

    public async Task<T?> UpsertAsync(T entity)
    {
        if (await IsConnectedAsync())
        {
            var response = await _container!.UpsertItemAsync(entity);
            return response.Resource;
        }

        return default;
    }

    public async Task<List<T>> GetAllAsync()
    {
        var list = new List<T>();
        if (await IsConnectedAsync())
        {
            const string sql = "SELECT * FROM c";
            var iterator = _container!.GetItemQueryIterator<T>(sql);
            while (iterator.HasMoreResults)
            {
                var page = await iterator.ReadNextAsync();
                list.AddRange(page.Resource);
            }
        }
        return list;
    }

    public async Task<T?> GetByIdAsync(Guid id)
    {
        if (id != Guid.Empty && await IsConnectedAsync())
        {
            var idString = id.ToString();
            try
            {
                var result = await _container!.ReadItemAsync<T>(idString, new PartitionKey(idString));
                return result.Resource;
            }
            catch (CosmosException e) when (e.StatusCode == HttpStatusCode.NotFound)
            {
                // eat
            }
        }

        return default;
    }

    public async Task FastForwardAsync(T item)
    {
        var eventStream = await _eventRepository.LoadStreamAsync(item.StreamId, item.AtSequence + 1);
        if (eventStream.Events.Any())
            item.Apply(eventStream.Events);
    }

    public async Task<List<T>> QueryAsync(string sqlQueryText, Dictionary<string, string>? parameters = null)
    {
        var list = new List<T>();

        if (!string.IsNullOrEmpty(sqlQueryText) && await IsConnectedAsync())
        {
            var queryDefinition = new QueryDefinition(sqlQueryText);

            if (parameters?.Any() == true)
            {
                foreach (var kv in parameters)
                {
                    queryDefinition = queryDefinition.WithParameter(kv.Key, kv.Value);
                }
            }

            var iterator = _container!.GetItemQueryIterator<T>(queryDefinition);
            while (iterator.HasMoreResults)
            {
                var page = await iterator.ReadNextAsync();
                list.AddRange(page.Resource);
            }
        }

        return list;
    }
}
