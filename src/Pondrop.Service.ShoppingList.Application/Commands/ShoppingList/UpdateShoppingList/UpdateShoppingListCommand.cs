using MediatR;
using Pondrop.Service.ShoppingList.Application.Models;
using Pondrop.Service.ShoppingList.Domain.Enums.ShoppingList;
using Pondrop.Service.ShoppingList.Domain.Models;

namespace Pondrop.Service.ShoppingList.Application.Commands;


public class UpdateShoppingListCommand : IRequest<Result<List<ShoppingListRecord>>>
{
    public List<ShoppingListItemRecord> ShoppingLists { get; set; } = new List<ShoppingListItemRecord>();
}


public record ShoppingListItemRecord(
    Guid Id,
    string Name)
{
    public ShoppingListItemRecord() : this(
        Guid.Empty,
        string.Empty)
    {
    }
}
