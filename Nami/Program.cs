﻿using DSharpPlus;
using DSharpPlus.CommandsNext;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Nami
{
    class Program : IDisposable
    {
        static Task Main(string[] args)
        {
            Console.Title = Assembly.GetExecutingAssembly().GetName().Name;
            
            if (new ResourceFile<bool>()["ftr"] || string.IsNullOrEmpty(new ResourceFile<string>()["token", encrypt: true]))
                new ResourceFile<string>()["token", encrypt: true] = Dispatcher.Request("token");
            if (new ResourceFile<bool>()["ftr"] || string.IsNullOrEmpty(new ResourceFile<string>()["lavalink_ip"]))
                new ResourceFile<string>()["lavalink_ip"] = Dispatcher.Request("lavalink_ip");
            if (new ResourceFile<bool>()["ftr"] || string.IsNullOrEmpty(new ResourceFile<string>()["lavalink_pass", encrypt: true]))
                new ResourceFile<string>()["lavalink_pass", encrypt: true] = Dispatcher.Request("lavalink_pass");


            if (new ResourceFile<bool>()["ftr"] || string.IsNullOrEmpty(new ResourceFile<string>()["reddit_appid", encrypt: true]))
                new ResourceFile<string>()["reddit_appid", encrypt: true] = Dispatcher.Request("reddit_appid");
            if (new ResourceFile<bool>()["ftr"] || string.IsNullOrEmpty(new ResourceFile<string>()["reddit_secret", encrypt: true]))
                new ResourceFile<string>()["reddit_secret", encrypt: true] = Dispatcher.Request("reddit_secret");
            if (new ResourceFile<bool>()["ftr"])
            new ResourceFile<bool>()["ftr"] = false;
            return new Program().MainAsync();

        }

        public static Program instance;
        public Program()
        {
            if(instance == null)
            {
                instance = this;
            }
            else if(instance != this)
            {
                this.Dispose();
            }
        }

        public enum CommandCode { Running, stop, Reset, }
        public CommandCode commandCode;
        private async Task MainAsync()
        {
            AssetDatabase.Init();
            if (commandCode == CommandCode.stop)
            {
                Console.ReadLine();
                return;
            }
        initialize:
            Console.Clear();
            using (var reddit = new RedditManager()) { }
            // Configurations
            var discordConfig = new DiscordConfiguration()
            {
                Token = new ResourceFile<string>()["token", "your-discord-token", true],
                TokenType = TokenType.Bot,
                MinimumLogLevel = LogLevel.Debug,
                LogTimestampFormat = "MMM dd yyyy - hh:mm:ss tt",
                Intents = DiscordIntents.DirectMessageReactions
                     | DiscordIntents.DirectMessages
                     | DiscordIntents.GuildBans
                     | DiscordIntents.GuildEmojis
                     | DiscordIntents.GuildInvites
                     | DiscordIntents.GuildMembers
                     | DiscordIntents.GuildMessages
                     | DiscordIntents.Guilds
                     | DiscordIntents.GuildVoiceStates
                     | DiscordIntents.GuildWebhooks
                     | DiscordIntents.GuildPresences,

            };
            var commandConfig = new CommandsNextConfiguration()
            {
                StringPrefixes = new ResourceFile<string[]>()["prefixes", new[] { "?", "-", "*" }],
                DmHelp = true,
                EnableMentionPrefix = true,
            };
            var discord = new DiscordClient(discordConfig);
            var commands = discord.UseCommandsNext(commandConfig);
            discord.GuildAvailable += Events.OnGuildsAvailable;
            discord.GuildMemberAdded += Events.OnMemberAdded;
            discord.GuildMemberRemoved += Events.OnMemberRemoved;
            commands.RegisterCommands<CommandHub>();
            if (AssetDatabase.compiledAssembly != null)
                commands.RegisterCommands(AssetDatabase.compiledAssembly);

            await discord.ConnectAsync();
            await Dispatcher.ConnectLavalinkAsync(discord);
            commandCode = CommandCode.Running;
            while (commandCode != CommandCode.stop && commandCode != CommandCode.Reset) { };
            await discord.DisconnectAsync();
            if (commandCode == CommandCode.Reset) 
                goto initialize;

        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
