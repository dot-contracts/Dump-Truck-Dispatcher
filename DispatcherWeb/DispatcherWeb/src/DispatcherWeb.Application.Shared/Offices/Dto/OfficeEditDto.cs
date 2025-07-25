using System;
using System.ComponentModel.DataAnnotations;

namespace DispatcherWeb.Offices.Dto
{
    public class OfficeEditDto
    {
        public int? Id { get; set; }

        [Required]
        [StringLength(150)]
        public string Name { get; set; }

        [Required]
        [StringLength(7)]
        public string TruckColor { get; set; }

        public bool CopyChargeTo { get; set; }

        public string HeartlandPublicKey { get; set; }

        public string HeartlandSecretKey { get; set; }

        public string FuelIds { get; set; }

        public DateTime? DefaultStartTime { get; set; }

        public Guid? LogoId { get; set; }

        public Guid? ReportsLogoId { get; set; }
    }
}
