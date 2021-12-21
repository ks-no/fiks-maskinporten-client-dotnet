using System.Security.Cryptography.X509Certificates;

namespace Ks.Fiks.Maskinporten.Client
{
    public class MaskinportenClientConfigurationFactory
    {
        public const string VER2_AUDIENCE = "https://ver2.maskinporten.no/";
        public const string VER2_TOKEN_ENDPOINT = "https://ver2.maskinporten.no/token";
        public const string PROD_AUDIENCE = "https://maskinporten.no/";
        public const string PROD_TOKEN_ENDPOINT = "https://maskinporten.no/token";
        private const int DEFAULT_NUMBER_SECONDS_LEFT = 10;

        public static MaskinportenClientConfiguration createVer2Configuration(
            string issuer,
            X509Certificate2 certificate,
            int numberOfSecondsLeftBeforeExpire = DEFAULT_NUMBER_SECONDS_LEFT,
            string consumerOrg = null)
        {
            return new MaskinportenClientConfiguration(VER2_AUDIENCE,
                VER2_TOKEN_ENDPOINT, issuer, numberOfSecondsLeftBeforeExpire, certificate,
                consumerOrg);
        }

        public static MaskinportenClientConfiguration createProdConfiguration(
            string issuer,
            X509Certificate2 certificate,
            int numberOfSecondsLeftBeforeExpire = DEFAULT_NUMBER_SECONDS_LEFT,
            string consumerOrg = null)
        {
            return new MaskinportenClientConfiguration(PROD_AUDIENCE,
                PROD_TOKEN_ENDPOINT, issuer, numberOfSecondsLeftBeforeExpire, certificate,
                consumerOrg);
        }
    }
}