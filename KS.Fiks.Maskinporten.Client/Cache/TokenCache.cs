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
            await _mutex.WaitAsync().ConfigureAwait(false);
            try
            {
                return HasValidEntry(tokenKey)
                    ? _cacheDictionary[tokenKey]
                    : await UpdateOrAddToken(tokenKey, tokenFactory).ConfigureAwait(false);
            }
            finally
            {
                _mutex.Release();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _mutex?.Dispose();
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

        private async Task<MaskinportenToken> UpdateOrAddToken(
            string tokenKey,
            Func<Task<MaskinportenToken>> tokenFactory)
        {
            var newToken = await tokenFactory().ConfigureAwait(false);
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
    }
}