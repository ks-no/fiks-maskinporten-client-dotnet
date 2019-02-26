using System;
using JWT.Builder;
using Newtonsoft.Json.Linq;

namespace Ks.Fiks.Maskinporten.Client
{
    public class MaskinportenToken
    {
        private string _rawJson;

        public string Audience { get; private set; }

        public string Scope { get; private set; }

        public string Issuer { get; private set; }

        public string TokenType { get; private set; }

        public DateTime ExpirationTime { get; private set; }

        public DateTime IssuedAt { get; private set; }

        public string ClientOrgno { get; private set; }

        public string JwtId { get; private set; }


        private MaskinportenToken(string rawJson)
        {
            _rawJson = rawJson;
            SetValuesFromJson();
        }

        public string AsJsonString()
        {
            return _rawJson;
        }

        public override bool Equals(object obj)
        {
            if (obj.GetType() != this.GetType())
            {
                return false;
            }
            else
            {
                var other = (MaskinportenToken) obj;
                return other.Audience == Audience
                       && other.Scope == Scope
                       && other.Issuer == Issuer
                       && other.TokenType == TokenType
                       && other.ExpirationTime == ExpirationTime
                       && other.IssuedAt == IssuedAt
                       && other.ClientOrgno == ClientOrgno
                       && other.JwtId == JwtId;
            }
        }

        public static MaskinportenToken CreateFromJsonString(string rawJson)
        {
            return new MaskinportenToken(rawJson);
        }

        private void SetValuesFromJson()
        {
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