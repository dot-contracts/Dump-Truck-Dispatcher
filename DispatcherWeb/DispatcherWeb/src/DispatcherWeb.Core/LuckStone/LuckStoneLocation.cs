using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using DispatcherWeb.Infrastructure;

namespace DispatcherWeb.LuckStone
{
    [Table("LuckStoneLocation")]
    public class LuckStoneLocation : Entity
    {
        [StringLength(EntityStringFieldLengths.LuckStoneLocation.Site)]
        public string Site { get; set; } //the lookup key

        [StringLength(EntityStringFieldLengths.Location.Name)]
        public string Name { get; set; }

        [StringLength(EntityStringFieldLengths.GeneralAddress.MaxStreetAddressLength)]
        public string StreetAddress { get; set; }

        [StringLength(EntityStringFieldLengths.GeneralAddress.MaxCityLength)]
        public string City { get; set; }

        [StringLength(EntityStringFieldLengths.GeneralAddress.MaxStateLength)]
        public string State { get; set; }

        [StringLength(EntityStringFieldLengths.GeneralAddress.MaxZipCodeLength)]
        public string ZipCode { get; set; }

        [StringLength(EntityStringFieldLengths.GeneralAddress.MaxCountryCodeLength)]
        public string CountryCode { get; set; }

        public decimal? Latitude { get; set; }

        public decimal? Longitude { get; set; }
    }
}
