using System;
using System.Globalization;
using JWT.Builder;
using Newtonsoft.Json.Linq;

namespace Ks.Fiks.Maskinporten.Client
{
    public class MaskinportenToken
    {
        private readonly DateTime _requestNewTokenAfterTime;

        public int ExpiresIn { get; }

        public MaskinportenToken(string token, int expiresIn)
        {
            _requestNewTokenAfterTime = DateTime.UtcNow.AddSeconds(expiresIn);
            Token = token;
            ExpiresIn = expiresIn;
        }

        public string Token { get; }

        public override bool Equals(object obj)
        {
            return obj.GetType() == GetType() && Equals((MaskinportenToken)obj);
        }

        public bool IsExpiring()
        {
            return _requestNewTokenAfterTime < DateTime.UtcNow;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return Token.GetHashCode();
            }
        }

        protected bool Equals(MaskinportenToken other)
        {
            return string.Equals(Token, other.Token, StringComparison.Ordinal);
        }
    }
}