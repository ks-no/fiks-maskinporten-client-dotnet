using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using JWT;
using JWT.Algorithms;
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

            var cert = new X509Certificate2(p12Filename, p12Password);
            var configuration = new MaskinportenClientConfiguration(
                audience: @"https://ver2.maskinporten.no/", // ID-porten audience path
                tokenEndpoint: @"https://ver2.maskinporten.no/token", // ID-porten token path
                issuer: issuer, // Issuer name
                numberOfSecondsLeftBeforeExpire: 10, // The token will be refreshed 10 seconds before it expires
                certificate: cert);
            var maskinportenClient = new MaskinportenClient(configuration);

            var tokenTask = maskinportenClient.GetAccessToken("ks:fiks").ContinueWith(t =>
            {
                var token = t.Result;
                Console.Out.WriteLine($"Token (expiring: {token.IsExpiring()}): {token.Token}");

                return DecodeToken(token, cert);
            });
            // Do something with the token. In this case we only wait for it to be decoded and written to the console
            tokenTask.GetAwaiter().GetResult();


        }

        private static string DecodeToken(MaskinportenToken token, X509Certificate2 pubprivCertificate)
        {
            var serializer = new JsonNetSerializer();
            var provider = new UtcDateTimeProvider();

            var jwtDecoder = new JwtDecoder(serializer, new JwtValidator(serializer, provider), new JwtBase64UrlEncoder(), new RS256Algorithm(pubprivCertificate));
            var decodedToken = jwtDecoder.Decode(token.Token);
            Console.Out.WriteLine($"Decoded token {decodedToken}");
            return decodedToken;
        }
    }
}