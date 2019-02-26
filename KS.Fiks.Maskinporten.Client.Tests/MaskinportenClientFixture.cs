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
        private string _accessToken;
        private HttpStatusCode _statusCode = HttpStatusCode.OK;
        private bool _useIncorrectCertificate = false;
        private Int64 _expirationTime;

        public MaskinportenClientFixture()
        {
            SetDefaultValues();
        }
        
        public Mock<HttpMessageHandler> HttpMessageHandleMock { get; private set; }
        
        public MaskinportenClientProperties Properties { get; private set; }

        public List<string> DefaultScopes => new List<string>();

        public MaskinportenClient CreateSut()
        {
            SetResponse();
            return new MaskinportenClient(
                _useIncorrectCertificate ? TestHelper.CertificateOtherThanUsedForDecode : TestHelper.Certificate, 
                Properties, 
                new HttpClient(HttpMessageHandleMock.Object));
        }

        public MaskinportenClientFixture WithAccessToken(string accessToken)
        {
            _accessToken = accessToken;
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

        public MaskinportenClientFixture WithExpirationTime(TimeSpan expirationTimeFromNow)
        {
            var newExpirationTime = UnixEpoch.GetSecondsSince(DateTime.UtcNow.Add(expirationTimeFromNow));
            _expirationTime = Convert.ToInt64(newExpirationTime);

            return this;
        }

        private void SetDefaultProperties()
        {
            Properties = new MaskinportenClientProperties("testAudience", "http://test.no", "testIssuer", 1);
        }

        private void SetDefaultValues()
        {
            SetDefaultProperties();
            _accessToken = "token";
            var newExpirationTime = UnixEpoch.GetSecondsSince(DateTimeOffset.UtcNow + TimeSpan.FromMinutes(10));
            _expirationTime = Convert.ToInt64(newExpirationTime);
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
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(responseMessage)
                .Verifiable();
        }
        
        

        private string GenerateJsonResponse()
        {
            dynamic response = new JObject();
            response.Add("expires_in", 1);

            var tokenResponse = new Dictionary<string, object>
            {
                {"aud", "test-aud"}, 
                {"scope", "test-scope"},
                {"iss", "https://test.no/oidc-provider/"},
                {"token_type", "bearer"},
                {"exp",  _expirationTime},
                {"iat", 1550832828},
                {"client_orgno", "987654321"},
                {"jti", "3Yi-C4E7wAYmCB1Qxaa44VSlmyyGtmrzQQCRN7p4xCY="}
            };
            var tokenHeader = new Dictionary<string, object>
            {
                {"kid", "mqT5A3LOSIHbpKrscb3EHGrr-WIFRfLdaqZ_5J9GR9s"}
            };
            var encodedToken = TestHelper.EncodeJwt(tokenHeader, tokenResponse);

            response.Add("access_token", encodedToken);
            return response.ToString();
        }
    }
}