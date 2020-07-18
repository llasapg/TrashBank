using System;
using Microsoft.Extensions.DependencyInjection;
using TrashBank.Domain.Core.Bus;
using TrashBank.Infra.Bus;
using MediatR;

namespace TrashBank.Infra.Ioc
{
    public class DependencyContainer
    {
        public static void RegisterServicesForBanking(IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<IEventBus, RabbitMQEventBus>();
            var assembly = AppDomain.CurrentDomain.Load("TrashBank.Microservices.Banking");
            serviceCollection.AddMediatR(assembly);
        }

        public static void RegisterServicesForTransfer(IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<IEventBus, RabbitMQEventBus>();
            var assembly = AppDomain.CurrentDomain.Load("TrashBank.Microservices.Transfer");
            serviceCollection.AddMediatR(assembly);
        }

        public static void RegisterServicesForMonitoring(IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<IEventBus, RabbitMQEventBus>();
            var assembly = AppDomain.CurrentDomain.Load("TrashBank.Microservices.Monitoring");
            serviceCollection.AddMediatR(assembly);
        }
    }
}
