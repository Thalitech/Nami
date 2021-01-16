using DSharpPlus.Entities;
using DSharpPlus.Lavalink;
using DSharpPlus.Net;
using GScraper;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
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
                RestEndpoint = new ConnectionEndpoint   { Hostname = new ResourceFile<string>()["lavalinkip", $"{IPAddress.Loopback}"], Port = 2336 },
                SocketEndpoint = new ConnectionEndpoint { Hostname = new ResourceFile<string>()["lavalinkip", $"{IPAddress.Loopback}"], Port = 2336 },
                Password = new ResourceFile<string>()["lavalinkpass", "dev_pass"]
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

        


        public static async Task<T> Generate<T>(GenType type, TrackInfo info)
        {
            switch (type)
            {
                case GenType.AuthorImageUrl:
                    var result = default(ImageResult);
                    using (var google = new GoogleScraper())
                    {
                        var _result = await google.GetImagesAsync(info.Author, 5, false);
                        if (_result.Count > 0) result = _result[new Random().Next(0, 5)];
                    }
                    return (T)(object)result.Link;
                case GenType.AlbumeImageUrl:
                    var albume = string.Empty;
                    new Thread(() =>
                    {
                        using (var process = new Process())
                        {
                            process.StartInfo.FileName = "cmd.exe";
                            var file = new FileInfo("Resources/youtube-dl.exe");
                            process.StartInfo.Arguments = $"/c call {file.FullName} --get-thumbnail {info.Uri.OriginalString}";
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
                            var file = new FileInfo("Resources/youtube-dl.exe");
                            process.StartInfo.Arguments = $"/c call {file.FullName} --get-description {info.Uri.OriginalString}";
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
    }
}