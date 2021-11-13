﻿using AutoMapper;
using GlobalTicket.TicketManagement.Application.Contracts.Infrastructure;
using GlobalTicket.TicketManagement.Application.Contracts.Presistence;
using GlobalTicket.TicketManagement.Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GlobalTicket.TicketManagement.Application.Features.Events.Queries.GetEventsExport
{
    public class GetEventsExportQueryHandler: IRequestHandler<GetEventsExportQuery, EventExportFileVm>
    {
        private readonly IMapper mapper;
        private readonly IAsyncRepository<Event> eventRepository;
        private readonly ICsvExporter csvExporter;

        public GetEventsExportQueryHandler(IMapper mapper, IAsyncRepository<Event> eventRepository, ICsvExporter csvExporter)
        {
            this.mapper = mapper;
            this.eventRepository = eventRepository;
            this.csvExporter = csvExporter;
        }

        public async Task<EventExportFileVm> Handle(GetEventsExportQuery request, CancellationToken cancellationToken)
        {
            var allEvents = mapper.Map<List<EventExportDto>>((await eventRepository.ListAllAsync()).OrderBy(x => x.Date));
            var fileData = csvExporter.ExportEventsToCsv(allEvents);

            var eventExportFileDto = new EventExportFileVm() { ContentType = "text/csv", Data = fileData, EventExportFileName = $"{Guid.NewGuid()}.csv" };

            return eventExportFileDto;

            
        }
    }
}
