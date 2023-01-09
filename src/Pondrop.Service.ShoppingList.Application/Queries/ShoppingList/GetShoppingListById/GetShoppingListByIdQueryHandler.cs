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

public class GetShoppingListByIdQueryHandler : IRequestHandler<GetShoppingListByIdQuery, Result<ShoppingListRecord?>>
{
    private readonly ICheckpointRepository<ShoppingListEntity> _checkpointRepository;
    private readonly IMapper _mapper;
    private readonly IUserService _userService;
    private readonly IValidator<GetShoppingListByIdQuery> _validator;
    private readonly ILogger<GetShoppingListByIdQueryHandler> _logger;

    public GetShoppingListByIdQueryHandler(
        ICheckpointRepository<ShoppingListEntity> checkpointRepository,
        IValidator<GetShoppingListByIdQuery> validator,
        IUserService userService,
        IMapper mapper,
        ILogger<GetShoppingListByIdQueryHandler> logger)
    {
        _checkpointRepository = checkpointRepository;
        _mapper = mapper;
        _userService = userService;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<ShoppingListRecord?>> Handle(GetShoppingListByIdQuery request, CancellationToken cancellationToken)
    {
        var validation = _validator.Validate(request);

        if (!validation.IsValid)
        {
            var errorMessage = $"Get ShoppingList by id failed {validation}";
            _logger.LogError(errorMessage);
            return Result<ShoppingListRecord?>.Error(errorMessage);
        }

        var result = default(Result<ShoppingListRecord?>);

        try
        {
            var query = $"SELECT * FROM c WHERE c.id = '{request.Id}' AND c.deletedUtc = null";

            query += _userService.CurrentUserType() == UserType.Shopper
                   ? $" AND c.createdBy = '{_userService.CurrentUserName()}'" : string.Empty;

            var entity = await _checkpointRepository.QueryAsync(query);


            result = entity is not null
                ? Result<ShoppingListRecord?>.Success(_mapper.Map<ShoppingListRecord>(entity.FirstOrDefault()))
                : Result<ShoppingListRecord?>.Success(null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            result = Result<ShoppingListRecord?>.Error(ex);
        }

        return result;
    }
}