using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Nami
{
    class Program
    {
        public static Bot bot;
        static Task Main(string[] args)
        {
            Console.Title = Assembly.GetExecutingAssembly().GetName().Name;
            return new Program().Run();
        }

        public async Task Run()
        {
            bot = new Bot();
            if (await AssetDatabase.Get<bool>(ValueType.FTR) || string.IsNullOrEmpty(await AssetDatabase.Get<string>(ValueType.Token, true)))
            {
                Console.Write("Please enter your discord Bot Token: ");
                await AssetDatabase.Set(ValueType.Token, Console.ReadLine(), true);
            }
            if (await AssetDatabase.Get<bool>(ValueType.FTR) || await AssetDatabase.Get<bool>(ValueType.UseLavalink))
            {
                if (await AssetDatabase.Get<bool>(ValueType.FTR) || string.IsNullOrEmpty(await AssetDatabase.Get<string>(ValueType.LavalinkIP)))
                {
                    Console.Write("Please enter your Lavalink IP Address: ");
                    await AssetDatabase.Set(ValueType.LavalinkIP, Console.ReadLine());
                }
                if (await AssetDatabase.Get<bool>(ValueType.FTR) || string.IsNullOrEmpty(await AssetDatabase.Get<string>(ValueType.LavalinkPassword)))
                {
                    Console.Write("Please enter your Lavalink Password: ");
                    await AssetDatabase.Set(ValueType.LavalinkPassword, Console.ReadLine(), true);
                }
            }
            if (await AssetDatabase.Get<bool>(ValueType.FTR) || await AssetDatabase.Get<bool>(ValueType.UseReddit))
            {
                if (await AssetDatabase.Get<bool>(ValueType.FTR) || string.IsNullOrEmpty(await AssetDatabase.Get<string>(ValueType.RedditAppID, true)))
                {
                    Console.Write("Please enter your Reddit App ID: ");
                    await AssetDatabase.Set(ValueType.RedditAppID, Console.ReadLine(), true);
                }
                if (await AssetDatabase.Get<bool>(ValueType.FTR) || string.IsNullOrEmpty(await AssetDatabase.Get<string>(ValueType.RedditSecret, true)))
                {
                    Console.Write("Please enter your Reddit App Secret: ");
                    await AssetDatabase.Set(ValueType.RedditSecret, Console.ReadLine(), true);
                }
            }
            if (await AssetDatabase.Get<bool>(ValueType.FTR)) await AssetDatabase.Set(ValueType.FTR, false);
            await bot.RunAsync();
        }
    }
}
