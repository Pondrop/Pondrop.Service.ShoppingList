using MediatR;
using Pondrop.Service.ShoppingList.Application.Models;
using Pondrop.Service.ShoppingList.Domain.Models;

namespace Pondrop.Service.ShoppingList.Application.Commands;

public class DeleteListItemCommand : IRequest<Result<List<ListItemRecord>>>
{
    public Guid? ShoppingListId { get; init; } = null;
    public List<Guid>? ListItemIds { get; init; } = null;
}