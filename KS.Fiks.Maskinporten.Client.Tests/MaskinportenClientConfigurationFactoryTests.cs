using Shouldly;
using Xunit;

namespace Ks.Fiks.Maskinporten.Client.Tests
{
    public class MaskinportenClientConfigurationFactoryTests
    {
        [Fact]
        public void CreateVer2Configuration()
        {
#pragma warning disable CS0618 // Type or member is obsolete
            const string issuer = "issuer";
            var certificate = TestHelper.Certificate;
            var maskinportenClientConfiguration = MaskinportenClientConfigurationFactory.CreateVer2Configuration(issuer, certificate);
            maskinportenClientConfiguration.TokenEndpoint.ShouldBe(MaskinportenClientConfigurationFactory.VER2_TOKEN_ENDPOINT);
            maskinportenClientConfiguration.Audience.ShouldBe(MaskinportenClientConfigurationFactory.VER2_AUDIENCE);
            maskinportenClientConfiguration.Issuer.ShouldBe(issuer);
            maskinportenClientConfiguration.Certificate.ShouldBe(certificate);
#pragma warning restore CS0618 // Type or member is obsolete
        }

        [Fact]
        public void CreateTestConfiguration()
        {
            const string issuer = "issuer";
            var certificate = TestHelper.Certificate;
            var maskinportenClientConfiguration = MaskinportenClientConfigurationFactory.CreateTestConfiguration(issuer, certificate);
            maskinportenClientConfiguration.TokenEndpoint.ShouldBe(MaskinportenClientConfigurationFactory.TEST_TOKEN_ENDPOINT);
            maskinportenClientConfiguration.Audience.ShouldBe(MaskinportenClientConfigurationFactory.TEST_AUDIENCE);
            maskinportenClientConfiguration.Issuer.ShouldBe(issuer);
            maskinportenClientConfiguration.Certificate.ShouldBe(certificate);
        }
        
        [Fact]
        public void CreateTestConfigurationWithKeyPair()
        {
            const string issuer = "issuer";
            const string keyIdentifier = "some-kid";
            var privateKey = TestHelper.PrivateKey;
            var publicKey = TestHelper.PublicKey;
            var maskinportenClientConfiguration = MaskinportenClientConfigurationFactory.CreateTestConfiguration(
                issuer,
                privateKey: privateKey,
                publicKey: publicKey,
                keyIdentifier: keyIdentifier);
            maskinportenClientConfiguration.TokenEndpoint.ShouldBe(MaskinportenClientConfigurationFactory.TEST_TOKEN_ENDPOINT);
            maskinportenClientConfiguration.Audience.ShouldBe(MaskinportenClientConfigurationFactory.TEST_AUDIENCE);
            maskinportenClientConfiguration.Issuer.ShouldBe(issuer);
            maskinportenClientConfiguration.PrivateKey.ShouldBeEquivalentTo(privateKey);
            maskinportenClientConfiguration.PublicKey.ShouldBeEquivalentTo(publicKey);
            maskinportenClientConfiguration.KeyIdentifier.ShouldBe(keyIdentifier);
        }

        [Fact]
        public void CreateProdConfiguration()
        {
            const string issuer = "issuer";
            var certificate = TestHelper.Certificate;
            var maskinportenClientConfiguration = MaskinportenClientConfigurationFactory.CreateProdConfiguration(issuer, certificate);
            maskinportenClientConfiguration.TokenEndpoint.ShouldBe(MaskinportenClientConfigurationFactory.PROD_TOKEN_ENDPOINT);
            maskinportenClientConfiguration.Audience.ShouldBe(MaskinportenClientConfigurationFactory.PROD_AUDIENCE);
            maskinportenClientConfiguration.Issuer.ShouldBe(issuer);
            maskinportenClientConfiguration.Certificate.ShouldBe(certificate);
        }

        [Fact]
        public void CreateProdConfigurationWithKeyPair()
        {
            const string issuer = "issuer";
            const string keyIdentifier = "some-kid";
            var privateKey = TestHelper.PrivateKey;
            var publicKey = TestHelper.PublicKey;
            var maskinportenClientConfiguration = MaskinportenClientConfigurationFactory.CreateProdConfiguration(
                issuer,
                privateKey: privateKey,
                publicKey: publicKey,
                keyIdentifier: keyIdentifier);
            maskinportenClientConfiguration.TokenEndpoint.ShouldBe(MaskinportenClientConfigurationFactory.PROD_TOKEN_ENDPOINT);
            maskinportenClientConfiguration.Audience.ShouldBe(MaskinportenClientConfigurationFactory.PROD_AUDIENCE);
            maskinportenClientConfiguration.Issuer.ShouldBe(issuer);
            maskinportenClientConfiguration.PrivateKey.ShouldBeEquivalentTo(privateKey);
            maskinportenClientConfiguration.PublicKey.ShouldBeEquivalentTo(publicKey);
            maskinportenClientConfiguration.KeyIdentifier.ShouldBe(keyIdentifier);
        }
    }
}