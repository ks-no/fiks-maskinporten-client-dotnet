#nullable enable
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using JWT.Algorithms;
using Moq;
using Moq.Protected;
using Newtonsoft.Json.Linq;

namespace Ks.Fiks.Maskinporten.Client.Tests
{
    public class MaskinportenClientFixture
    {
        private HttpStatusCode _statusCode = HttpStatusCode.OK;
        private bool _useIncorrectCertificate = false;
        private long _expirationTime;
        private string _tokenEndpoint = "http://test.no";
        private int _numberOfSecondsLeftBeforeExpire = 1;
        private string _audience = "testAudience";
        private string _issuer = "testIssuer";
        private string? _consumerOrg = null;
        private RSA? _privateKey = null;
        private RSA? _publicKey = null;
        private string? _keyIdentifier = null;

        public MaskinportenClientFixture()
        {
            SetDefaultValues();
        }

        public Mock<HttpMessageHandler> HttpMessageHandleMock { get; private set; }

        public MaskinportenClientConfiguration Configuration { get; private set; }

        public List<string> DefaultScopes { get; private set; }

        public MaskinportenClient CreateSut()
        {
            SetResponse();
            SetDefaultProperties();
            return new MaskinportenClient(
                Configuration,
                new HttpClient(HttpMessageHandleMock.Object));
        }

        public MaskinportenClientFixture WithTokenEndpoint(string tokenEndpoint)
        {
            _tokenEndpoint = tokenEndpoint;
            return this;
        }

        public MaskinportenClientFixture WithNumberOfSecondsLeftBeforeExpire(int number)
        {
            _numberOfSecondsLeftBeforeExpire = number;
            return this;
        }

        public MaskinportenClientFixture WithKeyPair(RSA publicKey, RSA privateKey, string? keyIdentifier = null)
        {
            _publicKey = publicKey;
            _privateKey = privateKey;
            _keyIdentifier = keyIdentifier;
            return this;
        }

        public MaskinportenClientFixture WithAudience(string audience)
        {
            _audience = audience;
            return this;
        }

        public MaskinportenClientFixture WithIssuer(string issuer)
        {
            _issuer = issuer;
            return this;
        }

        public MaskinportenClientFixture WithConsumerOrg(string consumerOrg)
        {
            _consumerOrg = consumerOrg;
            return this;
        }

        public MaskinportenClientFixture WithStatusCode(HttpStatusCode statusCode)
        {
            _statusCode = statusCode;
            return this;
        }

        public MaskinportenClientFixture WithIncorrectCertificate()
        {
            _useIncorrectCertificate = true;
            return this;
        }

        public MaskinportenClientFixture WithIdportenExpirationDuration(int expirationTime)
        {
            _expirationTime = expirationTime;

            return this;
        }

        private void SetDefaultValues()
        {
            _expirationTime = 120;
            DefaultScopes = new List<string>();
        }

        private void SetDefaultProperties()
        {
            X509Certificate2? cert = null;

            if (UseCertificate())
            {
                cert = _useIncorrectCertificate ? TestHelper.CertificateOtherThanUsedForDecode : TestHelper.Certificate;
            }

            Configuration = new MaskinportenClientConfiguration(
                 _audience,
                 _tokenEndpoint,
                 _issuer,
                 _numberOfSecondsLeftBeforeExpire,
                 cert,
                 _publicKey,
                 _privateKey,
                 _keyIdentifier,
                 _consumerOrg);
        }

        private bool UseCertificate()
        {
            return _privateKey == null && _publicKey == null;
        }

        private void SetResponse()
        {
            var responseMessage = new HttpResponseMessage()
            {
                StatusCode = _statusCode,
                Content = new StringContent(GenerateJsonResponse()),
            };

            HttpMessageHandleMock = new Mock<HttpMessageHandler>();
            HttpMessageHandleMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(responseMessage)
                .Verifiable();
        }

        private string GenerateJsonResponse()
        {
            const string KeyIdentifier = "some-key";
            dynamic response = new JObject();
            response.Add("expires_in", _expirationTime);

            var tokenResponse = new Dictionary<string, object>
            {
                {"aud", "test-aud"},
                {"scope", "test-scope"},
                {"iss", "https://test.no/oidc-provider/"},
                {"token_type", "bearer"},
                {"exp", 1550832858},
                {"iat", 1550832828},
                {"client_orgno", "987654321"},
                {"jti", "3Yi-C4E7wAYmCB1Qxaa44VSlmyyGtmrzQQCRN7p4xCY="}
            };

            string encodedToken;

            if (UseCertificate())
            {
                encodedToken = TestHelper.EncodeJwt(KeyIdentifier, tokenResponse);
            }
            else
            {
                encodedToken = TestHelper.EncodeJwt(
                    KeyIdentifier,
                    tokenResponse,
                    new RSAlgorithmFactory(_publicKey, _privateKey),
                    new RS256Algorithm(_publicKey, _privateKey));
            }

            response.Add("access_token", encodedToken);
            return response.ToString();
        }
    }
}