using MediatR;
using Pondrop.Service.ShoppingList.Application.Models;
using Pondrop.Service.ShoppingList.Domain.Models;

namespace Pondrop.Service.ShoppingList.Application.Commands;

public class AddSharedListShopperShoppingListCommand : IRequest<Result<ShoppingListRecord>>
{
    public Guid? ShoppingListId { get; init; } = null;
    public Guid? SharedListShopperId { get; init; } = null;
}