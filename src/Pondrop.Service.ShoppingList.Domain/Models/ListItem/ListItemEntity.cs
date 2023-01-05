using Microsoft.Azure.Cosmos.Spatial;
using Newtonsoft.Json;
using Pondrop.Service.Events;
using Pondrop.Service.Models;
using Pondrop.Service.ShoppingList.Domain.Enums.ShoppingList;
using Pondrop.Service.ShoppingList.Domain.Events.ListItem;
using Pondrop.Service.ShoppingList.Domain.Events.ShoppingList;

namespace Pondrop.Service.ShoppingList.Domain.Models;

public record ListItemEntity : EventEntity
{
    public ListItemEntity()
    {
        Id = Guid.Empty;
        ItemTitle = string.Empty;
        AddedBy = Guid.Empty;
        SelectedCategoryId = Guid.Empty;
        Quantity = 0;
        ItemNetSize = 0;
        ItemUOM = string.Empty;
        SelectedPreferenceIds = new List<Guid>();
        SelectedProductId = Guid.Empty;
        StoreId = null;
        SortOrder = 0;

    }

    public ListItemEntity(IEnumerable<IEvent> events) : this()
    {
        foreach (var e in events)
        {
            Apply(e);
        }
    }

    public ListItemEntity(string itemTitle, Guid addedBy, Guid selectedCategoryId, int quantity, double itemNetSize, string itemUOM, List<Guid> selectedPreferenceIds, Guid selectedProductId, Guid? storeId, int sortOrder, string createdBy) : this()
    {
        var create = new CreateListItem(Guid.NewGuid(), itemTitle, addedBy, selectedCategoryId, quantity, itemNetSize, itemUOM, selectedPreferenceIds, selectedProductId, storeId, sortOrder);
        Apply(create, createdBy);
    }

    [JsonProperty(PropertyName = "itemTitle")]
    public string ItemTitle { get; private set; }

    [JsonProperty(PropertyName = "addedBy")]
    public Guid AddedBy { get; private set; }

    [JsonProperty(PropertyName = "selectedCategoryId")]
    public Guid SelectedCategoryId { get; private set; }

    [JsonProperty(PropertyName = "quantity")]
    public int Quantity { get; private set; }

    [JsonProperty(PropertyName = "itemNetSize")]
    public double ItemNetSize { get; private set; }

    [JsonProperty(PropertyName = "itemUOM")]
    public string ItemUOM { get; private set; }

    [JsonProperty(PropertyName = "selectedPreferenceIds")]
    public List<Guid> SelectedPreferenceIds { get; private set; }

    [JsonProperty(PropertyName = "selectedProductId")]
    public Guid SelectedProductId { get; private set; }

    [JsonProperty(PropertyName = "storeId")]
    public Guid? StoreId { get; private set; }

    [JsonProperty(PropertyName = "sortOrder")]
    public int SortOrder { get; private set; }

    protected sealed override void Apply(IEvent eventToApply)
    {
        switch (eventToApply.GetEventPayload())
        {
            case CreateListItem create:
                When(create, eventToApply.CreatedBy, eventToApply.CreatedUtc);
                break;
            case UpdateListItem update:
                When(update, eventToApply.CreatedBy, eventToApply.CreatedUtc);
                break;
            case DeleteListItem delete:
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
        if (eventPayloadToApply is CreateListItem create)
        {
            Apply(new Event(GetStreamId<ListItemEntity>(create.Id), StreamType, 0, create, createdBy));
        }
        else
        {
            Apply(new Event(StreamId, StreamType, AtSequence + 1, eventPayloadToApply, createdBy));
        }
    }

    private void When(CreateListItem create, string createdBy, DateTime createdUtc)
    {
        Id = create.Id;
        ItemTitle = create.ItemTitle;
        AddedBy = create.AddedBy;
        SelectedCategoryId = create.SelectedCategoryId;
        SelectedPreferenceIds = create.SelectedPreferenceIds;
        ItemNetSize = create.ItemNetSize;
        Quantity = create.Quantity;
        ItemUOM = create.ItemUOM;
        SelectedProductId = create.SelectedProductId;
        StoreId = create.StoreId;
        SortOrder = create.SortOrder;

        CreatedBy = UpdatedBy = createdBy;
        CreatedUtc = UpdatedUtc = createdUtc;
    }

    private void When(UpdateListItem update, string createdBy, DateTime createdUtc)
    {
        var oldItemTitle = ItemTitle;
        var oldAddedBy = AddedBy;
        var oldSelectedCategoryId = SelectedCategoryId;
        var oldSelectedPreferenceIds = SelectedPreferenceIds;
        var oldItemNetSize = ItemNetSize;
        var oldQuantity = Quantity;
        var oldItemUOM = ItemUOM;
        var oldSelectedProductId = SelectedProductId;
        var oldStoreId = StoreId;
        var oldSortOrder = SortOrder;

        Id = update.Id;
        ItemTitle = update.ItemTitle;
        AddedBy = update.AddedBy;
        SelectedCategoryId = update.SelectedCategoryId;
        SelectedPreferenceIds = update.SelectedPreferenceIds;
        ItemNetSize = update.ItemNetSize;
        Quantity = update.Quantity;
        ItemUOM = update.ItemUOM;
        SelectedProductId = update.SelectedProductId;
        StoreId = update.StoreId;
        SortOrder = update.SortOrder;

        if (oldItemTitle != ItemTitle ||
            oldAddedBy != AddedBy ||
            oldSelectedCategoryId != SelectedCategoryId ||
            oldSelectedPreferenceIds != SelectedPreferenceIds ||
            oldItemNetSize != ItemNetSize ||
            oldQuantity != Quantity ||
            oldItemUOM != ItemUOM ||
            oldSelectedProductId != SelectedProductId ||
            oldStoreId != StoreId ||
            oldSortOrder != SortOrder)
        {
            UpdatedBy = createdBy;
            UpdatedUtc = createdUtc;
        }
    }

    private void When(DeleteListItem delete, string createdBy, DateTime deletedUtc)
    {
        UpdatedBy = createdBy;
        UpdatedUtc = deletedUtc;
        DeletedUtc = deletedUtc;
    }
}