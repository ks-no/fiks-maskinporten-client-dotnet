using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using JWT;
using JWT.Algorithms;
using JWT.Builder;
using JWT.Serializers;

namespace Ks.Fiks.Maskinporten.Client.Jwt
{
    public class JwtRequestTokenGenerator : IJwtRequestTokenGenerator
    {
        private const int JwtExpireTimeInMinutes = 2;
        private const string DummyKey = ""; // Required by encoder, but not used with RS256Algorithm

        private readonly JwtEncoder encoder;
        private readonly X509Certificate2 certificate;

        private static IDictionary<string, object> CreateJwtPayload(string scope, MaskinportenClientConfiguration configuration)
        {
            var jwtData = new JwtData();

            jwtData.Payload.Add("iss", configuration.Issuer);
            jwtData.Payload.Add("aud", configuration.Audience);
            jwtData.Payload.Add("iat", UnixEpoch.GetSecondsSince(DateTime.UtcNow));
            jwtData.Payload.Add("exp", UnixEpoch.GetSecondsSince(DateTime.UtcNow.AddMinutes(JwtExpireTimeInMinutes)));
            jwtData.Payload.Add("scope", scope);
            jwtData.Payload.Add("jti", Guid.NewGuid());

            return jwtData.Payload;
        }

        public JwtRequestTokenGenerator(X509Certificate2 certificate)
        {
            this.certificate = certificate;
            this.encoder = new JwtEncoder(
                new RS256Algorithm(this.certificate.GetRSAPublicKey(), this.certificate.GetRSAPrivateKey()),
                new JsonNetSerializer(),
                new JwtBase64UrlEncoder());
        }

        public string CreateEncodedJwt(string scope, MaskinportenClientConfiguration configuration)
        {
            var payload = CreateJwtPayload(scope, configuration);
            var header = CreateJwtHeader();
            var jwt = this.encoder.Encode(header, payload, DummyKey);

            return jwt;
        }

        private IDictionary<string, object> CreateJwtHeader()
        {
            return new Dictionary<string, object>
            {
                {
                    "x5c",
                    new List<string>() { Convert.ToBase64String(this.certificate.Export(X509ContentType.Cert)) }
                }
            };
        }
    }
}