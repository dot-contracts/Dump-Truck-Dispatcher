using System.Text;

namespace DispatcherWeb.QuickbooksDesktop.Models
{
    public class VendorRow : Row
    {
        public override string RowType => RowTypes.Vendor;

        public static VendorRowHeader HeaderRow = new VendorRowHeader();

        /// <summary>
        /// (Required) The name of the vendor.
        /// </summary>
        public virtual string Name { get; set; }

        /// <summary>
        /// The name you would like checks to be made out to. This field allows you to make checks out to a different name than the name that appears on your Vendor list.
        /// </summary>
        public virtual string PrintAs { get; set; }

        /// <summary>
        /// The first line of the vendor's address.
        /// </summary>
        public virtual string Address1 { get; set; }
        public virtual string Address2 { get; set; }
        public virtual string Address3 { get; set; }
        public virtual string Address4 { get; set; }
        public virtual string Address5 { get; set; }

        /// <summary>
        /// Your classification for the vendor (QuickBooks calls this a "vendor type"). If you import a vendor type that is not on your Vendor Type list, QuickBooks adds the new vendor type to the list.
        /// </summary>
        public virtual string VendorType { get; set; }

        /// <summary>
        /// The name of your primary contact with the vendor.
        /// </summary>
        public virtual string Contact1 { get; set; }
        public virtual string Contact2 { get; set; }
        public virtual string Phone1 { get; set; }
        public virtual string Phone2 { get; set; }
        public virtual string FaxNumber { get; set; }
        public virtual string Email { get; set; }

        /// <summary>
        /// A short note or phrase you want to associate with the vendor. QuickBooks automatically puts your note in the Memo field of checks you send to the vendor.
        /// </summary>
        public virtual string Note { get; set; }

        /// <summary>
        /// The vendor's tax identification number.
        /// </summary>
        public virtual string TaxId { get; set; }

        /// <summary>
        /// Your credit limit with the vendor. If you are creating an import file, enter the dollar amount.
        /// </summary>
        public virtual string Limit { get; set; }

        /// <summary>
        /// Your payment terms with the vendor.
        /// </summary>
        public virtual string Terms { get; set; }

        /// <summary>
        /// Your notes about the vendor. If you are creating an import file, the notes appear in the Notepad window for the vendor.
        /// </summary>
        public virtual string Notepad { get; set; }

        /// <summary>
        /// The vendor's salutation, or title (Mr., Ms., Doctor, etc.).
        /// </summary>
        public virtual string Salutation { get; set; }
        public virtual string CompanyName { get; set; }
        public virtual string FirstName { get; set; }
        public virtual string MiddleInitial { get; set; }
        public virtual string LastName { get; set; }

        public override StringBuilder AppendRow(StringBuilder s)
        {
            return AppendTabSeparatedLine(s, new[]
            {
                RowType,
                Name,
                PrintAs,
                Address1,
                Address2,
                Address3,
                Address4,
                Address5,
                VendorType,
                Contact1,
                Contact2,
                Phone1,
                Phone2,
                FaxNumber,
                Email,
                Note,
                TaxId,
                Limit,
                Terms,
                Notepad,
                Salutation,
                CompanyName,
                FirstName,
                MiddleInitial,
                LastName,
            });
        }

        public class VendorRowHeader : VendorRow
        {
            public override string RowType => "!" + base.RowType;
            public override string Name => "NAME";
            public override string PrintAs => "PRINTAS";
            public override string Address1 => "ADDR1";
            public override string Address2 => "ADDR2";
            public override string Address3 => "ADDR3";
            public override string Address4 => "ADDR4";
            public override string Address5 => "ADDR5";
            public override string VendorType => "VTYPE";
            public override string Contact1 => "CONT1";
            public override string Contact2 => "CONT2";
            public override string Phone1 => "PHONE1";
            public override string Phone2 => "PHONE2";
            public override string FaxNumber => "FAXNUM";
            public override string Email => "EMAIL";
            public override string Note => "NOTE";
            public override string TaxId => "TAXID";
            public override string Limit => "LIMIT";
            public override string Terms => "TERMS";
            public override string Notepad => "NOTEPAD";
            public override string Salutation => "SALUTATION";
            public override string CompanyName => "COMPANYNAME";
            public override string FirstName => "FIRSTNAME";
            public override string MiddleInitial => "MIDINIT";
            public override string LastName => "LASTNAME";
        }
    }
}