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
            var maskinportenClientConfiguration = MaskinportenClientConfigurationFactory.createVer2Configuration(issuer, certificate);
            maskinportenClientConfiguration.TokenEndpoint.Should()
                .Be(MaskinportenClientConfigurationFactory.VER2_TOKEN_ENDPOINT);
            maskinportenClientConfiguration.Audience.Should().Be(MaskinportenClientConfigurationFactory.VER2_AUDIENCE);
            maskinportenClientConfiguration.Issuer.Should().Be(issuer);
            maskinportenClientConfiguration.Certificate.Should().Be(certificate);
        }
        
        [Fact]
        public void CreateProdConfiguration()
        {
            const string issuer = "issuer";
            var certificate = TestHelper.Certificate;
            var maskinportenClientConfiguration = MaskinportenClientConfigurationFactory.createProdConfiguration(issuer, certificate);
            maskinportenClientConfiguration.TokenEndpoint.Should()
                .Be(MaskinportenClientConfigurationFactory.PROD_TOKEN_ENDPOINT);
            maskinportenClientConfiguration.Audience.Should().Be(MaskinportenClientConfigurationFactory.PROD_AUDIENCE);
            maskinportenClientConfiguration.Issuer.Should().Be(issuer);
            maskinportenClientConfiguration.Certificate.Should().Be(certificate);
        }
    }
}