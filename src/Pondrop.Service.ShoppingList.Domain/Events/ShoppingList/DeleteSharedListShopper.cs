using Pondrop.Service.Events;

namespace Pondrop.Service.ShoppingList.Domain.Events.SharedListShopper;
public record DeleteSharedListShopper(
    Guid Id) : EventPayload;