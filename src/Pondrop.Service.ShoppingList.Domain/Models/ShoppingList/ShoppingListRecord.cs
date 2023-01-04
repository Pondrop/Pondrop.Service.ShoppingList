using Pondrop.Service.Models;
using Pondrop.Service.ShoppingList.Domain.Enums.ShoppingList;

namespace Pondrop.Service.ShoppingList.Domain.Models;

public record SharedListShopperRecord(
    Guid Id,
    Guid ShopperId,
    ListPrivilegeType ListPrivilege,
    string CreatedBy,
    string UpdatedBy,
    DateTime CreatedUtc,
    DateTime UpdatedUtc) : AuditRecord(CreatedBy, UpdatedBy, CreatedUtc, UpdatedUtc)
{
    public SharedListShopperRecord() : this(
        Guid.Empty,
        Guid.Empty,
        ListPrivilegeType.unknown,
        string.Empty,
        string.Empty,
        DateTime.MinValue,
        DateTime.MinValue)
    {
    }
}