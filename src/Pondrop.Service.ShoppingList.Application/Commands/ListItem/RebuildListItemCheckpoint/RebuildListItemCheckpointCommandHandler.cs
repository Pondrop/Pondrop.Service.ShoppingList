using Microsoft.Extensions.Logging;
using Pondrop.Service.Interfaces;
using Pondrop.Service.ShoppingList.Domain.Models;

namespace Pondrop.Service.ShoppingList.Application.Commands;

public class RebuildListItemCheckpointCommandHandler : RebuildCheckpointCommandHandler<RebuildListItemCheckpointCommand, ListItemEntity>
{
    public RebuildListItemCheckpointCommandHandler(
        ICheckpointRepository<ListItemEntity> listItemCheckpointRepository,
        ILogger<RebuildListItemCheckpointCommandHandler> logger) : base(listItemCheckpointRepository, logger)
    {
    }
}