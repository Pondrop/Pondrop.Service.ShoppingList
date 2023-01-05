using MediatR;
using Pondrop.Service.ShoppingList.Application.Models;
using Pondrop.Service.ShoppingList.Domain.Models;

namespace Pondrop.Service.ShoppingList.Application.Queries;

public class GetSharedListShopperByShoppingListIdQuery : IRequest<Result<List<SharedListShopperRecord?>>>
{
    public Guid ShoppingListId { get; init; } = Guid.Empty;
}