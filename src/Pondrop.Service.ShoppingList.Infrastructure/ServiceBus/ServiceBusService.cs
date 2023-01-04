using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Pondrop.Service.Store.Application.Interfaces;
using Pondrop.Service.Store.Application.Models;
using System;

namespace Pondrop.Service.Store.Infrastructure.ServiceBus
{
    public class ServiceBusService : IServiceBusService
    {
        private readonly ILogger<ServiceBusService> _logger;
        private readonly ServiceBusConfiguration _config;

        private readonly ServiceBusClient _serviceBusClient;
        private readonly ServiceBusSender _sender;

        public ServiceBusService(
            IOptions<ServiceBusConfiguration> config,
            ILogger<ServiceBusService> logger)
        {
            _logger = logger;

            if (string.IsNullOrEmpty(config.Value?.ConnectionString))
                throw new ArgumentException("Service Bus 'ConnectionString' cannot be null or empty");
            if (string.IsNullOrEmpty(config.Value?.QueueName))
                throw new ArgumentException("Service Bus 'QueueName' cannot be null or empty"); ;

            _config = config.Value;
            _serviceBusClient = new ServiceBusClient(_config.ConnectionString);
            _sender = _serviceBusClient.CreateSender(_config.QueueName);
        }

        public Task SendMessageAsync(object payload)
        {
            if (payload is not null)
                return SendMessageAsync(payload.GetType().Name, payload);

            return Task.CompletedTask;
        }

        public async Task SendMessageAsync(string subject, object payload)
        {
            if (!string.IsNullOrWhiteSpace(subject) && payload is not null)
            {
                try
                {
                    var jsonMessage = JsonConvert.SerializeObject(payload);
                    var serviceBusMessage = new ServiceBusMessage(jsonMessage);

                    serviceBusMessage.Subject = subject;
                    await _sender.SendMessageAsync(serviceBusMessage);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, ex.Message);
                }
            }
        }
    }
}

