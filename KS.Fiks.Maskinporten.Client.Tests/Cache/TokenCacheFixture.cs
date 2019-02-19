using System;
using Ks.Fiks.Maskinporten.Client.Cache;

namespace Ks.Fiks.Maskinporten.Client.Tests.Cache
{
    public class TokenCacheFixture
    {
        private TimeSpan _expirationTime;

        public TokenCacheFixture()
        {
            _expirationTime = TimeSpan.FromSeconds(10);
        }
        
        public TokenCache<string> CreateSut()
        {
            return new TokenCache<string>(_expirationTime);
        }

        public TokenCacheFixture WithExpirationTime(TimeSpan expirationTime)
        {
            _expirationTime = expirationTime;
            return this;
        }
    }
}