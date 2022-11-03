using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JWT;
using JWT.Builder;

namespace Ks.Fiks.Maskinporten.Client.Jwt
{
    public static class JwtRequestTokenGeneratorFactory
    {
        private const int JwtExpireTimeInMinutes = 2;

        public static IJwtRequestTokenGenerator GetJwtRequestTokenGenerator(MaskinportenClientConfiguration configuration)
        {
            IJwtRequestTokenGenerator generator = null;

            if (configuration.RequestType == JwtRequestTokenType.JsonWebKey)
            {
                generator = new JwtRequestTokenGeneratorJwk(configuration.Jwk);
            }

            if (configuration.RequestType == JwtRequestTokenType.X509Certificate)
            {
                generator = new JwtRequestTokenGenerator(configuration.Certificate);
            }

            return generator;
        }

        public static IDictionary<string, object> CreateJwtPayload(string scope, MaskinportenClientConfiguration configuration)
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
    }
}
