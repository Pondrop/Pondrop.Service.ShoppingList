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

public class GetListItemByShoppingListIdQueryHandler : IRequestHandler<GetListItemByShoppingListIdQuery, Result<List<ListItemRecord>?>>
{
    private readonly ICheckpointRepository<ListItemEntity> _checkpointRepository;
    private readonly ICheckpointRepository<ShoppingListEntity> _shoppingListCheckpointRepository;
    private readonly IMapper _mapper;
    private readonly IValidator<GetListItemByShoppingListIdQuery> _validator;
    private readonly ILogger<GetListItemByShoppingListIdQueryHandler> _logger;
    private readonly IUserService _userService;

    public GetListItemByShoppingListIdQueryHandler(
        ICheckpointRepository<ListItemEntity> checkpointRepository,
        ICheckpointRepository<ShoppingListEntity> shoppingListCheckpointRepository,
        IUserService userService,
        IValidator<GetListItemByShoppingListIdQuery> validator,
        IMapper mapper,
        ILogger<GetListItemByShoppingListIdQueryHandler> logger)
    {
        _checkpointRepository = checkpointRepository;
        _shoppingListCheckpointRepository = shoppingListCheckpointRepository;
        _mapper = mapper;
        _userService = userService;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<List<ListItemRecord>?>> Handle(GetListItemByShoppingListIdQuery request, CancellationToken cancellationToken)
    {
        var validation = _validator.Validate(request);

        if (!validation.IsValid)
        {
            var errorMessage = $"Get ListItem by id failed {validation}";
            _logger.LogError(errorMessage);
            return Result<List<ListItemRecord>?>.Error(errorMessage);
        }

        var result = default(Result<List<ListItemRecord>?>);

        try
        {
            var shoppingListEntity = await _shoppingListCheckpointRepository.GetByIdAsync(request.ShoppingListId);

            if (shoppingListEntity == null)
            {
                var errorMessage = $"No ShoppingList found.";
                _logger.LogError(errorMessage);
                return Result<List<ListItemRecord>?>.Error(errorMessage);
            }

            var query = $"SELECT * FROM c WHERE c.id in ({string.Join(",", shoppingListEntity.ListItemIds?.Select(s => $"'{s}'").ToList())}) AND c.deletedUtc = null";

            query += _userService.CurrentUserType() == UserType.Shopper
                   ? $" AND c.createdBy = '{_userService.CurrentUserName()}'" : string.Empty;

            var entity = await _checkpointRepository.QueryAsync(query);

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