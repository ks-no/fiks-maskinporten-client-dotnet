using System;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Shouldly;
using Xunit;

namespace Ks.Fiks.Maskinporten.Client.Tests;

// JsonConvert.DefaultSettings is process-wide, so these tests must not run in parallel with other tests.
[CollectionDefinition(nameof(HostJsonSettingsCollection), DisableParallelization = true)]
public class HostJsonSettingsCollection
{
}

[Collection(nameof(HostJsonSettingsCollection))]
public class HostJsonSettingsTests
{
    private readonly MaskinportenClientFixture _fixture = new();

    [Fact]
    public async Task NonOkResponseIgnoresHostJsonSettings()
    {
        var sut = _fixture
            .WithStatusCode(HttpStatusCode.BadRequest)
            .WithResponseBody("{\"error\":\"invalid_grant\",\"error_description\":\"Invalid assertion\"}")
            .CreateSut();

        var previousSettings = JsonConvert.DefaultSettings;
        JsonConvert.DefaultSettings = () => new JsonSerializerSettings { Converters = { new ThrowOnReadConverter() } };
        try
        {
            var exception = await Assert.ThrowsAsync<UnexpectedResponseException>(
                    async () => await sut.GetAccessToken(_fixture.DefaultScopes).ConfigureAwait(false))
                .ConfigureAwait(false);

            exception.Message.ShouldContain("invalid_grant");
            exception.Message.ShouldContain("Invalid assertion");
        }
        finally
        {
            JsonConvert.DefaultSettings = previousSettings;
        }
    }

    // Throws on each read that uses the host settings. The test is about reads only, so CanWrite is false.
    private sealed class ThrowOnReadConverter : JsonConverter
    {
        public override bool CanWrite => false;

        public override bool CanConvert(Type objectType) => true;

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) =>
            throw new InvalidOperationException("The host converter read the JSON.");

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) =>
            throw new NotSupportedException();
    }
}
