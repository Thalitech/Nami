using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Lavalink;
using GScraper;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Nami
{
    public class MusicCommands : BaseCommandModule
    {
        [Command("join")]
        [Description("Has the bot join the voice channel that the user that ran the command is in. if user is not in voice channel it will join a channel that is name Music.")]
        public async Task Join(CommandContext ctx)
        {
            var lava = ctx.Client.GetLavalink();
            if (!lava.ConnectedNodes.Any())
            {
                await ctx.RespondAsync("The Lavalink connection is not established");
                return;
            }
            var node = lava.ConnectedNodes.Values.First();
            var conn = node.GetGuildConnection(ctx.Guild);
            if (conn == null)
            {
                var channel = default(DiscordChannel);
                if (ctx.Member.VoiceState != null)
                {
                    channel = ctx.Member.VoiceState.Channel;
                    if (channel.Type != ChannelType.Voice)
                    {
                        await ctx.RespondAsync("Not a valid voice channel.");
                        return;
                    }
                    conn = await node.ConnectAsync(channel);
                    await ctx.RespondAsync($"Connected!\nNow bound to {ctx.Channel.Mention} Text Channel and {channel.Mention} Voice Channel");
                }
                else return;
                // Create Music Instacne  for current guild
                var player = MusicPlayer.Connect(conn, ctx.Guild, ctx, channel, ctx.Channel);
                conn.PlaybackStarted += (a, b) => player.PlaybackStarted(a, new TrackEventArgs(b.Player, b.Track));
                conn.PlayerUpdated += (a, b) => player.PlaybackUpdated(a, new TrackEventArgs(b.Player));
                conn.PlaybackFinished += (a, b) => player.PlaybackFinished(a, new TrackEventArgs(b.Player, b.Track, (int)b.Reason));
            }
        }

        [Command("leave")]
        [Description("Has the bot leave the voice channel that it was currently in.")]
        public async Task Leave(CommandContext ctx)
        {
            if (ctx.Client.GetExtension<LavalinkExtension>() == null) return;

            var lava = ctx.Client.GetLavalink();
            if (!lava.ConnectedNodes.Any())
            {
                await ctx.RespondAsync("The Lavalink connection is not established");
                return;
            }
            var node = lava.ConnectedNodes.Values.First();
            var conn = node.GetGuildConnection(ctx.Guild);
            if (conn == null)
            {
                await ctx.RespondAsync("Lavalink is not connected.");
                return;
            }
            await conn.DisconnectAsync();
            await ctx.RespondAsync($"Disconnected!");
            MusicPlayer.Disconnect(ctx.Guild);
        }

        [Command("play")]
        [Description("This is the music play command, this is also used to add songs to the queue.")]
        public async Task Play(CommandContext ctx, [Description("The url or search query to be played")][RemainingText] string query)
        {
            if (ctx.Client.GetExtension<LavalinkExtension>() == null) return;

            await Join(ctx);
            if (MusicPlayer.Find(ctx.Guild).TextCh != ctx.Channel) { await ctx.RespondAsync($"You are not int the right text channel you must be in {MusicPlayer.Find(ctx.Guild).TextCh.Mention} or unbind the bot."); return; }

            var loadResult = await ctx.Client.GetLavalink().ConnectedNodes.Values.First().Rest.GetTracksAsync(query);
            if (loadResult.LoadResultType == LavalinkLoadResultType.LoadFailed || loadResult.LoadResultType == LavalinkLoadResultType.NoMatches)
            {
                await ctx.RespondAsync($"Track search failed for {query}.");
                return;
            }
            var track = loadResult.Tracks.First();


            var result = Dispatcher.Load<NamiTrack>(track.Title);
            await MusicPlayer.Find(ctx.Guild).Play(ctx, result != null ? result : track);
        }

        [Command("pause")]
        [Description("Pause the playback of the music player.")]
        public async Task Pause(CommandContext ctx)
        {

            if (MusicPlayer.Find(ctx.Guild).TextCh != ctx.Channel) { await ctx.RespondAsync($"You are not int the right text channel you must be in {MusicPlayer.Find(ctx.Guild).TextCh.Mention} or unbind the bot."); return; }

            if (ctx.Client.GetExtension<LavalinkExtension>() == null) return;
            await Join(ctx);
            var conn = ctx.Client.GetLavalink().ConnectedNodes.Values.First().GetGuildConnection(ctx.Guild);
            if (conn.CurrentState.CurrentTrack == null)
            {
                await ctx.RespondAsync("There are no tracks loaded.");
                return;
            }
            await MusicPlayer.Find(ctx.Guild).Pause();
        }

        [Command("Resume")]
        [Description("Resume the playback of the music player.")]
        public async Task Resume(CommandContext ctx)
        {
            if (MusicPlayer.Find(ctx.Guild).TextCh != ctx.Channel) { await ctx.RespondAsync($"You are not int the right text channel you must be in {MusicPlayer.Find(ctx.Guild).TextCh.Mention} or unbind the bot."); return; }
            if (ctx.Client.GetExtension<LavalinkExtension>() == null) return;
            await Join(ctx);
            var conn = ctx.Client.GetLavalink().ConnectedNodes.Values.First().GetGuildConnection(ctx.Guild);
            if (conn.CurrentState.CurrentTrack == null)
            {
                await ctx.RespondAsync("There are no tracks loaded.");
                return;
            }
            await MusicPlayer.Find(ctx.Guild).Resume();
        }

        [Command("stop")]
        [Description("Stop the playback of the music player")]
        public async Task Stop(CommandContext ctx)
        {
            if (MusicPlayer.Find(ctx.Guild).TextCh != ctx.Channel) { await ctx.RespondAsync($"You are not int the right text channel you must be in {MusicPlayer.Find(ctx.Guild).TextCh.Mention} or unbind the bot."); return; }
            if (ctx.Client.GetExtension<LavalinkExtension>() == null) return;
            await Join(ctx);
            var conn = ctx.Client.GetLavalink().ConnectedNodes.Values.First().GetGuildConnection(ctx.Guild);
            if (conn.CurrentState.CurrentTrack == null)
            {
                await ctx.RespondAsync("There are no tracks loaded.");
                return;
            }
            await MusicPlayer.Find(ctx.Guild).Stop();
        }

        [Command("next")]
        [Description("Stop the playback of the music player, and plays the next song in the queue")]
        public async Task Next(CommandContext ctx)
        {
            if (MusicPlayer.Find(ctx.Guild).TextCh != ctx.Channel) { await ctx.RespondAsync($"You are not int the right text channel you must be in {MusicPlayer.Find(ctx.Guild).TextCh.Mention} or unbind the bot."); return; }
            if (ctx.Client.GetExtension<LavalinkExtension>() == null) return;
            await Join(ctx);
            var conn = ctx.Client.GetLavalink().ConnectedNodes.Values.First().GetGuildConnection(ctx.Guild);
            if (conn.CurrentState.CurrentTrack == null)
            {
                await ctx.RespondAsync("There are no tracks loaded.");
                return;
            }
            await MusicPlayer.Find(ctx.Guild).Next();
        }

        [Command("previous")]
        [Description("Stop the playback of the music player, and plays the previous song in the queue")]
        public async Task Previous(CommandContext ctx)
        {
            if (MusicPlayer.Find(ctx.Guild).TextCh != ctx.Channel) { await ctx.RespondAsync($"You are not int the right text channel you must be in {MusicPlayer.Find(ctx.Guild).TextCh.Mention} or unbind the bot."); return; }
            if (ctx.Client.GetExtension<LavalinkExtension>() == null) return;
            await Join(ctx);
            var conn = ctx.Client.GetLavalink().ConnectedNodes.Values.First().GetGuildConnection(ctx.Guild);
            if (conn.CurrentState.CurrentTrack == null)
            {
                await ctx.RespondAsync("There are no tracks loaded.");
                return;
            }
            await MusicPlayer.Find(ctx.Guild).Previous();
        }

        [Command("clear")]
        [Description("Stop the playback of the music player, and clears the queue")]
        public async Task Clear(CommandContext ctx)
        {
            if (MusicPlayer.Find(ctx.Guild).TextCh != ctx.Channel) { await ctx.RespondAsync($"You are not int the right text channel you must be in {MusicPlayer.Find(ctx.Guild).TextCh.Mention} or unbind the bot."); return; }
            if (ctx.Client.GetExtension<LavalinkExtension>() == null) return;
            await Join(ctx);
            var conn = ctx.Client.GetLavalink().ConnectedNodes.Values.First().GetGuildConnection(ctx.Guild);
            if (conn.CurrentState.CurrentTrack == null)
            {
                await ctx.RespondAsync("There are no tracks loaded.");
                return;
            }
            await MusicPlayer.Find(ctx.Guild).Clear();
        }

        [Command("repeat")]
        [Description("Repeat the que, or one song in the queue.")]
        public async Task Repeat(CommandContext ctx, [Description("The repeat mode `one`, `all`, or `none`")] string mode)
        {
            if (MusicPlayer.Find(ctx.Guild).TextCh != ctx.Channel) { await ctx.RespondAsync($"You are not int the right text channel you must be in {MusicPlayer.Find(ctx.Guild).TextCh.Mention} or unbind the bot."); return; }
            if (ctx.Client.GetExtension<LavalinkExtension>() == null) return;
            await Join(ctx);
            var conn = ctx.Client.GetLavalink().ConnectedNodes.Values.First().GetGuildConnection(ctx.Guild);
            if (conn.CurrentState.CurrentTrack == null)
            {
                await ctx.RespondAsync("There are no tracks loaded.");
                return;
            }

            if (Enum.TryParse<RepeatMode>(mode, true, out RepeatMode _mode))
                await MusicPlayer.Find(ctx.Guild).Repeat(_mode);
            else await MusicPlayer.Find(ctx.Guild).Repeat(RepeatMode.none);
        }

        [Command("shuffle")]
        [Description("Suffles the que, and the order the songs are played.")]
        public async Task Shuffle(CommandContext ctx)
        {
            if (MusicPlayer.Find(ctx.Guild).TextCh != ctx.Channel) { await ctx.RespondAsync($"You are not int the right text channel you must be in {MusicPlayer.Find(ctx.Guild).TextCh.Mention} or unbind the bot."); return; }
            if (ctx.Client.GetExtension<LavalinkExtension>() == null) return;
            await Join(ctx);
            var conn = ctx.Client.GetLavalink().ConnectedNodes.Values.First().GetGuildConnection(ctx.Guild);
            if (conn.CurrentState.CurrentTrack == null)
            {
                await ctx.RespondAsync("There are no tracks loaded.");
                return;
            }
            await MusicPlayer.Find(ctx.Guild).Shuffle();
        }

        [Command("info")]
        [Description("Gets or updates the info of the embed that is most recently displayed")]
        public async Task Info(CommandContext ctx)
        {
            if (MusicPlayer.Find(ctx.Guild).TextCh != ctx.Channel) { await ctx.RespondAsync($"You are not int the right text channel you must be in {MusicPlayer.Find(ctx.Guild).TextCh.Mention} or unbind the bot."); return; }
            if (ctx.Client.GetExtension<LavalinkExtension>() == null) return;
            await Join(ctx);
            var conn = ctx.Client.GetLavalink().ConnectedNodes.Values.First().GetGuildConnection(ctx.Guild);
            if (conn.CurrentState.CurrentTrack == null)
            {
                await ctx.RespondAsync("There are no tracks loaded.");
                return;
            }
            await MusicPlayer.Find(ctx.Guild).Info();
        }
    }       
}
