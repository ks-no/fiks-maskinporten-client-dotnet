using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;
using Newtonsoft.Json.Linq;

namespace Ks.Fiks.Maskinporten.Client.Tests
{
    public class MaskinportenClientFixture
    {
        private string _accessToken;
        private HttpStatusCode _statusCode = HttpStatusCode.OK;

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
            return new MaskinportenClient(Properties, new HttpClient(HttpMessageHandleMock.Object));
        }

        public void SetAccessToken(string accessToken)
        {
            _accessToken = accessToken;
        }

        public void SetStatusCode(HttpStatusCode statusCode)
        {
            _statusCode = statusCode;
        }

        private void SetDefaultProperties()
        {
            Properties = new MaskinportenClientProperties("testAudience", "http://test.no", "testIssuer", 1);
        }

        private void SetDefaultValues()
        {
            SetDefaultProperties();
            _accessToken = "token";
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
            response.expires_in = 1;
            response.access_token = _accessToken;
            return response.ToString();
        }
    }
}