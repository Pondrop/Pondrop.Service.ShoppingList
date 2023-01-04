using Pondrop.Service.ShoppingList.Application.Commands;
using System.Collections.Concurrent;

namespace Pondrop.Service.ShoppingList.Api.Services;

public abstract class BaseBackgroundQueueService<T>
{
    private readonly ConcurrentQueue<T> _items = new ConcurrentQueue<T>();
    private readonly SemaphoreSlim _signal = new SemaphoreSlim(0);

    public async Task<T> DequeueAsync(CancellationToken cancellationToken)
    {
        await _signal.WaitAsync(cancellationToken);
        _items.TryDequeue(out var item);
        return item!;
    }

    public void Queue(T item)
    {
        _items.Enqueue(item);
        _signal.Release();
    }
}