namespace KS.Fiks.Maskinporten.Client
{
    public class TokenRequest
    {
        public string Scopes { get; set; }

        public string ConsumerOrg { get; set; }

        public string OnBehalfOf { get; set; }

        public string Audience { get; set; }

        public string Pid { get; set; }

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

            return obj.GetType() == GetType() && Equals((TokenRequest)obj);
        }

        public override int GetHashCode()
        {
            return (Scopes, ConsumerOrg, OnBehalfOf, Audience, Pid).GetHashCode();
        }

        private bool Equals(TokenRequest other)
        {
            return (Scopes, ConsumerOrg, OnBehalfOf, Audience, Pid) ==
                   (other.Scopes, other.ConsumerOrg, other.OnBehalfOf, other.Audience, other.Pid);
        }
    }
}