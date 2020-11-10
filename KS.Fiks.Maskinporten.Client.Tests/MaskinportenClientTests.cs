using System;
using System.Linq;
using System.Linq.Expressions;
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

            accessToken.Token.Should().NotBeEmpty();
        }

        [Fact]
        public async Task SendsRequestToTokenEndpoint()
        {
            var tokenEndpoint = "https://test.ks.no/api/token";

            var sut = _fixture.WithTokenEndpoint(tokenEndpoint).CreateSut();

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
        public async Task DoesNotSendDelegatedAccessTokenRequestTwiceIfSecondCallIsWithinTimelimit()
        {
            const string consumerOrg = "999888999";
            var sut = _fixture.CreateSut();

            var token1 = await sut.GetDelegatedAccessToken(consumerOrg, _fixture.DefaultScopes).ConfigureAwait(false);
            await Task.Delay(TimeSpan.FromMilliseconds(100)).ConfigureAwait(false);
            var token2 = await sut.GetDelegatedAccessToken(consumerOrg, _fixture.DefaultScopes).ConfigureAwait(false);

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
            var sut = _fixture.WithIdportenExpirationDuration(10).WithNumberOfSecondsLeftBeforeExpire(8).CreateSut();

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
            var sut = _fixture.WithIdportenExpirationDuration(2).WithNumberOfSecondsLeftBeforeExpire(1).CreateSut();

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
            var sut = _fixture.CreateSut(req =>
                    req.Method == HttpMethod.Post &&
                    TestHelper.RequestContentAsDictionary(req).ContainsKey("grant_type"));

            await sut.GetAccessToken(_fixture.DefaultScopes).ConfigureAwait(false);

            _fixture.HttpMessageHandleMock.Protected().Verify(
                "SendAsync",
                Times.Exactly(1),
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>());
        }

        [Fact]
        public async Task SendsAssertionInPost()
        {
            var sut = _fixture.CreateSut(req =>
                    req.Method == HttpMethod.Post &&
                    TestHelper.RequestContentAsDictionary(req).ContainsKey("assertion"));

            await sut.GetAccessToken(_fixture.DefaultScopes).ConfigureAwait(false);

            _fixture.HttpMessageHandleMock.Protected().Verify(
                "SendAsync",
                Times.Exactly(1),
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>());
        }

        [Fact]
        public async Task AssertionIsAValidSerializedJwt()
        {
            var sut = _fixture.CreateSut(req =>
                    TestHelper.RequestContentIsJwt(req, "assertion"));

            await sut.GetAccessToken(_fixture.DefaultScopes).ConfigureAwait(false);

            _fixture.HttpMessageHandleMock.Protected().Verify(
                "SendAsync",
                Times.Exactly(1),
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>());
        }

        [Fact]
        public async Task AssertionDeserializedHasCorrectAudience()
        {
            var expectedAudience = "testAudience";
            var sut = _fixture.WithAudience(expectedAudience).CreateSut(req =>
                        TestHelper.DeserializedFieldInJwt(req, "assertion", "aud") == expectedAudience);

            await sut.GetAccessToken(_fixture.DefaultScopes).ConfigureAwait(false);

            _fixture.HttpMessageHandleMock.Protected().Verify(
                "SendAsync",
                Times.Exactly(1),
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>());
        }

        [Fact]
        public async Task AssertionDeserializedHasCorrectIssuer()
        {
            var expectedIssuer = "testIssuer";
            var sut = _fixture.WithIssuer(expectedIssuer).CreateSut(req =>
                    TestHelper.DeserializedFieldInJwt(req, "assertion", "iss") == expectedIssuer);

            await sut.GetAccessToken(_fixture.DefaultScopes).ConfigureAwait(false);

            _fixture.HttpMessageHandleMock.Protected().Verify(
                "SendAsync",
                Times.Exactly(1),
                ItExpr.IsAny<HttpRequestMessage>(),
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

            var sut = _fixture.CreateSut(req =>
                    TestHelper.DeserializedFieldInJwt(req, "assertion", "scope") == expectedScopeAsString);

            await sut.GetAccessToken(expectedScope).ConfigureAwait(false);

            _fixture.HttpMessageHandleMock.Protected().Verify(
                "SendAsync",
                Times.Exactly(1),
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>());
        }

        [Fact]
        public async Task AssertionDeserializedHasIssueTimeWithinTwoSecondsOfNow()
        {
            var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var sut = _fixture.CreateSut(req =>
                    TestHelper.DeserializedFieldInJwt<double>(req, "assertion", "iat") <= now + 2 &&
                    TestHelper.DeserializedFieldInJwt<double>(req, "assertion", "iat") >= now - 2);

            await sut.GetAccessToken(_fixture.DefaultScopes).ConfigureAwait(false);

            _fixture.HttpMessageHandleMock.Protected().Verify(
                "SendAsync",
                Times.Exactly(1),
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>());
        }

        [Fact]
        public async Task AssertionDeserializedHasExpieryTimeTwoMinutesAfterNow()
        {
            var expectedExpiredTime = DateTimeOffset.UtcNow.AddMinutes(2).ToUnixTimeSeconds();
            var sut = _fixture.CreateSut(req =>
                    TestHelper.DeserializedFieldInJwt<double>(req, "assertion", "exp") <=
                    expectedExpiredTime + 1 &&
                    TestHelper.DeserializedFieldInJwt<double>(req, "assertion", "exp") >=
                    expectedExpiredTime - 1);

            await sut.GetAccessToken(_fixture.DefaultScopes).ConfigureAwait(false);

            _fixture.HttpMessageHandleMock.Protected().Verify(
                "SendAsync",
                Times.Exactly(1),
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>());
        }

        [Fact]
        public async Task TestHelperThrowsExceptionIfIncorrectSignature()
        {
            var sut = _fixture.WithIncorrectCertificate().CreateSut(req =>
                        TestHelper.DeserializedFieldInJwt(req, "assertion", "aud") != "nothing");

            await Assert.ThrowsAsync<JWT.Exceptions.SignatureVerificationException>(async () =>
            {
                await sut.GetAccessToken(_fixture.DefaultScopes).ConfigureAwait(false);
            });

            _fixture.HttpMessageHandleMock.Protected().Verify(
                "SendAsync",
                Times.Exactly(1),
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>());
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
        public async Task SendsHeaderConsumerOrgIfSet()
        {
            var consumerOrg = "123456789";
            var sut = _fixture.WithConsumerOrg(consumerOrg).CreateSut();

            await sut.GetAccessToken(_fixture.DefaultScopes).ConfigureAwait(false);

            _fixture.HttpMessageHandleMock.Protected().Verify(
                "SendAsync",
                Times.Exactly(1),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Content.Headers.GetValues("consumer_org").Contains(consumerOrg)),
                ItExpr.IsAny<CancellationToken>());
        }

        [Fact]
        public async Task DoesNotSendHeaderConsumerOrgIfNotSet()
        {
            var sut = _fixture.WithConsumerOrg(null).CreateSut();

            await sut.GetAccessToken(_fixture.DefaultScopes).ConfigureAwait(false);

            _fixture.HttpMessageHandleMock.Protected().Verify(
                "SendAsync",
                Times.Exactly(1),
                ItExpr.Is<HttpRequestMessage>(req =>
                    !req.Content.Headers.Contains("consumer_org")),
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
            var sut = _fixture.CreateSut(req =>
                    req.Content.Headers.ContentLength > 0);

            await sut.GetAccessToken(_fixture.DefaultScopes).ConfigureAwait(false);

            _fixture.HttpMessageHandleMock.Protected().Verify(
                "SendAsync",
                Times.Exactly(1),
                ItExpr.IsAny<HttpRequestMessage>(),
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