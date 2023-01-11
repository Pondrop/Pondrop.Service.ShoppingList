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

public class GetSharedListShopperByShoppingListIdQueryHandler : IRequestHandler<GetSharedListShopperByShoppingListIdQuery, Result<List<SharedListShopperRecord>?>>
{
    private readonly ICheckpointRepository<SharedListShopperEntity> _checkpointRepository;
    private readonly ICheckpointRepository<ShoppingListEntity> _shoppingListCheckpointRepository;
    private readonly IMapper _mapper;
    private readonly IUserService _userService;
    private readonly IValidator<GetSharedListShopperByShoppingListIdQuery> _validator;
    private readonly ILogger<GetSharedListShopperByShoppingListIdQueryHandler> _logger;

    public GetSharedListShopperByShoppingListIdQueryHandler(
        ICheckpointRepository<SharedListShopperEntity> checkpointRepository,
        ICheckpointRepository<ShoppingListEntity> shoppingListCheckpointRepository,
        IValidator<GetSharedListShopperByShoppingListIdQuery> validator,
        IMapper mapper,
        IUserService userService,
        ILogger<GetSharedListShopperByShoppingListIdQueryHandler> logger)
    {
        _checkpointRepository = checkpointRepository;
        _shoppingListCheckpointRepository = shoppingListCheckpointRepository;
        _mapper = mapper;
        _userService = userService;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<List<SharedListShopperRecord>?>> Handle(GetSharedListShopperByShoppingListIdQuery request, CancellationToken cancellationToken)
    {
        var validation = _validator.Validate(request);

        if (!validation.IsValid)
        {
            var errorMessage = $"Get all stores failed {validation}";
            _logger.LogError(errorMessage);
            return Result<List<SharedListShopperRecord>>.Error(errorMessage);
        }

        var sharedListShoppers = await GetSharedListShoppersByIdAsync(_userService.CurrentUserId());

        var result = default(Result<List<SharedListShopperRecord>>);

        try
        {
            if (sharedListShoppers != null && sharedListShoppers.Count > 0)
            {
                var query = $"SELECT * FROM c WHERE c.deletedUtc = null AND c.id = '{request.ShoppingListId}'";

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

                List<SharedListShopperRecord> responseRecords = new List<SharedListShopperRecord>();
                if (entities != null)
                {
                    foreach (var entity in entities)
                    {
                        if (entity.SharedListShopperIds.Count > 0)
                        {
                            var sharedListShopperQuery = $"SELECT * FROM c WHERE c.deletedUtc = null AND c.id in ({string.Join(",", entity.SharedListShopperIds?.Select(s => $"'{s}'").ToList())})";
                            var entityShoppers = await _checkpointRepository.QueryAsync(sharedListShopperQuery);
                            responseRecords = _mapper.Map<List<SharedListShopperRecord>>(entityShoppers);
                        }
                    }
                }

                result = responseRecords is not null && responseRecords.Count > 0 ?
                Result<List<SharedListShopperRecord>>.Success(responseRecords) :
                Result<List<SharedListShopperRecord>>.Success(new List<SharedListShopperRecord>());
            }
            else
            {
                result = Result<List<SharedListShopperRecord>>.Success(new List<SharedListShopperRecord>());
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            result = Result<List<SharedListShopperRecord>>.Error(ex);
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

        var affectedSharedListShoppers = await _checkpointRepository.QueryAsync(sqlQueryText, parameters);
        return affectedSharedListShoppers;
    }


}