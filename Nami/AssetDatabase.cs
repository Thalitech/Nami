using Microsoft.CodeDom.Providers.DotNetCompilerPlatform;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Nami
{
    class AssetDatabase
    {
        internal static Assembly compiledAssembly;
        [JsonProperty("data")]
        private static List<Asset<string, object>> data = DefaultData;
        private static FileSystemWatcher system;

        public static List<Asset<string, object>> DefaultData 
        { 
            get
            {
                return new List<Asset<string, object>>()
                {

                    new Asset<string, object>(ValueType.UseLavalink.ToString(), true),
                    new Asset<string, object>(ValueType.LavalinkIP.ToString(), "127.0.0.1"),
                    new Asset<string, object>(ValueType.LavalinkPassword.ToString(), Convert.ToBase64String(Encoding.UTF8.GetBytes("dev_pass"))),
                    new Asset<string, object>(ValueType.UseReddit.ToString(), false),
                    new Asset<string, object>(ValueType.Prefixes.ToString(), new[] { "?", "-", "*" }),
                };
            }
        }

        public static async Task<T> Get<T>(string key = default, bool encryped = false)
        {
            var type = typeof(T);
            T result = default;
            await LoadData();
            if (data.Find(x => x.key == key) != null)
            {
                var index = data.IndexOf(data.Find(x => x.key == key));
                if (encryped)
                {
                    var base64EncodedBytes = Convert.FromBase64String((string)data[index].value);
                    return (T)(object)Encoding.UTF8.GetString(base64EncodedBytes);
                }
                else
                {
                    if (type.IsArray)
                    {
                        var oldArray = (JArray)data[index].value;
                        var newArray = new List<string>();
                        foreach (var item in oldArray)
                            newArray.Add(item.Value<string>());
                        return (T)(object)newArray.ToArray();
                    }
                    return (T)data[index].value;
                }
            }

            return await Task.FromResult<T>(result);
        }
        public static async Task<T> Get<T>(ValueType key = default, bool encryped = false)
        {
            var type = typeof(T);
            T result = default;
            await LoadData();
            if (data.Find(x => x.key == key.ToString()) != null)
            {
                var index = data.IndexOf(data.Find(x => x.key == key.ToString()));
                if (encryped)
                {
                    var base64EncodedBytes = Convert.FromBase64String((string)data[index].value);
                    return (T)(object)Encoding.UTF8.GetString(base64EncodedBytes);
                }
                else
                {
                    if (type.IsArray)
                    {
                        var oldArray = (JArray)data[index].value;
                        var newArray = new List<string>();
                        foreach (var item in oldArray)
                            newArray.Add(item.Value<string>());
                        return (T)(object)newArray.ToArray();
                    }
                    return (T)data[index].value;
                }
            }

            return await Task.FromResult<T>(result);
        }
        public static async Task Set<T>(object key, T value = default, bool encryped = false)
        {
            if (value == null) return;
            await LoadData();
            if (data.Find(x => x.key == key.ToString()) != null)
            {
                var index = data.IndexOf(data.Find(x => x.key == key.ToString()));
                if (encryped)
                {
                    var plainTextBytes = Encoding.UTF8.GetBytes((string)(object)value);
                    var _value = (T)(object)Convert.ToBase64String(plainTextBytes);
                    data[index].value = _value;
                }
                else
                    data[index].value = value;
            }
            else
            {
                if(encryped)
                {
                    data.Add(new Asset<string, object>(key.ToString(), Convert.ToBase64String(Encoding.UTF8.GetBytes((string)(object)value))));
                }
                else 
                    data.Add(new Asset<string, object>(key.ToString(), value));
            }
            await SaveData();
        }

        private static async Task LoadData()
        {
           var file = Path.Combine(Directory.CreateDirectory("Resources").FullName, "Database.json");
            if(!File.Exists(file))
            {
                data = DefaultData;
                return;
            }
            var json = File.ReadAllText(file);
            data = JsonConvert.DeserializeObject<List<Asset<string, object>>>(json);
            if (data == null) data = DefaultData;
            await Task.CompletedTask;
        }
        private static async Task SaveData() 
        {   
            var json = JsonConvert.SerializeObject(data, Formatting.Indented);
            var file = Path.Combine(Directory.CreateDirectory("Resources").FullName, "Database.json");
            File.WriteAllText(file, json);
            await Task.CompletedTask;
        }

        internal static void Initialize()
        {
            system = new FileSystemWatcher();
            system.Path = Directory.CreateDirectory("Resources/UserCommands").FullName;
            system.NotifyFilter = NotifyFilters.LastAccess
                               | NotifyFilters.LastWrite
                               | NotifyFilters.FileName
                               | NotifyFilters.DirectoryName;
            system.Filter = "*.cs";
            system.Changed += (a, b) => Recompile();
            system.Created += (a, b) => Recompile();
            system.Deleted += (a, b) => Recompile();
            system.EnableRaisingEvents = true;
            Recompile();
        }

        private static void Recompile()
        {
            compiledAssembly = null;
            var files = Directory.GetFiles(system.Path, "*.cs", SearchOption.AllDirectories);
            if (files.Length <= 0) return;

            CSharpCodeProvider codeProvider = new CSharpCodeProvider();
            CompilerParameters cpParams = new CompilerParameters();
            cpParams.GenerateExecutable = false;
            cpParams.GenerateInMemory = true;
            cpParams.IncludeDebugInformation = true;
            cpParams.TreatWarningsAsErrors = false;
            cpParams.ReferencedAssemblies.Add(Assembly.GetExecutingAssembly().Location);
            var dir = new FileInfo(Assembly.GetExecutingAssembly().Location).Directory.FullName;
            cpParams.ReferencedAssemblies.Add("System.dll");
            cpParams.ReferencedAssemblies.Add("netstandard.dll");
            cpParams.ReferencedAssemblies.AddRange(typeof(Program).Assembly.GetReferencedAssemblies().Select(x => $"{ x.Name}.dll").ToArray());
            try
            {
                CompilerResults results = codeProvider.CompileAssemblyFromFile(cpParams, files);
                if (results.Errors.HasErrors)
                {
                    Console.WriteLine("|=== USER CODE ERROR ===|");
                    foreach (var error in results.Errors)
                    {
                        var errorText = error.ToString().Replace(" : ", "\n");
                        Console.WriteLine(errorText);
                    }
                    Program.bot.commandCode = Bot.CommandCode.stop;
                    return;
                }
                compiledAssembly = results.CompiledAssembly;
                Program.bot.commandCode = Bot.CommandCode.Reset;
                Console.WriteLine("|=== USER CODE COMPILED SUCCESSFULLY ===>");
                Thread.Sleep(3000);
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine("|=== USER CODE ERROR ===|");
                Console.WriteLine($"Unable to complile code\n{ex}");
                compiledAssembly = null;
            }
        }
    }
}
