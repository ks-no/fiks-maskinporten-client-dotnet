using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using JWT;
using JWT.Algorithms;
using JWT.Serializers;
using Ks.Fiks.Maskinporten.Client;
using Microsoft.Extensions.Configuration;
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
        const string SCOPES = "ks:fiks";
        private static IConfiguration _configuration;

        public static void Main(string[] args)
        {
            _configuration = new ConfigurationBuilder()
              .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
              .AddUserSecrets(typeof(Program).GetTypeInfo().Assembly)
              .AddEnvironmentVariables()
              .AddCommandLine(args)
              .Build();

            SampleUsingCertificateFromLocalFile();
            //SampleUsingCertificateFromWindowsStore("LARVIK KOMMUNE");
            //SampleUsingJsonWebKey();
        }

        private static void SampleUsingCertificateFromLocalFile()
        {
            // Relative or absolute path to the *.p12-file containing the test certificate
            var p12Filename = _configuration["P12FILENAME"];

            // Password required to use the certificate
            var p12Password = _configuration["P12PWD"];

            // The issuer as defined in Maskinporten
            var issuer = _configuration["MASKINPORTEN_ISSUER"];

            var cert = new X509Certificate2(p12Filename, p12Password);
            var configuration = MaskinportenClientConfigurationFactory.CreateVer2Configuration(issuer, cert);

            var maskinportenClient = new MaskinportenClient(configuration);

            
            var tokenTask = maskinportenClient.GetAccessToken(SCOPES).ContinueWith(t =>
            {
                var token = t.Result;
                Console.Out.WriteLine($"Token (expiring: {token.IsExpiring()}): {token.Token}");

                return DecodeToken(token, cert.GetRSAPublicKey(), cert.GetRSAPrivateKey());
            });

            // Do something with the token. In this case we only wait for it to be decoded and written to the console
            tokenTask.GetAwaiter().GetResult();
        }

        private static void SampleUsingCertificateFromWindowsStore(string certificateName) {
            var certificates = GetLocalMachineCertificates();
            var cert = certificates.Cast<X509Certificate2>().FirstOrDefault(cert => cert.FriendlyName.Equals(certificateName));

            if (cert == null)
            {
                Console.WriteLine($"Could not get sertificate with friendly name {certificateName}");
                return;
            }

            string issuer = _configuration["MASKINPORTEN_ISSUER"];
            var configuration = MaskinportenClientConfigurationFactory.CreateVer2Configuration(issuer, cert);

            var maskinportenClient = new MaskinportenClient(configuration);

            var tokenTask = maskinportenClient.GetAccessToken(SCOPES).ContinueWith(t =>
            {
                var token = t.Result;
                Console.Out.WriteLine($"Token (expiring: {token.IsExpiring()}): {token.Token}");

                return DecodeToken(token, cert.GetRSAPublicKey(), cert.GetRSAPrivateKey());
            });

            // Do something with the token. In this case we only wait for it to be decoded and written to the console
            tokenTask.GetAwaiter().GetResult();
        }

        private static void SampleUsingJsonWebKey()
        {
            // Relative or absolute path to the *.json file containing the JsonWebKey
            var jwkJson = System.IO.File.ReadAllText(_configuration["PATH_TO_JWKFILE"]);
            var jwk = new JsonWebKey(jwkJson);

            string issuer = _configuration["MASKINPORTEN_ISSUER"];

            var configuration = MaskinportenClientConfigurationFactory.CreateVer2Configuration(issuer, jwk);

            var maskinportenClient = new MaskinportenClient(configuration);

            var tokenTask = maskinportenClient.GetAccessToken(SCOPES).ContinueWith(t =>
            {
                var token = t.Result;
                Console.Out.WriteLine($"Token (expiring: {token.IsExpiring()}): {token.Token}");

                return DecodeToken(token, jwk.GetRSAPublicKey(), jwk.GetRSAPrivateKey());
            });

            // Do something with the token. In this case we only wait for it to be decoded and written to the console
            tokenTask.GetAwaiter().GetResult();
        }

        private static string DecodeToken(MaskinportenToken token, RSA publicKey, RSA privateKey)
        {
            var serializer = new JsonNetSerializer();
            var provider = new UtcDateTimeProvider();

            var jwtDecoder = new JwtDecoder(serializer, new JwtValidator(serializer, provider), new JwtBase64UrlEncoder(), new RS256Algorithm(publicKey, privateKey));
            var decodedToken = jwtDecoder.Decode(token.Token);
            Console.Out.WriteLine($"Decoded token {decodedToken}");
            return decodedToken;
        }

        private static X509Certificate2Collection GetLocalMachineCertificates()
        {
            var localMachineStore = new X509Store(StoreLocation.LocalMachine);
            localMachineStore.Open(OpenFlags.ReadOnly);
            var certificates = localMachineStore.Certificates;
            localMachineStore.Close();
            return certificates;
        }
    }
}