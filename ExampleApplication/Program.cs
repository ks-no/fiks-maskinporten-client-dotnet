using System;
using System.Security.Cryptography.X509Certificates;
using JWT;
using JWT.Serializers;
using Ks.Fiks.Maskinporten.Client;

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
        public static void Main(string[] args)
        {
            // Relative or absolute path to the *.p12-file containing the test certificate
            var p12Filename = Environment.GetEnvironmentVariable("P12FILENAME");
            
            // Password required to use the certificate
            var p12Password = Environment.GetEnvironmentVariable("P12PWD");
            
            // The issuer as defined in Maskinporten
            var issuer = Environment.GetEnvironmentVariable("MASKINPORTEN_ISSUER");

            var configuration = new MaskinportenClientConfiguration(
                audience: @"https://oidc-ver2.difi.no/idporten-oidc-provider/", // ID-porten audience path
                tokenEndpoint: @"https://oidc-ver2.difi.no/idporten-oidc-provider/token", // ID-porten token path
                issuer: issuer, // Issuer name
                numberOfSecondsLeftBeforeExpire: 10, // The token will be refreshed 10 seconds before it expires
                certificate: new X509Certificate2(p12Filename, p12Password));
            var maskinportenClient = new MaskinportenClient(configuration);
            
            var tokenTask = maskinportenClient.GetAccessToken("ks:fiks");
            tokenTask.Wait(TimeSpan.FromMinutes(2.0));
            
            var token = tokenTask.Result;

            Console.Out.WriteLine($"Token (expiring: {token.IsExpiring()}): {token.Token}");
            
            var decodedToken = DecodeToken(token);
            Console.Out.WriteLine($"Decoded token {decodedToken}");

        }

        private static string DecodeToken(MaskinportenToken token)
        {
            var serializer = new JsonNetSerializer();
            var provider = new UtcDateTimeProvider();

            var jwtDecoder = new JwtDecoder(serializer, new JwtValidator(serializer, provider), new JwtBase64UrlEncoder());
            var decodedToken = jwtDecoder.Decode(token.Token);
            return decodedToken;
        }
    }
}