using System.Linq;
using CsvHelper;
using DispatcherWeb.Imports.Columns;
using DispatcherWeb.Infrastructure;

namespace DispatcherWeb.Imports.RowReaders
{
    public class VendorImportRow : ImportRow
    {
        public VendorImportRow(CsvReader csv, ILookup<string, string> fieldMap) : base(csv, fieldMap)
        {
        }

        public bool IsActive => GetBoolean(VendorColumn.IsActive, true, "Active");
        public string Name => GetString(VendorColumn.Name, EntityStringFieldLengths.Location.Name * 2);

        public string Address => GetString(VendorColumn.Address, EntityStringFieldLengths.GeneralAddress.MaxStreetAddressLength);
        public string City => GetString(VendorColumn.City, EntityStringFieldLengths.GeneralAddress.MaxCityLength);
        public string State => GetString(VendorColumn.State, EntityStringFieldLengths.GeneralAddress.MaxStateLength);
        public string ZipCode => GetString(VendorColumn.ZipCode, EntityStringFieldLengths.GeneralAddress.MaxZipCodeLength);
        public string CountryCode => GetString(VendorColumn.CountryCode, EntityStringFieldLengths.GeneralAddress.MaxCountryCodeLength);

        public string MainEmail => GetString(VendorColumn.MainEmail, EntityStringFieldLengths.General.Email);

        public string ContactTitle => GetString(VendorColumn.ContactTitle, EntityStringFieldLengths.CustomerContact.Title);
        public string ContactName => GetString(VendorColumn.ContactName, EntityStringFieldLengths.CustomerContact.Name);
        public string ContactPhone => GetString(VendorColumn.ContactPhone, EntityStringFieldLengths.General.PhoneNumber);
        public string ContactFax => GetString(VendorColumn.ContactFax, EntityStringFieldLengths.General.PhoneNumber);
        public string Contact2Name => GetString(VendorColumn.Contact2Name, EntityStringFieldLengths.CustomerContact.Name);
        public string Contact2Phone => GetString(VendorColumn.Contact2Phone, EntityStringFieldLengths.General.PhoneNumber);
    }
}
