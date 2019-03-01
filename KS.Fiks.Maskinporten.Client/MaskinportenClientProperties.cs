namespace Ks.Fiks.Maskinporten.Client
{
    public class MaskinportenClientProperties
    {
        public string Audience { get; set; }

        public string TokenEndpoint { get; set; }

        public string Issuer { get; set; }

        public int NumberOfSecondsLeftBeforeExpire { get; set; }

        public MaskinportenClientProperties(
            string audience,
            string tokenEndpoint,
            string issuer,
            int numberOfSecondsLeftBeforeExpire)
        {
            Audience = audience;
            TokenEndpoint = tokenEndpoint;
            Issuer = issuer;
            NumberOfSecondsLeftBeforeExpire = numberOfSecondsLeftBeforeExpire;
        }
    }
}