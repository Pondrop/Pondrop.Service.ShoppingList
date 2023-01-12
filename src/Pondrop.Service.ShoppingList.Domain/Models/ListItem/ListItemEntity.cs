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
        SelectedCategoryId = Guid.Empty;
        Quantity = 0;
        ItemNetSize = 0;
        ItemUOM = string.Empty;
        SelectedPreferenceIds = new List<Guid>();
        SelectedProductId = null;
        StoreId = null;
        SortOrder = 0;
        Checked = false;
    }

    public ListItemEntity(IEnumerable<IEvent> events) : this()
    {
        foreach (var e in events)
        {
            Apply(e);
        }
    }

    public ListItemEntity(string itemTitle, Guid selectedCategoryId, int quantity, double itemNetSize, string itemUOM, List<Guid> selectedPreferenceIds, Guid? selectedProductId, Guid? storeId, int sortOrder, bool @checked, string createdBy) : this()
    {
        var create = new CreateListItem(Guid.NewGuid(), itemTitle, selectedCategoryId, quantity, itemNetSize, itemUOM, selectedPreferenceIds, selectedProductId, storeId, sortOrder, @checked);
        Apply(create, createdBy);
    }

    [JsonProperty(PropertyName = "itemTitle")]
    public string ItemTitle { get; private set; }

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
    public Guid? SelectedProductId { get; private set; }

    [JsonProperty(PropertyName = "storeId")]
    public Guid? StoreId { get; private set; }

    [JsonProperty(PropertyName = "sortOrder")]
    public int SortOrder { get; private set; }

    [JsonProperty(PropertyName = "checked")]
    public bool Checked { get; private set; }


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
        SelectedCategoryId = create.SelectedCategoryId;
        SelectedPreferenceIds = create.SelectedPreferenceIds;
        ItemNetSize = create.ItemNetSize;
        Quantity = create.Quantity;
        ItemUOM = create.ItemUOM;
        SelectedProductId = create.SelectedProductId;
        StoreId = create.StoreId;
        SortOrder = create.SortOrder;
        Checked = create.Checked;

        CreatedBy = UpdatedBy = createdBy;
        CreatedUtc = UpdatedUtc = createdUtc;
    }

    private void When(UpdateListItem update, string createdBy, DateTime createdUtc)
    {
        var oldItemTitle = ItemTitle;
        var oldSelectedCategoryId = SelectedCategoryId;
        var oldSelectedPreferenceIds = SelectedPreferenceIds;
        var oldItemNetSize = ItemNetSize;
        var oldQuantity = Quantity;
        var oldItemUOM = ItemUOM;
        var oldSelectedProductId = SelectedProductId;
        var oldStoreId = StoreId;
        var oldSortOrder = SortOrder;
        var oldChecked = Checked;

        Id = update.Id;
        ItemTitle = update.ItemTitle;
        SelectedCategoryId = update.SelectedCategoryId;
        SelectedPreferenceIds = update.SelectedPreferenceIds;
        ItemNetSize = update.ItemNetSize;
        Quantity = update.Quantity;
        ItemUOM = update.ItemUOM;
        SelectedProductId = update.SelectedProductId;
        StoreId = update.StoreId;
        SortOrder = update.SortOrder;
        Checked = update.Checked;

        if (oldItemTitle != ItemTitle ||
            oldSelectedCategoryId != SelectedCategoryId ||
            oldSelectedPreferenceIds != SelectedPreferenceIds ||
            oldItemNetSize != ItemNetSize ||
            oldQuantity != Quantity ||
            oldItemUOM != ItemUOM ||
            oldSelectedProductId != SelectedProductId ||
            oldStoreId != StoreId ||
            oldSortOrder != SortOrder ||
            oldChecked != Checked)
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