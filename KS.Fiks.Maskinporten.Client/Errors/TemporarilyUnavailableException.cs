using System;

namespace Ks.Fiks.Maskinporten.Client.Errors
{
    public class TemporarilyUnavailableException : UnexpectedResponseException
    {
        public TemporarilyUnavailableException(string message) : base(message)
        {
        }

        public TemporarilyUnavailableException()
            : base()
        {
        }

        public TemporarilyUnavailableException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}