using System.Collections.Generic;

namespace KS.Fiks.Maskinporten.Client.Builder
{
    public class TokenRequestBuilder
    {
        private readonly TokenRequest _tokenRequest = new TokenRequest();

        public TokenRequestBuilder WithScopes(IEnumerable<string> scopes)
        {
            _tokenRequest.Scopes = string.Join(" ", scopes);
            return this;
        }

        public TokenRequestBuilder WithScopes(string scopes)
        {
            _tokenRequest.Scopes = scopes;
            return this;
        }

        public TokenRequestBuilder WithConsumerOrg(string consumerOrg)
        {
            _tokenRequest.ConsumerOrg = consumerOrg;
            return this;
        }

        public TokenRequestBuilder WithOnBehalfOf(string onBehalfOf)
        {
            _tokenRequest.OnBehalfOf = onBehalfOf;
            return this;
        }

        public TokenRequestBuilder WithAudience(string audience)
        {
            _tokenRequest.Audience = audience;
            return this;
        }

        public TokenRequestBuilder WithPid(string pid)
        {
            _tokenRequest.Pid = pid;
            return this;
        }

        public TokenRequest Build()
        {
            return _tokenRequest;
        }
    }
}