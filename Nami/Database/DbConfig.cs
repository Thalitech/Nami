using Newtonsoft.Json;

namespace Nami.Database
{
    public sealed class DbConfig
    {
        [JsonProperty("database")]
        public string DatabaseName { get; set; } = "Nami-db";

        [JsonProperty("provider")]
        public DbProvider Provider { get; set; } = DbProvider.Sqlite;

        [JsonProperty("hostname")]
        public string Hostname { get; set; } = "localhost";

        [JsonProperty("username")]
        public string Username { get; set; } = "dev_user";

        [JsonProperty("password")]
        public string Password { get; set; } = "dev_pass";

        [JsonProperty("port")]
        public int Port { get; set; } = 5432;

    }
}