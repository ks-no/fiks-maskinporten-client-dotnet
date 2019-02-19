using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ks.Fiks.Maskinporten.Client.Cache
{
    public class TokenCache<T> : ITokenCache<T>
    {
        private readonly TimeSpan _expirationTime;
        private readonly ConcurrentDictionary<string, TimedCacheEntry<T>> _cacheDictionary;

        public TokenCache(TimeSpan expirationTime)
        {
            _expirationTime = expirationTime;
            _cacheDictionary = new ConcurrentDictionary<string, TimedCacheEntry<T>>();
        }

        public async Task<T> GetToken(string tokenKey, Func<Task<T>> tokenFactory)
        {
            if (HasValidEntry(tokenKey))
            {
                return _cacheDictionary[tokenKey].Value;
            }
            
            return await UpdateOrAddToken(tokenKey, tokenFactory);

        }

        private bool HasValidEntry(string tokenKey)
        {
            if (!_cacheDictionary.ContainsKey(tokenKey))
            {
                return false;
            }

            return !_cacheDictionary[tokenKey].IsExpired(_expirationTime);
        }

        private async Task<T> UpdateOrAddToken(string tokenKey, Func<Task<T>> tokenFactory)
        {
            var newToken = await tokenFactory();
            var newEntry = new TimedCacheEntry<T>(newToken);
            _cacheDictionary.AddOrUpdate(tokenKey, newEntry,
                (key, currentEntry) => HandleCacheEntryUpdate(key, currentEntry, newEntry));
            return newToken;
        }

        private TimedCacheEntry<T> HandleCacheEntryUpdate(
            string tokenKey, 
            TimedCacheEntry<T> currentEntry,
            TimedCacheEntry<T> newEntry)
        {
            if (!currentEntry.IsExpired(_expirationTime))
            {
                // Log this, as it means that we are in a race condition
            }
            return newEntry;
        } 
       
    }
}