using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using DispatcherWeb.Authorization.Users;
using DispatcherWeb.Dispatching;
using DispatcherWeb.Infrastructure;
using DispatcherWeb.LeaseHaulerRequests;
using DispatcherWeb.LeaseHaulers;
using DispatcherWeb.Offices;
using DispatcherWeb.TimeOffs;
using DispatcherWeb.Trucks;

namespace DispatcherWeb.Drivers
{
    [Table("Driver")]
    public class Driver : FullAuditedEntity, IMustHaveTenant
    {
        public int TenantId { get; set; }

        public long? UserId { get; set; }
        public User User { get; set; }

        [Required(ErrorMessage = "First Name is a required field")]
        [StringLength(EntityStringFieldLengths.Driver.FirstName)]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last Name is a required field")]
        [StringLength(EntityStringFieldLengths.Driver.LastName)]
        public string LastName { get; set; }

        public int? OfficeId { get; set; }

        public bool IsInactive { get; set; }

        public bool IsExternal { get; set; }

        public virtual Office Office { get; set; }

        [StringLength(EntityStringFieldLengths.Driver.EmailAddress)]
        public string EmailAddress { get; set; }

        [StringLength(EntityStringFieldLengths.General.PhoneNumber)]
        public string CellPhoneNumber { get; set; }

        [StringLength(EntityStringFieldLengths.GeneralAddress.MaxStreetAddressLength)]
        public string Address { get; set; }

        [StringLength(EntityStringFieldLengths.GeneralAddress.MaxCityLength)]
        public string City { get; set; }

        [StringLength(EntityStringFieldLengths.GeneralAddress.MaxStateLength)]
        public string State { get; set; }

        [StringLength(EntityStringFieldLengths.GeneralAddress.MaxZipCodeLength)]
        public string ZipCode { get; set; }

        public OrderNotifyPreferredFormat OrderNotifyPreferredFormat { get; set; }

        public ICollection<DriverAssignment> DriverAssignments { get; set; }

        public ICollection<Truck> DefaultTrucks { get; set; }

        public ICollection<Dispatch> Dispatches { get; set; }

        public virtual ICollection<DriverPushSubscription> DriverPushSubscriptions { get; set; }

        public virtual ICollection<EmployeeTimeClassification> EmployeeTimeClassifications { get; set; }

        public virtual ICollection<TimeOff> TimeOffs { get; set; }

        public LeaseHaulerDriver LeaseHaulerDriver { get; set; }

        [Column(TypeName = "date")]
        public DateTime? ContractStartDate { get; set; }

        [Column(TypeName = "date")]
        public DateTime? EmploymentStartDate { get; set; }

        [StringLength(EntityStringFieldLengths.GeneralAddress.MaxStateLength)]
        public string LicenseState { get; set; }

        [StringLength(EntityStringFieldLengths.Driver.LicenseNumber)]
        public string LicenseNumber { get; set; }

        [Column(TypeName = "date")]
        public DateTime? NextPhysicalDueDate { get; set; }

        public Guid? Guid { get; set; }

        [StringLength(EntityStringFieldLengths.Driver.TypeOfLicense)]
        public string TypeOfLicense { get; set; }

        public DateTime? LicenseExpirationDate { get; set; }

        public DateTime? LastPhysicalDate { get; set; }

        public DateTime? LastMvrDate { get; set; }

        public DateTime? NextMvrDueDate { get; set; }

        public DateTime? DateOfHire { get; set; }

        [Column(TypeName = "date")]
        public DateTime? TerminationDate { get; set; }

        [StringLength(EntityStringFieldLengths.Driver.EmployeeId)]
        public string EmployeeId { get; set; }

        //we can add this later if it is needed
        //public bool HasMaterialCompanyDrivers { get; set; }

        /// <summary>
        /// HaulingCompany's order line id. Only set for MaterialCompany drivers when a copy of this driver exists on another HaulingCompany tenant.
        /// </summary>
        public int? HaulingCompanyDriverId { get; set; }
        /// <summary>
        /// HaulingCompany's tenant id. Only set for MaterialCompany drivers when a copy of this driver exists on another HaulingCompany tenant.
        /// </summary>
        public int? HaulingCompanyTenantId { get; set; }

        public virtual ICollection<RequestedLeaseHaulerTruck> RequestedLeaseHaulerTrucks { get; set; }
    }
}
