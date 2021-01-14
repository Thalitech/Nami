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
        static void Main(string[] args)
        {
            Console.Title = Assembly.GetExecutingAssembly().GetName().Name;
            if(new ResourceFile<bool>()["ftr"])
            {
                Console.WriteLine("You have not setup your discord token.");
                Console.Write("Discord Bot Token: ");
                var token = Console.ReadLine();
                if(!string.IsNullOrEmpty(token))
                {
                    new ResourceFile<string>()["token"] = token;
                }
                new ResourceFile<bool>()["ftr"] = false;
            }    

            new Program().MainAsync().GetAwaiter().GetResult();
        }


        private async Task MainAsync()
        {
            var discord = new DiscordClient(new DiscordConfiguration()
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
                    | DiscordIntents.GuildWebhooks,

            });
            var commands = discord.UseCommandsNext(new CommandsNextConfiguration()
            {
                StringPrefixes = new ResourceFile<string[]>()["prefixes", new[] { "!" }],
                DmHelp = true,
                EnableMentionPrefix =true,
            });
            discord.GuildAvailable += Events.OnGuildsAvailable;
            discord.GuildMemberAdded += Events.OnMemberAdded;
            discord.GuildMemberRemoved += Events.OnMemberRemoved;
            commands.RegisterCommands<CommandDefault>();

            await discord.ConnectAsync();
            await Task.Delay(-1);
        }
    }
}
