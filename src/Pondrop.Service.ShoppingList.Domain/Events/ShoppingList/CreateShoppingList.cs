using Pondrop.Service.Events;
using Pondrop.Service.ShoppingList.Domain.Enums.ShoppingList;

namespace Pondrop.Service.ShoppingList.Domain.Events.ShoppingList;
public record CreateShoppingList(
    Guid Id,
    string Name,
    ShoppingListType? ShoppingListType,
    List<Guid>? SelectedStoreIds,
    List<Guid>? SharedListShopperIds,
    List<Guid>? ListItemIds,
    int SortOrder) : EventPayload;