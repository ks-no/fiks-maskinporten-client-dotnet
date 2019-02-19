using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Moq.Protected;
using Xunit;


namespace Ks.Fiks.Maskinporten.Client.Tests
{
    public class GetAccessToken
    {
        private MaskinportenClientFixture _fixture;

        public GetAccessToken()
        {
            _fixture = new MaskinportenClientFixture();
        }

        [Fact]
        public async Task ReturnsAccessToken()
        {
            var expectedAccessToken = "kldsfh39psdjf239i32+u9f";
            var sut = _fixture.WithAccessToken(expectedAccessToken).CreateSut();

            var accessToken = await sut.GetAccessToken(_fixture.DefaultScopes);
            accessToken.Should().Be(expectedAccessToken);
        }

        [Fact]
        public async Task SendsRequestToTokenEndpoint()
        {
            var tokenEndpoint = "https://test.ks.no/api/token";
            _fixture.Properties.TokenEndpoint = tokenEndpoint;

            var sut = _fixture.CreateSut();

            await sut.GetAccessToken(_fixture.DefaultScopes);

            _fixture.HttpMessageHandleMock.Protected().Verify(
                "SendAsync",
                Times.Exactly(1),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.RequestUri == new Uri(tokenEndpoint)
                ),
                ItExpr.IsAny<CancellationToken>()
            );
        }

        [Fact]
        public async Task DoesNotSendRequestTwiceIfSecondCallIsWithinTimelimit()
        {
            _fixture.Properties.NumberOfSecondsLeftBeforeExpire = 1000;
            var sut = _fixture.CreateSut();

            var token1 = await sut.GetAccessToken(_fixture.DefaultScopes);
            await Task.Delay(TimeSpan.FromMilliseconds(100));
            var token2 = await sut.GetAccessToken(_fixture.DefaultScopes);

            token1.Should().Be(token2);
            _fixture.HttpMessageHandleMock.Protected().Verify(
                "SendAsync",
                Times.Exactly(1),
                ItExpr.Is<HttpRequestMessage>(req => true),
                ItExpr.IsAny<CancellationToken>()
            );
        }
        
        [Fact]
        public async Task SendsRequestTwiceIfSecondCallIsOutsideTimelimit()
        {
            _fixture.Properties.NumberOfSecondsLeftBeforeExpire = 1;
            var sut = _fixture.CreateSut();

            var token1 = await sut.GetAccessToken(_fixture.DefaultScopes);
            await Task.Delay(TimeSpan.FromMilliseconds(1500));
            var token2 = await sut.GetAccessToken(_fixture.DefaultScopes);

            token1.Should().Be(token2);
            _fixture.HttpMessageHandleMock.Protected().Verify(
                "SendAsync",
                Times.Exactly(2),
                ItExpr.Is<HttpRequestMessage>(req => true),
                ItExpr.IsAny<CancellationToken>()
            );
        }

