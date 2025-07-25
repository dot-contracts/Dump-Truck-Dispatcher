using System;
using System.Linq;
using System.Text;

namespace DispatcherWeb.QuickbooksDesktop.Models
{
    public class CustomerRow : Row
    {
        public override string RowType => RowTypes.Customer;

        public static CustomerRowHeader HeaderRow = new CustomerRowHeader();

        /// <summary>
        /// (Required) The name of the customer. If a job name is included, the customer name appears first. If you are creating an import file, use a colon (:) to separate the customer name from the job name.
        /// </summary>
        public virtual string Name { get; set; }
        public virtual string BillingAddress1 { get; set; }
        public virtual string BillingAddress2 { get; set; }
        public virtual string BillingAddress3 { get; set; }
        public virtual string BillingAddress4 { get; set; }
        public virtual string BillingAddress5 { get; set; }
        public virtual string ShippingAddress1 { get; set; }
        public virtual string ShippingAddress2 { get; set; }
        public virtual string ShippingAddress3 { get; set; }
        public virtual string ShippingAddress4 { get; set; }
        public virtual string ShippingAddress5 { get; set; }
        public virtual string Phone1 { get; set; }
        public virtual string Phone2 { get; set; }
        public virtual string FaxNumber { get; set; }
        public virtual string Email { get; set; }
        public virtual string Note { get; set; }
        /// <summary>
        /// The name of your primary contact with the customer.
        /// </summary>
        public virtual string ContactName1 { get; set; }
        /// <summary>
        /// The name of an alternate contact with the customer.
        /// </summary>
        public virtual string ContactName2 { get; set; }

        /// <summary>
        /// Your classification for the customer. (QuickBooks calls this a "customer type."). If you import a customer type that is not on your Customer Type list, QuickBooks adds the new customer type to the list.
        /// </summary>
        public virtual string CustomerType { get; set; } //CTYPE

        /// <summary>
        /// The customer's payment terms with your company.
        /// </summary>
        public virtual string Terms { get; set; }

        /// <summary>
        /// Indicates whether you can charge sales tax to the customer. If you are creating an import file, type one of the following in the TAXABLE field:
        /// </summary>
        public virtual string Taxable { get; set; }

        /// <summary>
        /// The customer's credit limit with your company. If you are creating an import file, enter the dollar amount.
        /// </summary>
        public virtual string Limit { get; set; }

        /// <summary>
        /// The customer's resale number.
        /// </summary>
        public virtual string ResaleNumber { get; set; }

        /// <summary>
        /// Specifies the sales representative who deals with the customer. The sales representative will be added to the Sales Rep list and either the Vendor list, Employee list, or Other Names list.
        /// Enter the following items, separated by colons(for example, John Smith:2:JS).
        /// Name: (Required) First and last name of the sales representative.
        /// ListID: (Required) Number of the list to which the sales representative should be added.
        ///     1. Vendor
        ///     2. Employee
        ///     3. Other Names
        /// Initials: (Required) Initials of the sales representative.
        /// </summary>
        public virtual string SalesRepresentative { get; set; } //Rep

        /// <summary>
        /// The name of the tax item you have assigned to this customer. The name you enter must correspond to one of the sales tax items on your Item list.
        /// </summary>
        public virtual string TaxItem { get; set; }

        /// <summary>
        /// Your notes about the customer. If you are creating an import file, the notes appear in the Notepad window for the customer.
        /// </summary>
        public virtual string Notepad { get; set; }

        /// <summary>
        /// The customer's salutation, or title (Mr., Ms., Doctor, etc.).
        /// </summary>
        public virtual string Salutation { get; set; }

        /// <summary>
        /// The name of the customer's company.
        /// </summary>
        public virtual string CompanyName { get; set; }

        /// <summary>
        /// The customer's first name.
        /// </summary>
        public virtual string FirstName { get; set; }

        /// <summary>
        /// The customer's middle initial.
        /// </summary>
        public virtual string MiddleInitial { get; set; }

        /// <summary>
        /// The customer's last name.
        /// </summary>
        public virtual string LastName { get; set; }


