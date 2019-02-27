using System;
using FluentAssertions;
using Xunit;

namespace Ks.Fiks.Maskinporten.Client.Tests
{
    public class MaskinportenTokenTests
    {
        [Fact]
        public void CreatesMaskinportenTokenWithExpectedFieldsFromJsonString()
        {
            var maskinportenTokenAsJsonString = @"{
            ""aud"": ""oidc_ks_test"",
            ""scope"": ""ks"",
            ""iss"": ""https://oidc-ver2.difi.no/idporten-oidc-provider/"",
            ""token_type"": ""Bearer"",
            ""exp"": 1550837855,
            ""iat"": 1550837825,
            ""client_orgno"": ""987654321"",
            ""jti"": ""ifFO_xAYGepbtUxZhUcESoNkewGG6v15sfCWGPm_MUI=""
            }";

            var token = MaskinportenToken.CreateFromJsonString(maskinportenTokenAsJsonString, 120);
            
            token.Audience.Should().Be("oidc_ks_test");
            token.Scope.Should().Be("ks");
            token.Issuer.Should().Be("https://oidc-ver2.difi.no/idporten-oidc-provider/");
            token.TokenType.Should().Be("Bearer");
            token.ExpirationTime.Should().Be(DateTime.UnixEpoch.AddSeconds(1550837855));
            token.IssuedAt.Should().Be(DateTime.UnixEpoch.AddSeconds(1550837825));
            token.ClientOrgno.Should().Be("987654321");
            token.JwtId.Should().Be("ifFO_xAYGepbtUxZhUcESoNkewGG6v15sfCWGPm_MUI=");
        }
    }
}