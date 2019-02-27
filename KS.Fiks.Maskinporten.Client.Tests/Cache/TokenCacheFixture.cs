using System;
using Ks.Fiks.Maskinporten.Client.Cache;

namespace Ks.Fiks.Maskinporten.Client.Tests.Cache
{
    public class TokenCacheFixture
    {
        public TokenCacheFixture()
        {
        }

        public TokenCache CreateSut()
        {
            return new TokenCache();
        }

        public MaskinportenToken GetRandomToken(int expiresIn = 120)
        {
            var tokenString = @"{
            ""aud"": ""oidc_ks_test"",
            ""scope"": ""ks"",
            ""iss"": ""https://oidc-ver2.difi.no/idporten-oidc-provider/"",
            ""token_type"": ""Bearer"",
            ""exp"": 1550837855,
            ""iat"": 1550837825,
            ""client_orgno"": ""987654321"",
            ""jti"": """+Guid.NewGuid()+"\"}";
            
            return MaskinportenToken.CreateFromJsonString(tokenString, expiresIn);
        }
    }
}