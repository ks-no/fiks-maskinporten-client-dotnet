using System;

namespace Ks.Fiks.Maskinporten.Client.Errors
{
    public class UnexpectedResponseException : Exception
    {
        public UnexpectedResponseException(string message)
            : base(message)
        {
        }

        public UnexpectedResponseException()
            : base()
        {
        }

        public UnexpectedResponseException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}