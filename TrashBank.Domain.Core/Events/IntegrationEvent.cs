using System;
namespace TrashBank.Domain.Core.Events
{
    public abstract class IntegrationEvent : IIntegrationEvent
    {
        public readonly Guid _Id;
        public readonly DateTime _timeSpan;
        public readonly string _eventName;

        public IntegrationEvent()
        {
            _Id = new Guid();
            _timeSpan = DateTime.Now;
            _eventName = this.GetType().Name;
        }
    }
}
