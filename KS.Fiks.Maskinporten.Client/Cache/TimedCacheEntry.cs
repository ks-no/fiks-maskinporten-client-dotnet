using System;

namespace Ks.Fiks.Maskinporten.Client.Cache
{
    public class TimedCacheEntry<T>
    {
        private readonly DateTime _expirationTime;

        public T Value { get; }

        
        public TimedCacheEntry(T value, TimeSpan expirationDuration)
        {
            Value = value;
            _expirationTime = DateTime.Now + expirationDuration;
        }

        public bool IsExpired()
        {
            return DateTime.Now > _expirationTime;
        }
    }
}