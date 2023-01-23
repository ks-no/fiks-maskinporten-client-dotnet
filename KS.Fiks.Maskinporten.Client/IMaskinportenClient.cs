using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ks.Fiks.Maskinporten.Client
{
    public interface IMaskinportenClient
    {
        Task<MaskinportenToken> GetAccessToken(IEnumerable<string> scopes);

        Task<MaskinportenToken> GetAccessToken(string scopes);

        Task<MaskinportenToken> GetDelegatedAccessToken(string consumerOrg, IEnumerable<string> scopes);

        Task<MaskinportenToken> GetDelegatedAccessToken(string consumerOrg, string scopes);

        Task<MaskinportenToken> GetOnBehalfOfAccessToken(string consumerOrg, IEnumerable<string> scopes);

        Task<MaskinportenToken> GetOnBehalfOfAccessToken(string consumerOrg, string scopes);
    }
}