using System.Linq;
using CsvHelper;
using DispatcherWeb.Authorization.Users;
using DispatcherWeb.Imports.Columns;
using DispatcherWeb.Infrastructure;

namespace DispatcherWeb.Imports.RowReaders
{
    public class EmployeeImportRow : ImportRow
    {
        public EmployeeImportRow(CsvReader csv, ILookup<string, string> fieldMap) : base(csv, fieldMap)
        {
        }

        public string Name => GetString(EmployeeColumn.Name, User.MaxNameLength);
        public string Phone => GetString(EmployeeColumn.Phone, EntityStringFieldLengths.General.PhoneNumber);
        public string Email => GetString(EmployeeColumn.Email, EntityStringFieldLengths.General.Email);
        public bool SendEmail => GetBoolean(EmployeeColumn.SendEmail);
        public OrderNotifyPreferredFormat? NotifyPreferredFormat
        {
            get
            {
                var value = GetString(EmployeeColumn.NotifyPreferredFormat, 100);
                switch (value?.ToLower())
                {
                    case "sms": return OrderNotifyPreferredFormat.Sms;
                    case "email": return OrderNotifyPreferredFormat.Email;
                    case "both": return OrderNotifyPreferredFormat.Both;
                }
                return OrderNotifyPreferredFormat.Neither;
            }
        }
        public string Address => GetString(EmployeeColumn.Address, EntityStringFieldLengths.GeneralAddress.MaxStreetAddressLength);
        public string City => GetString(EmployeeColumn.City, EntityStringFieldLengths.GeneralAddress.MaxCityLength);
        public string State => GetString(EmployeeColumn.State, EntityStringFieldLengths.GeneralAddress.MaxStateLength);
        public string Zip => GetString(EmployeeColumn.Zip, EntityStringFieldLengths.GeneralAddress.MaxZipCodeLength);
    }
}
