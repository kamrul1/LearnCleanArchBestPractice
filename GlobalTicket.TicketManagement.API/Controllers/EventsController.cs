using GlobalTicket.TicketManagement.Application.Features.Events.Commands.CreateEvent;
using GlobalTicket.TicketManagement.Application.Features.Events.Commands.DeleteEvent;
using GlobalTicket.TicketManagement.Application.Features.Events.Commands.UpdateEvent;
using GlobalTicket.TicketManagement.Application.Features.Events.Queries.GetEventDetail;
using GlobalTicket.TicketManagement.Application.Features.Events.Queries.GetEventsExport;
using GlobalTicket.TicketManagement.Application.Features.Events.Queries.GetEventsList;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlobalTicket.TicketManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventsController:ControllerBase
    {
        private readonly IMediator mediator;

        public EventsController(IMediator mediator)
        {
            this.mediator = mediator;
        }

        [HttpGet(Name ="GetAllEvents")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<List<EventListVm>>> GetAllEvents()
        {
            var result = await mediator.Send(new GetEventsListQuery());
            return Ok(result);
        }

        [HttpGet("{id}", Name ="GetEentById")]
        public async Task<ActionResult<EventDetailVm>> GetEventById(Guid id)
        {
            var getEventDetailQuery = new GetEventDetailQuery() { Id = id };
            var eventDetailDto = await mediator.Send(getEventDetailQuery);
            return Ok(eventDetailDto);
        }

        [HttpPost(Name ="AddEvent")]
        public async Task<ActionResult<Guid>> Create([FromBody] CreateEventCommand createEventCommand)
        {
            var id = await mediator.Send(createEventCommand);
            return Ok(id);
        }

        [HttpPut(Name ="UpdateEvent")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        public async Task<ActionResult> Update([FromBody] UpdateEventCommand updateEventCommand)
        {
            await mediator.Send(updateEventCommand);
            return NoContent();
        }



        [HttpDelete("{id}",Name ="DeleteEvent")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        public async Task<ActionResult> Delete(Guid id)
        {
            var deleteEventCommand = new DeleteEventCommand() { EventId = id };
            await mediator.Send(deleteEventCommand);
            return NoContent();
        }

        [HttpGet("export", Name ="ExportEvents")]
        public async Task<FileResult> ExportEvents()
        {
            var fileDto = await mediator.Send(new GetEventsExportQuery());
            return File(fileDto.Data, fileDto.ContentType, fileDto.EventExportFileName);
        }



        
    }
}
