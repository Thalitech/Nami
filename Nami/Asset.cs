using Newtonsoft.Json;

namespace Nami
{
    public class Asset<T1, T2>
    {
        [JsonProperty("Key")]
        public T1 key { get; internal set; }
        [JsonProperty("Value")]
        public T2 value { get; internal set; }

        public Asset(T1 key, T2 value)
        {
            this.key = key;
            this.value = value;
        }
    }
}
