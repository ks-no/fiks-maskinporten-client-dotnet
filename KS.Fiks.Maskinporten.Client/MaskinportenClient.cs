using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using JWT;
using JWT.Algorithms;
using JWT.Builder;
using JWT.Serializers;
using Newtonsoft.Json;

namespace Ks.Fiks.Maskinporten.Client
{
    public class MaskinportenClient : IMaskinportenClient
    {
        private const string GRANT_TYPE = "urn:ietf:params:oauth:grant-type:jwt-bearer";
        private const int JWT_EXPIRE_TIME_IN_MINUTES = 2;

        private MaskinportenClientProperties _properties;
        private HttpClient _httpClient;
        private X509Certificate2 _certificate;
       
        private Dictionary<string, string> _accessTokenCache;

        public MaskinportenClient(X509Certificate2 certificate, MaskinportenClientProperties properties,   HttpClient httpClient = null)
        {
            _properties = properties;
            _certificate = certificate;
            _httpClient = httpClient ?? new HttpClient();
            _accessTokenCache = new Dictionary<string, string>();
        }

        public async Task<string> GetAccessToken(IEnumerable<string> scopes)
        {
            var scopesAsString = ScopesAsString(scopes);
            return await GetAccessToken(scopesAsString);
        }
        
        public async Task<string> GetAccessToken(string scopes)
        {
            if (HasValidCachedAccessToken(scopes))
            {
                return _accessTokenCache[scopes];
            }

            var accessToken = await GetNewAccessToken(scopes);
            StoreAccessTokenInCache(accessToken, scopes);

            return accessToken;
        }


        private string ScopesAsString(IEnumerable<string> scopes)
        {
            return string.Join(" ", scopes);
        }

        private bool HasValidCachedAccessToken(string scopeKey)
        {
            return _accessTokenCache.ContainsKey(scopeKey);
        }

        private async Task<string> GetNewAccessToken(string scopes)
        {
            SetRequestHeaders();
            var requestContent = CreateRequestContent(scopes);
            var response = await _httpClient.PostAsync(_properties.TokenEndpoint, requestContent);
            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new UnexpectedResponseException(
                    $"Got unexpected HTTP Status code {response.StatusCode} ({response.ReasonPhrase}) from {_properties.TokenEndpoint}");
            }
            var maskinportenResponse = await ReadResponse(response);
            return maskinportenResponse.AccessToken;
        }

        private void SetRequestHeaders()
        {
            _httpClient.DefaultRequestHeaders.Add("Charset", "utf-8");
            _httpClient.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue()
            {
                NoCache = true
            };
        }

        private ByteArrayContent CreateRequestContent(string scopes)
        {
            var request = new MaskinportenRequest()
            {
                GrantType = GRANT_TYPE,
                Assertion = CreateJwtToken(scopes)
            };
            var requestAsJson = JsonConvert.SerializeObject(request);
            var requestAsByteArray = Encoding.UTF8.GetBytes(requestAsJson);
            var content = new ByteArrayContent(requestAsByteArray);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
            content.Headers.ContentLength = requestAsByteArray.Length;
            return content;
        }
        
        private string CreateJwtToken(string scopes)
        {
            var jwt = new JwtBuilder()
                      .WithAlgorithm(new RS256Algorithm(_certificate))
                      .Audience(_properties.Audience)
                      .Issuer(_properties.Issuer)
                      .IssuedAt(DateTime.Now)
                      .ExpirationTime(DateTime.Now.AddMinutes(JWT_EXPIRE_TIME_IN_MINUTES))
                      .AddClaim("Scope",scopes)
                      .Build();

            return jwt;
        }


        private async Task<MaskinportenResponse> ReadResponse(HttpResponseMessage responseMessage)
        {
            var responseAsJson = await responseMessage.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<MaskinportenResponse>(responseAsJson);
        }

        private void StoreAccessTokenInCache(string accessToken, string scopeKey)
        {
            _accessTokenCache.Add(scopeKey, accessToken);
        }


    }
}