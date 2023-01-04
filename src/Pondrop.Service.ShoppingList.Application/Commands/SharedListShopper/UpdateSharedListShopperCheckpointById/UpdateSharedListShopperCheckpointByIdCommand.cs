using MediatR;
using Pondrop.Service.ShoppingList.Application.Commands;
using Pondrop.Service.ShoppingList.Application.Models;
using Pondrop.Service.ShoppingList.Domain.Models;

namespace Pondrop.Service.SharedListShopper.Application.Commands;

public class UpdateSharedListShopperCheckpointByIdCommand : UpdateCheckpointByIdCommand<Result<SharedListShopperRecord>>
{
}