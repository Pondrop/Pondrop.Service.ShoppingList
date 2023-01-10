using Pondrop.Service.Models;
using Pondrop.Service.ShoppingList.Domain.Enums.ShoppingList;

namespace Pondrop.Service.ShoppingList.Domain.Models;

public record SharedListShopperResponseRecord(
    Guid Id, 
    Guid UserId,
    ListPrivilegeType ListPrivilege)
{
    public SharedListShopperResponseRecord() : this(
        Guid.Empty,
        Guid.Empty,
        ListPrivilegeType.unknown)
    {
    }
}