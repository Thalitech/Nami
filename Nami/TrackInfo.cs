using DSharpPlus.Lavalink;
using System;

namespace Nami
{
    public class TrackInfo
    {
        private LavalinkTrack clip;

        public string Name => clip.Title;
        public string Author => clip.Author;
        public Uri Uri => clip.Uri;
        public string Description;


        public TimeSpan Position;
        public TimeSpan Length;

        public string AuthorImage;
        public string AlbumeImage;

        public TrackInfo(LavalinkTrack clip)
        {
            this.clip = clip;
            Position = clip.Position;
            Length = clip.Length;
        }

        public static implicit operator LavalinkTrack(TrackInfo track) => track.clip;
        public static implicit operator TrackInfo(LavalinkTrack track) => new TrackInfo(track);
    }
}