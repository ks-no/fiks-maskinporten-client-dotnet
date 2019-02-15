using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ks.Fiks.Maskinporten.Client
{
    public interface IMaskinportenClient
    {
        Task<string> GetAccessToken(IEnumerable<string> scopes);
    }
}