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

public class GetSharedListShopperByIdQueryHandler : IRequestHandler<GetSharedListShopperByIdQuery, Result<SharedListShopperRecord?>>
{
    private readonly ICheckpointRepository<SharedListShopperEntity> _checkpointRepository;
    private readonly IMapper _mapper;
    private readonly IUserService _userService;
    private readonly IValidator<GetSharedListShopperByIdQuery> _validator;
    private readonly ILogger<GetSharedListShopperByIdQueryHandler> _logger;

    public GetSharedListShopperByIdQueryHandler(
        ICheckpointRepository<SharedListShopperEntity> checkpointRepository,
        IUserService userService,
        IValidator<GetSharedListShopperByIdQuery> validator,
        IMapper mapper,
        ILogger<GetSharedListShopperByIdQueryHandler> logger)
    {
        _checkpointRepository = checkpointRepository;
        _mapper = mapper;
        _userService = userService;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<SharedListShopperRecord?>> Handle(GetSharedListShopperByIdQuery request, CancellationToken cancellationToken)
    {
        var validation = _validator.Validate(request);

        if (!validation.IsValid)
        {
            var errorMessage = $"Get SharedListShopper by id failed {validation}";
            _logger.LogError(errorMessage);
            return Result<SharedListShopperRecord?>.Error(errorMessage);
        }

        var result = default(Result<SharedListShopperRecord?>);

        try
        {
            var query = $"SELECT * FROM c WHERE c.id = '{request.Id}' AND c.deletedUtc = null";

            query += _userService.CurrentUserType() == UserType.Shopper
                   ? $" AND c.createdBy = '{_userService.CurrentUserName()}'" : string.Empty;

            var entity = await _checkpointRepository.QueryAsync(query);

            result = entity is not null
                ? Result<SharedListShopperRecord?>.Success(_mapper.Map<SharedListShopperRecord>(entity.FirstOrDefault()))
                : Result<SharedListShopperRecord?>.Success(null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            result = Result<SharedListShopperRecord?>.Error(ex);
        }

        return result;
    }
}