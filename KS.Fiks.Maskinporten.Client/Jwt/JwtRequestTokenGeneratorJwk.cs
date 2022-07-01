using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using JWT;
using JWT.Algorithms;
using JWT.Builder;
using JWT.Serializers;
using Microsoft.IdentityModel.Tokens;

namespace Ks.Fiks.Maskinporten.Client.Jwt
{
    public class JwtRequestTokenGeneratorJwk : IJwtRequestTokenGenerator
    {
        private const string DummyKey = ""; // Required by encoder, but not used with RS256Algorithm

        private readonly JwtEncoder encoder;
        private readonly string kid = string.Empty;

        public JwtRequestTokenGeneratorJwk(JsonWebKey jwk)
        {
            this.kid = jwk.Kid;

            var publicKey = jwk.GetRSAPublicKey();
            var privateKey = jwk.GetRSAPrivateKey();

            this.encoder = new JwtEncoder(
                new RS256Algorithm(publicKey, privateKey),
                new JsonNetSerializer(),
                new JwtBase64UrlEncoder());
        }

        public string CreateEncodedJwt(string scope, MaskinportenClientConfiguration configuration)
        {
            var payload = JwtRequestTokenGeneratorFactory.CreateJwtPayload(scope, configuration);
            var header = CreateJwtHeader();
            var jwt = this.encoder.Encode(header, payload, DummyKey);

            return jwt;
        }

        private IDictionary<string, object> CreateJwtHeader()
        {
            return new Dictionary<string, object>() { { "kid", this.kid } };
        }

    }
}