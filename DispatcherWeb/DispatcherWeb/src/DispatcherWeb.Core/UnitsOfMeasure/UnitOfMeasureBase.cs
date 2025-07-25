using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using DispatcherWeb.Infrastructure;

namespace DispatcherWeb.UnitsOfMeasure
{
    [Table("UnitOfMeasureBase")]
    public class UnitOfMeasureBase : Entity
    {
        [Required]
        [StringLength(EntityStringFieldLengths.UnitOfMeasure.Name)]
        public string Name { get; set; }
    }
}
