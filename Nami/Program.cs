using DSharpPlus;
using DSharpPlus.CommandsNext;
using Microsoft.Extensions.Logging;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Nami
{
    class Program
    {
        static Task Main(string[] args)
        {
            Console.Title = Assembly.GetExecutingAssembly().GetName().Name;
            if(new ResourceFile<bool>()["ftr"])
            {
                new ResourceFile<string>()["token"] = Dispatcher.Request("token");
                new ResourceFile<string>()["lavalinkip"] = Dispatcher.Request("lavalinkip");
                new ResourceFile<string>()["lavalinkpass"] = Dispatcher.Request("lavalinkpass");

                new ResourceFile<bool>()["ftr"] = false;
            }
            return new Program().MainAsync();
        }


        private async Task MainAsync()
        {
            // Configurations
            var discordConfig = new DiscordConfiguration()
            {
                Token = new ResourceFile<string>()["token", "your-discord-token"],
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
                StringPrefixes = new ResourceFile<string[]>()["prefixes", new[] { "?", "-" }],
                DmHelp = true,
                EnableMentionPrefix = true,
            };

            var discord = new DiscordClient(discordConfig);
            var commands = discord.UseCommandsNext(commandConfig);
            discord.GuildAvailable += Events.OnGuildsAvailable;
            discord.GuildMemberAdded += Events.OnMemberAdded;
            discord.GuildMemberRemoved += Events.OnMemberRemoved;
            commands.RegisterCommands<CommandHub>();
            await discord.ConnectAsync();
            await Dispatcher.ConnectLavalinkAsync(discord);
            await Task.Delay(-1);
        }
    }
}
