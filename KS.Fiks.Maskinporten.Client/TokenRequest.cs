using System;

namespace Ks.Fiks.Maskinporten.Client.Cache
{
    public class TokenRequest
    {
        public string Scopes { get; set; }

        public string ConsumerOrg { get; set; }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            return obj.GetType() == GetType() && Equals((TokenRequest) obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Scopes, ConsumerOrg);
        }

        private bool Equals(TokenRequest other)
        {
            return Scopes == other.Scopes && ConsumerOrg == other.ConsumerOrg;
        }
    }
}