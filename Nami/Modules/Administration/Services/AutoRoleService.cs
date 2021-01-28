using System.Linq;
using Microsoft.EntityFrameworkCore;
using Nami.Database;
using Nami.Database.Models;
using Nami.Services;

namespace Nami.Modules.Administration.Services
{
    public sealed class AutoRoleService : DbAbstractionServiceBase<AutoRole, ulong, ulong>
    {
        public override bool IsDisabled => false;


        public AutoRoleService(DbContextBuilder dbb)
            : base(dbb) { }


        public override DbSet<AutoRole> DbSetSelector(NamiDbContext db)
            => db.AutoRoles;

        public override IQueryable<AutoRole> GroupSelector(IQueryable<AutoRole> ars, ulong gid)
            => ars.Where(ar => ar.GuildIdDb == (long)gid);

        public override AutoRole EntityFactory(ulong gid, ulong rid)
            => new AutoRole { GuildId = gid, RoleId = rid };

        public override ulong EntityIdSelector(AutoRole ar)
            => ar.RoleId;

        public override ulong EntityGroupSelector(AutoRole ar)
            => ar.GuildId;

        public override object[] EntityPrimaryKeySelector(ulong gid, ulong rid)
            => new object[] { (long)gid, (long)rid };
    }
}
