using Pondrop.Service.Events;
using Pondrop.Service.ShoppingList.Domain.Enums.ShoppingList;
using Pondrop.Service.ShoppingList.Domain.Models;

namespace Pondrop.Service.ShoppingList.Domain.Events.ShoppingList;
public record UpdateShoppingList(
    Guid Id,
    string Name,
    ShoppingListType? ShoppingListType,
    List<ShoppingListStoreRecord>? Stores,
    List<Guid>? SharedListShopperIds,
    List<Guid>? ListItemIds) : EventPayload;