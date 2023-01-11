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
            var errorMessage = $"Get all stores failed {validation}";
            _logger.LogError(errorMessage);
            return Result<SharedListShopperRecord>.Error(errorMessage);
        }

        var sharedListShoppers = await GetSharedListShoppersByIdAsync(_userService.CurrentUserId(), request.Id);

        var result = default(Result<SharedListShopperRecord>);

        try
        {
            if (sharedListShoppers != null && sharedListShoppers.Count > 0)
            {
                result = sharedListShoppers is not null && sharedListShoppers.Count > 0 ?
                Result<SharedListShopperRecord>.Success(_mapper.Map<SharedListShopperRecord>(sharedListShoppers.FirstOrDefault())) :
                Result<SharedListShopperRecord>.Success(null);
            }
            else
            {
                result = Result<SharedListShopperRecord>.Success(null);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            result = Result<SharedListShopperRecord>.Error(ex);
        }

        return result;
    }


    private async Task<List<SharedListShopperEntity>> GetSharedListShoppersByIdAsync(string userId, Guid sharedListShopperId)
    {
        const string userIdKey = "@userId";
        const string sharedListShopperIdKey = "@sharedListShopperId"; 

         var conditions = new List<string>();
        var parameters = new Dictionary<string, string>();


        if (!string.IsNullOrEmpty(userId))
        {
            conditions.Add($"c.userId = {userIdKey}");
            parameters.Add(userIdKey, userId.ToString());
        }
        if (!string.IsNullOrEmpty(userId))
        {
            conditions.Add($"c.id = {sharedListShopperIdKey}");
            parameters.Add(sharedListShopperIdKey, sharedListShopperId.ToString());
        }

        if (!conditions.Any())
            return new List<SharedListShopperEntity>(0);

        var sqlQueryText = $"SELECT * FROM c WHERE c.deletedUtc = null AND {string.Join(" AND ", conditions)}";

        var affectedSharedListShoppers = await _checkpointRepository.QueryAsync(sqlQueryText, parameters);
        return affectedSharedListShoppers;
    }


}