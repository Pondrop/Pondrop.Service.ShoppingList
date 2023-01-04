using Dapr.Client;
using Microsoft.Extensions.Logging;
using Pondrop.Service.Store.Application.Interfaces;
using Pondrop.Service.Store.Domain.Events;

namespace Pondrop.Service.Store.Infrastructure.Dapr
{
    public class DaprService : IDaprService
    {
        private readonly DaprClient _daprClient;
        private readonly ILogger<DaprService> _logger;

        public DaprService(ILogger<DaprService> logger)
        {
            _daprClient = new DaprClientBuilder().Build();
            _logger = logger;
        }

        public async Task<bool> InvokeServiceAsync(string appId, string methodName, object? data = null)
        {
            var success = false;

            if (!string.IsNullOrEmpty(appId) && !string.IsNullOrEmpty(methodName))
            {
                try
                {
                    if (data is not null)
                        await _daprClient.InvokeMethodAsync(appId, methodName, data);
                    else
                        await _daprClient.InvokeMethodAsync(HttpMethod.Get, appId, methodName);

                    success = true;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, ex.Message);
                }
            }

            return success;
        }

        public async Task<bool> SendEventsAsync(string eventGridTopic, IEnumerable<IEvent> events)
        {
            var success = false;
            
            if (!string.IsNullOrEmpty(eventGridTopic))
            {
                try
                {
                    if (events.Any())
                        await _daprClient.InvokeBindingAsync(eventGridTopic, "create", events);
                    
                    success = true;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, ex.Message);
                }
            }

            return success;
        }
    }
}
