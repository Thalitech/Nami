using DSharpPlus;
using DSharpPlus.CommandsNext;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Nami
{
    internal class PermissionManager
    {
        [Flags]
        public enum PermissionType
        {
            None = 0,
            Developer = 1,
            Owner = 2,
            Admin = 4,
            Member = 8,
        }


        internal static async Task<bool> CheckPermissions(CommandContext ctx, PermissionType permissionType)
        {
            bool hasPermissions = true;
            if (permissionType.HasFlag(PermissionType.Developer))
            {
                if (ctx.Member.Id != 284913031572488194) hasPermissions = false;
            }
            if (permissionType.HasFlag(PermissionType.Owner))
            {
                if (ctx.Member.Id != 284913031572488194 &&
                    ctx.Member != ctx.Guild.Owner) hasPermissions = false;
            }
            else if(permissionType.HasFlag(PermissionType.Admin))
            {
                if (ctx.Member.Id != 284913031572488194 &&
                    ctx.Member != ctx.Guild.Owner &&
                    ctx.Member.Roles.ToList().Find(x => x.CheckPermission(Permissions.Administrator) == PermissionLevel.Allowed) == null) hasPermissions = false;
            }
            else if (permissionType.HasFlag(PermissionType.Member))
            {
                hasPermissions = true;
            }

            if (!hasPermissions)
                await ctx.Member.SendMessageAsync("It seams like you don't have the correct permissions to run this command. if you find this and error, please contact your server admin");
            return hasPermissions;
        }


    }
}