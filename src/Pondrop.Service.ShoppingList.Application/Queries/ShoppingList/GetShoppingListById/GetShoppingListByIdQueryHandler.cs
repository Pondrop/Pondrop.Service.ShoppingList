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

public class GetShoppingListByIdQueryHandler : IRequestHandler<GetShoppingListByIdQuery, Result<ShoppingListResponseRecord?>>
{
    private readonly ICheckpointRepository<ShoppingListEntity> _checkpointRepository;
    private readonly ICheckpointRepository<SharedListShopperEntity> _sharedListShopperCheckpointRepository;
    private readonly IMapper _mapper;
    private readonly IUserService _userService;
    private readonly IValidator<GetShoppingListByIdQuery> _validator;
    private readonly ILogger<GetShoppingListByIdQueryHandler> _logger;

    public GetShoppingListByIdQueryHandler(
        ICheckpointRepository<ShoppingListEntity> checkpointRepository,
        ICheckpointRepository<SharedListShopperEntity> sharedListShopperCheckpointRepository,
        IValidator<GetShoppingListByIdQuery> validator,
        IUserService userService,
        IMapper mapper,
        ILogger<GetShoppingListByIdQueryHandler> logger)
    {
        _checkpointRepository = checkpointRepository;
        _sharedListShopperCheckpointRepository = sharedListShopperCheckpointRepository;
        _mapper = mapper;
        _userService = userService;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<ShoppingListResponseRecord?>> Handle(GetShoppingListByIdQuery request, CancellationToken cancellationToken)
    {
        var validation = _validator.Validate(request);

        if (!validation.IsValid)
        {
            var errorMessage = $"Get all stores failed {validation}";
            _logger.LogError(errorMessage);
            return Result<ShoppingListResponseRecord>.Error(errorMessage);
        }

        var sharedListShoppers = await GetSharedListShoppersByIdAsync(_userService.CurrentUserId());

        var result = default(Result<ShoppingListResponseRecord>);

        try
        {
            if (sharedListShoppers != null && sharedListShoppers.Count > 0)
            {
                var query = $"SELECT * FROM c WHERE c.deletedUtc = null AND c.id = '{request.Id}'";

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

                var entities = await _checkpointRepository.QueryAsync(query);

                ShoppingListResponseRecord responseRecord = null;

                var entity = entities.FirstOrDefault();
                if (entity != null)
                {
                    if (entity.SharedListShopperIds.Count > 0)
                    {
                        var sharedListShopperQuery = $"SELECT * FROM c WHERE c.deletedUtc = null AND c.id in ({string.Join(",", entity.SharedListShopperIds?.Select(s => $"'{s}'").ToList())})";
                        var entityShoppers = await _sharedListShopperCheckpointRepository.QueryAsync(sharedListShopperQuery);
                        var entityShopperRecords = _mapper.Map<List<SharedListShopperRecord>>(entityShoppers);
                        var entityShopperResponseRecords = _mapper.Map<List<SharedListShopperResponseRecord>>(entityShoppers);
                        var userShopper = entityShopperRecords.FirstOrDefault(s => s.UserId.ToString() == _userService.CurrentUserId());

                        //SharedListShopperEntity? sharedListShopper = null;
                        //foreach (var sharedListShopperId in entity!.SharedListShopperIds)
                        //{
                        //    sharedListShopper = sharedListShoppers.First(s => s.Id == sharedListShopperId);
                        //    if (sharedListShopper != null)
                        //        break;
                        //}

                        responseRecord = _mapper.Map<ShoppingListResponseRecord>(entity) with
                        {
                            SharedListShoppers = entityShopperResponseRecords,
                            SortOrder = userShopper?.SortOrder ?? 0
                        };
                    }
                }

                result = responseRecord is not null ?
                    Result<ShoppingListResponseRecord>.Success(responseRecord) :
                    Result<ShoppingListResponseRecord>.Success(null);
            }
            else
            {
                result = Result<ShoppingListResponseRecord>.Success(null);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            result = Result<ShoppingListResponseRecord>.Error(ex);
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