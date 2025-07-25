using System.Collections.Generic;
using System.Linq;

namespace DispatcherWeb.DriverApplication.Dto
{
    public class SendPushMessageToDriversInput
    {
        public List<int> DriverIds { get; set; }
        public string Message { get; set; }
        public string LogMessage { get; set; }

        public SendPushMessageToDriversInput()
        {
        }

        public SendPushMessageToDriversInput(int driverId)
        {
            DriverIds = new List<int> { driverId };
        }

        public SendPushMessageToDriversInput(IEnumerable<int> driverIds)
        {
            DriverIds = driverIds.ToList();
        }
    }
}
