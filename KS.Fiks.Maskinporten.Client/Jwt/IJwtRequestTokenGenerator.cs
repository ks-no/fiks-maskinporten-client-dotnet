using Ks.Fiks.Maskinporten.Client.Cache;

namespace Ks.Fiks.Maskinporten.Client.Jwt
{
    public interface IJwtRequestTokenGenerator
    {
        string CreateEncodedJwt(string scope, MaskinportenClientConfiguration configuration, TokenRequest tokenRequest);
    }
}