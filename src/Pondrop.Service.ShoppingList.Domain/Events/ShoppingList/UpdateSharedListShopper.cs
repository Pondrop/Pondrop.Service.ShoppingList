﻿using Pondrop.Service.Events;
using Pondrop.Service.ShoppingList.Domain.Enums.ShoppingList;

namespace Pondrop.Service.ShoppingList.Domain.Events.SharedListShopper;
public record UpdateSharedListShopper(
    Guid Id,
    Guid UserId,
    ListPrivilegeType ListPrivilege,
    int SortOrder) : EventPayload;