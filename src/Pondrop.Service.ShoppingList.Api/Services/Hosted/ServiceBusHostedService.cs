using MediatR;
using Pondrop.Service.ShoppingList.Application.Commands;

namespace Pondrop.Service.ShoppingList.Api.Services;

public class ServiceBusHostedService : IHostedService, IDisposable
{
    private readonly IServiceBusListenerService _serviceBusListenerService;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ServiceBusHostedService> _logger;

    private bool _disposed;

    public ServiceBusHostedService(
        IServiceBusListenerService serviceBusListenerService,
        ILogger<ServiceBusHostedService> logger)
    {
        _serviceBusListenerService = serviceBusListenerService;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken stoppingToken)
    {
        await _serviceBusListenerService.StartListener().ConfigureAwait(false);
    }

    public async Task StopAsync(CancellationToken stoppingToken)
    {
        await _serviceBusListenerService.StopListener().ConfigureAwait(false);
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        Dispose(true);
        GC.SuppressFinalize(this);

        _disposed = true;
    }

    protected virtual async void Dispose(bool disposing)
    {
        if (disposing)
        {
            await _serviceBusListenerService.DisposeAsync().ConfigureAwait(false);
        }
    }
}