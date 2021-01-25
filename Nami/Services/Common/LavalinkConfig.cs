using Newtonsoft.Json;

namespace Nami.Services.Common
{
    public sealed class LavalinkConfig
    {
        [JsonProperty("hostname")]
        public string Hostname { get; set; } = "localhost";

        [JsonProperty("port")]
        public int Port { get; set; } = 2336;

        [JsonProperty("password")]
        public string Password { get; set; } = "dev_pass";

        [JsonProperty("retry")]
        public int RetryAmount { get; set; } = 5;
    }
}
