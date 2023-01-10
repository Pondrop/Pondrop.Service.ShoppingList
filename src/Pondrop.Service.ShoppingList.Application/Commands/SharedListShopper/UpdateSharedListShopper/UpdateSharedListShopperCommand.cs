using MediatR;
using Pondrop.Service.ShoppingList.Application.Models;
using Pondrop.Service.ShoppingList.Domain.Enums.ShoppingList;
using Pondrop.Service.ShoppingList.Domain.Models;

namespace Pondrop.Service.ShoppingList.Application.Commands;

public class UpdateSharedListShopperCommand : IRequest<Result<List<SharedListShopperRecord>>>
{
    public List<SharedListShopperUpdateRecord> SharedListShoppers { get; set; } = new List<SharedListShopperUpdateRecord>();
    public Guid ShoppingListId { get; init; } = Guid.Empty;
}

public record SharedListShopperUpdateRecord(
    Guid Id,
    Guid UserId,
    int SortOrder)
{
    public SharedListShopperUpdateRecord() : this(
        Guid.Empty,
        Guid.Empty,
        0)
    {
    }
}