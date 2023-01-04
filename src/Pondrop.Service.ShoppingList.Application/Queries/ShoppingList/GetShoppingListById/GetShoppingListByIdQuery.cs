using MediatR;
using Pondrop.Service.ShoppingList.Application.Models;
using Pondrop.Service.ShoppingList.Domain.Models;

namespace Pondrop.Service.ShoppingList.Application.Queries;

public class GetShoppingListByIdQuery : IRequest<Result<ShoppingListRecord?>>
{
    public Guid Id { get; init; } = Guid.Empty;
}