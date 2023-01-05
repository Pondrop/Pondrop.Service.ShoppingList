using MediatR;
using Pondrop.Service.ShoppingList.Application.Models;
using Pondrop.Service.ShoppingList.Domain.Models;

namespace Pondrop.Service.ShoppingList.Application.Commands;

public class CreateListItemCommand : IRequest<Result<List<ListItemRecord>>>
{
    public List<ListItemItemRecord> ListItems { get; set; } = new List<ListItemItemRecord>();
    public Guid ShoppingListId { get; init; } = Guid.Empty;
}

public record ListItemItemRecord(
    string ItemTitle,
    Guid AddedBy,
    Guid SelectedCategoryId,
    int Quantity,
    double ItemNetSize,
    string ItemUOM,
    List<Guid> SelectedPreferenceIds,
    Guid SelectedProductId,
    Guid? StoreId,
    int SortOrder)
{
    public ListItemItemRecord() : this(
        string.Empty,
        Guid.Empty,
        Guid.Empty,
        0,
        0,
        string.Empty,
        new List<Guid>(),
        Guid.Empty,
        null,
        0)
    {
    }
}