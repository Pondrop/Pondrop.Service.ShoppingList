using MediatR;
using Pondrop.Service.ShoppingList.Application.Models;
using Pondrop.Service.ShoppingList.Domain.Models;

namespace Pondrop.Service.ShoppingList.Application.Commands;

public class CreateListItemCommand : IRequest<Result<List<ListItemRecord>>>
{
    public List<ListItemCreateRecord> ListItems { get; set; } = new List<ListItemCreateRecord>();
    public Guid ShoppingListId { get; init; } = Guid.Empty;
}

public record ListItemCreateRecord(
    string ItemTitle,
    Guid SelectedCategoryId,
    int Quantity,
    double ItemNetSize,
    string ItemUOM,
    List<Guid> SelectedPreferenceIds,
    Guid SelectedProductId,
    Guid? StoreId,
    int SortOrder,
    bool Checked)
{
    public ListItemCreateRecord() : this(
        string.Empty,
        Guid.Empty,
        0,
        0,
        string.Empty,
        new List<Guid>(),
        Guid.Empty,
        null,
        0,
        false)
    {
    }
}