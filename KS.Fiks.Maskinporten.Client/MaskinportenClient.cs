using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using JWT.Algorithms;
using JWT.Builder;
using Ks.Fiks.Maskinporten.Client.Cache;
using Newtonsoft.Json;

namespace Ks.Fiks.Maskinporten.Client
{
    public class MaskinportenClient : IMaskinportenClient
    {
        private const string GrantType = "urn:ietf:params:oauth:grant-type:jwt-bearer";
        private const int JwtExpireTimeInMinutes = 2;

        private readonly MaskinportenClientProperties _properties;
        private readonly HttpClient _httpClient;
        private readonly X509Certificate2 _certificate;

        private readonly ITokenCache<string> _tokenCache;

        public MaskinportenClient(X509Certificate2 certificate, MaskinportenClientProperties properties,
            HttpClient httpClient = null)
        {
            _properties = properties;
            _certificate = certificate;
            _httpClient = httpClient ?? new HttpClient();
            _tokenCache = new TokenCache<string>(TimeSpan.FromSeconds(_properties.NumberOfSecondsLeftBeforeExpire));
        }

        public async Task<string> GetAccessToken(IEnumerable<string> scopes)
        {
            var scopesAsString = ScopesAsString(scopes);
            return await GetAccessToken(scopesAsString);
        }

        public async Task<string> GetAccessToken(string scopes)
        {
            return await _tokenCache.GetToken(scopes, async () => await GetNewAccessToken(scopes));
        }


        private string ScopesAsString(IEnumerable<string> scopes)
        {
            return string.Join(" ", scopes);
        }


        private async Task<string> GetNewAccessToken(string scopes)
        {
            SetRequestHeaders();
            var requestContent = CreateRequestContent(scopes);
            var response = await _httpClient.PostAsync(_properties.TokenEndpoint, requestContent);
            if (response.StatusCode != HttpStatusCode.OK)
            {
                var content = await response.Content.ReadAsStringAsync();
                throw new UnexpectedResponseException(
                    $"Got unexpected HTTP Status code {response.StatusCode} ({response.ReasonPhrase}) from {_properties.TokenEndpoint}. Content: {content}");
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
 
            var content = new FormUrlEncodedContent(new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("grant_type", GrantType),
                new KeyValuePair<string, string>("assertion", CreateJwtToken(scopes))
            });
            
            content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
            return content;
        }

        private string CreateJwtToken(string scopes)
        {
            var jwt = new JwtBuilder()
                      .WithAlgorithm(new RS256Algorithm(_certificate))
                      .Audience(_properties.Audience)
                      .Issuer(_properties.Issuer)
                      .IssuedAt(DateTime.Now)
                      .ExpirationTime(DateTime.Now.AddMinutes(JwtExpireTimeInMinutes))
                      .AddClaim("Scope", scopes)
                      .Build();

            return jwt;
        }

        private async Task<MaskinportenResponse> ReadResponse(HttpResponseMessage responseMessage)
        {
            var responseAsJson = await responseMessage.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<MaskinportenResponse>(responseAsJson);
        }
    }
}