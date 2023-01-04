using Microsoft.Extensions.Logging;
using Pondrop.Service.Interfaces;
using Pondrop.Service.ShoppingList.Domain.Models;

namespace Pondrop.Service.ShoppingList.Application.Commands;

public class RebuildShoppingListCheckpointCommandHandler : RebuildCheckpointCommandHandler<RebuildShoppingListCheckpointCommand, ShoppingListEntity>
{
    public RebuildShoppingListCheckpointCommandHandler(
        ICheckpointRepository<ShoppingListEntity> shoppingListCheckpointRepository,
        ILogger<RebuildShoppingListCheckpointCommandHandler> logger) : base(shoppingListCheckpointRepository, logger)
    {
    }
}