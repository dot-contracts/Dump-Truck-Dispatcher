using System.Linq;
using System.Text;

namespace DispatcherWeb.QuickbooksDesktop.Models
{
    public class TransactionRow : Row
    {
        public override string RowType => RowTypes.Transaction;

        public static TransactionRowHeader HeaderRow = new TransactionRowHeader();

        /// <summary>
        /// A unique number that identifies the transaction.
        /// </summary>
        public virtual string TransactionId { get; set; }

        /// <summary>
        /// (Required) A keyword that identifies the type of transaction. These keywords can appear in the TRNSTYPE field:
        /// see TransactionTypes
        /// </summary>
        public virtual string TransactionType { get; set; }

        /// <summary>
        /// The date of the transaction. The date is always in MM/DD/YY format. For example, 1/30/94.
        /// </summary>
        public virtual string Date { get; set; }

        /// <summary>
        /// (Bills and invoices only) The due date of the bill payment or invoice payment. The date is always in MM/DD/YY format. For example, 1/30/98.
        /// </summary>
        public virtual string DueDate { get; set; }

        /// <summary>
        /// (Required) The name of the account assigned to the transaction.
        /// </summary>
        public virtual string Account { get; set; }

        /// <summary>
        /// The name of the customer, vendor, payee, or employee.
        /// </summary>
        public virtual string Name { get; set; }

        /// <summary>
        /// The name of the class that applies to the transaction. If the class is a subclass, the class name includes the names of the parent classes, beginning with the highest level class. A colon (:) separates each class name.
        /// </summary>
        public virtual string Class { get; set; }

        /// <summary>
        /// (Required) The amount of the transaction. Debit amounts are always positive, credit amounts are always negative.
        /// </summary>
        public virtual string Amount { get; set; }

        /// <summary>
        /// The number of the transaction. For checks, the number is the check number; for invoices, the number is the invoice number; etc.
        /// </summary>
        public virtual string DocNumber { get; set; }

        /// <summary>
        /// The memo text associated with the transaction.
        /// </summary>
        public virtual string Memo { get; set; }

        /// <summary>
        /// (Invoices, credit memos, and sales receipts only) Your message to the customer—as it appears on the invoice, credit memo, or sales receipt.
        /// </summary>
        public virtual string InvoiceMemo { get; set; }

        /// <summary>
        /// (Invoices and credit memos only) The customer's purchase order number.
        /// </summary>
        public virtual string PoNumber { get; set; }

        /// <summary>
        /// Indicates whether the transaction has cleared. These keywords can appear in the CLEAR field: Y/N
        /// </summary>
        public virtual string HasCleared { get; set; }

        /// <summary>
        /// Indicates whether a check, invoice, credit memo, or sales receipt has been marked as "To be printed." These keywords can appear in the TOPRINT field: Y/N
        /// </summary>
        public virtual string ToBePrinted { get; set; }

        /// <summary>
        /// (Invoices and sales receipts only) Indicates whether the customer whose name appears in the transaction is taxable. (Y/N)
        /// </summary>
        public virtual string NameIsTaxable { get; set; }

        /// <summary>
        /// The first line of the customer's, vendor's, payee's, or employee's address.
        /// </summary>
        public virtual string Address1 { get; set; }
        public virtual string Address2 { get; set; }
        public virtual string Address3 { get; set; }
        public virtual string Address4 { get; set; }
        public virtual string Address5 { get; set; }

        /// <summary>
        /// (Invoices only) The terms of the invoice.
        /// </summary>
        public virtual string Terms { get; set; }

        /// <summary>
        /// (Invoices and sales receipts only) The method you used to ship the merchandise.
        /// </summary>
        public virtual string ShipVia { get; set; }

        /// <summary>
        /// (Invoices and sales receipts only) The shipping date. If you are creating an import file, enter the date in MM/DD/YY format. For example, 1/30/94.
        /// </summary>
        public virtual string ShipDate { get; set; }


        public TransactionRow SetAddress(params string[] addressParts)
        {
            var address = string.Join("\n", addressParts);
            if (string.IsNullOrEmpty(address))
            {
                return this;
            }
            var addressLines = address.Replace("\r", "").Split("\n");
            Address1 = addressLines.FirstOrDefault();
            Address2 = addressLines.Skip(1).FirstOrDefault();
            Address3 = addressLines.Skip(2).FirstOrDefault();
            Address4 = addressLines.Skip(3).FirstOrDefault();
            Address5 = addressLines.Skip(4).FirstOrDefault();
            return this;
        }

        public override StringBuilder AppendRow(StringBuilder s)
        {
            return AppendTabSeparatedLine(s, new[]
            {
                RowType,
                TransactionId,
                TransactionType,
                Date,
                DueDate,
                Account,
                Name,
                Class,
                Amount,
                DocNumber,
                Memo,
                InvoiceMemo,
                PoNumber,
                HasCleared,
                ToBePrinted,
                NameIsTaxable,
                Address1,
                Address2,
                Address3,
                Address4,
                Address5,
                Terms,
                ShipVia,
                ShipDate,
            });
        }

        public class TransactionRowHeader : TransactionRow
        {
            public override string RowType => "!" + base.RowType;
            public override string TransactionId => "TRNSID";
            public override string TransactionType => "TRNSTYPE";
            public override string Date => "DATE";
            public override string DueDate => "DUEDATE";
            public override string Account => "ACCNT";
            public override string Name => "NAME";
            public override string Class => "CLASS";
            public override string Amount => "AMOUNT";
            public override string DocNumber => "DOCNUM";
            public override string Memo => "MEMO";
            public override string InvoiceMemo => "INVMEMO";
            public override string PoNumber => "PONUM";
            public override string HasCleared => "CLEAR";
            public override string ToBePrinted => "TOPRINT";
            public override string NameIsTaxable => "NAMEISTAXABLE";
            public override string Address1 => "ADDR1";
            public override string Address2 => "ADDR2";
            public override string Address3 => "ADDR3";
            public override string Address4 => "ADDR4";
            public override string Address5 => "ADDR5";
            public override string Terms => "TERMS";
            public override string ShipVia => "SHIPVIA";
            public override string ShipDate => "SHIPDATE";
        }
    }
}