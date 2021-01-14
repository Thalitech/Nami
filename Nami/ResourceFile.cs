using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Nami
{
    public class ResourceFile<T>
    {
        public Dictionary<string, object> data = new Dictionary<string, object>()
        {
            { "ftr",  true},
            { "prefixes", new[] { "?", "-",  "*" } },
        };
        public T this[string key, T _default = default]
        {
            get
            {
                key = key.ToLower();
                key = key.Replace(' ', '-');

                Load();
                if (data.ContainsKey(key)) return (T)data[key];
                return _default != null ? _default : default;
            }
            set
            {
                Load();
                key = key.ToLower();
                key = key.Replace(' ', '-');

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
            var bf = new BinaryFormatter();
            if (!File.Exists(resoruceFile)) return;
            using (var fs = new FileStream(resoruceFile, FileMode.Open, FileAccess.Read))
            {
                data = (Dictionary<string, object>)bf.Deserialize(fs);
            }
        }
        private void Save()
        {
            var resoruceFile = "Resources/resources.r";
            Directory.CreateDirectory("Resources");
            var bf = new BinaryFormatter();
            using (var fs = new FileStream(resoruceFile, FileMode.Create, FileAccess.Write))
            {
                bf.Serialize(fs, data);
            }
        }
    }
}
