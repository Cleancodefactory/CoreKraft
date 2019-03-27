using System;

namespace Ccf.Ck.Models.Exceptions
{
    public class KraftException : Exception
    {
        public KraftException()
        {
        }

        public KraftException(string message)
        : base(message)
        {
        }

        public KraftException(Exception inner)
        : base(string.Empty, inner)
        {
        }

        public KraftException(string message, Exception inner)
        : base(message, inner)
        {
        }
    }
}
