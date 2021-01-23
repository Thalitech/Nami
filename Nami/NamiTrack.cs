using DSharpPlus.Lavalink;
using Newtonsoft.Json;
using System;

namespace Nami
{
    [Serializable]
    public class NamiTrack
    {
        [JsonProperty("ID")] public string ID { get; internal set; }
        [JsonProperty("Identifier")] public string Identifier { get; internal set; }
        [JsonProperty("IsSeekable")] public bool IsSeekable { get; private set; }
        [JsonProperty("Title")] public string Title { get; internal set; }
        [JsonProperty("Author")] public string Author { get; internal set; }
        [JsonProperty("Uri")] public string Uri { get; internal set; }
        [JsonProperty("Description")] public string Description { get; internal set; }
        [JsonProperty("Position")] public TimeSpan Position { get; internal set; }
        [JsonProperty("IsStream")] public bool IsStream { get; private set; }
        [JsonProperty("Clip")]public  LavalinkTrack clip { get; private set; }

        [JsonProperty("Length")] public TimeSpan Length { get; internal set; }
        [JsonProperty("AuthorImage")] public string AuthorImage { get; internal set; }
        [JsonProperty("AlbumeImage")] public string AlbumeImage { get; internal set; }


        public static implicit operator LavalinkTrack(NamiTrack track) 
        {
            return track.clip;
        }
        public static implicit operator NamiTrack(LavalinkTrack track)
        {
            var ntrack = new NamiTrack();
            ntrack.ID = track.TrackString;
            ntrack.Identifier = track.Identifier;
            ntrack.IsSeekable = track.IsSeekable;
            ntrack.Title = track.Title;
            ntrack.Author = track.Author;
            ntrack.Uri = track.Uri.OriginalString;
            ntrack.Length = track.Length;
            ntrack.Position = track.Position;
            ntrack.IsStream = track.IsStream;
            ntrack.clip = track;
            return ntrack;
        }
    }
}