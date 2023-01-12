using MediatR;
using Pondrop.Service.ShoppingList.Application.Models;
using Pondrop.Service.ShoppingList.Domain.Enums.ShoppingList;
using Pondrop.Service.ShoppingList.Domain.Models;

namespace Pondrop.Service.ShoppingList.Application.Commands;

public class CreateShoppingListCommand : IRequest<Result<ShoppingListRecord>>
{
    public string Name { get; init; }
    public ShoppingListType? ShoppingListType { get; init; } = null;
    public int SortOrder { get; init; } = 0;
    public List<ShoppingListStoreRecord>? Stores { get; init; } = new List<ShoppingListStoreRecord>();
    public List<Guid>? ListItemIds { get; init; } = new List<Guid>();
}
