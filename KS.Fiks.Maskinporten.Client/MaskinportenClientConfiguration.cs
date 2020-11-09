using System.Security.Cryptography.X509Certificates;

namespace Ks.Fiks.Maskinporten.Client
{
    public class MaskinportenClientConfiguration
    {
        public MaskinportenClientConfiguration(
            string audience,
            string tokenEndpoint,
            string issuer,
            int numberOfSecondsLeftBeforeExpire,
            X509Certificate2 certificate,
            string consumerOrg = null)
        {
            Audience = audience;
            TokenEndpoint = tokenEndpoint;
            Issuer = issuer;
            NumberOfSecondsLeftBeforeExpire = numberOfSecondsLeftBeforeExpire;
            Certificate = certificate;
            ConsumerOrg = consumerOrg;
        }

        public string Audience { get; }

        public string TokenEndpoint { get; }

        public string Issuer { get; }
        
        public string ConsumerOrg { get; }

        public int NumberOfSecondsLeftBeforeExpire { get; }

        public X509Certificate2 Certificate { get; }
    }
}