namespace Ks.Fiks.Maskinporten.Client.Jwt
{
    public interface IJwtResponseDecoder
    {
        string JwtAsString(string jwt);
    }
}