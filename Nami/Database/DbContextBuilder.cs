using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Nami.Services;
using Npgsql;
using Serilog;

namespace Nami.Database
{
    public class DbContextBuilder
    {
        public DbProvider Provider { get; }

        private string ConnectionString { get; }
        private DbContextOptions<NamiDbContext>? Options { get; }


        public DbContextBuilder(DbProvider provider, string connectionString, DbContextOptions<NamiDbContext>? options = null)
        {
            this.Provider = provider;
            this.ConnectionString = connectionString;
            this.Options = options;
        }

        public DbContextBuilder(DbConfig cfg, DbContextOptions<NamiDbContext>? options = null)
        {
            cfg ??= new DbConfig();
            this.Provider = cfg.Provider;
            this.Options = options;
            this.ConnectionString = this.Provider switch {
                DbProvider.PostgreSql => new NpgsqlConnectionStringBuilder {
                    Host = cfg.Hostname,
                    Port = cfg.Port,
                    Database = cfg.DatabaseName,
                    Username = cfg.Username,
                    Password = cfg.Password,
                    Pooling = true,
                    MaxAutoPrepare = 50,
                    AutoPrepareMinUsages = 3,
                    SslMode = SslMode.Prefer,
                    TrustServerCertificate = true
                }.ConnectionString,
                DbProvider.Sqlite => $"Data Source={cfg.DatabaseName}.db;",
                DbProvider.SqlServer => $@"Data Source=(localdb)\ProjectsV13;Initial Catalog={cfg.DatabaseName};" +
                                          "Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;" +
                                          "ApplicationIntent=ReadWrite;MultiSubnetFailover=False",
                DbProvider.SqliteInMemory => @"DataSource=:memory:;foreign keys=true;",
                _ => throw new NotSupportedException("Unsupported database provider!"),
            };
        }

        internal static async Task<DbContextBuilder> LoadConfigAsync(BotConfigService cfg)
        {
            Log.Information("Establishing database connection");
            var dbb = new DbContextBuilder(cfg.CurrentConfiguration.DatabaseConfig);

            Log.Information("Testing database context creation");
            using (NamiDbContext db = dbb.CreateContext()) {
                IEnumerable<string> pendingMigrations = await db.Database.GetPendingMigrationsAsync();
                if (pendingMigrations.Any()) {
                    Log.Information("Applying pending database migrations: {PendingDbMigrations}", pendingMigrations);
                    await db.Database.MigrateAsync();
                }
            }
            return dbb;
        }

        public NamiDbContext CreateContext()
        {
            try 
            {
                return this.Options is null
                    ? new NamiDbContext(this.Provider, this.ConnectionString)
                    : new NamiDbContext(this.Provider, this.ConnectionString, this.Options);
            } catch (Exception e) {
                Log.Fatal(e, "An exception occured during database initialization:");
                throw;
            }
        }

        public NamiDbContext CreateContext(DbContextOptions<NamiDbContext> options)
        {
            try {
                return new NamiDbContext(this.Provider, this.ConnectionString, options);
            } catch (Exception e) {
                Log.Fatal(e, "An exception occured during database initialization:");
                throw;
            }
        }
    }
}