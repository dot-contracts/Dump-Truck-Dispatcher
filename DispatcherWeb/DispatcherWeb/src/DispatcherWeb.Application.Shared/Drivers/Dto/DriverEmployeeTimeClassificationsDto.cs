using System.Collections.Generic;

namespace DispatcherWeb.Drivers.Dto
{
    public class DriverEmployeeTimeClassificationsDto
    {
        public List<EmployeeTimeClassificationEditDto> EmployeeTimeClassifications { get; set; }

        public List<TimeClassificationDto> AllTimeClassifications { get; set; }
    }
}
