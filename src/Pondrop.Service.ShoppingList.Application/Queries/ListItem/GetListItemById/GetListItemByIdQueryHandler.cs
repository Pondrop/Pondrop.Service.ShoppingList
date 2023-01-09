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
    private readonly IMapper _mapper;
    private readonly IUserService _userService;
    private readonly IValidator<GetListItemByIdQuery> _validator;
    private readonly ILogger<GetListItemByIdQueryHandler> _logger;

    public GetListItemByIdQueryHandler(
        ICheckpointRepository<ListItemEntity> checkpointRepository,
        IValidator<GetListItemByIdQuery> validator,
        IUserService userService,
        IMapper mapper,
        ILogger<GetListItemByIdQueryHandler> logger)
    {
        _checkpointRepository = checkpointRepository;
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
            var errorMessage = $"Get ListItem by id failed {validation}";
            _logger.LogError(errorMessage);
            return Result<ListItemRecord?>.Error(errorMessage);
        }

        var result = default(Result<ListItemRecord?>);

        try
        {
            var query = $"SELECT * FROM c WHERE c.id = '{request.Id}' AND c.deletedUtc = null";

            query += _userService.CurrentUserType() == UserType.Shopper
                   ? $" AND c.createdBy = '{_userService.CurrentUserName()}'" : string.Empty;

            var entity = await _checkpointRepository.QueryAsync(query);

            result = entity is not null
                ? Result<ListItemRecord?>.Success(_mapper.Map<ListItemRecord>(entity.FirstOrDefault()))
                : Result<ListItemRecord?>.Success(null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            result = Result<ListItemRecord?>.Error(ex);
        }

        return result;
    }
}