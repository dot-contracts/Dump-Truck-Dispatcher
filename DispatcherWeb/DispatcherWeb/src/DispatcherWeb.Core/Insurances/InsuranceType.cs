using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using DispatcherWeb.Infrastructure;

namespace DispatcherWeb.Insurances
{
    [Table("InsuranceType")]
    public class InsuranceType : Entity
    {
        [Required]
        [StringLength(EntityStringFieldLengths.Insurance.InsuranceTypeName)]
        public string Name { get; set; }
        public DocumentType? DocumentType { get; set; }

        public virtual ICollection<Insurance> Insurances { get; set; }
    }
}
