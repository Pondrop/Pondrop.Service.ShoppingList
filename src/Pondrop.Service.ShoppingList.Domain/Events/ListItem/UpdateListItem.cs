﻿using Pondrop.Service.Events;

namespace Pondrop.Service.ShoppingList.Domain.Events.ListItem;
public record UpdateListItem(
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
    bool Checked
    ) : EventPayload;