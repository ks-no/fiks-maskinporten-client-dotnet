using System;

namespace Ks.Fiks.Maskinporten.Client
{
    public class UnexpectedResponseException : Exception
    {
        public UnexpectedResponseException(string message) : base(message)
        {
            
        }
    }
}