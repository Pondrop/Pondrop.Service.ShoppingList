using MediatR;
using Microsoft.Extensions.Logging;
using Pondrop.Service.ShoppingList.Application.Models;
using Pondrop.Service.Models;
using Pondrop.Service.Interfaces;
using Pondrop.Service.Events;

namespace Pondrop.Service.ShoppingList.Application.Commands;

public abstract class DirtyCommandHandler<TEntity, TRequest, TResponse> : IRequestHandler<TRequest, TResponse>
    where TEntity : EventEntity, new()
    where TRequest : IRequest<TResponse>
{
    private readonly IEventRepository _eventRepository;

    private readonly DaprEventTopicConfiguration _daprUpdateConfig;
    private readonly IDaprService _daprService;
    private readonly ILogger _logger;

    public DirtyCommandHandler(
        IEventRepository eventRepository,
        DaprEventTopicConfiguration daprUpdateConfig,
        IDaprService daprService,
        ILogger logger)
    {
        _eventRepository = eventRepository;
        _daprUpdateConfig = daprUpdateConfig;
        _daprService = daprService;
        _logger = logger;
    }

    public abstract Task<TResponse> Handle(TRequest command, CancellationToken cancellationToken);

    protected async Task InvokeDaprMethods(Guid id, IEnumerable<IEvent> events)
    {
        if (id != Guid.Empty && events.Any())
        {
            // Update Materialized View
            // if (!string.IsNullOrWhiteSpace(_daprUpdateConfig.AppId) && !string.IsNullOrWhiteSpace(_daprUpdateConfig.MethodName))
            // {
            //     var viewUpdated = await _daprService.InvokeServiceAsync(
            //         _daprUpdateConfig.AppId,
            //         _daprUpdateConfig.MethodName,
            //         new TUpdateByIdCommand() { Id = id });
            //     System.Diagnostics.Debug.WriteLine($"{GetType().Name} Dapr Invoke Service {(viewUpdated ? "Success" : "Fail")}");
            // }

            // Send Events to Event Grid
            // if (!string.IsNullOrWhiteSpace(_daprUpdateConfig.EventTopic))
            // {
            //     var bindingInvoked = await _daprService.SendEventsAsync(_daprUpdateConfig.EventTopic, events);
            //     System.Diagnostics.Debug.WriteLine($"{GetType().Name} Dapr Send Events {(bindingInvoked ? "Success" : "Fail")}");
            // }
        }
    }

    protected async Task<TEntity?> GetFromStreamAsync(Guid id)
    {
        var stream = await _eventRepository.LoadStreamAsync(EventEntity.GetStreamId<TEntity>(id));
        if (stream.Events.Any())
        {
            var entity = new TEntity();
            entity.Apply(stream.Events);
        }

        return null;
    }

    protected async Task<bool> UpdateStreamAsync(TEntity entity, IEventPayload evtPayload, string createdBy)
    {
        var appliedEntity = entity with { };
        appliedEntity.Apply(evtPayload, createdBy);

        var success = await _eventRepository.AppendEventsAsync(appliedEntity.StreamId, appliedEntity.AtSequence - 1, appliedEntity.GetEvents(appliedEntity.AtSequence));

        if (success)
            entity.Apply(evtPayload, createdBy);

        return success;
    }
}