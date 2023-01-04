using Microsoft.Extensions.Logging;
using Pondrop.Service.Interfaces;
using Pondrop.Service.ShoppingList.Domain.Models;

namespace Pondrop.Service.ShoppingList.Application.Commands;

public class RebuildSharedListShopperCheckpointCommandHandler : RebuildCheckpointCommandHandler<RebuildSharedListShopperCheckpointCommand, SharedListShopperEntity>
{
    public RebuildSharedListShopperCheckpointCommandHandler(
        ICheckpointRepository<SharedListShopperEntity> sharedListShopperCheckpointRepository,
        ILogger<RebuildSharedListShopperCheckpointCommandHandler> logger) : base(sharedListShopperCheckpointRepository, logger)
    {
    }
}