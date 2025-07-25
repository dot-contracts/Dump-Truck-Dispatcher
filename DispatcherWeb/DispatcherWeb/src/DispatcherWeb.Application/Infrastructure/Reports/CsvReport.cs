using CsvHelper;

namespace DispatcherWeb.Infrastructure.Reports
{
    public class CsvReport : IReport
    {
        private readonly CsvWriter _csv;

        public CsvReport(CsvWriter csv)
        {
            _csv = csv;
        }

        public CsvWriter CsvWriter => _csv;

        public void AddReportHeader(string text)
        {
            AddRow(text);
        }

        public void AddHeader(string text)
        {
            AddRow(text);
        }

        public void AddText(string text)
        {
            AddRow(text);
        }


        private void AddRow(string text)
        {
            _csv.WriteField(text);

            _csv.NextRecord();
        }

        public void AddEmptyLine()
        {
            _csv.NextRecord();
        }

    }
}
