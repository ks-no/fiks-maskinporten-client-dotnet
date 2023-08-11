using FluentAssertions;
using Xunit;

namespace Ks.Fiks.Maskinporten.Client.Tests
{
    public class MaskinportenClientConfigurationFactoryTests
    {
        [Fact]
        public void CreateVer2Configuration()
        {
            const string issuer = "issuer";
            var certificate = TestHelper.Certificate;
            var maskinportenClientConfiguration = MaskinportenClientConfigurationFactory.CreateVer2Configuration(issuer, certificate);
            maskinportenClientConfiguration.TokenEndpoint.Should()
                .Be(MaskinportenClientConfigurationFactory.VER2_TOKEN_ENDPOINT);
            maskinportenClientConfiguration.Audience.Should().Be(MaskinportenClientConfigurationFactory.VER2_AUDIENCE);
            maskinportenClientConfiguration.Issuer.Should().Be(issuer);
            maskinportenClientConfiguration.Certificate.Should().Be(certificate);
        }

        [Fact]
        public void CreateTestConfiguration()
        {
            const string issuer = "issuer";
            var certificate = TestHelper.Certificate;
            var maskinportenClientConfiguration = MaskinportenClientConfigurationFactory.CreateTestConfiguration(issuer, certificate);
            maskinportenClientConfiguration.TokenEndpoint.Should()
                .Be(MaskinportenClientConfigurationFactory.TEST_TOKEN_ENDPOINT);
            maskinportenClientConfiguration.Audience.Should().Be(MaskinportenClientConfigurationFactory.TEST_AUDIENCE);
            maskinportenClientConfiguration.Issuer.Should().Be(issuer);
            maskinportenClientConfiguration.Certificate.Should().Be(certificate);
        }
        
        [Fact]
        public void CreateTestConfigurationWithKeyPair()
        {
            const string issuer = "issuer";
            const string keyIdentifier = "some-kid";
            var privateKey = TestHelper.PrivateKey;
            var publicKey = TestHelper.PublicKey;
            var maskinportenClientConfiguration = MaskinportenClientConfigurationFactory.CreateVer2Configuration(
                issuer,
                privateKey: privateKey,
                publicKey: publicKey,
                keyIdentifier: keyIdentifier);
            maskinportenClientConfiguration.TokenEndpoint.Should()
                .Be(MaskinportenClientConfigurationFactory.VER2_TOKEN_ENDPOINT);
            maskinportenClientConfiguration.Audience.Should().Be(MaskinportenClientConfigurationFactory.VER2_AUDIENCE);
            maskinportenClientConfiguration.Issuer.Should().Be(issuer);
            maskinportenClientConfiguration.PrivateKey.Should().Be(privateKey);
            maskinportenClientConfiguration.PublicKey.Should().Be(publicKey);
            maskinportenClientConfiguration.KeyIdentifier.Should().Be(keyIdentifier);
        }

        [Fact]
        public void CreateProdConfiguration()
        {
            const string issuer = "issuer";
            var certificate = TestHelper.Certificate;
            var maskinportenClientConfiguration = MaskinportenClientConfigurationFactory.CreateProdConfiguration(issuer, certificate);
            maskinportenClientConfiguration.TokenEndpoint.Should()
                .Be(MaskinportenClientConfigurationFactory.PROD_TOKEN_ENDPOINT);
            maskinportenClientConfiguration.Audience.Should().Be(MaskinportenClientConfigurationFactory.PROD_AUDIENCE);
            maskinportenClientConfiguration.Issuer.Should().Be(issuer);
            maskinportenClientConfiguration.Certificate.Should().Be(certificate);
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
            maskinportenClientConfiguration.TokenEndpoint.Should()
                .Be(MaskinportenClientConfigurationFactory.PROD_TOKEN_ENDPOINT);
            maskinportenClientConfiguration.Audience.Should().Be(MaskinportenClientConfigurationFactory.PROD_AUDIENCE);
            maskinportenClientConfiguration.Issuer.Should().Be(issuer);
            maskinportenClientConfiguration.PrivateKey.Should().Be(privateKey);
            maskinportenClientConfiguration.PublicKey.Should().Be(publicKey);
            maskinportenClientConfiguration.KeyIdentifier.Should().Be(keyIdentifier);
        }
    }
}