        [Fact]
        public async Task SendsGrantTypeInPost()
        {
            var sut = _fixture.CreateSut();

            await sut.GetAccessToken(_fixture.DefaultScopes);

            _fixture.HttpMessageHandleMock.Protected().Verify(
                "SendAsync",
                Times.Exactly(1),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Post &&
                    TestHelper.RequestContentAsDictionary(req).ContainsKey("grant_type")
                ),
                ItExpr.IsAny<CancellationToken>()
            );
        }

        [Fact]
        public async Task SendsAssertionInPost()
        {
            var sut = _fixture.CreateSut();

            await sut.GetAccessToken(_fixture.DefaultScopes);

            _fixture.HttpMessageHandleMock.Protected().Verify(
                "SendAsync",
                Times.Exactly(1),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Post &&
                    TestHelper.RequestContentAsDictionary(req).ContainsKey("assertion")
                ),
                ItExpr.IsAny<CancellationToken>()
            );
        }

        [Fact]
        public async Task AssertionIsAValidSerializedJwt()
        {
            var sut = _fixture.CreateSut();
            
            await sut.GetAccessToken(_fixture.DefaultScopes);

            _fixture.HttpMessageHandleMock.Protected().Verify(
                "SendAsync",
                Times.Exactly(1),
                ItExpr.Is<HttpRequestMessage>(req =>
                    TestHelper.RequestContentIsJwt(req, "assertion")
                ),
                ItExpr.IsAny<CancellationToken>()
            );
        }

        [Fact]
        public async Task AssertionDeserializedHasCorrectAudience()
        {
            var expectedAudience = "testAudience";
            _fixture.Properties.Audience = expectedAudience; 
            var sut = _fixture.CreateSut();
            
            await sut.GetAccessToken(_fixture.DefaultScopes);

            _fixture.HttpMessageHandleMock.Protected().Verify(
                "SendAsync",
                Times.Exactly(1),
                ItExpr.Is<HttpRequestMessage>(req =>
                    TestHelper.DeserializedFieldInJwt(req,"assertion", "aud") == expectedAudience
                ),
                ItExpr.IsAny<CancellationToken>()
            );
        }
        
        [Fact]
        public async Task AssertionDeserializedHasCorrectIssuer()
        {
            var expectedIssuer = "testIssuer";
            _fixture.Properties.Issuer = expectedIssuer; 
            var sut = _fixture.CreateSut();
            
            await sut.GetAccessToken(_fixture.DefaultScopes);

            _fixture.HttpMessageHandleMock.Protected().Verify(
                "SendAsync",
                Times.Exactly(1),
                ItExpr.Is<HttpRequestMessage>(req =>
                    TestHelper.DeserializedFieldInJwt(req,"assertion", "iss") == expectedIssuer
                ),
                ItExpr.IsAny<CancellationToken>()
            );
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
            
            await sut.GetAccessToken(expectedScope);

            _fixture.HttpMessageHandleMock.Protected().Verify(
                "SendAsync",
                Times.Exactly(1),
                ItExpr.Is<HttpRequestMessage>(req =>
                    TestHelper.DeserializedFieldInJwt(req,"assertion", "Scope") == expectedScopeAsString
                ),
                ItExpr.IsAny<CancellationToken>()
            );
        }
        
        [Fact]
        public async Task AssertionDeserializedHasIssueTimeWithinASecondOfNow()
        {

            var sut = _fixture.CreateSut();
            
            await sut.GetAccessToken(_fixture.DefaultScopes);
            var now = DateTimeOffset.Now.ToUnixTimeSeconds();

            _fixture.HttpMessageHandleMock.Protected().Verify(
                "SendAsync",
                Times.Exactly(1),
                ItExpr.Is<HttpRequestMessage>(req =>
                    long.Parse(TestHelper.DeserializedFieldInJwt(req,"assertion", "iat")) <= now + 1 &&
                    long.Parse(TestHelper.DeserializedFieldInJwt(req,"assertion", "iat")) >= now - 1 

                ),
                ItExpr.IsAny<CancellationToken>()
            );
        }
        
        [Fact]
        public async Task AssertionDeserializedHasExpieryTimeTwoMinutesAfterNow()
        {
            var sut = _fixture.CreateSut();
            
            await sut.GetAccessToken(_fixture.DefaultScopes);
            var expectedExpiredTime = DateTimeOffset.Now.AddMinutes(2).ToUnixTimeSeconds();

            _fixture.HttpMessageHandleMock.Protected().Verify(
                "SendAsync",
                Times.Exactly(1),
                ItExpr.Is<HttpRequestMessage>(req =>
                    long.Parse(TestHelper.DeserializedFieldInJwt(req,"assertion", "exp")) <= expectedExpiredTime + 1 &&
                    long.Parse(TestHelper.DeserializedFieldInJwt(req,"assertion", "exp")) >= expectedExpiredTime - 1 

                ),
                ItExpr.IsAny<CancellationToken>()
            );
        }
        
        [Fact]
        public async Task TestHelperThrowsExceptionIfIncorrectSignature()
        {
            var sut = _fixture.WithIncorrectCertificate().CreateSut();
            
            await sut.GetAccessToken(_fixture.DefaultScopes);
            Assert.Throws<JWT.SignatureVerificationException>(() =>
            {
                _fixture.HttpMessageHandleMock.Protected().Verify(
                    "SendAsync",
                    Times.Exactly(1),
                    ItExpr.Is<HttpRequestMessage>(req =>
                        TestHelper.DeserializedFieldInJwt(req, "assertion", "aud") != "nothing"
                    ),
                    ItExpr.IsAny<CancellationToken>()
                );
            });
        }
        
        [Fact]
        public async Task SendsHeaderCharsetUtf8()
        {
            var sut = _fixture.CreateSut();

            await sut.GetAccessToken(_fixture.DefaultScopes);

            _fixture.HttpMessageHandleMock.Protected().Verify(
                "SendAsync",
                Times.Exactly(1),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Headers.GetValues("Charset").Contains("utf-8")
                ),
                ItExpr.IsAny<CancellationToken>()
            );
        }


        [Fact]
        public async Task SendsHeaderCorrectContentType()
        {
            var sut = _fixture.CreateSut();

            await sut.GetAccessToken(_fixture.DefaultScopes);

            _fixture.HttpMessageHandleMock.Protected().Verify(
                "SendAsync",
                Times.Exactly(1),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Content.Headers.ContentType.MediaType == "application/x-www-form-urlencoded"
                ),
                ItExpr.IsAny<CancellationToken>()
            );
        }

        [Fact]
        public async Task SendsHeaderContentLength()
        {
            var sut = _fixture.CreateSut();

            await sut.GetAccessToken(_fixture.DefaultScopes);

            _fixture.HttpMessageHandleMock.Protected().Verify(
                "SendAsync",
                Times.Exactly(1),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Content.Headers.ContentLength > 0
                ),
                ItExpr.IsAny<CancellationToken>()
            );
        }

        [Fact]
        public async Task SendsHeaderNoCacheTrue()
        {
            var sut = _fixture.CreateSut();

            await sut.GetAccessToken(_fixture.DefaultScopes);

            _fixture.HttpMessageHandleMock.Protected().Verify(
                "SendAsync",
                Times.Exactly(1),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Headers.CacheControl.NoCache
                ),
                ItExpr.IsAny<CancellationToken>()
            );
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
                async () => await sut.GetAccessToken(_fixture.DefaultScopes)
            );
        }
    }
}