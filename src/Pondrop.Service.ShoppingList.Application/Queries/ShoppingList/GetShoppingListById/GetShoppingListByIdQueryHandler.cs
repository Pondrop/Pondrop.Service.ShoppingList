using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Pondrop.Service.Interfaces;
using Pondrop.Service.ShoppingList.Application.Models;
using Pondrop.Service.ShoppingList.Domain.Models;

namespace Pondrop.Service.ShoppingList.Application.Queries;

public class GetShoppingListByIdQueryHandler : IRequestHandler<GetShoppingListByIdQuery, Result<ShoppingListRecord?>>
{
    private readonly ICheckpointRepository<ShoppingListEntity> _checkpointRepository;
    private readonly IMapper _mapper;
    private readonly IValidator<GetShoppingListByIdQuery> _validator;
    private readonly ILogger<GetShoppingListByIdQueryHandler> _logger;

    public GetShoppingListByIdQueryHandler(
        ICheckpointRepository<ShoppingListEntity> checkpointRepository,
        IValidator<GetShoppingListByIdQuery> validator,
        IMapper mapper,
        ILogger<GetShoppingListByIdQueryHandler> logger)
    {
        _checkpointRepository = checkpointRepository;
        _mapper = mapper;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<ShoppingListRecord?>> Handle(GetShoppingListByIdQuery query, CancellationToken cancellationToken)
    {
        var validation = _validator.Validate(query);

        if (!validation.IsValid)
        {
            var errorMessage = $"Get ShoppingList by id failed {validation}";
            _logger.LogError(errorMessage);
            return Result<ShoppingListRecord?>.Error(errorMessage);
        }

        var result = default(Result<ShoppingListRecord?>);

        try
        {
            var entity = await _checkpointRepository.GetByIdAsync(query.Id);

            result = entity is not null
                ? Result<ShoppingListRecord?>.Success(_mapper.Map<ShoppingListRecord>(entity))
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