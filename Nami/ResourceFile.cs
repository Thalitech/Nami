using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace Nami
{
    public class ResourceFile<T>
    {
        [JsonProperty("data")]
        public Dictionary<string, object> data = new Dictionary<string, object>()
        {
            { "ftr",  true},
            { "prefixes", new [] { "?", "-",  "*" } },
        };
        public T this[string key, T _default = default, bool encrypt = false]
        {
            get
            {
                var type = typeof(T);

                key = key.ToLower();
                key = key.Replace(' ', '-');

                Load();
                if (data.ContainsKey(key))
                {
                    if (encrypt && typeof(T).Equals(typeof(string)))
                    {
                        var base64EncodedBytes = Convert.FromBase64String((string)data[key]);
                        return (T)(object)Encoding.UTF8.GetString(base64EncodedBytes);
                    }
                    else
                    {
                        if (type.IsArray)
                        {
                            var oldArray = (JArray)data[key];
                            var newArray = new List<string>();
                            foreach (var item in oldArray)
                                newArray.Add(item.Value<string>());

                            return (T)(object)newArray.ToArray();
                        }
                        return (T)data[key];
                    }
                }    
                if(_default != null)
                {
                    return _default;
                }
                return default;
            }
            set
            {
                if (value == null) return;
                Load();
                key = key.ToLower();
                key = key.Replace(' ', '-');

                if(encrypt && typeof(T).Equals(typeof(string)))
                {
                        var plainTextBytes = Encoding.UTF8.GetBytes((string)(object)value);
                        var _value =  (T)(object)Convert.ToBase64String(plainTextBytes);

                    if (data.ContainsKey(key))
                        data[key] = _value;
                    if (!data.ContainsKey(key))
                        data.Add(key, _value);
                    Save();
                    return;
                }


                if (data.ContainsKey(key))
                    data[key] = value;
                if (!data.ContainsKey(key))
                    data.Add(key, value);
                Save();
            }
        }

        private void Load()
        {
            var resoruceFile = "Resources/resources.r";
            Directory.CreateDirectory("Resources");
            if (!File.Exists(resoruceFile)) return;
            var json = File.ReadAllText(resoruceFile, Encoding.UTF8);
            data = (Dictionary<string, object>)JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
        }
        private void Save()
        {
            var resoruceFile = "Resources/resources.r";
            Directory.CreateDirectory("Resources");
            var json = JsonConvert.SerializeObject(data, Formatting.Indented);
            File.WriteAllText(resoruceFile, json, Encoding.UTF8);
        }
    }
}
