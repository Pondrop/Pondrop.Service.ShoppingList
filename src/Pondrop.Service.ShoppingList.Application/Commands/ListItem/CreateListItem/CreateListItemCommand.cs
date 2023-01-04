using MediatR;
using Pondrop.Service.ShoppingList.Application.Models;
using Pondrop.Service.ShoppingList.Domain.Models;

namespace Pondrop.Service.ShoppingList.Application.Commands;

public class CreateListItemCommand : IRequest<Result<ListItemRecord>>
{
    public string ItemTitle { get; init; } = String.Empty;

    public Guid AddedBy { get; init; } = Guid.Empty;

    public Guid SelectedCategoryId { get; init; } = Guid.Empty;

    public int Quantity { get; init; } = 0;

    public double ItemNetSize { get; init; } = 0;

    public string ItemUOM { get; init; } = string.Empty;

    public List<Guid> SelectedPreferenceIds { get; init; } = new List<Guid>();

    public Guid SelectedProductId { get; init; } = Guid.Empty;

}