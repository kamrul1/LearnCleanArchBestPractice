﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlobalTicket.TicketManagement.Application.Models.Mail
{
    public class EmailSettings
    {
        public string APiKey { get; set; }
        public string FromAddress { get; set; }
        public string FromName { get; set; }
    }
}
