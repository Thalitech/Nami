using DSharpPlus.CommandsNext;

namespace Nami.Exceptions
{
    public class ServiceDisabledException : LocalizedException
    {
        public ServiceDisabledException(CommandContext ctx)
            : base(ctx, "err-service-disabled")
        {

        }
    }
}
