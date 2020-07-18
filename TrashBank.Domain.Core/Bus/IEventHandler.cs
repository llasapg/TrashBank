using System;
using System.Threading.Tasks;
using TrashBank.Domain.Core.Events;

namespace TrashBank.Domain.Core.Bus
{
    public interface IEventHandler<T> where T : IIntegrationEvent
    {
        Task Handle(T @event);
    }
}
