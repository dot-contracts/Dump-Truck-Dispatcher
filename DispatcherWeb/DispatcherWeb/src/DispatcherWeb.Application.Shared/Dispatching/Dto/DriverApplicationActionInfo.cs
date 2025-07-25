using System;
using DispatcherWeb.DriverApplication.Dto;

namespace DispatcherWeb.Dispatching.Dto
{
    public class DriverApplicationActionInfo
    {
        public DriverApplicationActionInfo()
        {
        }

        public DriverApplicationActionInfo(ExecuteDriverApplicationActionInput input, AuthDriverByDriverGuidResult authInfo)
        {
            ActionTimeInUtc = input.ActionTimeInUtc;
            DeviceId = input.DeviceId;
            DeviceGuid = input.DeviceGuid;
            TenantId = authInfo.TenantId;
            DriverId = authInfo.DriverId;
            UserId = authInfo.UserId;
        }

        public int TenantId { get; set; }
        public int DriverId { get; set; }
        public long UserId { get; set; }
        public int? DeviceId { get; set; }
        public Guid? DeviceGuid { get; set; }

        public DateTime ActionTimeInUtc { get; set; }
        public string TimeZone { get; set; }
    }
}
