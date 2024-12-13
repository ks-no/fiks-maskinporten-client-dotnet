using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using JWT;
using JWT.Algorithms;
using JWT.Builder;
using JWT.Serializers;
using Ks.Fiks.Maskinporten.Client.Cache;

namespace Ks.Fiks.Maskinporten.Client.Jwt
{
    public class JwtRequestTokenGenerator : IJwtRequestTokenGenerator
    {
        private const int JwtExpireTimeInMinutes = 2;
        private const string DummyKey = ""; // Required by encoder, but not used with RS256Algorithm

        private readonly JwtEncoder encoder;
        private readonly X509Certificate2 certificate;
        private readonly string keyIdentifier;
        private readonly RSA privateKey;
        private readonly RSA publicKey;

        private static IDictionary<string, object> CreateJwtPayload(TokenRequest tokenRequest, MaskinportenClientConfiguration configuration)
        {
            var jwtData = new JwtData();

            jwtData.Payload.Add("iss", configuration.Issuer);

            if (!string.IsNullOrEmpty(tokenRequest.OnBehalfOf))
            {
                jwtData.Payload.Add("iss_onbehalfof", tokenRequest.OnBehalfOf);
            }

            if (!string.IsNullOrEmpty(tokenRequest.Audience))
            {
                jwtData.Payload.Add("resource", tokenRequest.Audience);
            }

            jwtData.Payload.Add("aud", configuration.Audience);
            jwtData.Payload.Add("iat", UnixEpoch.GetSecondsSince(DateTime.UtcNow));
            jwtData.Payload.Add("exp", UnixEpoch.GetSecondsSince(DateTime.UtcNow.AddMinutes(JwtExpireTimeInMinutes)));
            jwtData.Payload.Add("scope", tokenRequest.Scopes);
            jwtData.Payload.Add("jti", Guid.NewGuid());

            if (!string.IsNullOrEmpty(tokenRequest.ConsumerOrg))
            {
                jwtData.Payload.Add("consumer_org", tokenRequest.ConsumerOrg);
            }

            return jwtData.Payload;
        }

        public JwtRequestTokenGenerator(X509Certificate2 certificate)
        {
            this.certificate = certificate;
            this.privateKey = certificate.GetRSAPrivateKey();
            this.publicKey = certificate.GetRSAPublicKey();
            this.encoder = new JwtEncoder(
                new RS256Algorithm(publicKey, privateKey),
                new JsonNetSerializer(),
                new JwtBase64UrlEncoder());
        }

        public JwtRequestTokenGenerator(RSA publicKey, RSA privateKey, string keyIdentifier = null)
        {
            this.keyIdentifier = keyIdentifier;
            this.privateKey = privateKey;
            this.publicKey = publicKey;
            this.encoder = new JwtEncoder(
                new RS256Algorithm(publicKey, privateKey),
                new JsonNetSerializer(),
                new JwtBase64UrlEncoder());
        }

        public string CreateEncodedJwt(TokenRequest tokenRequest, MaskinportenClientConfiguration configuration)
        {
            var payload = CreateJwtPayload(tokenRequest, configuration);
            var header = CreateJwtHeader();
            var jwt = this.encoder.Encode(header, payload, DummyKey);

            return jwt;
        }

        private IDictionary<string, object> CreateJwtHeader()
        {
            Dictionary<string, object> jwtHeaderValues = new Dictionary<string, object>();

            if (certificate != null)
            {
                jwtHeaderValues.Add("x5c", new List<string>() { Convert.ToBase64String(this.certificate.Export(X509ContentType.Cert)) });
            }

            if (!string.IsNullOrWhiteSpace(keyIdentifier))
            {
                jwtHeaderValues.Add("kid", keyIdentifier);
            }

            return jwtHeaderValues;
        }
    }
}