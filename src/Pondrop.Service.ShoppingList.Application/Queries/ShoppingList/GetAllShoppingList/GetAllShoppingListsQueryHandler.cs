using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Pondrop.Service.Interfaces;
using Pondrop.Service.ShoppingList.Application.Models;
using Pondrop.Service.ShoppingList.Domain.Models;

namespace Pondrop.Service.ShoppingList.Application.Queries;

public class GetAllShoppingListsQueryHandler : IRequestHandler<GetAllShoppingListsQuery, Result<List<ShoppingListRecord>>>
{
    private readonly ICheckpointRepository<ShoppingListEntity> _checkpointRepository;
    private readonly IMapper _mapper;
    private readonly IValidator<GetAllShoppingListsQuery> _validator;
    private readonly ILogger<GetAllShoppingListsQueryHandler> _logger;

    public GetAllShoppingListsQueryHandler(
        ICheckpointRepository<ShoppingListEntity> checkpointRepository,
        IMapper mapper,
        IValidator<GetAllShoppingListsQuery> validator,
        ILogger<GetAllShoppingListsQueryHandler> logger)
    {
        _checkpointRepository = checkpointRepository;
        _mapper = mapper;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<List<ShoppingListRecord>>> Handle(GetAllShoppingListsQuery request, CancellationToken cancellationToken)
    {
        var validation = _validator.Validate(request);

        if (!validation.IsValid)
        {
            var errorMessage = $"Get all stores failed {validation}";
            _logger.LogError(errorMessage);
            return Result<List<ShoppingListRecord>>.Error(errorMessage);
        }

        var result = default(Result<List<ShoppingListRecord>>);

        try
        {
            var entities = await _checkpointRepository.GetAllAsync();
            result = Result<List<ShoppingListRecord>>.Success(_mapper.Map<List<ShoppingListRecord>>(entities));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            result = Result<List<ShoppingListRecord>>.Error(ex);
        }

        return result;
    }
}