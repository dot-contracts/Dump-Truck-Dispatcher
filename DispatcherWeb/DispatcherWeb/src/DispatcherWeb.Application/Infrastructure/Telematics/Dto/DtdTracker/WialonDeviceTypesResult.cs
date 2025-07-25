using System.Collections.Generic;

namespace DispatcherWeb.Infrastructure.Telematics.Dto.DtdTracker
{
    public class WialonDeviceTypesResult
    {
        public List<DeviceTypeDto> Items { get; set; }

        public string HardwareGatewayDomain { get; set; }
    }
}
