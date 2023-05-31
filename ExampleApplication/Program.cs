using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using JWT;
using JWT.Algorithms;
using JWT.Serializers;
using Ks.Fiks.Maskinporten.Client;
using Microsoft.IdentityModel.Tokens;

namespace ExampleApplication
{
    /**
     * Shows how to integrate with Difi Maskinporten using KS MaskinportenClient.
     * Prerequisites:
     *  - install nuget package KS.Fiks.Maskinporten.Client, v.1.0.0
     *  - a P12 file containing av valid testcertificate signed by Digicert or Comfides
     */
    public class Program
    {
        public static async Task Main(string[] args)
        {
            await CertExample();
            await RsaExample();
        }

        private static async Task RsaExample()
        {
            // The issuer as defined in Maskinporten
            var issuer = Environment.GetEnvironmentVariable("MASKINPORTEN_ISSUER");
            
            // Content of PEM file containing public/private key with ----BEGIN.. and ----END removed, e.g., "MIIE...A=="
            var pem = Environment.GetEnvironmentVariable("pem")!;
            var rsa = RSA.Create();
            
            //Will import both public and private part in same RSA, could be done in two different variables
            rsa.ImportPkcs8PrivateKey(Convert.FromBase64String(pem), out _);

            const string keyIdentifier = "23b3f45a-be84-43c4-a654-2182ff14dc40";
            var configuration = MaskinportenClientConfigurationFactory.CreateVer2Configuration(
                issuer,
                privateKey: rsa,
                publicKey: rsa,
                keyIdentifier: keyIdentifier
            );

            var maskinportenClient = new MaskinportenClient(configuration);
            var token = await maskinportenClient.GetAccessToken("ks:fiks");
            
            using var client = new HttpClient();
            var json = await client.GetStringAsync("https://ver2.maskinporten.no/jwk");
            var jwks = new JsonWebKeySet(json);
            var jwk = jwks.Keys.First();

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = false,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = "https://ver2.maskinporten.no/",
                IssuerSigningKey = jwk
            };

            var handler = new JwtSecurityTokenHandler();
            var claimsPrincipal = handler.ValidateToken(token.Token, validationParameters, out var validatedToken);

            var claims = claimsPrincipal.Claims;
            foreach (var claim in claims) Console.WriteLine($"{claim.Type}: {claim.Value}");

            Console.WriteLine($"Token (expiring: {token.IsExpiring()}): {token.Token}");
            Console.WriteLine($"Token valid until: {validatedToken.ValidTo}");
        }

        private static async Task CertExample()
        {
            // Relative or absolute path to the *.p12-file containing the test certificate
            var p12Filename = Environment.GetEnvironmentVariable("P12FILENAME");

            // Password required to use the certificate
            var p12Password = Environment.GetEnvironmentVariable("P12PWD");

            // The issuer as defined in Maskinporten
            var issuer = Environment.GetEnvironmentVariable("MASKINPORTEN_ISSUER");

            var cert = new X509Certificate2(p12Filename, p12Password);
            var configuration = new MaskinportenClientConfiguration(
                @"https://ver2.maskinporten.no/", // ID-porten audience path
                @"https://ver2.maskinporten.no/token", // ID-porten token path
                issuer, // Issuer name
                10, // The token will be refreshed 10 seconds before it expires
                cert);
            var maskinportenClient = new MaskinportenClient(configuration);

            var token = await maskinportenClient.GetAccessToken("ks:fiks");
            Console.WriteLine($"Token (expiring: {token.IsExpiring()}): {token.Token}");

            var serializer = new JsonNetSerializer();
            var provider = new UtcDateTimeProvider();

            var jwtDecoder = new JwtDecoder(serializer, new JwtValidator(serializer, provider),
                new JwtBase64UrlEncoder(),
                new RS256Algorithm(cert));
            var decodedToken = jwtDecoder.Decode(token.Token);
            Console.WriteLine($"Decoded token {decodedToken}");
        }
    }
}