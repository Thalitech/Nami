using Microsoft.EntityFrameworkCore.Design;
using Nami.Services;
using Nami.Services.Common;

namespace Nami.Database
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<NamiDbContext>
    {
        private readonly BotConfigService cfg;
        private readonly AsyncExecutionService async;


        public DesignTimeDbContextFactory()
        {
            this.cfg = new BotConfigService();
            this.async = new AsyncExecutionService();
        }


        public NamiDbContext CreateDbContext(params string[] _)
        {
            BotConfig cfg = this.async.Execute(this.cfg.LoadConfigAsync("Resources/config.json"));
            return new DbContextBuilder(cfg.DatabaseConfig).CreateContext();
        }
    }
}
