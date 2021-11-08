using GlobalTicket.TicketManagement.Application.Contracts.Presistence;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GlobalTicket.TicketManagement.Application.Features.Events.Commands.DeleteEvent
{
    public class DeleteEventCommandHandler : IRequestHandler<DeleteEventCommand>
    {
        private readonly IEventRepository eventRepository;

        public DeleteEventCommandHandler(IEventRepository eventRepository)
        {
            this.eventRepository = eventRepository;
        }

        public async Task<Unit> Handle(DeleteEventCommand request, CancellationToken cancellationToken)
        {
            var eventToDelete = await eventRepository.GetByIdAsync(request.EventId);

            await eventRepository.DeleteAsync(eventToDelete);
            return Unit.Value;
        }
    }
}
