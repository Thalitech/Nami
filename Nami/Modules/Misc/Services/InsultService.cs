using System.Net;
using System.Threading.Tasks;
using Nami.Services;

namespace Nami.Modules.Misc.Services
{
    public class InsultService : INamiService
    {
        public const string Provider = "insult.mattbas.org";

        private readonly string endpoint = "https://insult.mattbas.org/api/en/insult.txt";

        public bool IsDisabled => false;


        public Task<string> FetchInsultAsync(string username)
            => HttpService.GetStringAsync($"{this.endpoint}?who={WebUtility.UrlEncode(username)}");
    }
}
