using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#nullable enable

namespace Ccf.Ck.SysPlugins.Support.ActionQueryLibs.BasicWeb
{
    public class CoreKraftResponseException : Exception
    {
        public CoreKraftResponseException(string? message, List<ErrorMessage>? errorMessages) : this(message, null, errorMessages) { }

        public CoreKraftResponseException(string? message, Exception? innerException, List<ErrorMessage>? errorMessages) :
            base(message, innerException)
        {
            ErrorMessages = errorMessages;
        }

        public List<ErrorMessage>? ErrorMessages { get; private set; }
    }
}
