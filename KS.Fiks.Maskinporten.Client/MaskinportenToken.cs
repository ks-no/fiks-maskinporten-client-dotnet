using System;
using System.Globalization;
using JWT.Builder;
using Newtonsoft.Json.Linq;

namespace Ks.Fiks.Maskinporten.Client
{
    public class MaskinportenToken
    {
        private readonly string _rawJson;

        private readonly DateTime _requestNewTokenAfterTime;

        public string Audience { get; }

        public string Scope { get; }

        public string Issuer { get; }

        public string TokenType { get; }

        public DateTime ExpirationTime { get; }

        public DateTime IssuedAt { get; }

        public string ClientOrgno { get; }

        public string JwtId { get; }

        private MaskinportenToken(string rawJson, int expiresIn)
        {
            _requestNewTokenAfterTime = DateTime.UtcNow.AddSeconds(expiresIn);

            _rawJson = rawJson;
            var json = JObject.Parse(_rawJson);
            Audience = GetStringValueFromJson(json, "aud");
            Scope = GetStringValueFromJson(json, "scope");
            Issuer = GetStringValueFromJson(json, "iss");
            TokenType = GetStringValueFromJson(json, "token_type");
            ExpirationTime = GetDateTimeFromJson(json, "exp");
            IssuedAt = GetDateTimeFromJson(json, "iat");
            JwtId = GetStringValueFromJson(json, "jti");
            ClientOrgno = GetStringValueFromJson(json, "client_orgno");
        }

        public static MaskinportenToken CreateFromJsonString(string rawJson, int expiresIn)
        {
            return new MaskinportenToken(rawJson, expiresIn);
        }

        public string AsJsonString()
        {
            return _rawJson;
        }

        public override bool Equals(object obj)
        {
            return obj.GetType() == GetType() && Equals((MaskinportenToken)obj);
        }

        public bool IsExpiring()
        {
            return _requestNewTokenAfterTime < DateTime.UtcNow;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Audience != null ? Audience.GetHashCode(StringComparison.Ordinal) : 0;
                hashCode = (hashCode * 397) ^ (Scope != null ? Scope.GetHashCode(StringComparison.Ordinal) : 0);
                hashCode = (hashCode * 397) ^ (Issuer != null ? Issuer.GetHashCode(StringComparison.Ordinal) : 0);
                hashCode = (hashCode * 397) ^ (TokenType != null ? TokenType.GetHashCode(StringComparison.Ordinal) : 0);
                hashCode = (hashCode * 397) ^ ExpirationTime.GetHashCode();
                hashCode = (hashCode * 397) ^ IssuedAt.GetHashCode();
                hashCode = (hashCode * 397) ^
                           (ClientOrgno != null ? ClientOrgno.GetHashCode(StringComparison.Ordinal) : 0);
                hashCode = (hashCode * 397) ^ (JwtId != null ? JwtId.GetHashCode(StringComparison.Ordinal) : 0);
                return hashCode;
            }
        }

        protected bool Equals(MaskinportenToken other)
        {
            return string.Equals(Audience, other.Audience, StringComparison.Ordinal) &&
                   string.Equals(Scope, other.Scope, StringComparison.Ordinal) &&
                   string.Equals(Issuer, other.Issuer, StringComparison.Ordinal) &&
                   string.Equals(TokenType, other.TokenType, StringComparison.Ordinal) &&
                   ExpirationTime.Equals(other.ExpirationTime) &&
                   IssuedAt.Equals(other.IssuedAt) &&
                   string.Equals(ClientOrgno, other.ClientOrgno, StringComparison.Ordinal) &&
                   string.Equals(JwtId, other.JwtId, StringComparison.Ordinal);
        }

        private static string GetStringValueFromJson(JObject json, string field)
        {
            try
            {
                if (!json.ContainsKey(field))
                {
                    // Log warning?
                    return string.Empty;
                }

                return json[field].ToString();
            }
            catch
            {
                // Log warning?
                return string.Empty;
            }
        }

        private static DateTime GetDateTimeFromJson(JObject json, string field)
        {
            try
            {
                if (!json.ContainsKey(field))
                {
                    // Log warning?
                    return DateTime.UnixEpoch;
                }

                var secondsSinceEpoch = json[field].ToString();
                var unixEpochTime = long.Parse(secondsSinceEpoch, CultureInfo.InvariantCulture);
                return DateTime.UnixEpoch.AddSeconds(unixEpochTime);
            }
            catch
            {
                // Log warning?
                return DateTime.UnixEpoch;
            }
        }
    }
}