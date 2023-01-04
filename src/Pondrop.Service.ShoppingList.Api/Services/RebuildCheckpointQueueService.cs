using Pondrop.Service.ShoppingList.Application.Commands;

namespace Pondrop.Service.ShoppingList.Api.Services;

public class RebuildCheckpointQueueService : BaseBackgroundQueueService<RebuildCheckpointCommand>, IRebuildCheckpointQueueService
{
}