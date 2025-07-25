using System;
using System.ComponentModel.DataAnnotations;
using DispatcherWeb.Infrastructure;

namespace DispatcherWeb.LeaseHaulers.Dto
{
    public class LeaseHaulerEditDto
    {
        public int? Id { get; set; }

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

        [StringLength(EntityStringFieldLengths.LeaseHauler.AccountNumber)]
        public string AccountNumber { get; set; }

        [StringLength(EntityStringFieldLengths.LeaseHauler.MotorCarrierNumber)]
        public string MotorCarrierNumber { get; set; }

        [StringLength(EntityStringFieldLengths.LeaseHauler.DeptOfTransportationNumber)]
        public string DeptOfTransportationNumber { get; set; }

        [StringLength(EntityStringFieldLengths.LeaseHauler.EinOrTin)]
        public string EinOrTin { get; set; }

        public DateTime? HireDate { get; set; }

        public DateTime? TerminationDate { get; set; }

        [StringLength(EntityStringFieldLengths.LeaseHauler.PhoneNumber)]
        public string PhoneNumber { get; set; }

        public bool IsActive { get; set; }

        public int? HaulingCompanyTenantId { get; set; }
    }
}
