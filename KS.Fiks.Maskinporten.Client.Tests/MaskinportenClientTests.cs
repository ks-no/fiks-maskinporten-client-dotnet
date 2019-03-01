using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using JWT;
using Moq;
using Moq.Protected;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Ks.Fiks.Maskinporten.Client.Tests
{
    public class MaskinportenClientTests
    {
        private MaskinportenClientFixture _fixture;

        public MaskinportenClientTests()
        {
            _fixture = new MaskinportenClientFixture();
        }

        [Fact]
        public async Task ReturnsAccessToken()
        {
            var sut = _fixture.CreateSut();

            var accessToken = await sut.GetAccessToken(_fixture.DefaultScopes).ConfigureAwait(false);
            accessToken.Should().BeOfType<MaskinportenToken>();
        }

        [Fact]
        public async Task ReturnsAccesstokenWithNonemptyFields()
        {
            var sut = _fixture.CreateSut();

            var accessToken = await sut.GetAccessToken(_fixture.DefaultScopes).ConfigureAwait(false);

            accessToken.Issuer.Should().NotBeEmpty();
            accessToken.Audience.Should().NotBeEmpty();
            accessToken.Scope.Should().NotBeEmpty();
            accessToken.JwtId.Should().NotBeEmpty();
            accessToken.ClientOrgno.Should().NotBeEmpty();
            accessToken.ExpirationTime.Should().BeAfter(UnixEpoch.Value);
            accessToken.IssuedAt.Should().BeAfter(UnixEpoch.Value);
        }

        [Fact]
        public async Task SendsRequestToTokenEndpoint()
        {
            var tokenEndpoint = "https://test.ks.no/api/token";
            _fixture.Properties.TokenEndpoint = tokenEndpoint;

            var sut = _fixture.CreateSut();

            await sut.GetAccessToken(_fixture.DefaultScopes).ConfigureAwait(false);

            _fixture.HttpMessageHandleMock.Protected().Verify(
                "SendAsync",
                Times.Exactly(1),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.RequestUri == new Uri(tokenEndpoint)),
                ItExpr.IsAny<CancellationToken>());
        }

        [Fact]
        public async Task DoesNotSendRequestTwiceIfSecondCallIsWithinTimelimit()
        {
            var sut = _fixture.CreateSut();

            var token1 = await sut.GetAccessToken(_fixture.DefaultScopes).ConfigureAwait(false);
            await Task.Delay(TimeSpan.FromMilliseconds(100)).ConfigureAwait(false);
            var token2 = await sut.GetAccessToken(_fixture.DefaultScopes).ConfigureAwait(false);

            token1.Should().Be(token2);
            _fixture.HttpMessageHandleMock.Protected().Verify(
                "SendAsync",
                Times.Exactly(1),
                ItExpr.Is<HttpRequestMessage>(req => true),
                ItExpr.IsAny<CancellationToken>());
        }

        [Fact]
        public async Task SendsRequestTwiceIfSecondCallIsOutsideTimelimit()
        {
            var sut = _fixture.WithIdportenExpirationDuration(1).CreateSut();

            var token1 = await sut.GetAccessToken(_fixture.DefaultScopes).ConfigureAwait(false);
            await Task.Delay(TimeSpan.FromMilliseconds(1100)).ConfigureAwait(false);
            var token2 = await sut.GetAccessToken(_fixture.DefaultScopes).ConfigureAwait(false);

            token1.Should().Be(token2);
            _fixture.HttpMessageHandleMock.Protected().Verify(
                "SendAsync",
                Times.Exactly(2),
                ItExpr.Is<HttpRequestMessage>(req => true),
                ItExpr.IsAny<CancellationToken>());
        }

        [Fact]
        public async Task DoesNotSendRequestTwiceIfSecondCallIsWithinTimelimitGivenNumberOfSecondsLeftBeforeExpire()
        {
            _fixture.Properties.NumberOfSecondsLeftBeforeExpire = 8;
            var sut = _fixture.WithIdportenExpirationDuration(10).CreateSut();

            var token1 = await sut.GetAccessToken(_fixture.DefaultScopes).ConfigureAwait(false);
            await Task.Delay(TimeSpan.FromMilliseconds(100)).ConfigureAwait(false);
            var token2 = await sut.GetAccessToken(_fixture.DefaultScopes).ConfigureAwait(false);

            token1.Should().Be(token2);
            _fixture.HttpMessageHandleMock.Protected().Verify(
                "SendAsync",
                Times.Exactly(1),
                ItExpr.Is<HttpRequestMessage>(req => true),
                ItExpr.IsAny<CancellationToken>());
        }

        [Fact]
        public async Task SendsRequestTwiceIfSecondCallIsOutsideTimelimitGivenNumberOfSecondsLeftBeforeExpire()
        {
            _fixture.Properties.NumberOfSecondsLeftBeforeExpire = 1;
            var sut = _fixture.WithIdportenExpirationDuration(2).CreateSut();

            var token1 = await sut.GetAccessToken(_fixture.DefaultScopes).ConfigureAwait(false);
            await Task.Delay(TimeSpan.FromMilliseconds(1100)).ConfigureAwait(false);
            var token2 = await sut.GetAccessToken(_fixture.DefaultScopes).ConfigureAwait(false);

            token1.Should().Be(token2);
            _fixture.HttpMessageHandleMock.Protected().Verify(
                "SendAsync",
                Times.Exactly(2),
                ItExpr.Is<HttpRequestMessage>(req => true),
                ItExpr.IsAny<CancellationToken>());
        }

        [Fact]
        public async Task SendsGrantTypeInPost()
        {
            var sut = _fixture.CreateSut();

            await sut.GetAccessToken(_fixture.DefaultScopes).ConfigureAwait(false);

            _fixture.HttpMessageHandleMock.Protected().Verify(
                "SendAsync",
                Times.Exactly(1),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Post &&
                    TestHelper.RequestContentAsDictionary(req).ContainsKey("grant_type")),
                ItExpr.IsAny<CancellationToken>());
        }

        [Fact]
        public async Task SendsAssertionInPost()
        {
            var sut = _fixture.CreateSut();

            await sut.GetAccessToken(_fixture.DefaultScopes).ConfigureAwait(false);

            _fixture.HttpMessageHandleMock.Protected().Verify(
                "SendAsync",
                Times.Exactly(1),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Post &&
                    TestHelper.RequestContentAsDictionary(req).ContainsKey("assertion")),
                ItExpr.IsAny<CancellationToken>());
        }

        [Fact]
        public async Task AssertionIsAValidSerializedJwt()
        {
            var sut = _fixture.CreateSut();

            await sut.GetAccessToken(_fixture.DefaultScopes).ConfigureAwait(false);

            _fixture.HttpMessageHandleMock.Protected().Verify(
                "SendAsync",
                Times.Exactly(1),
                ItExpr.Is<HttpRequestMessage>(req =>
                    TestHelper.RequestContentIsJwt(req, "assertion")),
                ItExpr.IsAny<CancellationToken>());
        }

        [Fact]
        public async Task AssertionDeserializedHasCorrectAudience()
        {
            var expectedAudience = "testAudience";
            _fixture.Properties.Audience = expectedAudience;
            var sut = _fixture.CreateSut();

            await sut.GetAccessToken(_fixture.DefaultScopes).ConfigureAwait(false);

            _fixture.HttpMessageHandleMock.Protected().Verify(
                "SendAsync",
                Times.Exactly(1),
                ItExpr.Is<HttpRequestMessage>(req =>
                    TestHelper.DeserializedFieldInJwt(req, "assertion", "aud") == expectedAudience),
                ItExpr.IsAny<CancellationToken>());
        }

        [Fact]
        public async Task AssertionDeserializedHasCorrectIssuer()
        {
            var expectedIssuer = "testIssuer";
            _fixture.Properties.Issuer = expectedIssuer;
            var sut = _fixture.CreateSut();

            await sut.GetAccessToken(_fixture.DefaultScopes).ConfigureAwait(false);

            _fixture.HttpMessageHandleMock.Protected().Verify(
                "SendAsync",
                Times.Exactly(1),
                ItExpr.Is<HttpRequestMessage>(req =>
                    TestHelper.DeserializedFieldInJwt(req, "assertion", "iss") == expectedIssuer),
                ItExpr.IsAny<CancellationToken>());
        }

        [Fact]
        public async Task AssertionDeserializedHasCorrectScope()
        {
            var expectedScope = new string[]
            {
                "Scope1", "Scope2"
            };

            var expectedScopeAsString = string.Join(" ", expectedScope);

            var sut = _fixture.CreateSut();

            await sut.GetAccessToken(expectedScope).ConfigureAwait(false);

            _fixture.HttpMessageHandleMock.Protected().Verify(
                "SendAsync",
                Times.Exactly(1),
                ItExpr.Is<HttpRequestMessage>(req =>
                    TestHelper.DeserializedFieldInJwt(req, "assertion", "scope") == expectedScopeAsString),
                ItExpr.IsAny<CancellationToken>());
        }

        [Fact]
        public async Task AssertionDeserializedHasIssueTimeWithinASecondOfNow()
        {
            var sut = _fixture.CreateSut();

            await sut.GetAccessToken(_fixture.DefaultScopes).ConfigureAwait(false);
            var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            _fixture.HttpMessageHandleMock.Protected().Verify(
                "SendAsync",
                Times.Exactly(1),
                ItExpr.Is<HttpRequestMessage>(req =>
                    TestHelper.DeserializedFieldInJwt<double>(req, "assertion", "iat") <= now + 1 &&
                    TestHelper.DeserializedFieldInJwt<double>(req, "assertion", "iat") >= now - 1),
                ItExpr.IsAny<CancellationToken>());
        }

        [Fact]
        public async Task AssertionDeserializedHasExpieryTimeTwoMinutesAfterNow()
        {
            var sut = _fixture.CreateSut();

            await sut.GetAccessToken(_fixture.DefaultScopes).ConfigureAwait(false);
            var expectedExpiredTime = DateTimeOffset.UtcNow.AddMinutes(2).ToUnixTimeSeconds();

            _fixture.HttpMessageHandleMock.Protected().Verify(
                "SendAsync",
                Times.Exactly(1),
                ItExpr.Is<HttpRequestMessage>(req =>
                    TestHelper.DeserializedFieldInJwt<double>(req, "assertion", "exp") <=
                    expectedExpiredTime + 1 &&
                    TestHelper.DeserializedFieldInJwt<double>(req, "assertion", "exp") >=
                    expectedExpiredTime - 1),
                ItExpr.IsAny<CancellationToken>());
        }

        [Fact]
        public async Task TestHelperThrowsExceptionIfIncorrectSignature()
        {
            var sut = _fixture.WithIncorrectCertificate().CreateSut();

            await sut.GetAccessToken(_fixture.DefaultScopes).ConfigureAwait(false);
            Assert.Throws<JWT.SignatureVerificationException>(() =>
            {
                _fixture.HttpMessageHandleMock.Protected().Verify(
                    "SendAsync",
                    Times.Exactly(1),
                    ItExpr.Is<HttpRequestMessage>(req =>
                        TestHelper.DeserializedFieldInJwt(req, "assertion", "aud") != "nothing"),
                    ItExpr.IsAny<CancellationToken>());
            });
        }

        [Fact]
        public async Task SendsHeaderCharsetUtf8()
        {
            var sut = _fixture.CreateSut();

            await sut.GetAccessToken(_fixture.DefaultScopes).ConfigureAwait(false);

            _fixture.HttpMessageHandleMock.Protected().Verify(
                "SendAsync",
                Times.Exactly(1),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Content.Headers.GetValues("Charset").Contains("utf-8")),
                ItExpr.IsAny<CancellationToken>());
        }

        [Fact]
        public async Task SendsHeaderCorrectContentType()
        {
            var sut = _fixture.CreateSut();

            await sut.GetAccessToken(_fixture.DefaultScopes).ConfigureAwait(false);

            _fixture.HttpMessageHandleMock.Protected().Verify(
                "SendAsync",
                Times.Exactly(1),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Content.Headers.ContentType.MediaType == "application/x-www-form-urlencoded"),
                ItExpr.IsAny<CancellationToken>());
        }

        [Fact]
        public async Task SendsHeaderContentLength()
        {
            var sut = _fixture.CreateSut();

            await sut.GetAccessToken(_fixture.DefaultScopes).ConfigureAwait(false);

            _fixture.HttpMessageHandleMock.Protected().Verify(
                "SendAsync",
                Times.Exactly(1),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Content.Headers.ContentLength > 0),
                ItExpr.IsAny<CancellationToken>());
        }

        [Fact]
        public async Task SendsHeaderNoCacheTrue()
        {
            var sut = _fixture.CreateSut();

            await sut.GetAccessToken(_fixture.DefaultScopes).ConfigureAwait(false);

            _fixture.HttpMessageHandleMock.Protected().Verify(
                "SendAsync",
                Times.Exactly(1),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Headers.CacheControl.NoCache),
                ItExpr.IsAny<CancellationToken>());
        }

        [Theory]
        [InlineData(HttpStatusCode.NotFound)]
        [InlineData(HttpStatusCode.Unauthorized)]
        [InlineData(HttpStatusCode.InternalServerError)]
        [InlineData(HttpStatusCode.NoContent)]
        [InlineData(HttpStatusCode.Redirect)]
        [InlineData(HttpStatusCode.Forbidden)]
        public async Task ThrowsExceptionIfStatusCodeIsNot200(HttpStatusCode statusCode)
        {
            var sut = _fixture.WithStatusCode(statusCode).CreateSut();

            await Assert.ThrowsAsync<UnexpectedResponseException>(
                async () => await sut.GetAccessToken(_fixture.DefaultScopes).ConfigureAwait(false)).ConfigureAwait(false);
        }
    }
}