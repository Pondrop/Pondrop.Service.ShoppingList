﻿using Pondrop.Service.Events;
using Pondrop.Service.ShoppingList.Domain.Enums.ShoppingList;

namespace Pondrop.Service.ShoppingList.Domain.Events.ShoppingList;
public record CreateSharedListShopper(
    Guid Id,
    Guid ShopperId,
    ListPrivilegeType ListPrivilege) : EventPayload;