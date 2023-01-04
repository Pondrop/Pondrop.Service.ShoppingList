using MediatR;
using Pondrop.Service.ShoppingList.Application.Models;
using Pondrop.Service.ShoppingList.Domain.Enums.ShoppingList;
using Pondrop.Service.ShoppingList.Domain.Models;

namespace Pondrop.Service.ShoppingList.Application.Commands;

public class UpdateShoppingListCommand : IRequest<Result<ShoppingListRecord>>
{
    public Guid Id { get; init; } = Guid.Empty;
    public string Name { get; init; } = null;
    public ShoppingListType? ShoppingListType { get; init; } = null;
    public List<Guid>? SelectedStoreIds { get; init; } = null;
    public List<Guid>? SharedListShopperIds { get; init; } = null;
    public List<Guid>? ListItemIds { get; init; } = null;
}