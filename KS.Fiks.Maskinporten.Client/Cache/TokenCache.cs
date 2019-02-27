using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Ks.Fiks.Maskinporten.Client.Cache
{
    public class TokenCache : ITokenCache, IDisposable
    {
        private readonly Dictionary<string, MaskinportenToken> _cacheDictionary;
        private readonly SemaphoreSlim _mutex;


        public TokenCache()
        {
            _cacheDictionary = new Dictionary<string, MaskinportenToken>();
            _mutex = new SemaphoreSlim(1);
        }

        public async Task<MaskinportenToken> GetToken(string tokenKey, Func<Task<MaskinportenToken>> tokenFactory)
        {
            await _mutex.WaitAsync();
            try
            {
                return HasValidEntry(tokenKey)
                    ? _cacheDictionary[tokenKey]
                    : await UpdateOrAddToken(tokenKey, tokenFactory);
            }
            finally
            {
                _mutex.Release();
            }
        }

        private bool HasValidEntry(string tokenKey)
        {
            if (!_cacheDictionary.ContainsKey(tokenKey))
            {
                return false;
            }

            return !_cacheDictionary[tokenKey].IsExpiring();
        }

        private async Task<MaskinportenToken> UpdateOrAddToken(string tokenKey,
            Func<Task<MaskinportenToken>> tokenFactory)
        {
            var newToken = await tokenFactory();
            if (_cacheDictionary.ContainsKey(tokenKey))
            {
                _cacheDictionary[tokenKey] = newToken;
            }
            else
            {
                _cacheDictionary.Add(tokenKey, newToken);
            }

            return newToken;
        }

        public void Dispose()
        {
            _mutex?.Dispose();
        }
    }
}