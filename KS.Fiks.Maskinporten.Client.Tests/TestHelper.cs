using System;
using System.Collections.Generic;
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
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Ks.Fiks.Maskinporten.Client.Tests;

public static class TestHelper
{
    private static readonly ITestOutputHelper _testOutputHelper = new TestOutputHelper();
    private static readonly RSAlgorithmFactory _factory = new RSAlgorithmFactory(() => Certificate);
    private static readonly JsonNetSerializer _serializer = new JsonNetSerializer();
    private static readonly JwtValidator _validator = new JwtValidator(_serializer, new UtcDateTimeProvider());
    private static readonly JwtBase64UrlEncoder _urlEncoder = new JwtBase64UrlEncoder();
    private static readonly RSA _publicKey = RSA.Create();
    private static readonly RSA _privateKey = RSA.Create();

    public static RSA PublicKey => _publicKey;

    public static RSA PrivateKey => _privateKey;

    public static X509Certificate2 Certificate =>
        new X509Certificate2(
            "alice-virksomhetssertifikat.p12",
            "PASSWORD",
            X509KeyStorageFlags.EphemeralKeySet);

    public static X509Certificate2 CertificateOtherThanUsedForDecode =>
        new X509Certificate2(
            "bob-virksomhetssertifikat.p12",
            "PASSWORD",
            X509KeyStorageFlags.EphemeralKeySet);

    public static Dictionary<string, string> RequestContentAsDictionary(HttpRequestMessage request)
    {
        var formData = request.Content.ReadAsStringAsync().Result;
        var query = HttpUtility.ParseQueryString(formData);
        return query.AllKeys.ToDictionary(t => t, t => query[t]);
    }

    public static bool RequestContentIsJwt(HttpRequestMessage request, string jwtFieldName)
    {
        var content = RequestContentAsDictionary(request);
        var serializedJwt = content[jwtFieldName];

        try
        {
            var decodedJwt = GetDeserializedJwt(serializedJwt, _factory);
            return decodedJwt?.Length > 0;
        }
        catch (Exception ex)
        {
            _testOutputHelper.WriteLine("Could not decode JWT");
            return false;
        }
    }

    public static string DeserializedFieldInJwt(HttpRequestMessage request, string jwtFieldName, string field)
    {
        return DeserializedFieldInJwt(request, jwtFieldName, field, _factory);
    }

    public static string DeserializedFieldInJwt(HttpRequestMessage request, string jwtFieldName, string field, IAlgorithmFactory factory)
    {
        return DeserializedFieldInJwt<string>(request, jwtFieldName, field, factory);
    }

    public static string EncodeJwt(string keyId, Dictionary<string, object> claims)
    {
        return EncodeJwt(keyId, claims, _factory, new RS256Algorithm(Certificate));
    }

    public static string EncodeJwt(string keyId, Dictionary<string, object> claims, IAlgorithmFactory factory, IJwtAlgorithm algorithm)
    {

        var builder = new JwtBuilder()
            .WithAlgorithmFactory(factory)
            .WithAlgorithm(algorithm)
            .WithJsonSerializer(_serializer)
            .WithValidator(_validator)
            .WithSecret("passord")
            .AddHeader(HeaderName.KeyId, keyId);

        foreach (var (key, value) in claims)
        {
            builder.AddClaim(key, value);
        }

        return builder.Encode();
    }

    public static T DeserializedFieldInJwt<T>(HttpRequestMessage request, string jwtFieldName, string field, IAlgorithmFactory factory)
    {
        var content = RequestContentAsDictionary(request);
        var serializedJwt = content[jwtFieldName];
        var deserializedJwt = GetDeserializedJwt(serializedJwt, factory);

        var jwtAsDictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(deserializedJwt);
        return (T)jwtAsDictionary[field];
    }

    public static T DeserializedFieldInJwt<T>(HttpRequestMessage request, string jwtFieldName, string field)
    {
        return DeserializedFieldInJwt<T>(request, jwtFieldName, field, _factory);
    }

    private static string GetDeserializedJwt(string serializedJwt, IAlgorithmFactory factory)
    {
        var decoder = new JwtDecoder(_serializer, _validator, _urlEncoder, factory);

        return decoder.Decode(serializedJwt, "MustBeNonNullButValueDoesNotMatterForRS256", true);
    }

    public static string DeserializedHeadersInJwt(HttpRequestMessage request, string jwtFieldName, string field, IAlgorithmFactory factory)
    {
        var content = RequestContentAsDictionary(request);
        var serializedJwt = content[jwtFieldName];
        var decoder = new JwtDecoder(_serializer, _validator, _urlEncoder, factory);
        var decodedHeader = decoder.DecodeHeader(serializedJwt);

        var headersDictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(decodedHeader);
        return (string)headersDictionary[field];
    }

    public static bool JwtHeadersContainsField(HttpRequestMessage request, string jwtFieldName, string field, IAlgorithmFactory factory)
    {
        try
        {
            var content = RequestContentAsDictionary(request);
            if (!content.TryGetValue(jwtFieldName, out var serializedJwt))
            {
                return false;
            }

            var decoder = new JwtDecoder(_serializer, _validator, _urlEncoder, factory);
            var decodedHeader = decoder.DecodeHeader(serializedJwt);

            var headersDictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(decodedHeader);
            return headersDictionary.ContainsKey(field);
        }
        catch (Exception)
        {
            _testOutputHelper.WriteLine("Error checking JWT headers");
            return false;
        }
    }
}