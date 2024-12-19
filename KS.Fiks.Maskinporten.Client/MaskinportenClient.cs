using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using KS.Fiks.Maskinporten.Client;
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
        private readonly MaskinportenClientConfiguration _configuration;
        private readonly HttpClient _httpClient;

        private readonly IJwtRequestTokenGenerator _tokenGenerator;

        private readonly ITokenCache _tokenCache;

        public MaskinportenClient(
            MaskinportenClientConfiguration configuration,
            HttpClient httpClient = null)
        {
            _configuration = configuration;
            _httpClient = httpClient ?? new HttpClient();
            _tokenCache = new TokenCache();
            if (_configuration.Certificate != null)
            {
                _tokenGenerator = new JwtRequestTokenGenerator(
                    _configuration.Certificate,
                    _configuration.KeyIdentifier);
            }
            else
            {
                _tokenGenerator = new JwtRequestTokenGenerator(
                    _configuration.PublicKey,
                    _configuration.PrivateKey,
                    _configuration.KeyIdentifier);
            }
        }

        public async Task<MaskinportenToken> GetAccessToken(IEnumerable<string> scopes)
        {
            var scopesAsString = ScopesAsString(scopes);
            return await GetAccessToken(scopesAsString).ConfigureAwait(false);
        }

        public async Task<MaskinportenToken> GetAccessToken(string scopes)
        {
            var tokenRequest = new TokenRequest
            {
                Scopes = scopes
            };
            return await GetAccessTokenForRequest(tokenRequest).ConfigureAwait(false);
        }

        public async Task<MaskinportenToken> GetDelegatedAccessToken(string consumerOrg, IEnumerable<string> scopes)
        {
            return await GetDelegatedAccessToken(consumerOrg, ScopesAsString(scopes)).ConfigureAwait(false);
        }

        public async Task<MaskinportenToken> GetDelegatedAccessToken(string consumerOrg, string scopes)
        {
            return await GetAccessTokenForRequest(new TokenRequest
            {
                Scopes = scopes,
                ConsumerOrg = consumerOrg
            }).ConfigureAwait(false);
        }

        public async Task<MaskinportenToken> GetDelegatedAccessTokenForAudience(string consumerOrg, string audience,
            IEnumerable<string> scopes)
        {
            return await GetDelegatedAccessTokenForAudience(consumerOrg, audience, ScopesAsString(scopes))
                .ConfigureAwait(false);
        }

        public async Task<MaskinportenToken> GetDelegatedAccessTokenForAudience(string consumerOrg, string audience,
            string scopes)
        {
            return await GetAccessTokenForRequest(new TokenRequest
            {
                Audience = audience,
                Scopes = scopes,
                ConsumerOrg = consumerOrg
            }).ConfigureAwait(false);
        }

        public async Task<MaskinportenToken> GetOnBehalfOfAccessToken(string consumerOrg, IEnumerable<string> scopes)
        {
            return await GetOnBehalfOfAccessToken(consumerOrg, ScopesAsString(scopes)).ConfigureAwait(false);
        }

        public async Task<MaskinportenToken> GetOnBehalfOfAccessToken(string consumerOrg, string scopes)
        {
            return await GetAccessTokenForRequest(new TokenRequest
            {
                Scopes = scopes,
                OnBehalfOf = consumerOrg
            }).ConfigureAwait(false);
        }

        private async Task<MaskinportenToken> GetAccessTokenForRequest(TokenRequest tokenRequest)
        {
            return await this._tokenCache.GetToken(
                tokenRequest,
                async () => await GetNewAccessToken(tokenRequest).ConfigureAwait(false)).ConfigureAwait(false);
        }

        private static async Task<MaskinportenResponse> ReadResponse(HttpResponseMessage responseMessage)
        {
            var responseAsJson = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
            return JsonConvert.DeserializeObject<MaskinportenResponse>(responseAsJson);
        }

        private static string ScopesAsString(IEnumerable<string> scopes)
        {
            return string.Join(" ", scopes);
        }

        private async Task<MaskinportenToken> GetNewAccessToken(TokenRequest tokenRequest)
        {
            SetRequestHeaders();
            var requestContent = CreateRequestContent(tokenRequest);

            var tokenUri = new Uri(_configuration.TokenEndpoint);
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

        private FormUrlEncodedContent CreateRequestContent(TokenRequest tokenRequest)
        {
            var content = new FormUrlEncodedContent(new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("grant_type", GrantType),
                new KeyValuePair<string, string>("assertion",
                    _tokenGenerator.CreateEncodedJwt(tokenRequest, _configuration))
            });

            var consumerOrg = tokenRequest.ConsumerOrg ?? this._configuration.ConsumerOrg;
            if (consumerOrg != null)
            {
                content.Headers.Add("consumer_org", consumerOrg);
            }

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
                    $"Got unexpected HTTP Status code {response.StatusCode} from {_configuration.TokenEndpoint}. Content: {content}.");
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
            return tokenExpiresIn - _configuration.NumberOfSecondsLeftBeforeExpire;
        }
    }
}