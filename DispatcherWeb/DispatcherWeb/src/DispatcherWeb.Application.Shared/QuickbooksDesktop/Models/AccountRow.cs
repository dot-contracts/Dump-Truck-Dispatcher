using System.Text;

namespace DispatcherWeb.QuickbooksDesktop.Models
{
    public class AccountRow : Row
    {
        public override string RowType => RowTypes.Account;

        public static AccountRowHeader HeaderRow = new AccountRowHeader();

        /// <summary>
        /// (Required) The name of an account in your chart of accounts. If the account is a subaccount, the account's name includes the names of the parent accounts, beginning with the highest level account. If you are creating an import file, use a colon (:) to separate subaccount names.
        /// </summary>
        public virtual string Name { get; set; }

        /// <summary>
        /// (Required) The type of account.If you are creating an import file, use one of the keywords below to indicate the account type:
        /// see AccountTypes
        /// </summary>
        public virtual string AccountType { get; set; }

        /// <summary>
        /// A brief description of the account.
        /// </summary>
        public virtual string Description { get; set; }

        /// <summary>
        /// The account number of the account.
        /// </summary>
        public virtual string AccountNumber { get; set; }

        /// <summary>
        /// Identifies an account as one of the special balance sheet accounts that QuickBooks automatically creates when the need for the account arises. If you are creating an import file, use one of these keywords to identify the account:
        /// OPENBAL - Opening Balance Equity
        /// RETEARNINGS - Retained Earnings
        /// SALESTAX - Sales Tax Payable
        /// UNDEPOSIT - Undeposited Funds
        /// </summary>
        public virtual string Extra { get; set; }

        public override StringBuilder AppendRow(StringBuilder s)
        {
            return AppendTabSeparatedLine(s, new[]
            {
                RowType,
                Name,
                AccountType,
                Description,
                AccountNumber,
                Extra,
            });
        }

        public class AccountRowHeader : AccountRow
        {
            public override string RowType => "!" + base.RowType;
            public override string Name => "NAME";
            public override string AccountType => "ACCNTTYPE";
            public override string Description => "DESC";
            public override string AccountNumber => "ACCNUM";
            public override string Extra => "EXTRA";
        }
    }
}
