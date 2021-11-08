using FluentValidation.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlobalTicket.TicketManagement.Application.Exceptions
{
    public class ValidationException: ApplicationException
    {
        public List<string> ValidationErrors { get; set; }

        public ValidationException(ValidationResult validationResult)
        {
            ValidationErrors = new List<string>();

            foreach(var valiationError in validationResult.Errors)
            {
                ValidationErrors.Add(valiationError.ErrorMessage);
            }

        }
    }
}
