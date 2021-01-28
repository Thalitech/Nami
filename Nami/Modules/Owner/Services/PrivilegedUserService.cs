using Microsoft.EntityFrameworkCore;
using Nami.Database;
using Nami.Database.Models;
using Nami.Services;

namespace Nami.Modules.Owner.Services
{
    public sealed class PrivilegedUserService : DbAbstractionServiceBase<PrivilegedUser, ulong>
    {
        public override bool IsDisabled => false;


        public PrivilegedUserService(DbContextBuilder dbb)
            : base(dbb) { }


        public override DbSet<PrivilegedUser> DbSetSelector(NamiDbContext db) => db.PrivilegedUsers;
        public override PrivilegedUser EntityFactory(ulong id) => new PrivilegedUser { UserId = id };
        public override ulong EntityIdSelector(PrivilegedUser entity) => entity.UserId;
        public override object[] EntityPrimaryKeySelector(ulong id) => new object[] { (long)id };
    }
}
