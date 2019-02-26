using System;
using Ks.Fiks.Maskinporten.Client.Cache;

namespace Ks.Fiks.Maskinporten.Client.Tests.Cache
{
    public class TokenCacheFixture
    {
        public TokenCacheFixture()
        {
        }

        public TokenCache<string> CreateSut()
        {
            return new TokenCache<string>();
        }
    }
}