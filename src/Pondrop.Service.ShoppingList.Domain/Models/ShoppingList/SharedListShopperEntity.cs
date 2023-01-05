using Microsoft.Azure.Cosmos.Spatial;
using Newtonsoft.Json;
using Pondrop.Service.Events;
using Pondrop.Service.Models;
using Pondrop.Service.ShoppingList.Domain.Enums.ShoppingList;
using Pondrop.Service.ShoppingList.Domain.Events.SharedListShopper;
using Pondrop.Service.ShoppingList.Domain.Events.ShoppingList;

namespace Pondrop.Service.ShoppingList.Domain.Models;

public record SharedListShopperEntity : EventEntity
{
    public SharedListShopperEntity()
    {
        Id = Guid.Empty;
        ListPrivilege = ListPrivilegeType.unknown;
        ShopperId = Guid.Empty;
    }

    public SharedListShopperEntity(IEnumerable<IEvent> events) : this()
    {
        foreach (var e in events)
        {
            Apply(e);
        }
    }

    public SharedListShopperEntity(Guid shopperId, ListPrivilegeType listPrivilege, string createdBy) : this()
    {
        var create = new CreateSharedListShopper(Guid.NewGuid(), shopperId, listPrivilege);
        Apply(create, createdBy);
    }

    [JsonProperty(PropertyName = "shopperId")]
    public Guid ShopperId { get; private set; }

    [JsonProperty(PropertyName = "listPrivilege")]
    public ListPrivilegeType ListPrivilege { get; private set; }

    protected sealed override void Apply(IEvent eventToApply)
    {
        switch (eventToApply.GetEventPayload())
        {
            case CreateSharedListShopper create:
                When(create, eventToApply.CreatedBy, eventToApply.CreatedUtc);
                break;
            case UpdateSharedListShopper update:
                When(update, eventToApply.CreatedBy, eventToApply.CreatedUtc);
                break;
            case DeleteSharedListShopper delete:
                When(delete, eventToApply.CreatedBy, eventToApply.CreatedUtc);
                break;
            default:
                throw new InvalidOperationException($"Unrecognised event type for '{StreamType}', got '{eventToApply.GetType().Name}'");
        }

        Events.Add(eventToApply);

        AtSequence = eventToApply.SequenceNumber;
    }

    public sealed override void Apply(IEventPayload eventPayloadToApply, string createdBy)
    {
        if (eventPayloadToApply is CreateSharedListShopper create)
        {
            Apply(new Event(GetStreamId<SharedListShopperEntity>(create.Id), StreamType, 0, create, createdBy));
        }
        else
        {
            Apply(new Event(StreamId, StreamType, AtSequence + 1, eventPayloadToApply, createdBy));
        }
    }

    private void When(CreateSharedListShopper create, string createdBy, DateTime createdUtc)
    {
        Id = create.Id;
        ShopperId = create.ShopperId;
        ListPrivilege = create.ListPrivilege;
        CreatedBy = UpdatedBy = createdBy;
        CreatedUtc = UpdatedUtc = createdUtc;
    }


    private void When(UpdateSharedListShopper update, string createdBy, DateTime createdUtc)
    {
        var oldShopperId = ShopperId;
        var oldListPrivilege = ListPrivilege;

        Id = update.Id;
        ShopperId = update.ShopperId;
        ListPrivilege = update.ListPrivilege;

        if (oldShopperId != ShopperId ||
            oldListPrivilege != ListPrivilege)
        {
            UpdatedBy = createdBy;
            UpdatedUtc = createdUtc;
        }
    }

    private void When(DeleteSharedListShopper delete, string createdBy, DateTime deletedUtc)
    {
        UpdatedBy = createdBy;
        UpdatedUtc = deletedUtc;
        DeletedUtc = deletedUtc;
    }
}