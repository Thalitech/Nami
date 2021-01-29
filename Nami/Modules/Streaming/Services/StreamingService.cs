using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Serilog;
using Nami.Database;
using Nami.Database.Models;
using Nami.Services;
using Nami.Common.Collections;
using System.Text.RegularExpressions;
using Nami.Services.Common;
using Microsoft.EntityFrameworkCore;

namespace Nami.Modules.Streaming.Services
{
    public sealed class StreamingService : DbAbstractionServiceBase<Stream, ulong, ulong>, INamiService
    {

        public override bool IsDisabled => false;
        private readonly ConcurrentDictionary<ulong, CachedGuildConfig> gcfg;

        public async Task<GuildConfig> GetConfigAsync(ulong gid)
        {
            GuildConfig? gcfg = null;
            using (NamiDbContext db = this.dbb.CreateContext())
                gcfg = await db.Configs.FindAsync((long)gid);
            return gcfg ?? new GuildConfig();
        }
        public async Task<GuildConfig> ModifyConfigAsync(ulong gid, Action<GuildConfig> modifyAction)
        {
            if (!this.gcfg.ContainsKey(gid))
                throw new KeyNotFoundException($"Failed to find the guild in internal list: {gid}");

            GuildConfig? gcfg = null;
            using (NamiDbContext db = this.dbb.CreateContext()) {
                gcfg = await db.Configs.FindAsync((long)gid) ?? new GuildConfig();
                modifyAction(gcfg);
                db.Configs.Update(gcfg);
                await db.SaveChangesAsync();
            }

            this.gcfg.AddOrUpdate(gid, gcfg.CachedConfig, (k, v) => gcfg.CachedConfig);
            return gcfg;
        }


        public StreamingService(DbContextBuilder dbb)
            : base(dbb) { }


        public override DbSet<Stream> DbSetSelector(NamiDbContext db)
            => db.Streams;

        public override Stream EntityFactory(ulong grid, ulong id)
            => new Stream { MemberID = id, GuildId = grid };

        public override ulong EntityGroupSelector(Stream entity)
            => entity.GuildId;

        public override ulong EntityIdSelector(Stream entity)
            => entity.MemberID;

        public override object[] EntityPrimaryKeySelector(ulong grid, ulong id)
            => new object[] { (long)grid, (long)id };

        public override IQueryable<Stream> GroupSelector(IQueryable<Stream> entities, ulong grid)
            => entities.Where(acc => (ulong)acc.GuildIdDb == grid);

        public Task IncreaseStreamAsync(ulong gid, ulong uid, int _amount)
            => this.ModifyStreamAsync(gid, uid, b => b + _amount);

        public async Task<bool> TryDecreaseStreamAsync(ulong gid, ulong uid, int _amount)
        {
            using NamiDbContext db = this.dbb.CreateContext();
            Stream? stream = await this.GetAsync(gid, uid);
            if (stream is null || stream.Joined < _amount)
                return false;

            stream.Joined -= _amount;
            db.Streams.Update(stream);

            await db.SaveChangesAsync();
            return true;
        }

        public async Task ModifyStreamAsync(ulong gid, ulong uid, Func<int, int> balanceModifier)
        {
            using NamiDbContext db = this.dbb.CreateContext();

            bool created = false;

            Stream? stream = await this.GetAsync(gid, uid);
            if (stream is null) 
            {
                stream = new Stream 
                {
                    GuildId = gid,
                    MemberID = uid
                };
                created = true;
            }

            stream.Joined = balanceModifier(stream.Joined);
            if (stream.Joined < 0)
                stream.Joined = 0;

            if (created)
                db.Streams.Add(stream);
            else
                db.Streams.Update(stream);

            await db.SaveChangesAsync();
        }

        public async Task<IReadOnlyList<Stream>> GetTopStreamAsync(ulong? gid = null, int amount = 3)
        {
            List<Stream> topAccounts;
            using NamiDbContext db = this.dbb.CreateContext();
            topAccounts = await
                (gid is { } ? db.Streams.Where(a => a.GuildIdDb == (long)gid) : db.Streams)
                .OrderByDescending(a => a.Joined)
                .Take(amount)
                .ToListAsync();
            return topAccounts;
        }

        public async Task RemoveAllAsync(ulong uid)
        {
            using NamiDbContext db = this.dbb.CreateContext();
            db.Streams.RemoveRange(db.Streams.Where(str => str.MemberID == uid));
            await db.SaveChangesAsync();
        }
    }
}
