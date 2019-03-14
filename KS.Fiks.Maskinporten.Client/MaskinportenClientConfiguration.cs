using System.Security.Cryptography.X509Certificates;

namespace Ks.Fiks.Maskinporten.Client
{
    public class MaskinportenClientConfiguration
    {
        public string Audience { get; set; }

        public string TokenEndpoint { get; set; }

        public string Issuer { get; set; }

        public int NumberOfSecondsLeftBeforeExpire { get; set; }

        public X509Certificate2 Certificate { get; set; }
    }
}