﻿using Pondrop.Service.Models;
using Pondrop.Service.ShoppingList.Domain.Enums.ShoppingList;

namespace Pondrop.Service.ShoppingList.Domain.Models;

public record ListItemRecord(
    Guid Id,
    string ItemTitle,
    Guid SelectedCategoryId,
    int Quantity,
    double ItemNetSize,
    string ItemUOM,
    List<Guid> SelectedPreferenceIds,
    Guid? SelectedProductId,
    Guid? StoreId,
    int SortOrder,
    bool Checked,
    string CreatedBy,
        string UpdatedBy,
        DateTime CreatedUtc,
        DateTime UpdatedUtc,
        DateTime? DeletedUtc)
    : AuditRecord(CreatedBy, UpdatedBy, CreatedUtc, UpdatedUtc, DeletedUtc)
{
    public ListItemRecord() : this(
        Guid.Empty,
        string.Empty,
        Guid.Empty,
        0,
        0,
        string.Empty,
        new List<Guid>(),
        null,
        null,
        0,
        false,
        string.Empty,
        string.Empty,
        DateTime.MinValue,
        DateTime.MinValue,
        null)
    {
    }
}