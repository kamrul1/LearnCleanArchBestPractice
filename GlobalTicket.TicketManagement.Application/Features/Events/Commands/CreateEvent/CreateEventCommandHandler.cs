using AutoMapper;
using GlobalTicket.TicketManagement.Application.Contracts.Presistence;
using GlobalTicket.TicketManagement.Application.Exceptions;
using GlobalTicket.TicketManagement.Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GlobalTicket.TicketManagement.Application.Features.Events.Commands.CreateEvent
{
    public class CreateEventCommandHandler : IRequestHandler<CreateEventCommand, Guid>
    {
        private readonly IMapper mapper;
        private readonly IEventRepository eventRepository;

        public CreateEventCommandHandler(IMapper mapper, IEventRepository eventRepository)
        {
            this.mapper = mapper;
            this.eventRepository = eventRepository;
        }
        public async Task<Guid> Handle(CreateEventCommand request, CancellationToken cancellationToken)
        {
            var validator = new CreateEventCommandValidator();
            var validationResult = await validator.ValidateAsync(request);

            if (validationResult.Errors.Count > 0)
            {
                throw new ValidationException(validationResult);
            }

            var @event = mapper.Map<Event>(request);
            @event = await eventRepository.AddAsync(@event);

            return @event.EventId;
        }
    }
}

