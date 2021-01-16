using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.Lavalink;
using DSharpPlus.Lavalink.EventArgs;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Nami
{
    internal class MusicPlayer : IDisposable
    {
        private static Dictionary<DiscordGuild, MusicPlayer> players = new Dictionary<DiscordGuild, MusicPlayer>();

        internal static MusicPlayer Connect(LavalinkGuildConnection conn, DiscordGuild guild, CommandContext ctx)
        {
            var player = new MusicPlayer();
            player.connection = conn;
            player.server = guild;
            player.context = ctx;
            players.Add(guild, player);
            return player;
        }
        internal static void Disconnect(DiscordGuild guild)
        {
            var player = players[guild];
            players.Remove(guild);
            player.Dispose();
        }

        internal Task PlaybackStarted(LavalinkGuildConnection sender, TrackStartEventArgs e)
        {
            status = PlayerStatus.Playing;
            return Task.CompletedTask;
        }
        internal Task PlaybackUpdated(LavalinkGuildConnection sender, PlayerUpdateEventArgs e)
        {

            return Task.CompletedTask;
        }
        internal Task PlaybackFinished(LavalinkGuildConnection sender, TrackFinishEventArgs e)
        {
            status = (PlayerStatus)e.Reason;
            return Task.CompletedTask;
        }

        internal static MusicPlayer Find(DiscordGuild guild) => players[guild];

        private LavalinkGuildConnection connection;
        private DiscordGuild server;
        private CommandContext context;

        public List<TrackInfo> tracks = new List<TrackInfo>();
        private PlayerStatus status = PlayerStatus.Ready;
        private RepeatMode repeatMode = RepeatMode.None;
        private bool suffleMode = false;
        private int currentIndex;
        private DiscordMessage currentMessage;

        public Task Play(CommandContext ctx, LavalinkTrack _track)
        {
            new Thread(async () =>
            {
                var track = new TrackInfo(_track);
                track.AuthorImage = await Dispatcher.Generate<string>(GenType.AuthorImageUrl, _track);
                track.AlbumeImage = await Dispatcher.Generate<string>(GenType.AlbumeImageUrl, _track);
                track.Description = await Dispatcher.Generate<string>(GenType.Description, _track);
                tracks.Add(track);
            }).Start();
            new Thread(async () =>
            {
                while (tracks.Count <= 0) { };
                await Playing();
            }).Start();
            return Task.CompletedTask;
        }
        public async Task Pause()
        {
            status = PlayerStatus.Paused;
            await connection.PauseAsync();
        }
        public async Task Resume()
        {
            status = PlayerStatus.Playing;
            await connection.ResumeAsync();
        }
        public async Task Stop()
        {
            status = PlayerStatus.Stopped;
            await connection.StopAsync();
        }
        public async Task Next() 
        {
            await Stop();
            if (repeatMode == RepeatMode.LoopAll) { currentIndex++;  if (currentIndex > tracks.Count -1) { currentIndex = 0; } }
            else if (repeatMode == RepeatMode.Loop) { /* Don't change the index of the queue*/  if (currentIndex > tracks.Count - 1) { currentIndex = 0; } }
            else
            {
                if(currentIndex < tracks.Count - 1)
                    currentIndex++;
            }
            status = PlayerStatus.Ready;
            await Playing();
        }
        public async Task Previous()
        {
            await Stop();
            if (repeatMode == RepeatMode.LoopAll) { currentIndex--; if (currentIndex < 0) { currentIndex = tracks.Count - 1; } }
            else if (repeatMode == RepeatMode.Loop) { /* Don't change the index of the queue*/  if (currentIndex > tracks.Count - 1) { currentIndex = 0; } }
            else
            {
                if (currentIndex > 0)
                    currentIndex--;
            }
            status = PlayerStatus.Ready;
            await Playing();
        }
        public async Task Clear()
        {
            await Stop();
            tracks.Clear();
            currentIndex = 0;
        }
        public async Task Info()
        {
            await UpdateEmbed();
        }


        public async Task UpdateEmbed()
        {
            var track = tracks[currentIndex];
            DiscordEmbedBuilder builder = new DiscordEmbedBuilder();
            builder.Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail() { Url = track.AuthorImage };
            builder.Author = new DiscordEmbedBuilder.EmbedAuthor() { Name = track.Author, };
            builder.Description = track.Description;
            builder.Color = DiscordColor.Azure;
            builder.Title = track.Name;
            var field = builder.AddField("** **", "** **");
            field.ImageUrl = track.AlbumeImage;
            field.WithImageUrl(track.Uri);
            var _result = builder.Build();
            _result.Video = new DiscordEmbedVideo() { Url = track.Uri, Width = 133, Height = 100 };

            if (currentMessage == null)
                currentMessage = await context.RespondAsync(embed: _result);
            else
                currentMessage = await currentMessage.ModifyAsync(embed: _result);
        }
        private async Task Playing()
        {
            if(status == PlayerStatus.Stopped || status == PlayerStatus.Ready)
            {
            playNext:
                await UpdateEmbed();

                status = PlayerStatus.Playing;
                await connection.PlayAsync(tracks[currentIndex]);
                await PlaybackStarted(connection, new TrackStartEventArgs(connection, tracks[currentIndex]));
                while (tracks[currentIndex].Position < tracks[currentIndex].Length)
                {
                    while (status == PlayerStatus.Paused) { }
                    var seconds = tracks[currentIndex].Position.TotalSeconds;
                    seconds++;
                    tracks[currentIndex].Position = TimeSpan.FromSeconds(seconds);
                    Thread.Sleep(1000);
                    if (status != PlayerStatus.Stopped) break;
                };
                if(status != PlayerStatus.Stopped)
                {
                    status = PlayerStatus.Finished;
                    await PlaybackFinished(connection, new TrackFinishEventArgs(connection, tracks[currentIndex], status != PlayerStatus.Stopped ? TrackEndReason.Finished : TrackEndReason.Stopped));
                    tracks[currentIndex].Position = TimeSpan.FromSeconds(0);
                    if (currentIndex < tracks.Count - 1)
                    {
                        if(suffleMode)
                        {
                            currentIndex = new Random().Next(0, tracks.Count - 1);
                        }
                        else currentIndex++;
                        goto playNext;
                    }
                }
                else
                {
                    tracks[currentIndex].Position = TimeSpan.FromSeconds(0);
                    await PlaybackFinished(connection, new TrackFinishEventArgs(connection, tracks[currentIndex], status != PlayerStatus.Stopped ? TrackEndReason.Finished : TrackEndReason.Stopped));
                }
                
            }
        }
        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}