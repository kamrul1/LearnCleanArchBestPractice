﻿using GlobalTicket.TicketManagement.Application.Contracts.Infrastructure;
using GlobalTicket.TicketManagement.Application.Models.Mail;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlobalTicket.TicketManagement.Infrastructure.Mail
{
    public class EmailService : IEmailService
    {
        public EmailSettings emailSettings { get; set; }
        public EmailService(IOptions<EmailSettings> mailSettings)
        {
            this.emailSettings = mailSettings.Value;
        }

        public async Task<bool> SendEmail(Email email)
        {
            var client = new SendGridClient(emailSettings.APiKey);

            var subject = email.Subject;
            var to = new EmailAddress(email.To);
            var emailBody = email.Body;

            var from = new EmailAddress
            {
                Email = emailSettings.FromAddress,
                Name = emailSettings.FromName
            };

            var sendGridMessage = MailHelper.CreateSingleEmail(from, to, subject, emailBody, emailBody);
            var response = await client.SendEmailAsync(sendGridMessage);

            if(response.StatusCode==System.Net.HttpStatusCode.Accepted 
                || response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return true;
            }

            return false;
        }
    }
}
