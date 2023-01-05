using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Pondrop.Service.Interfaces;
using Pondrop.Service.ShoppingList.Application.Models;
using Pondrop.Service.ShoppingList.Domain.Models;

namespace Pondrop.Service.ShoppingList.Application.Queries;

public class GetListItemByShoppingListIdQueryHandler : IRequestHandler<GetListItemByShoppingListIdQuery, Result<List<ListItemRecord>?>>
{
    private readonly ICheckpointRepository<ListItemEntity> _checkpointRepository;
    private readonly ICheckpointRepository<ShoppingListEntity> _shoppingListCheckpointRepository;
    private readonly IMapper _mapper;
    private readonly IValidator<GetListItemByShoppingListIdQuery> _validator;
    private readonly ILogger<GetListItemByShoppingListIdQueryHandler> _logger;

    public GetListItemByShoppingListIdQueryHandler(
        ICheckpointRepository<ListItemEntity> checkpointRepository,
        ICheckpointRepository<ShoppingListEntity> shoppingListCheckpointRepository,
        IValidator<GetListItemByShoppingListIdQuery> validator,
        IMapper mapper,
        ILogger<GetListItemByShoppingListIdQueryHandler> logger)
    {
        _checkpointRepository = checkpointRepository;
        _shoppingListCheckpointRepository = shoppingListCheckpointRepository;
        _mapper = mapper;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<List<ListItemRecord>?>> Handle(GetListItemByShoppingListIdQuery query, CancellationToken cancellationToken)
    {
        var validation = _validator.Validate(query);

        if (!validation.IsValid)
        {
            var errorMessage = $"Get ListItem by id failed {validation}";
            _logger.LogError(errorMessage);
            return Result<List<ListItemRecord>?>.Error(errorMessage);
        }

        var result = default(Result<List<ListItemRecord>?>);

        try
        {
            var shoppingListEntity = await _shoppingListCheckpointRepository.GetByIdAsync(query.ShoppingListId);

            if (shoppingListEntity == null) {
                var errorMessage = $"No ShoppingList found.";
                _logger.LogError(errorMessage);
                return Result<List<ListItemRecord>?>.Error(errorMessage);
            }

            var entity = await _checkpointRepository.QueryAsync($"SELECT * FROM c WHERE c.id in ({string.Join(",", shoppingListEntity.ListItemIds?.Select(s => $"'{s}'").ToList())})");

            result = entity is not null
                ? Result<List<ListItemRecord>?>.Success(_mapper.Map<List<ListItemRecord>>(entity))
                : Result<List<ListItemRecord>?>.Success(null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            result = Result<List<ListItemRecord>?>.Error(ex);
        }

        return result;
    }
}