using AutoMapper;
using GlobalTicket.TicketManagement.Application.Features.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlobalTicket.TicketManagement.Application.Features.Profiles
{
    public class MappingProfile:Profile
    {
        public MappingProfile()
        {
            CreateMap<EventArgs, EventListVm>().ReverseMap();
        }
    }
}
