using System;
using JWT.Builder;
using Newtonsoft.Json.Linq;

namespace Ks.Fiks.Maskinporten.Client
{
    public class MaskinportenToken
    {
        private readonly string _rawJson;

        public string Audience { get; }

        public string Scope { get; }

        public string Issuer { get; }

        public string TokenType { get; }

        public DateTime ExpirationTime { get; }

        public DateTime IssuedAt { get; }

        public string ClientOrgno { get; }

        public string JwtId { get; }


        private MaskinportenToken(string rawJson)
        {
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

        public string AsJsonString()
        {
            return _rawJson;
        }

        public override bool Equals(object obj)
        {
            return obj.GetType() == GetType() && Equals((MaskinportenToken) obj);
        }

        protected bool Equals(MaskinportenToken other)
        {
            return string.Equals(Audience, other.Audience) &&
                   string.Equals(Scope, other.Scope) && string.Equals(Issuer, other.Issuer) &&
                   string.Equals(TokenType, other.TokenType) && ExpirationTime.Equals(other.ExpirationTime) &&
                   IssuedAt.Equals(other.IssuedAt) && string.Equals(ClientOrgno, other.ClientOrgno) &&
                   string.Equals(JwtId, other.JwtId);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (Audience != null ? Audience.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Scope != null ? Scope.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Issuer != null ? Issuer.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (TokenType != null ? TokenType.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ ExpirationTime.GetHashCode();
                hashCode = (hashCode * 397) ^ IssuedAt.GetHashCode();
                hashCode = (hashCode * 397) ^ (ClientOrgno != null ? ClientOrgno.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (JwtId != null ? JwtId.GetHashCode() : 0);
                return hashCode;
            }
        }

        public static MaskinportenToken CreateFromJsonString(string rawJson)
        {
            return new MaskinportenToken(rawJson);
        }

        private void SetValuesFromJson()
        {
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
                var unixEpochTime = long.Parse(secondsSinceEpoch);
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