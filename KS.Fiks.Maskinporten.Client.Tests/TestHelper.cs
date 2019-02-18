using System;
using System.Collections.Generic;
using System.Net.Http;
using JWT;
using JWT.Serializers;
using Newtonsoft.Json;

namespace Ks.Fiks.Maskinporten.Client.Tests
{
    public static class TestHelper
    {
        public static Dictionary<string, string> RequestContentAsDictionary(HttpRequestMessage request)
        {
            var json = request.Content.ReadAsStringAsync().Result;
            return JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
        }

        public static bool RequestContentIsJwt(HttpRequestMessage request, string jwtFieldName)
        {
            var content = RequestContentAsDictionary(request);
            var serializedJwt = content[jwtFieldName];

            
            try
            {
                var decodedJwt = GetDeserializedJwt(serializedJwt);
                return decodedJwt?.Length > 0;
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"Could not decode: {ex.Message}");
                return false;
            }

        }

        public static string DeserializedFieldInJwt(HttpRequestMessage request, string jwtFieldName, string field)
        {
            var content = RequestContentAsDictionary(request);
            var serializedJwt = content[jwtFieldName];
            var deserializedJwt = GetDeserializedJwt(serializedJwt);


            var jwtAsDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(deserializedJwt);
            return jwtAsDictionary[field];
        }

        private static string GetDeserializedJwt(string serializedJwt)
        {
            var serializer = new JsonNetSerializer();
            var validator = new JwtValidator(serializer, new UtcDateTimeProvider());
            var encoder = new JwtBase64UrlEncoder();
            var decoder = new JwtDecoder(serializer, validator, encoder);
            return decoder.Decode(serializedJwt);
        }
    }
}