using MediatR;
using Pondrop.Service.ShoppingList.Application.Models;
using Pondrop.Service.ShoppingList.Domain.Models;

namespace Pondrop.Service.ShoppingList.Application.Queries;

public class GetShoppingListByIdQuery : IRequest<Result<ShoppingListResponseRecord?>>
{
    public Guid Id { get; init; } = Guid.Empty;
}