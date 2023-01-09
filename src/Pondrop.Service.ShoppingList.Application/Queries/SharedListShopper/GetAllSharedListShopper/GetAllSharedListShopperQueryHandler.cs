﻿using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Pondrop.Service.Interfaces;
using Pondrop.Service.Interfaces.Services;
using Pondrop.Service.Models.User;
using Pondrop.Service.ShoppingList.Application.Models;
using Pondrop.Service.ShoppingList.Application.Queries;
using Pondrop.Service.ShoppingList.Domain.Models;

namespace Pondrop.Service.SharedListShopper.Application.Queries;

public class GetAllSharedListShopperQueryHandler : IRequestHandler<GetAllSharedListShoppersQuery, Result<List<SharedListShopperRecord>>>
{
    private readonly ICheckpointRepository<SharedListShopperEntity> _checkpointRepository;
    private readonly IMapper _mapper;
    private readonly IUserService _userService;
    private readonly IValidator<GetAllSharedListShoppersQuery> _validator;
    private readonly ILogger<GetAllSharedListShopperQueryHandler> _logger;

    public GetAllSharedListShopperQueryHandler(
        ICheckpointRepository<SharedListShopperEntity> checkpointRepository,
        IMapper mapper,
        IUserService userService,
        IValidator<GetAllSharedListShoppersQuery> validator,
        ILogger<GetAllSharedListShopperQueryHandler> logger)
    {
        _checkpointRepository = checkpointRepository;
        _mapper = mapper;
        _userService = userService;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<List<SharedListShopperRecord>>> Handle(GetAllSharedListShoppersQuery request, CancellationToken cancellationToken)
    {
        var validation = _validator.Validate(request);

        if (!validation.IsValid)
        {
            var errorMessage = $"Get all stores failed {validation}";
            _logger.LogError(errorMessage);
            return Result<List<SharedListShopperRecord>>.Error(errorMessage);
        }

        var result = default(Result<List<SharedListShopperRecord>>);

        try
        {

            var query = $"SELECT * FROM c WHERE c.deletedUtc = null";

            query += _userService.CurrentUserType() == UserType.Shopper
                   ? $" AND c.createdBy = '{_userService.CurrentUserName()}'" : string.Empty;

            var entities = await _checkpointRepository.QueryAsync(query);
            result = Result<List<SharedListShopperRecord>>.Success(_mapper.Map<List<SharedListShopperRecord>>(entities));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            result = Result<List<SharedListShopperRecord>>.Error(ex);
        }

        return result;
    }
}