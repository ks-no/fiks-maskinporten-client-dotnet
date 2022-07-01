using System.Security.Cryptography.X509Certificates;
using Ks.Fiks.Maskinporten.Client.Jwt;
using Microsoft.IdentityModel.Tokens;

namespace Ks.Fiks.Maskinporten.Client
{
    public class MaskinportenClientConfiguration
    {
        public MaskinportenClientConfiguration(
            string audience,
            string tokenEndpoint,
            string issuer,
            int numberOfSecondsLeftBeforeExpire,
            X509Certificate2 certificate = null,
            string consumerOrg = null,
            JwtRequestTokenType requestType = JwtRequestTokenType.X509Certificate,
            JsonWebKey jwk = null)
        {
            Audience = audience;
            TokenEndpoint = tokenEndpoint;
            Issuer = issuer;
            NumberOfSecondsLeftBeforeExpire = numberOfSecondsLeftBeforeExpire;
            Certificate = certificate;
            ConsumerOrg = consumerOrg;
            RequestType = requestType;
            Jwk = jwk;
        }

        public string Audience { get; }

        public string TokenEndpoint { get; }

        public string Issuer { get; }

        public string ConsumerOrg { get; }

        public int NumberOfSecondsLeftBeforeExpire { get; }

        public X509Certificate2 Certificate { get; }

        public JwtRequestTokenType RequestType { get; }

        public JsonWebKey Jwk{ get; }
    }
}