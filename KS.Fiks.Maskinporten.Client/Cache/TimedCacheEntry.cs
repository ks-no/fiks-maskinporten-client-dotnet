using System;

namespace Ks.Fiks.Maskinporten.Client.Cache
{
    public class TimedCacheEntry<T>
    {
        private DateTime _createdTime;

        public T Value { get; }

        
        public TimedCacheEntry(T value)
        {
            Value = value;
            _createdTime = DateTime.Now;
        }

        public bool IsExpired(TimeSpan expirationTime)
        {
            return DateTime.Now - _createdTime > expirationTime;
        }
    }
}