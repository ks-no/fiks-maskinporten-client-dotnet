using System.Collections.Generic;
using System.Net.Http;
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
    }
}