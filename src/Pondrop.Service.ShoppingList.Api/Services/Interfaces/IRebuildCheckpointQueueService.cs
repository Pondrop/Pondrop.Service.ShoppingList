using Pondrop.Service.ShoppingList.Application.Commands;

namespace Pondrop.Service.ShoppingList.Api.Services;

public interface IRebuildCheckpointQueueService
{
    Task<RebuildCheckpointCommand> DequeueAsync(CancellationToken cancellationToken);
    void Queue(RebuildCheckpointCommand command);
}