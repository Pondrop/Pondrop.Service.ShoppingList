﻿using Microsoft.Azure.Cosmos.Spatial;
using Newtonsoft.Json;
using Pondrop.Service.Events;
using Pondrop.Service.Models;
using Pondrop.Service.ShoppingList.Domain.Enums.ShoppingList;
using Pondrop.Service.ShoppingList.Domain.Events.ShoppingList;

namespace Pondrop.Service.ShoppingList.Domain.Models;

public record ShoppingListEntity : EventEntity
{
    public ShoppingListEntity()
    {
        Id = Guid.Empty;
        Name = string.Empty;
        ShoppingListType = Enums.ShoppingList.ShoppingListType.unknown;
        SelectedStoreIds = new List<Guid>();
        ListItemIds = new List<Guid>();
        SharedListShopperIds = new List<Guid>();
        SortOrder = 0;
    }

    public ShoppingListEntity(IEnumerable<IEvent> events) : this()
    {
        foreach (var e in events)
        {
            Apply(e);
        }
    }

    public ShoppingListEntity(string name, ShoppingListType? shoppingListType, List<Guid>? selectedStoreIds, List<Guid>? sharedListShopperIds, List<Guid>? listItemIds, int sortOrder, string createdBy) : this()
    {
        var create = new CreateShoppingList(Guid.NewGuid(), name, shoppingListType, selectedStoreIds, sharedListShopperIds, listItemIds, sortOrder);
        Apply(create, createdBy);
    }

    [JsonProperty(PropertyName = "name")]
    public string Name { get; private set; }

    [JsonProperty(PropertyName = "shoppingListType")]
    public ShoppingListType? ShoppingListType { get; private set; }

    [JsonProperty(PropertyName = "selectedStoreIds")]
    public List<Guid>? SelectedStoreIds { get; private set; }

    [JsonProperty(PropertyName = "sharedListShopperIds")]
    public List<Guid>? SharedListShopperIds { get; private set; }

    [JsonProperty(PropertyName = "listItemIds")]
    public List<Guid>? ListItemIds { get; private set; }

    [JsonProperty(PropertyName = "sortOrder")]
    public int SortOrder { get; private set; }

    protected sealed override void Apply(IEvent eventToApply)
    {
        switch (eventToApply.GetEventPayload())
        {
            case CreateShoppingList create:
                When(create, eventToApply.CreatedBy, eventToApply.CreatedUtc);
                break;
            case UpdateShoppingList update:
                When(update, eventToApply.CreatedBy, eventToApply.CreatedUtc);
                break;
            //case AddShoppingListAddress addAddress:
            //    When(addAddress, eventToApply.CreatedBy, eventToApply.CreatedUtc);
            //    break;
            default:
                throw new InvalidOperationException($"Unrecognised event type for '{StreamType}', got '{eventToApply.GetType().Name}'");
        }

        Events.Add(eventToApply);

        AtSequence = eventToApply.SequenceNumber;
    }

    public sealed override void Apply(IEventPayload eventPayloadToApply, string createdBy)
    {
        if (eventPayloadToApply is CreateShoppingList create)
        {
            Apply(new Event(GetStreamId<ShoppingListEntity>(create.Id), StreamType, 0, create, createdBy));
        }
        else
        {
            Apply(new Event(StreamId, StreamType, AtSequence + 1, eventPayloadToApply, createdBy));
        }
    }

    private void When(CreateShoppingList create, string createdBy, DateTime createdUtc)
    {
        Id = create.Id;
        Name = create.Name;
        ShoppingListType = create.ShoppingListType;
        SelectedStoreIds = create.SelectedStoreIds;
        SharedListShopperIds = create.SharedListShopperIds;
        ListItemIds = create.ListItemIds;
        SortOrder = create.SortOrder;

        CreatedBy = UpdatedBy = createdBy;
        CreatedUtc = UpdatedUtc = createdUtc;
    }

    private void When(UpdateShoppingList update, string createdBy, DateTime createdUtc)
    {
        var oldName = Name;
        var oldShoppingListType = ShoppingListType;
        var oldSelectedStoreIds = SelectedStoreIds;
        var oldSharedListShopperIds = SharedListShopperIds;
        var oldListItemIds = ListItemIds;
        var oldSortOrder = SortOrder;

        Name = update.Name;
        ShoppingListType = update.ShoppingListType;
        SelectedStoreIds = update.SelectedStoreIds;
        SharedListShopperIds = update.SharedListShopperIds;
        ListItemIds = update.ListItemIds;
        SortOrder = update.SortOrder;

        if (oldName != Name ||
            oldShoppingListType != ShoppingListType ||
            oldSelectedStoreIds != SelectedStoreIds ||
            oldSharedListShopperIds != SharedListShopperIds ||
            oldSortOrder != SortOrder ||
            oldListItemIds != ListItemIds)
        {
            UpdatedBy = createdBy;
            UpdatedUtc = createdUtc;
        }
    }
    
}