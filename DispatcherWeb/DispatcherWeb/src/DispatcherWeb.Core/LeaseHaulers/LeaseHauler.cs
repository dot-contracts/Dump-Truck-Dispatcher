using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using DispatcherWeb.Infrastructure;
using DispatcherWeb.Insurances;
using DispatcherWeb.LeaseHaulerRequests;

namespace DispatcherWeb.LeaseHaulers
{
    [Table("LeaseHauler")]
    public class LeaseHauler : FullAuditedEntity, IMustHaveTenant
    {
        public LeaseHauler()
        {
            LeaseHaulerContacts = new HashSet<LeaseHaulerContact>();
            LeaseHaulerDrivers = new HashSet<LeaseHaulerDriver>();
            LeaseHaulerTrucks = new HashSet<LeaseHaulerTruck>();
            LeaseHaulerUsers = new HashSet<LeaseHaulerUser>();
            LeaseHaulerInsurances = new HashSet<Insurance>();
        }

        public int TenantId { get; set; }

        /// <summary>
        /// HaulingCompany's Tenant id. Only set for MaterialCompany LeaseHaulers which represent a whole HaulingCompany tenant
        /// </summary>
        public int? HaulingCompanyTenantId { get; set; }

        [Required]
        [StringLength(EntityStringFieldLengths.LeaseHauler.Name)]
        public string Name { get; set; }

        [StringLength(EntityStringFieldLengths.GeneralAddress.MaxStreetAddressLength)]
        public string StreetAddress1 { get; set; }

        [StringLength(EntityStringFieldLengths.GeneralAddress.MaxStreetAddressLength)]
        public string StreetAddress2 { get; set; }

        [StringLength(EntityStringFieldLengths.GeneralAddress.MaxCityLength)]
        public string City { get; set; }

        [StringLength(EntityStringFieldLengths.GeneralAddress.MaxStateLength)]
        public string State { get; set; }

        [StringLength(EntityStringFieldLengths.GeneralAddress.MaxZipCodeLength)]
        public string ZipCode { get; set; }

        [StringLength(EntityStringFieldLengths.GeneralAddress.MaxCountryCodeLength)]
        public string CountryCode { get; set; }

        [StringLength(EntityStringFieldLengths.LeaseHauler.AccountNumber)]
        public string AccountNumber { get; set; }

        [StringLength(EntityStringFieldLengths.LeaseHauler.PhoneNumber)]
        public string PhoneNumber { get; set; }

        public bool IsActive { get; set; }

        [StringLength(EntityStringFieldLengths.GeneralAddress.MaxStreetAddressLength)]
        public string MailingAddress1 { get; set; }

        [StringLength(EntityStringFieldLengths.GeneralAddress.MaxStreetAddressLength)]
        public string MailingAddress2 { get; set; }

        [StringLength(EntityStringFieldLengths.GeneralAddress.MaxCityLength)]
        public string MailingCity { get; set; }

        [StringLength(EntityStringFieldLengths.GeneralAddress.MaxStateLength)]
        public string MailingState { get; set; }

        [StringLength(EntityStringFieldLengths.GeneralAddress.MaxCountryCodeLength)]
        public string MailingCountryCode { get; set; }

        [StringLength(EntityStringFieldLengths.GeneralAddress.MaxZipCodeLength)]
        public string MailingZipCode { get; set; }

        [StringLength(EntityStringFieldLengths.LeaseHauler.MotorCarrierNumber)]
        public string MotorCarrierNumber { get; set; }

        [StringLength(EntityStringFieldLengths.LeaseHauler.DeptOfTransportationNumber)]
        public string DeptOfTransportationNumber { get; set; }

        [StringLength(EntityStringFieldLengths.LeaseHauler.EinOrTin)]
        public string EinOrTin { get; set; }

        public DateTime? HireDate { get; set; }

        public DateTime? TerminationDate { get; set; }

        public virtual ICollection<LeaseHaulerContact> LeaseHaulerContacts { get; set; }

        public virtual ICollection<LeaseHaulerDriver> LeaseHaulerDrivers { get; set; }

        public virtual ICollection<LeaseHaulerTruck> LeaseHaulerTrucks { get; set; }

        public virtual ICollection<LeaseHaulerUser> LeaseHaulerUsers { get; set; }

        public virtual ICollection<AvailableLeaseHaulerTruck> AvailableLeaseHaulerTrucks { get; set; }

        public virtual ICollection<Insurance> LeaseHaulerInsurances { get; set; }
    }
}
