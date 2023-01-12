using Pondrop.Service.Models;
using Pondrop.Service.ShoppingList.Domain.Enums.ShoppingList;

namespace Pondrop.Service.ShoppingList.Domain.Models;
public record ShoppingListStoreRecord(
    Guid? StoreId,
    string StoreTitle,
    int SortOrder)
{
    public ShoppingListStoreRecord() : this(
        Guid.Empty,
        string.Empty,
        0)
    {
    }
}
