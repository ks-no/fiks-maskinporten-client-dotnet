using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using JWT;
using JWT.Serializers;
using Ks.Fiks.Maskinporten.Client.Cache;
using Ks.Fiks.Maskinporten.Client.Jwt;
using Newtonsoft.Json;

namespace Ks.Fiks.Maskinporten.Client
{
    public class MaskinportenClient : IMaskinportenClient
    {
        private const string GrantType = "urn:ietf:params:oauth:grant-type:jwt-bearer";

        private readonly MaskinportenClientProperties _properties;
        private readonly HttpClient _httpClient;

        private readonly IJwtRequestTokenGenerator _tokenGenerator;
        private readonly IJwtResponseDecoder _responseDecoder;

        private readonly ITokenCache<string> _tokenCache;

        public MaskinportenClient(
            X509Certificate2 certificate,
            MaskinportenClientProperties properties,
            HttpClient httpClient = null)
        {
            _properties = properties;
            _httpClient = httpClient ?? new HttpClient();
            _tokenCache = new TokenCache<string>(TimeSpan.FromSeconds(_properties.NumberOfSecondsLeftBeforeExpire));
            _tokenGenerator = new JwtRequestTokenGenerator(certificate);
            _responseDecoder = new JwtResponseDecoder();
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
            var requestContent = await CreateRequestContent(scopes);

            var response = await _httpClient.PostAsync(_properties.TokenEndpoint, requestContent);
            await ThrowIfResponseIsInvalid(response);

            var maskinportenResponse = await ReadResponse(response);
            var accessTokenAsJwt = maskinportenResponse.AccessToken;
            var accessToken = _responseDecoder.JwtAsString(accessTokenAsJwt);
            return accessToken;
        }

        private void SetRequestHeaders()
        {
            _httpClient.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue()
            {
                NoCache = true
            };
        }

        private async Task<ByteArrayContent> CreateRequestContent(string scopes)
        {
            var content = new FormUrlEncodedContent(new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("grant_type", GrantType),
                new KeyValuePair<string, string>("assertion", _tokenGenerator.CreateEncodedJwt(scopes, _properties))
            });

            content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
            content.Headers.Add("Charset", "utf-8");

            return content;
        }

        private async Task<MaskinportenResponse> ReadResponse(HttpResponseMessage responseMessage)
        {
            var responseAsJson = await responseMessage.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<MaskinportenResponse>(responseAsJson);
        }

        private async Task ThrowIfResponseIsInvalid(HttpResponseMessage response)
        {
            if (response.StatusCode != HttpStatusCode.OK)
            {
                var content = await response.Content.ReadAsStringAsync();
                throw new UnexpectedResponseException(
                    $"Got unexpected HTTP Status code {response.StatusCode} from {_properties.TokenEndpoint}. Content: {content}.");
            }
        }
    }
}