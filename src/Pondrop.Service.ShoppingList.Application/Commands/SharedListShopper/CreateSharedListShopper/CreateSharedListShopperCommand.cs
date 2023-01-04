using MediatR;
using Pondrop.Service.ShoppingList.Application.Models;
using Pondrop.Service.ShoppingList.Domain.Enums.ShoppingList;
using Pondrop.Service.ShoppingList.Domain.Models;

namespace Pondrop.Service.ShoppingList.Application.Commands;

public class CreateSharedListShopperCommand : IRequest<Result<SharedListShopperRecord>>
{
    public Guid ShopperId { get; init; } = Guid.Empty;

    public ListPrivilegeType ListPrivilege { get; init; } = ListPrivilegeType.unknown;

}