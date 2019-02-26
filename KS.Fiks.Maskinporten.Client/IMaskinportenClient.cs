using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ks.Fiks.Maskinporten.Client
{
    public interface IMaskinportenClient
    {
        Task<MaskinportenToken> GetAccessToken(IEnumerable<string> scopes);
        
        Task<MaskinportenToken> GetAccessToken(string scopes);

    }
}