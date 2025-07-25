using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities.Auditing;
using DispatcherWeb.Infrastructure;

namespace DispatcherWeb.VehicleCategories
{
    [Table("VehicleCategory")]
    public class VehicleCategory : FullAuditedEntity
    {
        [StringLength(EntityStringFieldLengths.VehicleCategory.Name)]
        public string Name { get; set; }

        public AssetType AssetType { get; set; }

        public bool IsPowered { get; set; }

        public int SortOrder { get; set; }
    }
}
