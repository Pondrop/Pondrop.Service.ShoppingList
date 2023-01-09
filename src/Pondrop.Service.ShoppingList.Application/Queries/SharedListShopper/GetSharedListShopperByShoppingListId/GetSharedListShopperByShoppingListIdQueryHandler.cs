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
            var errorMessage = $"Get SharedListShopper by id failed {validation}";
            _logger.LogError(errorMessage);
            return Result<List<SharedListShopperRecord>?>.Error(errorMessage);
        }

        var result = default(Result<List<SharedListShopperRecord>?>);

        try
        {
            var shoppingListEntity = await _shoppingListCheckpointRepository.GetByIdAsync(request.ShoppingListId);

            if (shoppingListEntity == null)
            {
                var errorMessage = $"No ShoppingList found.";
                _logger.LogError(errorMessage);
                return Result<List<SharedListShopperRecord>?>.Error(errorMessage);
            }

            var query = $"SELECT * FROM c WHERE c.id in ({string.Join(",", shoppingListEntity.SharedListShopperIds?.Select(s => $"'{s}'").ToList())}) AND c.deletedUtc = null";

            query += _userService.CurrentUserType() == UserType.Shopper
                   ? $" AND c.createdBy = '{_userService.CurrentUserName()}'" : string.Empty;

            var entity = await _checkpointRepository.QueryAsync(query);

            result = entity is not null
                ? Result<List<SharedListShopperRecord>?>.Success(_mapper.Map<List<SharedListShopperRecord>>(entity))
                : Result<List<SharedListShopperRecord>?>.Success(null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            result = Result<List<SharedListShopperRecord>?>.Error(ex);
        }

        return result;
    }
}