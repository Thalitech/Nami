#region USING_DIRECTIVES
using System.Net.Http;
#endregion

namespace Nami.Services
{
    public abstract class NamiHttpService : INamiService
    {
        protected static readonly HttpClientHandler _handler = new HttpClientHandler { AllowAutoRedirect = false };
        protected static readonly HttpClient _http = new HttpClient(_handler, true);

        public abstract bool IsDisabled { get; }
    }
}
