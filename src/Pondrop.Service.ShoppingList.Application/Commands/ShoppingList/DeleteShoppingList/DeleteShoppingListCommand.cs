using MediatR;
using Pondrop.Service.ShoppingList.Application.Models;
using Pondrop.Service.ShoppingList.Domain.Enums.ShoppingList;
using Pondrop.Service.ShoppingList.Domain.Models;

namespace Pondrop.Service.ShoppingList.Application.Commands;

public class DeleteShoppingListCommand : IRequest<Result<List<ShoppingListRecord>>>
{
    public List<Guid>? Ids { get; init; } = null;
}

