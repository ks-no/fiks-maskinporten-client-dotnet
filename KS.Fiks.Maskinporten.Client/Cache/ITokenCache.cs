using System;
using System.Threading.Tasks;

namespace Ks.Fiks.Maskinporten.Client.Cache
{
    public interface ITokenCache
    {
        Task<MaskinportenToken> GetToken(TokenRequest tokenRequest, Func<Task<MaskinportenToken>> tokenGetter);
    }
}