using System.Collections.Generic;

namespace DispatcherWeb.EmployeeTime.Dto
{
    public class AddBulkTimeResult
    {
        public bool Success { get; set; }
        public List<AddBulkTimeError> Errors { get; set; }

        public class AddBulkTimeError
        {
            public int DriverId { get; set; }
            public string ErrorMessage { get; set; }
        }
    }
}
