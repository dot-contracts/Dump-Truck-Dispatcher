using System.Text;

namespace DispatcherWeb.QuickbooksDesktop.Models
{
    public abstract class Row
    {
        protected const string Tab = "\t";
        public abstract string RowType { get; }

        public abstract StringBuilder AppendRow(StringBuilder s);

        protected StringBuilder AppendTabSeparatedLine(StringBuilder s, string[] values)
        {
            for (var i = 0; i < values.Length; i++)
            {
                if (i > 0)
                {
                    s.Append(Tab);
                }
                s.Append(RemoveRestrictedCharacters(values[i] ?? ""));
            }
            s.AppendLine();
            return s;
        }

        protected string RemoveRestrictedCharacters(string val)
        {
            return val?./*Replace(":", "").*/Replace("\t", " ").Replace("\r", "").Replace("\n", " ");
        }
    }
}
