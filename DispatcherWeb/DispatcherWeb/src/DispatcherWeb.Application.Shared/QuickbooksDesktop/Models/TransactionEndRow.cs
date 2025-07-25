using System.Text;

namespace DispatcherWeb.QuickbooksDesktop.Models
{
    public class TransactionEndRow : Row
    {
        public override string RowType => RowTypes.TransactionEnd;

        public static TransactionEndRowHeader HeaderRow = new TransactionEndRowHeader();


        public override StringBuilder AppendRow(StringBuilder s)
        {
            return AppendTabSeparatedLine(s, new[]
            {
                RowType,
            });
        }

        public class TransactionEndRowHeader : TransactionEndRow
        {
            public override string RowType => "!" + base.RowType;
        }
    }
}
