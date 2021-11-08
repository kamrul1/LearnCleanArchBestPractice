using AutoMapper;
using GlobalTicket.TicketManagement.Application.Contracts.Presistence;
using GlobalTicket.TicketManagement.Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GlobalTicket.TicketManagement.Application.Features.Events.Commands.UpdateEvent
{
    public class UpdateEventCommandHandler : IRequestHandler<UpdateEventCommand>
    {
        private readonly IMapper mapper;
        private readonly IEventRepository eventRepository;

        public UpdateEventCommandHandler(IMapper mapper, IEventRepository eventRepository)
        {
            this.mapper = mapper;
            this.eventRepository = eventRepository;
        }
        public async Task<Unit> Handle(UpdateEventCommand request, CancellationToken cancellationToken)
        {
            var eventToUpdate = await eventRepository.GetByIdAsync(request.EventId);
            mapper.Map(request, eventToUpdate, typeof(UpdateEventCommand), typeof(Event));

            await eventRepository.UpdateAsync(eventToUpdate);

            return Unit.Value;
        }
    }
}
