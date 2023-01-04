using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Pondrop.Service.Interfaces;
using Pondrop.Service.Models;
using Pondrop.Service.ShoppingList.Application.Models;

namespace Pondrop.Service.ShoppingList.Application.Commands;

public class UpdateCheckpointByIdCommandHandler<TCommand, TEntity, TRecord> : IRequestHandler<TCommand, Result<TRecord>>
    where TCommand : UpdateCheckpointByIdCommand, IRequest<Result<TRecord>>
    where TEntity : EventEntity, new()
{
    private readonly IEventRepository _eventRepository;
    private readonly ICheckpointRepository<TEntity> _checkpointRepository;
    private readonly IMapper _mapper;
    private readonly IValidator<UpdateCheckpointByIdCommand> _validator;
    private readonly ILogger _logger;

    public UpdateCheckpointByIdCommandHandler(
        IEventRepository eventRepository,
        ICheckpointRepository<TEntity> checkpointRepository,
        IMapper mapper,
        IValidator<UpdateCheckpointByIdCommand> validator,
        ILogger logger)
    {
        _eventRepository = eventRepository;
        _checkpointRepository = checkpointRepository;
        _mapper = mapper;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<TRecord>> Handle(TCommand command, CancellationToken cancellationToken)
    {
        var validation = _validator.Validate(command);

        if (!validation.IsValid)
        {
            var errorMessage = $"Update {typeof(TEntity).Name} materialized view failed, errors on validation {validation}";
            _logger.LogError(errorMessage);
            return Result<TRecord>.Error(errorMessage);
        }

        var result = default(Result<TRecord>);

        try
        {
            var entity = await _checkpointRepository.GetByIdAsync(command.Id);

            if (entity is not null)
            {
                await _checkpointRepository.FastForwardAsync(entity);
            }
            else
            {
                var stream = await _eventRepository.LoadStreamAsync(EventEntity.GetStreamId<TEntity>(command.Id), 0);
                if (stream.Events.Any())
                {
                    entity = new TEntity();
                    entity.Apply(stream.Events);
                }
            }

            if (entity is not null)
            {
                entity = await _checkpointRepository.UpsertAsync(entity);
                result = entity is not null
                    ? Result<TRecord>.Success(_mapper.Map<TRecord>(entity))
                    : Result<TRecord>.Error(FailedToMessage(command));
            }
            else if (entity is not null)
            {
                result = Result<TRecord>.Success(_mapper.Map<TRecord>(entity));
            }
            else
            {
                result = Result<TRecord>.Error($"{typeof(TEntity).Name} does not exist '{command.Id}'");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, FailedToMessage(command));
            result = Result<TRecord>.Error(ex);
        }

        return result;
    }

    private static string FailedToMessage(TCommand byIdCommand) =>
        $"Failed to update materialized {typeof(TEntity).Name} '{byIdCommand.Id}'";
}