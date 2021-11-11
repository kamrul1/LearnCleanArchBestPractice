using AutoMapper;
using GlobalTicket.TicketManagement.Application.Contracts.Infrastructure;
using GlobalTicket.TicketManagement.Application.Contracts.Presistence;
using GlobalTicket.TicketManagement.Application.Exceptions;
using GlobalTicket.TicketManagement.Application.Models.Mail;
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
        private readonly IEmailService emailService;

        public CreateEventCommandHandler(IMapper mapper, IEventRepository eventRepository, IEmailService emailService)
        {
            this.mapper = mapper;
            this.eventRepository = eventRepository;
            this.emailService = emailService;
        }
        public async Task<Guid> Handle(CreateEventCommand request, CancellationToken cancellationToken)
        {
            var validator = new CreateEventCommandValidator(eventRepository);
            var validationResult = await validator.ValidateAsync(request);

            if (validationResult.Errors.Count > 0)
            {
                throw new ValidationException(validationResult);
            }

            var @event = mapper.Map<Event>(request);
            @event = await eventRepository.AddAsync(@event);

            var email = new Email
            {
                To="gill@snowball.be", Body=$"A new event was created {request}", Subject="A new event was created"
            };

            try
            {
                await emailService.SendEmail(email);
            }
            catch (Exception ex)
            {

                //this shouldn't stop the API from doing anything else so that can be logged
            }

            return @event.EventId;
        }
    }
}

