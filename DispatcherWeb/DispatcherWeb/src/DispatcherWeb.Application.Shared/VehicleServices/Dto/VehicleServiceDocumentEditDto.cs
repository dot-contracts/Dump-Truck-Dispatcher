using System;
using System.ComponentModel.DataAnnotations;

namespace DispatcherWeb.VehicleServices.Dto
{
    public class VehicleServiceDocumentEditDto
    {
        public int Id { get; set; }
        public int VehicleServiceId { get; set; }
        public Guid FileId { get; set; }

        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(1000)]
        public string Description { get; set; }
    }
}
