using KS.Fiks.Maskinporten.Client;

namespace Ks.Fiks.Maskinporten.Client.Jwt
{
    public interface IJwtRequestTokenGenerator
    {
        string CreateEncodedJwt(TokenRequest tokenRequest, MaskinportenClientConfiguration configuration);
    }
}