using System;
namespace TrashBank.Domain.Core.Commands
{
    public abstract class Command
    {
        public readonly Guid _Id;
        public readonly DateTime _timeSpan;
        public readonly string _commandName;

        public Command()
        {
            _Id = new Guid();
            _timeSpan = DateTime.Now;
            _commandName = this.GetType().Name;
        }
    }
}
