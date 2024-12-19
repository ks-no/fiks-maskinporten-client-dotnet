using System;
using System.Threading.Tasks;
using KS.Fiks.Maskinporten.Client;

namespace Ks.Fiks.Maskinporten.Client.Cache
{
    public interface ITokenCache
    {
        Task<MaskinportenToken> GetToken(TokenRequest tokenRequest, Func<Task<MaskinportenToken>> tokenGetter);
    }
}