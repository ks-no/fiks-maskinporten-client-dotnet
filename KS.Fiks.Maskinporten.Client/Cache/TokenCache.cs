using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Ks.Fiks.Maskinporten.Client.Cache
{
    public class TokenCache<T> : ITokenCache<T>, IDisposable
    {
        private const int DefaultFactoryTimeoutInSeconds = 7;
        
        private readonly TimeSpan _defaultExpirationTime;
        private readonly Dictionary<string, TimedCacheEntry<T>> _cacheDictionary;
        private readonly SemaphoreSlim _mutex;
        

        public TokenCache(TimeSpan defaultExpirationTime)
        {
            _defaultExpirationTime = defaultExpirationTime;
            _cacheDictionary = new Dictionary<string, TimedCacheEntry<T>>();
            _mutex = new SemaphoreSlim(1 );

        }

        public async Task<T> GetToken(string tokenKey, Func<Task<T>> tokenFactory, Func<T, TimeSpan> entryExpirationTime = null)
        {
            entryExpirationTime = entryExpirationTime ?? ((x) => _defaultExpirationTime);
            await _mutex.WaitAsync();
            try
            {
                return HasValidEntry(tokenKey)
                    ? _cacheDictionary[tokenKey].Value
                    : await UpdateOrAddToken(tokenKey, tokenFactory, entryExpirationTime);
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

            return !_cacheDictionary[tokenKey].IsExpired();
        }

        private async Task<T> UpdateOrAddToken(string tokenKey, Func<Task<T>> tokenFactory, Func<T, TimeSpan> entryExpirationTime)
        {
            var newToken = await tokenFactory();
            var newEntry = new TimedCacheEntry<T>(newToken, entryExpirationTime(newToken));
            if (_cacheDictionary.ContainsKey(tokenKey))
            {
                _cacheDictionary[tokenKey] = newEntry;
            }
            else
            {
                _cacheDictionary.Add(tokenKey, newEntry);
            }

            return newToken;
        }


        public void Dispose()
        {
            _mutex?.Dispose();
        }
    }
}