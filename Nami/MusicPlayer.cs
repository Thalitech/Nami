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

        internal static MusicPlayer Connect(LavalinkGuildConnection conn, DiscordGuild guild, CommandContext ctx, DiscordChannel voiceChannel, DiscordChannel textChannel)
        {
            var player = new MusicPlayer();
            player.connection = conn;
            player.server = guild;
            player.context = ctx;
            player.voiceChannel = voiceChannel;
            player.textChannel = textChannel;
            players.Add(guild, player);
            return player;
        }
        internal static void Disconnect(DiscordGuild guild)
        {
            var player = players[guild];
            players.Remove(guild);
            player.Dispose();
        }

        internal Task PlaybackStarted(LavalinkGuildConnection sender, TrackEventArgs e)
        {
            status = PlayerStatus.Playing;
            return Task.CompletedTask;
        }
        internal Task PlaybackUpdated(LavalinkGuildConnection sender, TrackEventArgs e)
        {

            return Task.CompletedTask;
        }
        internal Task PlaybackFinished(LavalinkGuildConnection sender, TrackEventArgs e)
        {
            status = (PlayerStatus)e.Reason;
            return Task.CompletedTask;
        }

        internal static MusicPlayer Find(DiscordGuild guild)
        {
            if (players.Count > 0)
                return players[guild];
            return null;
        }

        private LavalinkGuildConnection connection;
        private DiscordGuild server;
        private CommandContext context;
        private DiscordChannel voiceChannel;
        private DiscordChannel textChannel;
        public List<NamiTrack> tracks = new List<NamiTrack>();
        private PlayerStatus status = PlayerStatus.Ready;
        private RepeatMode repeatMode = RepeatMode.none;
        private bool suffleMode = false;
        private int currentIndex = -1;
        private DiscordMessage currentMessage;


        public DiscordChannel TextCh => textChannel;
        public DiscordChannel VoiceCh => voiceChannel;

        public async Task Play(CommandContext ctx, NamiTrack _track)
        {

            var _hasTrack = tracks.Find(x => x.Title == _track.Title) != null;

            if(_hasTrack)
            {
                var index = tracks.IndexOf(tracks.Find(x => x.Title == _track.Title));
                if (status != PlayerStatus.Playing) await Next();
                return;
            }

            await AddTrack(_track);
            new Thread(async () =>
            {
                while (tracks.Count <= 0) { };
                currentIndex = 0;
                await Playing();
            }).Start();
            return;
        }

        private async Task AddTrack(NamiTrack track)
        {
            track.AuthorImage = await Dispatcher.Generate<string>(GenType.AuthorImageUrl, track);
            track.AlbumeImage = await Dispatcher.Generate<string>(GenType.AlbumeImageUrl, track);
            track.Description = await Dispatcher.Generate<string>(GenType.Description, track);
            Dispatcher.Save(track);
            tracks.Add(track);
            if (status != PlayerStatus.Playing) await Next();
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
        public async Task Stop(bool changeStatus = true)
        {
            if (changeStatus)
                status = PlayerStatus.Stopped;
            if (currentIndex > -1)
                tracks[currentIndex].Position = TimeSpan.FromSeconds(0);
            await connection.StopAsync();
        }
        public async Task Next() 
        {
            await Stop();
            if (repeatMode == RepeatMode.all) { currentIndex++;  if (currentIndex > tracks.Count -1) { currentIndex = 0; } }
            else if (repeatMode == RepeatMode.one) { /* Don't change the index of the queue*/  if (currentIndex > tracks.Count - 1) { currentIndex = 0; } }
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
            if (repeatMode == RepeatMode.all) { currentIndex--; if (currentIndex < 0) { currentIndex = tracks.Count - 1; } }
            else if (repeatMode == RepeatMode.one) { /* Don't change the index of the queue*/  if (currentIndex > tracks.Count - 1) { currentIndex = 0; } }
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
        public async Task Repeat(RepeatMode mode)
        {
            repeatMode = mode;
            await TextCh.SendMessageAsync($"Repeat Mode has been changed to {mode}");
            await Task.CompletedTask;
        }
        public async Task Shuffle()
        {
            suffleMode = !suffleMode;
            await TextCh.SendMessageAsync($"Shuffle Mode has been changed to {(suffleMode ? "Enabled" : "Disabled")}");
            await Task.CompletedTask;
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
            builder.Title = track.Title;
            var field = builder.AddField("** **", "** **");
            builder.AddField("** **", track.Uri);

            field.ImageUrl = track.AlbumeImage;
            var _result = builder.Build();

            if (currentMessage == null)
                currentMessage = await context.RespondAsync(embed: _result);
            else
                currentMessage = await currentMessage.ModifyAsync(embed: _result);

            var jumpEmbed = new DiscordEmbedBuilder();
            jumpEmbed.AddField("Song Info Jump>", currentMessage.JumpLink.OriginalString, true);
            await textChannel.SendMessageAsync(embed: jumpEmbed);
        }
        private async Task Playing()
        {
            if (status == PlayerStatus.Stopped || status == PlayerStatus.Ready)
            {
            playNext:
                await UpdateEmbed();

                status = PlayerStatus.Playing;
                await connection.PlayAsync(tracks[currentIndex]);
                await PlaybackStarted(connection, new TrackEventArgs(connection, tracks[currentIndex]));
                while (tracks[currentIndex].Position.TotalSeconds < tracks[currentIndex].Length.TotalSeconds)
                {
                    while (status == PlayerStatus.Paused) { }
                    var seconds = tracks[currentIndex].Position.TotalSeconds;
                    seconds++;
                    tracks[currentIndex].Position = TimeSpan.FromSeconds(seconds);
                    Thread.Sleep(1000);
                    if (status == PlayerStatus.Stopped) break;
                };
                if (status == PlayerStatus.Playing)
                {
                    status = PlayerStatus.Finished;
                    await PlaybackFinished(connection, new TrackEventArgs(connection, tracks[currentIndex], status != PlayerStatus.Stopped ? (int)TrackReason.Finished : (int)TrackReason.Stopped));
                    await Stop(false);
                    if (repeatMode == RepeatMode.all)
                    {
                        if (suffleMode)
                        {
                            currentIndex = new Random().Next(0, tracks.Count - 1);
                        }
                        else { currentIndex++;  if (currentIndex > tracks.Count - 1) currentIndex = 0; }
                        goto playNext;
                    }
                    else if (repeatMode == RepeatMode.one)
                    {
                        goto playNext;
                    }
                    else
                    {
                        if (currentIndex < tracks.Count - 1)
                        {
                            if (suffleMode)
                            {
                                currentIndex = new Random().Next(0, tracks.Count - 1);
                            }
                            else currentIndex++;
                            goto playNext;
                        }
                    }
                }
                else
                {
                    await Stop();
                    await PlaybackFinished(connection, new TrackEventArgs(connection, tracks[currentIndex], status != PlayerStatus.Stopped ? (int)TrackReason.Finished : (int)TrackReason.Stopped));
                }
                
            }
        }


        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}