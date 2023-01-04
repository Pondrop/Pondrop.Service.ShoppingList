using Pondrop.Service.Events;

namespace Pondrop.Service.ShoppingList.Domain.Events.ShoppingList;
public record CreateListItem(
    Guid Id,
    string ItemTitle,
    Guid AddedBy,
    Guid SelectedCategoryId,
    int Quantity,
    double ItemNetSize,
    string ItemUOM,
    List<string> SelectedPreferenceIds,
    Guid SelectedProductId
    ) : EventPayload;