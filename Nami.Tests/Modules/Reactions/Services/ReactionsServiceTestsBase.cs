using NUnit.Framework;
using Nami.Modules.Reactions.Services;

namespace Nami.Tests.Modules.Reactions.Services
{
    [TestFixture]
    public class ReactionsServiceTestsBase : INamiServiceTest<ReactionsService>
    {
        public ReactionsService Service { get; private set; } = null!;


        [SetUp]
        public void InitializeService()
        {
            this.Service = new ReactionsService(TestDbProvider.Database, loadData: false);
        }
    }
}
