using System;
using System.Threading.Tasks;
using TrashBank.Domain.Core.Commands;
using TrashBank.Domain.Core.Events;

namespace TrashBank.Domain.Core.Bus
{
    /// <summary>
    /// Core interface to work with message brockers ( Like rabbitMq and others )
    /// </summary>
    public interface IEventBus
    {
        Task SendCommand<T>(T command) where T : ICommand;

        void Publish<T>(T @event) where T : IIntegrationEvent;

        // Subscribe + Unsubscribe

        void Subscribe<T, TH>()
            where T : IIntegrationEvent
            where TH : IEventHandler<T>;

        void UnSubscribe<T, TH>()
            where T : IIntegrationEvent
            where TH : IEventHandler<T>;
    }
}
