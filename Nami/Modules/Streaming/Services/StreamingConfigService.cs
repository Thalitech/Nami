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

namespace Nami.Modules.Streaming.Services
{
    public sealed class StreamingConfigService : INamiService
    {
        public bool IsDisabled => false;

        private ConcurrentDictionary<ulong, ConcurrentHashSet<Stream>> streams;
        private readonly DbContextBuilder dbb;
        private readonly ConcurrentDictionary<ulong, CachedGuildConfig> gcfg;

        public StreamingConfigService(BotConfigService cfg, DbContextBuilder dbb, bool loadData = true)
        {
            this.dbb = dbb;
            this.gcfg = new ConcurrentDictionary<ulong, CachedGuildConfig>();
            this.streams = new ConcurrentDictionary<ulong, ConcurrentHashSet<Stream>>();
            if (loadData)
                this.LoadData();
        }


        public void LoadData()
        {
            Log.Debug("Loading StreamJoined");
            try 
            {
                using NamiDbContext db = this.dbb.CreateContext();
                // Convert to Enumerable
                var collection = db.Streams
                    .AsEnumerable()
                    .GroupBy(x => x.GuildId)
                    .ToDictionary(x => x.Key, x => new ConcurrentHashSet<Stream>(x));

                this.streams = new ConcurrentDictionary<ulong, ConcurrentHashSet<Stream>>(collection);
            } catch (Exception e) 
            {
                Log.Error(e, "Loading Stream Joined failed");
            }
        }

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


        public bool TextContainsFilter(ulong gid, ulong? member_id, out Stream? match)
        {
            match = null;

            if (!this.streams.TryGetValue(gid, out ConcurrentHashSet<Stream>? fs) || fs is null)
                return false;

            match = fs.FirstOrDefault(f => f.MemberID == member_id);
            return match is { };
        }

        public IReadOnlyList<Stream> GetGuildFilters(ulong gid)
          => this.streams.TryGetValue(gid, out ConcurrentHashSet<Stream>? fs) ? fs.ToList() : (IReadOnlyList<Stream>)Array.Empty<Stream>();

        public Task<bool> AddFilterAsync(ulong gid, ulong member_id)
        {
            if(member_id == default) 
            {
                throw new ArgumentException($"Invalid regex string: {member_id}", nameof(member_id));
            }
            return this.AddFilterAsync(gid, member_id);
        }

        public async Task<bool> AddPlayerAsync(ulong gid, ulong? member_id)
        {
            if (member_id is null)
                return false;

            
            ConcurrentHashSet<Stream> fs = this.streams.GetOrAdd(gid, new ConcurrentHashSet<Stream>());
            if (!fs.Any(f => f.MemberID == member_id))
                return false;

            using NamiDbContext db = this.dbb.CreateContext();
            var stream = new Stream {
                GuildId = gid,
                MemberID = member_id,
                Joined = 1,
            };
            db.Streams.Add(stream);
            await db.SaveChangesAsync();
            return fs.Add(stream);
        }

        public async Task<bool> AddPlayersAsync(ulong gid, IEnumerable<ulong> member_ids)
        {
            bool[] res = await Task.WhenAll(member_ids.Select(r => this.AddPlayerAsync(gid, r)));
            return res.All(r => r);
        }

        public Task<int> RemovePlayerAsync(ulong gid)
            => this.InternalRemoveByPredicateAsync(gid, _ => true);

        public Task<int> RemovePlayerAsync(ulong gid, IEnumerable<long> ids)
            => this.InternalRemoveByPredicateAsync(gid, f => ids.Contains(f.GuildIdDb));

        public Task<int> RemovePlayersAsync(ulong gid, IEnumerable<ulong> member_ids)
            => this.InternalRemoveByPredicateAsync(gid, f => member_ids.Any(x => x == f.MemberID));

        public Task<int> RemoveFiltersMatchingAsync(ulong gid, ulong member_id)
            => this.InternalRemoveByPredicateAsync(gid, f => f.MemberID == member_id);


        private IQueryable<Stream> InternalGetStreamsForGuild(NamiDbContext db, ulong gid)
            => db.Streams.Where(n => n.GuildIdDb == (long)gid);

        private async Task<int> InternalRemoveByPredicateAsync(ulong gid, Func<Stream, bool> predicate)
        {
            using NamiDbContext db = this.dbb.CreateContext();
            var streams = this.InternalGetStreamsForGuild(db, gid)
                .AsEnumerable()
                .Where(predicate)
                .ToList();
            db.Streams.RemoveRange(streams);
            await db.SaveChangesAsync();
            return this.streams.GetValueOrDefault(gid)?.RemoveWhere(predicate) ?? 0;
        }
    }
}
