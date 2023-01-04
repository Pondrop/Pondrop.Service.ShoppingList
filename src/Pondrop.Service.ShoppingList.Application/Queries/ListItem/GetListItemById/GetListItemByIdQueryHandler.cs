using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Pondrop.Service.Interfaces;
using Pondrop.Service.ShoppingList.Application.Models;
using Pondrop.Service.ShoppingList.Domain.Models;

namespace Pondrop.Service.ShoppingList.Application.Queries;

public class GetListItemByIdQueryHandler : IRequestHandler<GetListItemByIdQuery, Result<ListItemRecord?>>
{
    private readonly ICheckpointRepository<ListItemEntity> _checkpointRepository;
    private readonly IMapper _mapper;
    private readonly IValidator<GetListItemByIdQuery> _validator;
    private readonly ILogger<GetListItemByIdQueryHandler> _logger;

    public GetListItemByIdQueryHandler(
        ICheckpointRepository<ListItemEntity> checkpointRepository,
        IValidator<GetListItemByIdQuery> validator,
        IMapper mapper,
        ILogger<GetListItemByIdQueryHandler> logger)
    {
        _checkpointRepository = checkpointRepository;
        _mapper = mapper;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<ListItemRecord?>> Handle(GetListItemByIdQuery query, CancellationToken cancellationToken)
    {
        var validation = _validator.Validate(query);

        if (!validation.IsValid)
        {
            var errorMessage = $"Get ListItem by id failed {validation}";
            _logger.LogError(errorMessage);
            return Result<ListItemRecord?>.Error(errorMessage);
        }

        var result = default(Result<ListItemRecord?>);

        try
        {
            var entity = await _checkpointRepository.GetByIdAsync(query.Id);

            result = entity is not null
                ? Result<ListItemRecord?>.Success(_mapper.Map<ListItemRecord>(entity))
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