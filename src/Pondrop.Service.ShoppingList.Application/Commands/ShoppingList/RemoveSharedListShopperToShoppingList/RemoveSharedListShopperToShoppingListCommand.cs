using MediatR;
using Pondrop.Service.ShoppingList.Application.Models;
using Pondrop.Service.ShoppingList.Domain.Models;

namespace Pondrop.Service.ShoppingList.Application.Commands;

public class RemoveSharedListShopperToShoppingListCommand : IRequest<Result<ShoppingListRecord>>
{
    public Guid? ShoppingListId { get; init; } = null;
    public List<Guid>? SharedListShopperIds { get; init; } = null;
}