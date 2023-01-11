using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Pondrop.Service.Interfaces;
using Pondrop.Service.Interfaces.Services;
using Pondrop.Service.Models.User;
using Pondrop.Service.ShoppingList.Application.Models;
using Pondrop.Service.ShoppingList.Domain.Models;

namespace Pondrop.Service.ShoppingList.Application.Queries;

public class GetAllListItemQueryHandler : IRequestHandler<GetAllListItemsQuery, Result<List<ListItemRecord>>>
{
    private readonly ICheckpointRepository<ListItemEntity> _checkpointRepository;
    private readonly ICheckpointRepository<ShoppingListEntity> _shoppingListCheckpointRepository;
    private readonly ICheckpointRepository<SharedListShopperEntity> _sharedListShopperCheckpointRepository;
    private readonly IMapper _mapper;
    private readonly IUserService _userService;
    private readonly IValidator<GetAllListItemsQuery> _validator;
    private readonly ILogger<GetAllListItemQueryHandler> _logger;

    public GetAllListItemQueryHandler(
        ICheckpointRepository<ListItemEntity> checkpointRepository,
        ICheckpointRepository<ShoppingListEntity> shoppingListCheckpointRepository,
        ICheckpointRepository<SharedListShopperEntity> sharedListShopperCheckpointRepository,
        IMapper mapper,
        IUserService userService,
        IValidator<GetAllListItemsQuery> validator,
        ILogger<GetAllListItemQueryHandler> logger)
    {
        _checkpointRepository = checkpointRepository;
        _shoppingListCheckpointRepository = shoppingListCheckpointRepository;
        _sharedListShopperCheckpointRepository = sharedListShopperCheckpointRepository;
        _mapper = mapper;
        _userService = userService;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<List<ListItemRecord>>> Handle(GetAllListItemsQuery request, CancellationToken cancellationToken)
    {
        var validation = _validator.Validate(request);

        if (!validation.IsValid)
        {
            var errorMessage = $"Get all stores failed {validation}";
            _logger.LogError(errorMessage);
            return Result<List<ListItemRecord>>.Error(errorMessage);
        }

        var sharedListShoppers = await GetSharedListShoppersByIdAsync(_userService.CurrentUserId());

        var result = default(Result<List<ListItemRecord>>);

        try
        {
            if (sharedListShoppers != null && sharedListShoppers.Count > 0)
            {
                var query = $"SELECT * FROM c WHERE c.deletedUtc = null";

                if (_userService.CurrentUserType() == UserType.Shopper)
                {
                    var sharedListShopperIdString = string.Join(',', sharedListShoppers.Select(s => $"'{s.Id}'"));
                    query += $" AND ARRAY_CONTAINS(c.sharedListShopperIds, {sharedListShopperIdString})";
                }

                var entities = await _shoppingListCheckpointRepository.QueryAsync(query);

                List<ListItemRecord> responseRecords = null;

                var entity = entities.FirstOrDefault();
                if (entity == null)
                {

                    var errorMessage = $"No ShoppingList found.";
                    _logger.LogError(errorMessage);
                    return Result<List<ListItemRecord>?>.Error(errorMessage);
                }

                var listItemQuery = $"SELECT * FROM c WHERE c.id in ({string.Join(",", entity.ListItemIds?.Select(s => $"'{s}'").ToList())}) AND c.deletedUtc = null";

                var listItemEntities = await _checkpointRepository.QueryAsync(listItemQuery);

                responseRecords = _mapper.Map<List<ListItemRecord>>(listItemEntities);

                result = responseRecords is not null ?
                    Result<List<ListItemRecord>>.Success(responseRecords) :
                    Result<List<ListItemRecord>>.Success(new List<ListItemRecord>());
            }
            else
            {
                return Result<List<ListItemRecord>>.Success(new List<ListItemRecord>());
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            result = Result<List<ListItemRecord>>.Error(ex);
        }

        return result;
    }


    private async Task<List<SharedListShopperEntity>> GetSharedListShoppersByIdAsync(string userId)
    {
        const string userIdKey = "@userId";

        var conditions = new List<string>();
        var parameters = new Dictionary<string, string>();


        if (!string.IsNullOrEmpty(userId))
        {
            conditions.Add($"c.userId = {userIdKey}");
            parameters.Add(userIdKey, userId.ToString());
        }

        if (!conditions.Any())
            return new List<SharedListShopperEntity>(0);

        var sqlQueryText = $"SELECT * FROM c WHERE c.deletedUtc = null AND {string.Join(" AND ", conditions)}";

        var affectedSharedListShoppers = await _sharedListShopperCheckpointRepository.QueryAsync(sqlQueryText, parameters);
        return affectedSharedListShoppers;
    }
}
