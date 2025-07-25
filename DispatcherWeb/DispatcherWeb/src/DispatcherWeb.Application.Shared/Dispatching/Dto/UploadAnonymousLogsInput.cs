using System;
using System.Collections.Generic;

namespace DispatcherWeb.Dispatching.Dto
{
    public class UploadAnonymousLogsInput
    {
        public DateTime ActionTime { get; set; }
        public int? TimezoneOffset { get; set; }
        //public Guid? DriverGuid { get; set; }
        public int? DeviceId { get; set; }
        public Guid? DeviceGuid { get; set; }
        //public DriverApplicationAction Action { get; set; }
        public List<UploadLogsInput> UploadLogsData { get; set; }
    }
}
