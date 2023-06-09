﻿using Pondrop.Service.Models;
using Pondrop.Service.ShoppingList.Domain.Enums.ShoppingList;

namespace Pondrop.Service.ShoppingList.Domain.Models;
public record ShoppingListRecord(
 Guid Id,
    string Name,
    ShoppingListType? ShoppingListType,
    List<ShoppingListStoreRecord>? Stores,
    List<Guid>? SharedListShopperIds,
    List<Guid>? ListItemIds,
        string CreatedBy,
        string UpdatedBy,
        DateTime CreatedUtc,
        DateTime UpdatedUtc,
        DateTime? DeletedUtc)
    : AuditRecord(CreatedBy, UpdatedBy, CreatedUtc, UpdatedUtc, DeletedUtc)
{
    public ShoppingListRecord() : this(
        Guid.Empty,
        string.Empty,
        null,
        new List<ShoppingListStoreRecord>(0),
        new List<Guid>(0),
        new List<Guid>(0),
        string.Empty,
        string.Empty,
        DateTime.MinValue,
        DateTime.MinValue,
        null)
    {
    }
}