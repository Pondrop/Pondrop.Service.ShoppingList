using MediatR;
using Pondrop.Service.ShoppingList.Application.Models;
using Pondrop.Service.ShoppingList.Domain.Enums.ShoppingList;
using Pondrop.Service.ShoppingList.Domain.Models;

namespace Pondrop.Service.ShoppingList.Application.Commands;

public class CreateSharedListShopperCommand : IRequest<Result<List<SharedListShopperRecord>>>
{
    public List<SharedListShopperItemRecord> SharedListShoppers { get; set; } = new List<SharedListShopperItemRecord>();
    public Guid ShoppingListId { get; init; } = Guid.Empty;


}

public record SharedListShopperItemRecord(
    Guid ShopperId,
    ListPrivilegeType ListPrivilege)
{
    public SharedListShopperItemRecord() : this(
        Guid.Empty,
        ListPrivilegeType.unknown)
    {

    }
}

