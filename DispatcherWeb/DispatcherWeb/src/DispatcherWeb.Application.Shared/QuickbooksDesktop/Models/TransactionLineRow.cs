using System.Text;

namespace DispatcherWeb.QuickbooksDesktop.Models
{
    public class TransactionLineRow : Row
    {
        public override string RowType => RowTypes.TransactionLine;

        public static TransactionLineRowHeader HeaderRow = new TransactionLineRowHeader();

        /// <summary>
        /// (Required) A unique number that identifies the distribution line in the transaction.
        /// </summary>
        public virtual string TransactionLineId { get; set; }

        /// <summary>
        /// A keyword that indicates the type of transaction. The keyword in this field must match the keyword in the TRNSTYPE field for the transaction. The keyword will be one of the following:
        /// </summary>
        public virtual string TransactionType { get; set; }

        /// <summary>
        /// The date of the transaction. The date in this field must match the date in the DATE field for the transaction.
        /// </summary>
        public virtual string Date { get; set; }

        /// <summary>
        /// Not documented
        /// </summary>
        public virtual string ServiceDate { get; set; }

        /// <summary>
        /// (Required) The income or expense account to which you assigned the amount on the distribution line.
        /// </summary>
        public virtual string Account { get; set; }

        /// <summary>
        /// The name of the customer, vendor, payee, or employee.
        /// </summary>
        public virtual string Name { get; set; }

        /// <summary>
        /// The name of the class that applies to the distribution amount. If the class is a subclass, the class name includes the names of the parent classes, beginning with the highest level class. A colon (:) separates each class name.
        /// </summary>
        public virtual string Class { get; set; }

        /// <summary>
        /// (Required) The distribution amount. Credit amounts are negative.
        /// </summary>
        public virtual string Amount { get; set; }

        /// <summary>
        /// The number of the transaction. For checks, the number is the check number; for invoices, the number is the invoice number, etc.
        /// </summary>
        public virtual string DocNumber { get; set; }

        /// <summary>
        /// The memo text associated with the distribution line.
        /// </summary>
        public virtual string Memo { get; set; }

        /// <summary>
        /// Indicates whether the distribution amount has cleared. These keywords can appear in the CLEAR field: Y/N
        /// </summary>
        public virtual string HasCleared { get; set; }

        /// <summary>
        /// The number of items sold. This value is part of a line item on an invoice, credit memo, or sales receipt.
        /// </summary>
        public virtual string Quantity { get; set; }

        /// <summary>
        /// The unit cost of the item.
        /// </summary>
        public virtual string Price { get; set; }

        /// <summary>
        /// The type of items sold. This value is part of a line item on an invoice, credit memo, or sales receipt.
        /// </summary>
        public virtual string Item { get; set; }

        /// <summary>
        /// Indicates that a line item on an invoice, credit memo, or sales receipt is taxable.
        /// </summary>
        public virtual string Taxable { get; set; }
        public virtual string Other2 { get; set; }
        public virtual string YearToDate { get; set; }
        public virtual string WageBase { get; set; }
        public virtual string Extra { get; set; }


        public override StringBuilder AppendRow(StringBuilder s)
        {
            return AppendTabSeparatedLine(s, new[]
            {
                RowType,
                TransactionLineId,
                TransactionType,
                Date,
                ServiceDate,
                Account,
                Name,
                Class,
                Amount,
                DocNumber,
                Memo,
                HasCleared,
                Quantity,
                Price,
                Item,
                Taxable,
                Other2,
                YearToDate,
                WageBase,
                Extra,
            });
        }

        public class TransactionLineRowHeader : TransactionLineRow
        {
            public override string RowType => "!" + base.RowType;
            public override string TransactionLineId => "SPLID";
            public override string TransactionType => "TRNSTYPE";
            public override string Date => "DATE";
            public override string ServiceDate => "SERVICEDATE";
            public override string Account => "ACCNT";
            public override string Name => "NAME";
            public override string Class => "CLASS";
            public override string Amount => "AMOUNT";
            public override string DocNumber => "DOCNUM";
            public override string Memo => "MEMO";
            public override string HasCleared => "CLEAR";
            public override string Quantity => "QNTY";
            public override string Price => "PRICE";
            public override string Item => "INVITEM";
            public override string Taxable => "TAXABLE";
            public override string Other2 => "OTHER2";
            public override string YearToDate => "YEARTODATE";
            public override string WageBase => "WAGEBASE";
            public override string Extra => "EXTRA";
        }
    }
}
