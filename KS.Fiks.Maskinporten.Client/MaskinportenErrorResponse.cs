using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace Ks.Fiks.Maskinporten.Client
{
    [JsonObject(MemberSerialization.OptIn)]
    [SuppressMessage("Performance", "CA1812", Justification = "Newtonsoft.Json creates the instances")]
    internal class MaskinportenErrorResponse
    {
        [JsonProperty("error")]
        public string Error { get; private set; }

        [JsonProperty("error_description")]
        public string ErrorDescription { get; private set; }
    }
}
