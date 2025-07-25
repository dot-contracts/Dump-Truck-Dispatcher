using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using DispatcherWeb.Infrastructure;

namespace DispatcherWeb.EmployeeTime.Dto
{
    public class AddBulkTimeDto
    {
        [Required]
        public DateTime StartDateTime { get; set; }

        [Required]
        public DateTime EndDateTime { get; set; }

        public int? TimeClassificationId { get; set; }

        public string TimeClassificationName { get; set; }

        [StringLength(EntityStringFieldLengths.EmployeeTime.Description)]
        public string Description { get; set; }

        public IList<int> DriverIds { get; set; } = new List<int>();
    }
}
