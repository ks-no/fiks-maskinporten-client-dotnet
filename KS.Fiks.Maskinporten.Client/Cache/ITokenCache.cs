using System;
using System.Threading.Tasks;

namespace Ks.Fiks.Maskinporten.Client.Cache
{
    public interface ITokenCache<T>
    {
        Task<T> GetToken(string tokenKey, Func<Task<T>> tokenGetter);
    }
}