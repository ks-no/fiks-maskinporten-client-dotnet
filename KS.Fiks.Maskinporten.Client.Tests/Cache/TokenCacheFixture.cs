using System;
using Castle.Core.Internal;
using Ks.Fiks.Maskinporten.Client.Cache;

namespace Ks.Fiks.Maskinporten.Client.Tests.Cache
{
    public class TokenCacheFixture
    {
        private string _tokenString;
        private bool _dummy;

        public TokenCacheFixture()
        {
            _tokenString = @"{
            ""aud"": ""oidc_ks_test"",
            ""scope"": ""ks"",
            ""iss"": ""https://oidc-ver2.difi.no/idporten-oidc-provider/"",
            ""token_type"": ""Bearer"",
            ""exp"": 1550837855,
            ""iat"": 1550837825,
            ""client_orgno"": ""987654321"",
            ""jti"": """ + Guid.NewGuid() + "\"}";
        }

        public TokenCache CreateSut()
        {
            _dummy = _dummy ? true : false; // Disable static rule for fixture;
            return new TokenCache();
        }

        public MaskinportenToken GetRandomToken(int expiresIn = 120)
        {
            return MaskinportenToken.CreateFromJsonString(_tokenString, expiresIn);
        }
    }
}