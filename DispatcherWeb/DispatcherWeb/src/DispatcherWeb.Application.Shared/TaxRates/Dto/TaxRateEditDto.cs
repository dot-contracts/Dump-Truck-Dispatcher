using System.ComponentModel.DataAnnotations;
using DispatcherWeb.Infrastructure;

namespace DispatcherWeb.TaxRates.Dto
{
    public class TaxRateEditDto
    {
        public int? Id { get; set; }

        [Required]
        [StringLength(EntityStringFieldLengths.TaxRate.Name)]
        public string Name { get; set; }

        [Required]
        public decimal? Rate { get; set; }
    }
}
