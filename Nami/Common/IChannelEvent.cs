using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using Nami.Services;

namespace Nami.Common
{
    public interface IChannelEvent
    {
        DiscordChannel Channel { get; }
        InteractivityExtension Interactivity { get; }


        Task RunAsync(LocalizationService lcs);
    }
}
