﻿using MediatR;
using Pondrop.Service.ShoppingList.Application.Models;
using Pondrop.Service.ShoppingList.Domain.Models;

namespace Pondrop.Service.ShoppingList.Application.Queries;

public class GetAllSharedListShoppersQuery : IRequest<Result<List<SharedListShopperRecord>>>
{
}