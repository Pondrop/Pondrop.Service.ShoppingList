using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Pondrop.Service.Interfaces;
using Pondrop.Service.ShoppingList.Application.Models;
using Pondrop.Service.ShoppingList.Domain.Models;

namespace Pondrop.Service.ShoppingList.Application.Queries;

public class GetSharedListShopperByShoppingListIdQueryHandler : IRequestHandler<GetSharedListShopperByShoppingListIdQuery, Result<List<SharedListShopperRecord>?>>
{
    private readonly ICheckpointRepository<SharedListShopperEntity> _checkpointRepository;
    private readonly ICheckpointRepository<ShoppingListEntity> _shoppingListCheckpointRepository;
    private readonly IMapper _mapper;
    private readonly IValidator<GetSharedListShopperByShoppingListIdQuery> _validator;
    private readonly ILogger<GetSharedListShopperByShoppingListIdQueryHandler> _logger;

    public GetSharedListShopperByShoppingListIdQueryHandler(
        ICheckpointRepository<SharedListShopperEntity> checkpointRepository,
        ICheckpointRepository<ShoppingListEntity> shoppingListCheckpointRepository,
        IValidator<GetSharedListShopperByShoppingListIdQuery> validator,
        IMapper mapper,
        ILogger<GetSharedListShopperByShoppingListIdQueryHandler> logger)
    {
        _checkpointRepository = checkpointRepository;
        _shoppingListCheckpointRepository = shoppingListCheckpointRepository;
        _mapper = mapper;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<List<SharedListShopperRecord>?>> Handle(GetSharedListShopperByShoppingListIdQuery query, CancellationToken cancellationToken)
    {
        var validation = _validator.Validate(query);

        if (!validation.IsValid)
        {
            var errorMessage = $"Get SharedListShopper by id failed {validation}";
            _logger.LogError(errorMessage);
            return Result<List<SharedListShopperRecord>?>.Error(errorMessage);
        }

        var result = default(Result<List<SharedListShopperRecord>?>);

        try
        {
            var shoppingListEntity = await _shoppingListCheckpointRepository.GetByIdAsync(query.ShoppingListId);

            if (shoppingListEntity == null)
            {
                var errorMessage = $"No ShoppingList found.";
                _logger.LogError(errorMessage);
                return Result<List<SharedListShopperRecord>?>.Error(errorMessage);
            }

            var entity = await _checkpointRepository.QueryAsync($"SELECT * FROM c WHERE c.id in ({string.Join(",", shoppingListEntity.SharedListShopperIds?.Select(s => $"'{s}'").ToList())})");

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