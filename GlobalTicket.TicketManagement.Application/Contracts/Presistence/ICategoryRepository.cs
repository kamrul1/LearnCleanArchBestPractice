using GlobalTicket.TicketManagement.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlobalTicket.TicketManagement.Application.Contracts.Presistence
{
    public interface ICategoryRepository : IAsyncRepository<Category>
    {
        
    }
}
