using JWT;
using JWT.Serializers;

namespace Ks.Fiks.Maskinporten.Client.Jwt
{
    public class JwtResponseDecoder : IJwtResponseDecoder
    {
        private readonly JwtDecoder _decoder;

        public JwtResponseDecoder()
        {
            var serializer = new JsonNetSerializer();
            var validator = new JwtValidator(serializer, new UtcDateTimeProvider());
            var encoder = new JwtBase64UrlEncoder();

            _decoder = new JwtDecoder(serializer, validator, encoder);
        }

        public string JwtAsString(string jwt)
        {
            return _decoder.Decode(jwt);
        }
    }
}