        public CustomerRow SetBillingAddress(QuickbooksOnline.Dto.PhysicalAddressDto address)
        {
            var addressLines = GetAddressLines(address);
            if (!addressLines.Any())
            {
                return this;
            }
            BillingAddress1 = addressLines.FirstOrDefault();
            BillingAddress2 = addressLines.Skip(1).FirstOrDefault();
            BillingAddress3 = addressLines.Skip(2).FirstOrDefault();
            BillingAddress4 = addressLines.Skip(3).FirstOrDefault();
            BillingAddress5 = addressLines.Skip(4).FirstOrDefault();
            return this;
        }

        public CustomerRow SetShippingAddress(QuickbooksOnline.Dto.PhysicalAddressDto address)
        {
            var addressLines = GetAddressLines(address);
            if (!addressLines.Any())
            {
                return this;
            }
            ShippingAddress1 = addressLines.FirstOrDefault();
            ShippingAddress2 = addressLines.Skip(1).FirstOrDefault();
            ShippingAddress3 = addressLines.Skip(2).FirstOrDefault();
            ShippingAddress4 = addressLines.Skip(3).FirstOrDefault();
            ShippingAddress5 = addressLines.Skip(4).FirstOrDefault();
            return this;
        }

        private string[] GetAddressLines(QuickbooksOnline.Dto.PhysicalAddressDto address)
        {
            var addressFormatted = Utilities.FormatAddress2(address.Address1, address.Address2, address.City, address.State, address.ZipCode);
            if (string.IsNullOrEmpty(addressFormatted))
            {
                return Array.Empty<string>();
            }
            return addressFormatted.Replace("\r", "").Split("\n");
        }

        public override StringBuilder AppendRow(StringBuilder s)
        {
            return AppendTabSeparatedLine(s, new[]
            {
                RowType,
                Name,
                BillingAddress1,
                BillingAddress2,
                BillingAddress3,
                BillingAddress4,
                BillingAddress5,
                ShippingAddress1,
                ShippingAddress2,
                ShippingAddress3,
                ShippingAddress4,
                ShippingAddress5,
                Phone1,
                Phone2,
                FaxNumber,
                Email,
                Note,
                ContactName1,
                ContactName2,
                CustomerType,
                Terms,
                Taxable,
                Limit,
                ResaleNumber,
                SalesRepresentative,
                TaxItem,
                Notepad,
                Salutation,
                CompanyName,
                FirstName,
                MiddleInitial,
                LastName,
            });
        }

        public class CustomerRowHeader : CustomerRow
        {
            public override string RowType => "!" + base.RowType;
            public override string Name => "NAME";
            public override string BillingAddress1 => "BADDR1";
            public override string BillingAddress2 => "BADDR2";
            public override string BillingAddress3 => "BADDR3";
            public override string BillingAddress4 => "BADDR4";
            public override string BillingAddress5 => "BADDR5";
            public override string ShippingAddress1 => "SADDR1";
            public override string ShippingAddress2 => "SADDR2";
            public override string ShippingAddress3 => "SADDR3";
            public override string ShippingAddress4 => "SADDR4";
            public override string ShippingAddress5 => "SADDR5";
            public override string Phone1 => "PHONE1";
            public override string Phone2 => "PHONE2";
            public override string FaxNumber => "FAXNUM";
            public override string Email => "EMAIL";
            public override string Note => "NOTE";
            public override string ContactName1 => "CONT1";
            public override string ContactName2 => "CONT2";
            public override string CustomerType => "CTYPE";
            public override string Terms => "TERMS";
            public override string Taxable => "TAXABLE";
            public override string Limit => "LIMIT";
            public override string ResaleNumber => "RESALENUM";
            public override string SalesRepresentative => "REP";
            public override string TaxItem => "TAXITEM";
            public override string Notepad => "NOTEPAD";
            public override string Salutation => "SALUTATION";
            public override string CompanyName => "COMPANYNAME";
            public override string FirstName => "FIRSTNAME";
            public override string MiddleInitial => "MIDINIT";
            public override string LastName => "LASTNAME";
        }
    }
}
