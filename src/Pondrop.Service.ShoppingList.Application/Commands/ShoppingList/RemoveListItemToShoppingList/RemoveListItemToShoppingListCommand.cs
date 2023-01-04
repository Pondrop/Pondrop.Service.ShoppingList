using MediatR;
using Pondrop.Service.ShoppingList.Application.Models;
using Pondrop.Service.ShoppingList.Domain.Models;

namespace Pondrop.Service.ShoppingList.Application.Commands;

public class RemoveListItemToShoppingListCommand : IRequest<Result<ShoppingListRecord>>
{
    public Guid? ShoppingListId { get; init; } = null;
    public Guid? ListItemId { get; init; } = null;
}