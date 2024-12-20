using System.Collections.Generic;
using System.Threading.Tasks;
using KS.Fiks.Maskinporten.Client;

namespace Ks.Fiks.Maskinporten.Client
{
    public interface IMaskinportenClient
    {
        Task<MaskinportenToken> GetAccessToken(TokenRequest tokenRequest);

        Task<MaskinportenToken> GetAccessToken(IEnumerable<string> scopes);

        Task<MaskinportenToken> GetAccessToken(string scopes);

        Task<MaskinportenToken> GetDelegatedAccessToken(string consumerOrg, IEnumerable<string> scopes);

        Task<MaskinportenToken> GetDelegatedAccessToken(string consumerOrg, string scopes);

        Task<MaskinportenToken> GetDelegatedAccessTokenForAudience(string consumerOrg, string audience, IEnumerable<string> scopes);

        Task<MaskinportenToken> GetDelegatedAccessTokenForAudience(string consumerOrg, string audience, string scopes);

        Task<MaskinportenToken> GetOnBehalfOfAccessToken(string consumerOrg, IEnumerable<string> scopes);

        Task<MaskinportenToken> GetOnBehalfOfAccessToken(string consumerOrg, string scopes);
    }
}