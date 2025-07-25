using CsvHelper;

namespace DispatcherWeb.Infrastructure.Reports
{
    public abstract class TableCsvBase : IAddColumnHeaders
    {
        protected readonly CsvWriter _csv;

        protected TableCsvBase(CsvWriter csv)
        {
            _csv = csv;
        }
        public void AddColumnHeaders(params string[] headers)
        {
            foreach (string header in headers)
            {
                if (header == null)
                {
                    continue;
                }
                _csv.WriteField(header);
            }
            _csv.NextRecord();
        }


    }
}
