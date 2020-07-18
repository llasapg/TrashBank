using MediatR;

namespace TrashBank.Domain.Core.Commands
{
    public interface ICommand : IRequest<bool>
    {}
}
