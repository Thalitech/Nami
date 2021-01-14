using DSharpPlus;
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
                TokenType = TokenType.Bot
            });
        }
    }
}
