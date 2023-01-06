using MediatR;
using Pondrop.Service.ShoppingList.Application.Models;
using Pondrop.Service.ShoppingList.Domain.Enums.ShoppingList;
using Pondrop.Service.ShoppingList.Domain.Models;

namespace Pondrop.Service.ShoppingList.Application.Commands;

public class UpdateListItemCommand : IRequest<Result<List<ListItemRecord>>>
{
    public List<ListItemUpdateRecord> ListItems { get; set; } = new List<ListItemUpdateRecord>();
    public Guid ShoppingListId { get; init; } = Guid.Empty;
}

public record ListItemUpdateRecord(
    Guid Id,
    int SortOrder,
    bool Checked)
{
    public ListItemUpdateRecord() : this(
        Guid.Empty,
        0, 
        false)
    {
    }
}