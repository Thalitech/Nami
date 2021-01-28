using System.Collections.Generic;
using DSharpPlus.Entities;
using Nami.Common;

namespace Nami.Modules.Games.Common
{
    public interface IQuiz : IChannelEvent
    {
        int NumberOfQuestions { get; }
        bool IsTimeoutReached { get; }
        IReadOnlyDictionary<DiscordUser, int> Results { get; }
    }
}
