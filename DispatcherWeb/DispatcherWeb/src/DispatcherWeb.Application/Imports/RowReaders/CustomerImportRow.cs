using System.Linq;
using CsvHelper;
using DispatcherWeb.Imports.Columns;
using DispatcherWeb.Infrastructure;

namespace DispatcherWeb.Imports.RowReaders
{
    public class CustomerImportRow : ImportRow
    {
        public CustomerImportRow(CsvReader csv, ILookup<string, string> fieldMap) : base(csv, fieldMap)
        {
        }

        public bool IsActive => GetBoolean(CustomerColumn.IsActive, true, "Active");
        public string Name => GetString(CustomerColumn.Name, EntityStringFieldLengths.Customer.Name);
        public string AccountNumber => GetString(CustomerColumn.AccountNumber, EntityStringFieldLengths.Customer.AccountNumber);
        public bool IsCod => GetBoolean(CustomerColumn.IsCod);
        public string InvoiceEmail => GetString(CustomerColumn.InvoiceEmail, EntityStringFieldLengths.General.Email);
        public string Terms => GetString(CustomerColumn.Terms, 100);

        public string Address1 => GetString(CustomerColumn.Address1, EntityStringFieldLengths.GeneralAddress.MaxStreetAddressLength);
        public string Address2 => GetString(CustomerColumn.Address2, EntityStringFieldLengths.GeneralAddress.MaxStreetAddressLength);
        public string City => GetString(CustomerColumn.City, EntityStringFieldLengths.GeneralAddress.MaxCityLength);
        public string State => GetString(CustomerColumn.State, EntityStringFieldLengths.GeneralAddress.MaxStateLength);
        public string ZipCode => GetString(CustomerColumn.ZipCode, EntityStringFieldLengths.GeneralAddress.MaxZipCodeLength);
        public string CountryCode => GetString(CustomerColumn.CountryCode, EntityStringFieldLengths.GeneralAddress.MaxCountryCodeLength);

        public string BillingAddress1 => GetString(CustomerColumn.BillingAddress1, EntityStringFieldLengths.GeneralAddress.MaxStreetAddressLength);
        public string BillingAddress2 => GetString(CustomerColumn.BillingAddress2, EntityStringFieldLengths.GeneralAddress.MaxStreetAddressLength);
        public string BillingCity => GetString(CustomerColumn.BillingCity, EntityStringFieldLengths.GeneralAddress.MaxCityLength);
        public string BillingState => GetString(CustomerColumn.BillingState, EntityStringFieldLengths.GeneralAddress.MaxStateLength);
        public string BillingZipCode => GetString(CustomerColumn.BillingZipCode, EntityStringFieldLengths.GeneralAddress.MaxZipCodeLength);
        public string BillingCountryCode => GetString(CustomerColumn.BillingCountryCode, EntityStringFieldLengths.GeneralAddress.MaxCountryCodeLength);


        public string ContactTitle => GetString(CustomerColumn.ContactTitle, EntityStringFieldLengths.CustomerContact.Title);
        public string ContactName => GetString(CustomerColumn.ContactName, EntityStringFieldLengths.CustomerContact.Name);
        public string ContactPhone => GetString(CustomerColumn.ContactPhone, EntityStringFieldLengths.General.PhoneNumber);
        public string ContactFax => GetString(CustomerColumn.ContactFax, EntityStringFieldLengths.General.PhoneNumber);
        public string ContactEmail => GetString(CustomerColumn.ContactEmail, EntityStringFieldLengths.General.Email);
        public string Contact2Title => GetString(CustomerColumn.Contact2Title, EntityStringFieldLengths.CustomerContact.Title);
        public string Contact2Name => GetString(CustomerColumn.Contact2Name, EntityStringFieldLengths.CustomerContact.Name);
        public string Contact2Phone => GetString(CustomerColumn.Contact2Phone, EntityStringFieldLengths.General.PhoneNumber);
        public string Contact2Email => GetString(CustomerColumn.Contact2Email, EntityStringFieldLengths.General.Email);
    }
}
