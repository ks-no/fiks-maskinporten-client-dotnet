using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Web;
using JWT;
using JWT.Algorithms;
using JWT.Builder;
using JWT.Serializers;
using Newtonsoft.Json;

namespace Ks.Fiks.Maskinporten.Client.Tests
{
    public static class TestHelper
    {        
        private static readonly RSAlgorithmFactory _factory = new RSAlgorithmFactory(() => Certificate);
        private static readonly JsonNetSerializer _serializer = new JsonNetSerializer();
        private static readonly JwtValidator _validator = new JwtValidator(_serializer, new UtcDateTimeProvider());
        private static readonly JwtBase64UrlEncoder _urlEncoder = new JwtBase64UrlEncoder();

        public static Dictionary<string, string> RequestContentAsDictionary(HttpRequestMessage request)
        {
            var formData = request.Content.ReadAsStringAsync().Result;
            var query = HttpUtility.ParseQueryString(formData);
            return query.AllKeys.ToDictionary(t => t, t => query[t]);
        }

        public static X509Certificate2 Certificate =>
            new X509Certificate2(
                "alice-virksomhetssertifikat.p12",
                "PASSWORD");

        public static X509Certificate2 CertificateOtherThanUsedForDecode =>
            new X509Certificate2(
                "bob-virksomhetssertifikat.p12",
                "PASSWORD");

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
            return DeserializedFieldInJwt<string>(request, jwtFieldName, field);
        }

        public static string EncodeJwt(Dictionary<string, object> header, object payload)
        {
            var algorithm = _factory.Create(JwtHashAlgorithm.RS256);
            var encoder = new JwtEncoder(algorithm, _serializer, _urlEncoder);
            return encoder.Encode(header, payload, "password");
        }

        public static T DeserializedFieldInJwt<T>(HttpRequestMessage request, string jwtFieldName, string field)
        {
            var content = RequestContentAsDictionary(request);
            var serializedJwt = content[jwtFieldName];
            var deserializedJwt = GetDeserializedJwt(serializedJwt);

            var jwtAsDictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(deserializedJwt);
            return (T)jwtAsDictionary[field];
        }

        private static string GetDeserializedJwt(string serializedJwt)
        {
            var decoder = new JwtDecoder(_serializer, _validator, _urlEncoder, _factory);
            return decoder.Decode(serializedJwt, "MustBeNonNullButValueDoesNotMatterForRS256", true);
        }
    }
}