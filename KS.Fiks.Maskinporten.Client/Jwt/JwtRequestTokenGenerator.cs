using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using JWT;
using JWT.Algorithms;
using JWT.Builder;
using JWT.Serializers;
using KS.Fiks.Maskinporten.Client;

namespace Ks.Fiks.Maskinporten.Client.Jwt
{
    public class JwtRequestTokenGenerator : IJwtRequestTokenGenerator
    {
        private const int JwtExpireTimeInMinutes = 2;
        private const string DummyKey = ""; // Required by encoder, but not used with RS256Algorithm

        private readonly JwtEncoder _encoder;
        private readonly X509Certificate2 _certificate;
        private readonly string _keyIdentifier;

        private static IDictionary<string, object> CreateJwtPayload(
            TokenRequest tokenRequest,
            MaskinportenClientConfiguration configuration)
        {
            var jwtData = new JwtData();

            jwtData.Payload.Add("iss", configuration.Issuer);

            if (!string.IsNullOrWhiteSpace(tokenRequest.OnBehalfOf))
            {
                jwtData.Payload.Add("iss_onbehalfof", tokenRequest.OnBehalfOf);
            }

            if (!string.IsNullOrWhiteSpace(tokenRequest.Audience))
            {
                jwtData.Payload.Add("resource", tokenRequest.Audience);
            }

            if (!string.IsNullOrWhiteSpace(tokenRequest.PersonIdentifier))
            {
                jwtData.Payload.Add("pid", tokenRequest.PersonIdentifier);
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

        public JwtRequestTokenGenerator(X509Certificate2 certificate, string keyIdentifier = null)
        {
            _certificate = certificate;
            var privateKey = certificate.GetRSAPrivateKey();
            var publicKey = certificate.GetRSAPublicKey();
            _keyIdentifier = keyIdentifier;
            _encoder = new JwtEncoder(
                new RS256Algorithm(publicKey, privateKey),
                new JsonNetSerializer(),
                new JwtBase64UrlEncoder());
        }

        public JwtRequestTokenGenerator(RSA publicKey, RSA privateKey, string keyIdentifier = null)
        {
            _keyIdentifier = keyIdentifier;
            _encoder = new JwtEncoder(
                new RS256Algorithm(publicKey, privateKey),
                new JsonNetSerializer(),
                new JwtBase64UrlEncoder());
        }

        public string CreateEncodedJwt(TokenRequest tokenRequest, MaskinportenClientConfiguration configuration)
        {
            var payload = CreateJwtPayload(tokenRequest, configuration);
            var header = CreateJwtHeader();
            var jwt = _encoder.Encode(header, payload, DummyKey);

            return jwt;
        }

        private IDictionary<string, object> CreateJwtHeader()
        {
            Dictionary<string, object> jwtHeaderValues = new Dictionary<string, object>();

            if (_certificate != null)
            {
                jwtHeaderValues.Add("x5c",
                    new List<string>() { Convert.ToBase64String(_certificate.Export(X509ContentType.Cert)) });
            }

            if (!string.IsNullOrWhiteSpace(_keyIdentifier))
            {
                jwtHeaderValues.Add("kid", _keyIdentifier);
            }

            return jwtHeaderValues;
        }
    }
}