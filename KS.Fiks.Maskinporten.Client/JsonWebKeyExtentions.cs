using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;

namespace Ks.Fiks.Maskinporten.Client
{
    public static class JsonWebKeyExtentions
    {
        public static RSA GetRSAPublicKey(this JsonWebKey jwk)
        {
            var rsaPublicParameters = new RSAParameters
            {
                Exponent = Base64UrlEncoder.DecodeBytes(jwk.E),
                Modulus = Base64UrlEncoder.DecodeBytes(jwk.N)
            };

            var rsaPublic = RSA.Create();
            rsaPublic.ImportParameters(rsaPublicParameters);

            return rsaPublic;
        }

        public static RSA GetRSAPrivateKey(this JsonWebKey jwk)
        {
            var rsaPrivateParameters = new RSAParameters
            {
                Exponent = Base64UrlEncoder.DecodeBytes(jwk.E),
                Modulus = Base64UrlEncoder.DecodeBytes(jwk.N),
                D = Base64UrlEncoder.DecodeBytes(jwk.D),
                DP = Base64UrlEncoder.DecodeBytes(jwk.DP),
                DQ = Base64UrlEncoder.DecodeBytes(jwk.DQ),
                P = Base64UrlEncoder.DecodeBytes(jwk.P),
                Q = Base64UrlEncoder.DecodeBytes(jwk.Q),
                InverseQ = Base64UrlEncoder.DecodeBytes(jwk.QI)
            };

            var rsaPrivate = RSA.Create();
            rsaPrivate.ImportParameters(rsaPrivateParameters);

            return rsaPrivate;
        }
    }
}
