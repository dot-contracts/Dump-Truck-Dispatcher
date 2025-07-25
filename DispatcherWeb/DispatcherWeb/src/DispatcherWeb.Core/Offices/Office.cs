using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using Abp.Organizations;
using DispatcherWeb.Authorization.Users;
using DispatcherWeb.Infrastructure;
using DispatcherWeb.Orders;
using DispatcherWeb.Quotes;
using DispatcherWeb.Trucks;

namespace DispatcherWeb.Offices
{
    [Table("Office")]
    public class Office : FullAuditedEntity, IMustHaveTenant, IMayHaveOrganizationUnit, ILogoStorageObject
    {
        public const int MaxNameLength = 150;
        public const int MaxFuelIdsLength = 1000;

        public Office()
        {
            Trucks = new HashSet<Truck>();
            Users = new HashSet<User>();
        }

        public int TenantId { get; set; }

        public long? OrganizationUnitId { get; set; }

        public virtual OrganizationUnit OrganizationUnit { get; set; }

        [Required]
        [StringLength(MaxNameLength)]
        public string Name { get; set; }

        [Required]
        [StringLength(7)]
        public string TruckColor { get; set; }

        public bool CopyDeliverToLoadAtChargeTo { get; set; } //now it's only CopyChargeTo

        [StringLength(50)] //29
        public string HeartlandPublicKey { get; set; }

        [StringLength(200)] //88
        public string HeartlandSecretKey { get; set; }

        [StringLength(MaxFuelIdsLength)]
        public string FuelIds { get; set; }

        public DateTime? DefaultStartTime { get; set; }

        public Guid? LogoId { get; set; }

        [MaxLength(EntityStringFieldLengths.Tenant.MaxLogoMimeTypeLength)]
        public string LogoFileType { get; set; }

        public Guid? ReportsLogoId { get; set; }

        [MaxLength(EntityStringFieldLengths.Tenant.MaxLogoMimeTypeLength)]
        public string ReportsLogoFileType { get; set; }

        public virtual ICollection<Truck> Trucks { get; set; }

        public virtual ICollection<User> Users { get; set; }

        public virtual ICollection<Order> Orders { get; set; }

        public virtual ICollection<Quote> Quotes { get; set; }
    }
}
