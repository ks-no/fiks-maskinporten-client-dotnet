using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Ks.Fiks.Maskinporten.Client.Cache;
using Ks.Fiks.Maskinporten.Client.Jwt;
using Newtonsoft.Json;

namespace Ks.Fiks.Maskinporten.Client
{
    public class MaskinportenClient : IMaskinportenClient
    {
        private const string GrantType = "urn:ietf:params:oauth:grant-type:jwt-bearer";
        private const string MediaTypeFromUrl = "application/x-www-form-urlencoded";
        private const string CharsetUtf8 = "utf-8";
        private readonly MaskinportenClientProperties _properties;
        private readonly HttpClient _httpClient;

        private readonly IJwtRequestTokenGenerator _tokenGenerator;
        private readonly IJwtResponseDecoder _responseDecoder;

        private readonly ITokenCache _tokenCache;

        public MaskinportenClient(
            X509Certificate2 certificate,
            MaskinportenClientProperties properties,
            HttpClient httpClient = null)
        {
            _properties = properties;
            _httpClient = httpClient ?? new HttpClient();
            _tokenCache = new TokenCache();
            _tokenGenerator = new JwtRequestTokenGenerator(certificate);
            _responseDecoder = new JwtResponseDecoder();
        }

        public async Task<MaskinportenToken> GetAccessToken(IEnumerable<string> scopes)
        {
            var scopesAsString = ScopesAsString(scopes);
            return await GetAccessToken(scopesAsString).ConfigureAwait(false);
        }

        public async Task<MaskinportenToken> GetAccessToken(string scopes)
        {
            return await _tokenCache.GetToken(
                                        scopes,
                                        async () => await GetNewAccessToken(scopes).ConfigureAwait(false))
                                    .ConfigureAwait(false);
        }

        private static async Task<MaskinportenResponse> ReadResponse(HttpResponseMessage responseMessage)
        {
            var responseAsJson = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
            return JsonConvert.DeserializeObject<MaskinportenResponse>(responseAsJson);
        }

        private string ScopesAsString(IEnumerable<string> scopes)
        {
            return string.Join(" ", scopes);
        }

        private async Task<MaskinportenToken> GetNewAccessToken(string scopes)
        {
            SetRequestHeaders();
            var requestContent = CreateRequestContent(scopes);

            var tokenUri = new Uri(_properties.TokenEndpoint);
            var response = await _httpClient.PostAsync(tokenUri, requestContent).ConfigureAwait(false);

            await ThrowIfResponseIsInvalid(response).ConfigureAwait(false);

            return await CreateTokenFromResponse(response).ConfigureAwait(false);
        }

        private void SetRequestHeaders()
        {
            _httpClient.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue()
            {
                NoCache = true
            };
        }

        private FormUrlEncodedContent CreateRequestContent(string scopes)
        {
            var content = new FormUrlEncodedContent(new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("grant_type", GrantType),
                new KeyValuePair<string, string>("assertion", _tokenGenerator.CreateEncodedJwt(scopes, _properties))
            });

            content.Headers.ContentType = new MediaTypeHeaderValue(MediaTypeFromUrl);
            content.Headers.Add("Charset", CharsetUtf8);

            return content;
        }

        private async Task ThrowIfResponseIsInvalid(HttpResponseMessage response)
        {
            if (response.StatusCode != HttpStatusCode.OK)
            {
                var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                throw new UnexpectedResponseException(
                    $"Got unexpected HTTP Status code {response.StatusCode} from {_properties.TokenEndpoint}. Content: {content}.");
            }
        }

        private async Task<MaskinportenToken> CreateTokenFromResponse(HttpResponseMessage response)
        {
            var maskinportenResponse = await ReadResponse(response).ConfigureAwait(false);

            return new MaskinportenToken(
                maskinportenResponse.AccessToken,
                ExpirationTimeInSeconds(maskinportenResponse.ExpiresIn));

        }

        private int ExpirationTimeInSeconds(int tokenExpiresIn)
        {
            return tokenExpiresIn - _properties.NumberOfSecondsLeftBeforeExpire;
        }
    }
}