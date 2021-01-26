using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using Nami.Attributes;
using Nami.Services;

namespace Nami.Modules
{
    public abstract class NamiModule : BaseCommandModule
    {
        public LocalizationService Localization { get; set; }
        public DiscordColor ModuleColor { get; }


#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        protected NamiModule()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {
            var moduleAttr = ModuleAttribute.AttachedTo(this.GetType());
            this.ModuleColor = moduleAttr?.Module.ToDiscordColor() ?? DiscordColor.Green;
        }
    }
}
