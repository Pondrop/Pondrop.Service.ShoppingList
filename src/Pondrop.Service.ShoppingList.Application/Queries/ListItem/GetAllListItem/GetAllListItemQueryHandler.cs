using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Pondrop.Service.Interfaces;
using Pondrop.Service.ShoppingList.Application.Models;
using Pondrop.Service.ShoppingList.Domain.Models;

namespace Pondrop.Service.ShoppingList.Application.Queries;

public class GetAllListItemQueryHandler : IRequestHandler<GetAllListItemsQuery, Result<List<ListItemRecord>>>
{
    private readonly ICheckpointRepository<ListItemEntity> _checkpointRepository;
    private readonly IMapper _mapper;
    private readonly IValidator<GetAllListItemsQuery> _validator;
    private readonly ILogger<GetAllListItemQueryHandler> _logger;

    public GetAllListItemQueryHandler(
        ICheckpointRepository<ListItemEntity> checkpointRepository,
        IMapper mapper,
        IValidator<GetAllListItemsQuery> validator,
        ILogger<GetAllListItemQueryHandler> logger)
    {
        _checkpointRepository = checkpointRepository;
        _mapper = mapper;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<List<ListItemRecord>>> Handle(GetAllListItemsQuery request, CancellationToken cancellationToken)
    {
        var validation = _validator.Validate(request);

        if (!validation.IsValid)
        {
            var errorMessage = $"Get all stores failed {validation}";
            _logger.LogError(errorMessage);
            return Result<List<ListItemRecord>>.Error(errorMessage);
        }

        var result = default(Result<List<ListItemRecord>>);

        try
        {
            var entities = await _checkpointRepository.GetAllAsync();
            result = Result<List<ListItemRecord>>.Success(_mapper.Map<List<ListItemRecord>>(entities));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            result = Result<List<ListItemRecord>>.Error(ex);
        }

        return result;
    }
}