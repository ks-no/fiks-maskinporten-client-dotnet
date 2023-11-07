using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace Ks.Fiks.Maskinporten.Client
{
    public class MaskinportenClientConfigurationFactory
    {
        [Obsolete("VER2 is deprecated, use TEST instead")]
        public const string VER2_AUDIENCE = "https://ver2.maskinporten.no/";
        [Obsolete("VER2 is deprecated, use TEST instead")]
        public const string VER2_TOKEN_ENDPOINT = "https://ver2.maskinporten.no/token";
        public const string TEST_AUDIENCE = "https://test.maskinporten.no/";
        public const string TEST_TOKEN_ENDPOINT = "https://test.maskinporten.no/token";

        public const string PROD_AUDIENCE = "https://maskinporten.no/";
        public const string PROD_TOKEN_ENDPOINT = "https://maskinporten.no/token";
        private const int DEFAULT_NUMBER_SECONDS_LEFT = 10;

        [Obsolete("VER2 is deprecated, use CreateTestConfiguration instead")]
        public static MaskinportenClientConfiguration CreateVer2Configuration(
            string issuer,
            X509Certificate2 certificate = null,
            RSA privateKey = null,
            RSA publicKey = null,
            string keyIdentifier = null,
            int numberOfSecondsLeftBeforeExpire = DEFAULT_NUMBER_SECONDS_LEFT,
            string consumerOrg = null)
        {
            return new MaskinportenClientConfiguration(
                VER2_AUDIENCE,
                VER2_TOKEN_ENDPOINT,
                issuer,
                numberOfSecondsLeftBeforeExpire,
                certificate,
                privateKey,
                publicKey,
                keyIdentifier,
                consumerOrg);
        }

        public static MaskinportenClientConfiguration CreateTestConfiguration(
            string issuer,
            X509Certificate2 certificate = null,
            RSA privateKey = null,
            RSA publicKey = null,
            string keyIdentifier = null,
            int numberOfSecondsLeftBeforeExpire = DEFAULT_NUMBER_SECONDS_LEFT,
            string consumerOrg = null)
        {
            return new MaskinportenClientConfiguration(
                TEST_AUDIENCE,
                TEST_TOKEN_ENDPOINT,
                issuer,
                numberOfSecondsLeftBeforeExpire,
                certificate,
                privateKey,
                publicKey,
                keyIdentifier,
                consumerOrg);
        }

        public static MaskinportenClientConfiguration CreateProdConfiguration(
            string issuer,
            X509Certificate2 certificate = null,
            RSA privateKey = null,
            RSA publicKey = null,
            string keyIdentifier = null,
            int numberOfSecondsLeftBeforeExpire = DEFAULT_NUMBER_SECONDS_LEFT,
            string consumerOrg = null)
        {
            return new MaskinportenClientConfiguration(
                PROD_AUDIENCE,
                PROD_TOKEN_ENDPOINT,
                issuer,
                numberOfSecondsLeftBeforeExpire,
                certificate,
                privateKey,
                publicKey,
                keyIdentifier,
                consumerOrg);
        }
    }
}