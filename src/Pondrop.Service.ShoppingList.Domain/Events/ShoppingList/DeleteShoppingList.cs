using Pondrop.Service.Events;

namespace Pondrop.Service.ShoppingList.Domain.Events.ShoppingList;
public record DeleteShoppingList(
    Guid Id) : EventPayload;