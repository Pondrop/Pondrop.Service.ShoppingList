using AutoMapper;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Scripts;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Pondrop.Service.Store.Application.Interfaces;
using Pondrop.Service.Store.Application.Models;
using Pondrop.Service.Store.Domain.Events;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace Pondrop.Service.Store.Infrastructure.CosmosDb;

public class EventRepository : IEventRepository
{
    private const string ContainerName = "events";
    private const string PartitionKey = "/streamId";
    private const string SpAppendToStreamName = "spAppendToStream";
    
    private readonly IMapper _mapper;
    private readonly ILogger<EventRepository> _logger;
    private readonly CosmosConfiguration _config;

    private readonly CosmosClient _cosmosClient;
    private readonly SemaphoreSlim _connectSemaphore = new SemaphoreSlim(1, 1);

    private Database? _database;
    private Container? _container;

    public EventRepository(
        IMapper mapper,
        IOptions<CosmosConfiguration> config,
        ILogger<EventRepository> logger)
    {
        _mapper = mapper;
        _logger = logger;

        if (string.IsNullOrEmpty(config.Value?.ApplicationName))
            throw new ArgumentException("CosmosDB 'ApplicationName' cannot be null or empty");
        if (string.IsNullOrEmpty(config.Value?.DatabaseName))
            throw new ArgumentException("CosmosDB 'DatabaseName' cannot be null or empty");
        if (string.IsNullOrEmpty(config.Value?.ConnectionString))
            throw new ArgumentException("CosmosDB 'ConnectionString' cannot be null or empty");
        
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
                _container = await _database.DefineContainer(ContainerName, PartitionKey)
                    .WithIndexingPolicy()
                    .WithCompositeIndex()
                    .Path("/streamId", CompositePathSortOrder.Ascending)
                    .Path("/sequenceNumber", CompositePathSortOrder.Ascending)
                    .Attach()
                    .Attach()
                    .CreateIfNotExistsAsync();
            }

            if (_container is not null)
            {
                var spDirInfo = new DirectoryInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"CosmosDb/StoredProcedures/Events"));
                await _container.EnsureStoreProcedures(spDirInfo);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            throw;
        }
        finally
        {
            _connectSemaphore.Release();
        }

        return _container is not null;
    }

    public async Task<bool> AppendEventsAsync(string streamId, long expectedVersion, IEnumerable<IEvent> events)
    {
        if (await IsConnectedAsync())
        {
            var parameters = new dynamic[]
            {
                streamId,
                expectedVersion,
                JsonConvert.SerializeObject(events)
            };
            
            var count = await _container!.Scripts.ExecuteStoredProcedureAsync<int>(SpAppendToStreamName, new PartitionKey(streamId), parameters);
            return count > 0;
        }

        return false;
    }

    public Task<EventStream> LoadStreamAsync(string streamId)
        => LoadStreamAsync(streamId, 0);
    
    public async Task<EventStream> LoadStreamAsync(string streamId, long fromSequenceNumber)
    {
        var events = default(List<IEvent>);

        if (await IsConnectedAsync())
        {
            const string streamIdKey = "@streamId";
            const string sequenceNumberKey = "@sequenceNumber";
            
            var queryDefinition = default(QueryDefinition);

            if (fromSequenceNumber > 0)
            {
                var sqlQueryText = $"SELECT * FROM c WHERE c.streamId = {streamIdKey} AND c.sequenceNumber >= {sequenceNumberKey} ORDER BY c.sequenceNumber";
                queryDefinition = new QueryDefinition(sqlQueryText)
                    .WithParameter(streamIdKey, streamId)
                    .WithParameter(sequenceNumberKey, fromSequenceNumber);
            }
            else
            {
                var sqlQueryText = $"SELECT * FROM c WHERE c.streamId = {streamIdKey} ORDER BY c.sequenceNumber";
                queryDefinition = new QueryDefinition(sqlQueryText)
                    .WithParameter(streamIdKey, streamId);
            }
            
            var iterator = _container!.GetItemQueryIterator<Event>(queryDefinition);

            events = new List<IEvent>();

            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync();
                events.AddRange(response);
            }
        }

        return events?.Any() == true
            ? new EventStream(streamId, events)
            : new EventStream(streamId);
    }

    public Task<Dictionary<string, EventStream>> LoadStreamsByTypeAsync(string streamType)
        => LoadStreamsByTypeAsync(streamType, DateTime.MinValue);

    public async Task<Dictionary<string, EventStream>> LoadStreamsByTypeAsync(string streamType, DateTime fromDateTime)
    {
        var events = default(List<IEvent>);

        if (await IsConnectedAsync())
        {
            const string streamTypeKey = "@streamType";
            const string createdUtcKey = "@createdUtc";

            var queryDefinition = default(QueryDefinition);

            if (fromDateTime > DateTime.MinValue)
            {
                var sqlQueryText = $"SELECT * FROM c WHERE c.streamType = {streamTypeKey} AND c.payload.fromDateTime >= {createdUtcKey} ORDER BY c.streamId, c.sequenceNumber";
                queryDefinition = new QueryDefinition(sqlQueryText)
                    .WithParameter(streamTypeKey, streamType)
                    .WithParameter(createdUtcKey, fromDateTime.ToUniversalTime());
            }
            else
            {
                var sqlQueryText = $"SELECT * FROM c WHERE c.streamType = {streamTypeKey} ORDER BY c.streamId, c.sequenceNumber";
                queryDefinition = new QueryDefinition(sqlQueryText)
                    .WithParameter(streamTypeKey, streamType);
            }

            var iterator = _container!.GetItemQueryIterator<Event>(queryDefinition);

            events = new List<IEvent>();

            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync();
                events.AddRange(response);
            }
        }

        return events?.Any() == true
            ? events.GroupBy(i => i.StreamId).ToDictionary(g => g.Key, g => new EventStream(g.Key, g))
            : new Dictionary<string, EventStream>(0);
    }
}
