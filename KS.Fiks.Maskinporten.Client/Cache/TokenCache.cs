using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using KS.Fiks.Maskinporten.Client;

namespace Ks.Fiks.Maskinporten.Client.Cache
{
    public class TokenCache : ITokenCache, IDisposable
    {
        private readonly Dictionary<TokenRequest, MaskinportenToken> _cacheDictionary;
        private readonly SemaphoreSlim _mutex;

        public TokenCache()
        {
            _cacheDictionary = new Dictionary<TokenRequest, MaskinportenToken>();
            _mutex = new SemaphoreSlim(1);
        }

        public async Task<MaskinportenToken> GetToken(TokenRequest tokenRequest, Func<Task<MaskinportenToken>> tokenFactory)
        {
            await _mutex.WaitAsync().ConfigureAwait(false);
            try
            {
                return HasValidEntry(tokenRequest)
                    ? _cacheDictionary[tokenRequest]
                    : await UpdateOrAddToken(tokenRequest, tokenFactory).ConfigureAwait(false);
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

        private bool HasValidEntry(TokenRequest tokenRequest)
        {
            if (!_cacheDictionary.ContainsKey(tokenRequest))
            {
                return false;
            }

            return !_cacheDictionary[tokenRequest].IsExpiring();
        }

        private async Task<MaskinportenToken> UpdateOrAddToken(
            TokenRequest tokenRequest,
            Func<Task<MaskinportenToken>> tokenFactory)
        {
            var newToken = await tokenFactory().ConfigureAwait(false);
            if (_cacheDictionary.ContainsKey(tokenRequest))
            {
                _cacheDictionary[tokenRequest] = newToken;
            }
            else
            {
                _cacheDictionary.Add(tokenRequest, newToken);
            }

            return newToken;
        }
    }
}