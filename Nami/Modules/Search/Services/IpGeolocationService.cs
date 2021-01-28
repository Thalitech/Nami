using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Nami.Modules.Search.Common;
using Nami.Services;

namespace Nami.Modules.Search.Services
{
    public sealed class IpGeolocationService : NamiHttpService
    {
        private  const string IpApiUrl = "http://ip-api.com/json";

        public override bool IsDisabled => false;


        public static Task<IpInfo?> GetInfoForIpAsync(string ipstr)
        {
            if (string.IsNullOrWhiteSpace(ipstr) || !IPAddress.TryParse(ipstr, out IPAddress? ip))
                return Task.FromResult<IpInfo?>(null);

            return GetInfoForIpAsync(ip);
        }

        public static async Task<IpInfo?> GetInfoForIpAsync(IPAddress? ip)
        {
            if (ip is null)
                return null;

            try {
                string response = await _http.GetStringAsync($"{IpApiUrl}/{ip}").ConfigureAwait(false);
                IpInfo data = JsonConvert.DeserializeObject<IpInfo>(response);
                return data;
            } catch {
                return null;
            }
        }
    }
}
