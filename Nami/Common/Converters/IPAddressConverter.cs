using System.Net;

namespace Nami.Common.Converters
{
    public class IPAddressConverter : BaseArgumentConverter<IPAddress>
    {
        public override bool TryConvert(string value, out IPAddress? result)
            => IPAddress.TryParse(value, out result);
    }
}