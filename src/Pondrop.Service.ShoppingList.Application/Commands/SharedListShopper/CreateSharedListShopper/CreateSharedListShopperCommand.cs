using MediatR;
using Pondrop.Service.ShoppingList.Application.Models;
using Pondrop.Service.ShoppingList.Domain.Enums.ShoppingList;
using Pondrop.Service.ShoppingList.Domain.Models;

namespace Pondrop.Service.ShoppingList.Application.Commands;

public class CreateSharedListShopperCommand : IRequest<Result<List<SharedListShopperRecord>>>
{
    public List<SharedListShopperCreateRecord> SharedListShoppers { get; set; } = new List<SharedListShopperCreateRecord>();
    public Guid ShoppingListId { get; init; } = Guid.Empty;


}

public record SharedListShopperCreateRecord(
    Guid ShopperId,
    ListPrivilegeType ListPrivilege)
{
    public SharedListShopperCreateRecord() : this(
        Guid.Empty,
        ListPrivilegeType.unknown)
    {

    }
}

