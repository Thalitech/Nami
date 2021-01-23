using DSharpPlus.Lavalink;
using DSharpPlus.Net;
using GScraper;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Nami
{
    enum GenType
    {
        Description,
        AlbumeImageUrl,
        AuthorImageUrl,
    }
    internal class Dispatcher
    {


        internal static async Task ConnectLavalinkAsync(DSharpPlus.DiscordClient discord)
        {
            var lavalink = discord.UseLavalink();
            var lavalinkConfig = new LavalinkConfiguration
            {
                RestEndpoint = new ConnectionEndpoint { Hostname = await AssetDatabase.Get<string>(ValueType.LavalinkIP), Port = 2336 },
                SocketEndpoint = new ConnectionEndpoint { Hostname = await AssetDatabase.Get<string>(ValueType.LavalinkIP), Port = 2336 },
                Password = await AssetDatabase.Get<string>(ValueType.LavalinkPassword, true)
            };
            await lavalink.ConnectAsync(lavalinkConfig);
        }

        internal static string Request(string key)
        {
            Console.Write($"You have not setup your {key}.\n Enter your {key}: ");
            var result = Console.ReadLine();
            if (!string.IsNullOrEmpty(result)) return result;
            return null;
        }

        


        public static async Task<T> Generate<T>(GenType type, NamiTrack info)
        {
            switch (type)
            {
                case GenType.AuthorImageUrl:
                    var result = default(ImageResult);
                    using (var google = new GoogleScraper())
                    {
                        var _result = await google.GetImagesAsync(info.Author, 5, false);
                        if (_result.Count > 0) result = _result[new Random().Next(0, 3)];
                    }
                    return (T)(object)result.Link;
                case GenType.AlbumeImageUrl:
                    var albume = string.Empty;
                    new Thread(() =>
                    {
                        using (var process = new Process())
                        {
                            process.StartInfo.FileName = "cmd.exe";
                            var dir = Directory.CreateDirectory("Resources");
                            var file = new FileInfo(Path.Combine(dir.FullName, "youtube-dl.exe"));
                            if(!file.Exists)
                            {
                                Console.WriteLine("youtube-dl.exe is not in project!");
                                albume = default;
                                return;
                            }
                            process.StartInfo.Arguments = $"/c call {file.FullName} --get-thumbnail {info.Uri}";
                            process.StartInfo.WorkingDirectory = new FileInfo("Resources").Directory.FullName;
                            process.StartInfo.UseShellExecute = false;
                            process.StartInfo.RedirectStandardOutput = true;
                            process.Start();
                            // Synchronously read the standard output of the spawned process.
                            StreamReader reader = process.StandardOutput;
                            string output = reader.ReadToEnd();
                            // Write the redirected output to this application's window.
                            Console.WriteLine(output);
                            process.WaitForExit();
                            albume = output;
                        }
                    }).Start();
                    while (string.IsNullOrEmpty(albume)) { }
                    return (T)(object)albume;
                case GenType.Description:
                    var desc = string.Empty;
                    new Thread(() => 
                    {
                        using(var process = new Process())
                        {
                            process.StartInfo.FileName = "cmd.exe";
                            var dir = Directory.CreateDirectory("Resources");
                            var file = new FileInfo(Path.Combine(dir.FullName, "youtube-dl.exe"));
                            if (!file.Exists)
                            {
                                Console.WriteLine("youtube-dl.exe is not in project!");
                                albume = default;
                                return;
                            }
                            process.StartInfo.Arguments = $"/c call {file.FullName} --get-description {info.Uri}";
                            process.StartInfo.WorkingDirectory = new FileInfo("Resources").Directory.FullName;
                            process.StartInfo.UseShellExecute = false;
                            process.StartInfo.RedirectStandardOutput = true;
                            process.Start();
                            // Synchronously read the standard output of the spawned process.
                            StreamReader reader = process.StandardOutput;
                            string output = reader.ReadToEnd();
                            // Write the redirected output to this application's window.
                            Console.WriteLine(output);
                            process.WaitForExit();
                            desc = output;
                        }
                    }).Start();
                    while (string.IsNullOrEmpty(desc)) { }
                    return (T)(object)desc;

            }
            return default;
        }

        internal static void Save<T>(T value)
        {
            if (typeof(T).Equals(typeof(NamiTrack)))
            {
                var track = (NamiTrack)(object)value;

                var json = JsonConvert.SerializeObject(track, Formatting.Indented);
                var dir = Directory.CreateDirectory("Resources/SongLibrary/");
                var file = new FileInfo(Path.Combine(dir.FullName, $"{track.Title}.json"));
                if (!file.Exists)
                    File.WriteAllText(file.FullName, json);
            }
        }
        internal static T Load<T>(string title)
        {
            if (typeof(T).Equals(typeof(NamiTrack)))
            {
                var dir = Directory.CreateDirectory("Resources/SongLibrary/");
                var file = new FileInfo(Path.Combine(dir.FullName, $"{title}.json"));
                if (!file.Exists) return default;
                var json = File.ReadAllText(file.FullName);
                var track = JsonConvert.DeserializeObject<NamiTrack>(json);
                if (track != null)
                    return (T)(object)track;
            }
            return default;
        }
    }
}