using System;
using System.Collections.Generic;

namespace Orchestrator.Models
{
    public class CoreKraftResponseException : Exception
    {
        public CoreKraftResponseException(string message, List<ErrorMessage> errorMessages) : this(message, null, errorMessages) { }

        public CoreKraftResponseException(string message, Exception innerException, List<ErrorMessage> errorMessages) :
            base(message, innerException)
        {
            ErrorMessages = errorMessages;
        }

        public List<ErrorMessage> ErrorMessages { get; private set; }
    }
}
