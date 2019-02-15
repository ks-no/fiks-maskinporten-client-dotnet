namespace Ks.Fiks.Maskinporten.Client
{
    public class MaskinportenClientProperties
    {
        public string Audience { get; set; }
        public string TokenEndpoint{ get; set; }
        public string Issuer{ get; set; }
        public int NumberOfSecondsLeftBeforeExpire{ get; set; }

        public MaskinportenClientProperties(string audience = "standardAudience", string tokenEndpoint = "http://temp.no", string issuer = "standardValue",
            int numberOfSecondsLeftBeforeExpire = 100)
        {
            Audience = audience;
            TokenEndpoint = tokenEndpoint;
            Issuer = issuer;
            NumberOfSecondsLeftBeforeExpire = numberOfSecondsLeftBeforeExpire;
        }
    }
}