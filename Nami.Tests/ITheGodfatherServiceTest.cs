using Nami.Services;

namespace Nami.Tests
{
    public interface INamiServiceTest<T> where T : INamiService
    {
        T Service { get; }


        void InitializeService();
    }
}
