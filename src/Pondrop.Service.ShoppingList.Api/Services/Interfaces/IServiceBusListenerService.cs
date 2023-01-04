using Pondrop.Service.ShoppingList.Application.Commands;
using System.Collections.Concurrent;

namespace Pondrop.Service.ShoppingList.Api.Services;

public interface IServiceBusListenerService
{
    Task StartListener();

    Task StopListener();

    ValueTask DisposeAsync();
}