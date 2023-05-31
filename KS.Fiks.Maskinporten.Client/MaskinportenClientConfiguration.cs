using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace Ks.Fiks.Maskinporten.Client
{
    public class MaskinportenClientConfiguration
    {
        public MaskinportenClientConfiguration(
            string audience,
            string tokenEndpoint,
            string issuer,
            int numberOfSecondsLeftBeforeExpire,
            X509Certificate2 certificate = null,
            RSA privateKey = null,
            RSA publicKey = null,
            string keyIdentifier = null,
            string consumerOrg = null)
        {
            if (certificate == null && (privateKey == null || publicKey == null))
            {
                throw new ArgumentException("Either certificate or private and public key must be set!");
            }

            if (certificate != null && (privateKey != null || publicKey != null))
            {
                throw new ArgumentException("Only certificate or public/private key must be set. Not both");
            }

            Audience = audience;
            TokenEndpoint = tokenEndpoint;
            Issuer = issuer;
            NumberOfSecondsLeftBeforeExpire = numberOfSecondsLeftBeforeExpire;
            Certificate = certificate;
            PrivateKey = privateKey;
            PublicKey = publicKey;
            KeyIdentifier = keyIdentifier;
            ConsumerOrg = consumerOrg;
        }

        public string Audience { get; }

        public string TokenEndpoint { get; }

        public string Issuer { get; }

        public string ConsumerOrg { get; }

        public int NumberOfSecondsLeftBeforeExpire { get; }

        public X509Certificate2 Certificate { get; }

        public RSA PublicKey { get; }

        public RSA PrivateKey { get; }

        /// <summary>
        /// Gets an optional identifier for the key given by the <see cref="PublicKey"/>
        /// and <see cref="PrivateKey"/> key pair. Can be used if several keys are set up for your integration.
        /// </summary>
        /// <value>An optional identifier for the key given by the <see cref="PublicKey"/> and <see cref="PrivateKey"/>
        /// key pair. Can be used if several keys are set up for your integration.</value>
        public string KeyIdentifier { get; }
    }
}