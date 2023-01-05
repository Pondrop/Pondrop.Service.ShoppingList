using MediatR;
using Pondrop.Service.ShoppingList.Application.Models;
using Pondrop.Service.ShoppingList.Domain.Models;

namespace Pondrop.Service.ShoppingList.Application.Commands;

public class AddListItemsToShoppingListCommand : IRequest<Result<ShoppingListRecord>>
{
    public Guid? ShoppingListId { get; init; } = null;
    public List<Guid>? ListItemIds { get; init; } = null;
}