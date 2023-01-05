using Pondrop.Service.Events;

namespace Pondrop.Service.ShoppingList.Domain.Events.ListItem;
public record DeleteListItem(
    Guid Id) : EventPayload;