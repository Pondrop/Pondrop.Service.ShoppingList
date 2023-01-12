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

public class GetListItemByIdQueryHandler : IRequestHandler<GetListItemByIdQuery, Result<ListItemRecord?>>
{
    private readonly ICheckpointRepository<ListItemEntity> _checkpointRepository;
    private readonly ICheckpointRepository<ShoppingListEntity> _shoppingListCheckpointRepository;
    private readonly ICheckpointRepository<SharedListShopperEntity> _sharedListShopperCheckpointRepository;
    private readonly IMapper _mapper;
    private readonly IUserService _userService;
    private readonly IValidator<GetListItemByIdQuery> _validator;
    private readonly ILogger<GetListItemByIdQueryHandler> _logger;

    public GetListItemByIdQueryHandler(
        ICheckpointRepository<ListItemEntity> checkpointRepository,
        ICheckpointRepository<ShoppingListEntity> shoppingListCheckpointRepository,
        ICheckpointRepository<SharedListShopperEntity> sharedListShopperCheckpointRepository,
        IValidator<GetListItemByIdQuery> validator,
        IUserService userService,
        IMapper mapper,
        ILogger<GetListItemByIdQueryHandler> logger)
    {
        _checkpointRepository = checkpointRepository;
        _shoppingListCheckpointRepository = shoppingListCheckpointRepository;
        _sharedListShopperCheckpointRepository = sharedListShopperCheckpointRepository;
        _mapper = mapper;
        _userService = userService;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<ListItemRecord?>> Handle(GetListItemByIdQuery request, CancellationToken cancellationToken)
    {
        var validation = _validator.Validate(request);

        if (!validation.IsValid)
        {
            var errorMessage = $"Get all stores failed {validation}";
            _logger.LogError(errorMessage);
            return Result<ListItemRecord>.Error(errorMessage);
        }

        var sharedListShoppers = await GetSharedListShoppersByIdAsync(_userService.CurrentUserId());

        var result = default(Result<ListItemRecord>);

        try
        {
            if (sharedListShoppers != null && sharedListShoppers.Count > 0)
            {
                var query = $"SELECT * FROM c WHERE c.deletedUtc = null";

                if (_userService.CurrentUserType() == UserType.Shopper)
                {
                    bool isFirst = true;
                    query += " AND (";
                    foreach (var sharedListShopper in sharedListShoppers)
                    {
                        if (isFirst)
                        {
                            query += $"ARRAY_CONTAINS(c.sharedListShopperIds, '{sharedListShopper.Id}')";
                            isFirst = false;
                        }
                        else
                            query += $" OR ARRAY_CONTAINS(c.sharedListShopperIds, '{sharedListShopper.Id}')";
                    }
                    query += ")";
                }

                var entities = await _shoppingListCheckpointRepository.QueryAsync(query);

                var entity = entities.FirstOrDefault();
                if (entity == null && entity.ListItemIds != null && !entity.ListItemIds.Any(l => l == request.Id))
                {

                    var errorMessage = $"No ShoppingList found.";
                    _logger.LogError(errorMessage);
                    return Result<ListItemRecord?>.Error(errorMessage);
                }

                var listItemQuery = $"SELECT * FROM c WHERE c.id = '{request.Id}' AND c.deletedUtc = null";

                var listItemEntities = await _checkpointRepository.QueryAsync(listItemQuery);

                var responseRecord = _mapper.Map<ListItemRecord>(listItemEntities.FirstOrDefault());

                result = responseRecord is not null ?
                    Result<ListItemRecord>.Success(responseRecord) :
                    Result<ListItemRecord>.Success(null);
            }
            else
            {
                result = Result<ListItemRecord>.Success(null);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            result = Result<ListItemRecord>.Error(ex);
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
