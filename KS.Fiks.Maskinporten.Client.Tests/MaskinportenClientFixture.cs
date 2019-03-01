using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using JWT;
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

        public MaskinportenClientFixture()
        {
            SetDefaultValues();
        }

        public Mock<HttpMessageHandler> HttpMessageHandleMock { get; private set; }

        public MaskinportenClientProperties Properties { get; private set; }

        public List<string> DefaultScopes { get; private set; }

        public MaskinportenClient CreateSut()
        {
            SetResponse();
            return new MaskinportenClient(
                _useIncorrectCertificate ? TestHelper.CertificateOtherThanUsedForDecode : TestHelper.Certificate,
                Properties,
                new HttpClient(HttpMessageHandleMock.Object));
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
            SetDefaultProperties();
            _expirationTime = 120;
            DefaultScopes = new List<string>();
        }

        private void SetDefaultProperties()
        {
            Properties = new MaskinportenClientProperties("testAudience", "http://test.no", "testIssuer", 1);
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
            dynamic response = new JObject();
            response.Add("expires_in", _expirationTime);

            var tokenResponse = new Dictionary<string, object>
            {
                { "aud", "test-aud" },
                { "scope", "test-scope" },
                { "iss", "https://test.no/oidc-provider/" },
                { "token_type", "bearer" },
                { "exp", 1550832858 },
                { "iat", 1550832828 },
                { "client_orgno", "987654321" },
                { "jti", "3Yi-C4E7wAYmCB1Qxaa44VSlmyyGtmrzQQCRN7p4xCY=" }
            };
            var tokenHeader = new Dictionary<string, object>
            {
                { "kid", "mqT5A3LOSIHbpKrscb3EHGrr-WIFRfLdaqZ_5J9GR9s" }
            };
            var encodedToken = TestHelper.EncodeJwt(tokenHeader, tokenResponse);

            response.Add("access_token", encodedToken);
            return response.ToString();
        }
    }
}