using DSharpPlus.Lavalink;
using DSharpPlus.Lavalink.EventArgs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nami
{
    enum TrackReason
    {
        None = -1,
        Finished = TrackEndReason.Finished,
        LoadFailed = TrackEndReason.LoadFailed,
        Stopped = TrackEndReason.Stopped,
        Replaced = TrackEndReason.Replaced,
        Cleanup = TrackEndReason.Cleanup
    }

    class TrackEventArgs : EventArgs
    {
        public TrackEventArgs(LavalinkGuildConnection connection, NamiTrack namiTrack = null, int trackEndReason = (int)TrackReason.None)
        {
            this.Player = connection;
            this.Track = namiTrack;
            this.Reason = (TrackReason)trackEndReason;
        }

        public LavalinkGuildConnection Player { get; internal set; }
        public NamiTrack Track { get; internal set; }
        public TrackReason Reason { get; internal set; }

    }
}
