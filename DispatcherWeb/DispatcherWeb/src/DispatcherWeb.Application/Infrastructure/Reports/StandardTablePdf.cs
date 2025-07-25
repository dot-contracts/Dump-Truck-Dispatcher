using System;
using System.Diagnostics;
using MigraDocCore.DocumentObjectModel;
using MigraDocCore.DocumentObjectModel.Tables;

namespace DispatcherWeb.Infrastructure.Reports
{
    public class StandardTablePdf
    {
        protected Table _table;
        protected int _columnNumber;

        public StandardTablePdf(Section section)
        {
            CreateTable(section);
        }

        public StandardTablePdf(Section section, int columnNumber) : this(section)
        {
            _columnNumber = columnNumber;

            AddColumns();
        }
        public StandardTablePdf(Section section, double[] columnWidths) : this(section)
        {
            _columnNumber = columnWidths.Length;

            AddColumns(columnWidths);
        }

        public void AddRow(params string[] columns)
        {
            if (columns.Length != _columnNumber)
            {
                throw new ArgumentException("Number of columns must be equal column number.");
            }
            Row row = _table.AddRow();

            for (int i = 0; i < columns.Length; i++)
            {
                row.Cells[i].AddParagraph(columns[i] ?? "");
                row.Cells[i].Format.Alignment = ParagraphAlignment.Left;
            }
        }

        public void AddColumnHeaders(params string[] headers)
        {
            AddColumnHeaders(Colors.White, headers);
        }
        public void AddColumnHeaders(Color shadingColor, params string[] headers)
        {
            if (headers.Length != _columnNumber)
            {
                throw new ArgumentException("Number of headers must be equal column number.");
            }

            Row row = _table.AddRow();
            row.HeadingFormat = true;
            row.Format.Alignment = ParagraphAlignment.Center;
            row.Format.Font.Bold = true;
            row.Shading.Color = shadingColor;

            for (int i = 0; i < headers.Length; i++)
            {
                if (row.Cells[i].Column.Width.Value == 0)
                {
                    continue;
                }
                row.Cells[i].AddParagraph(headers[i]);
                row.Cells[i].Format.Alignment = ParagraphAlignment.Left;
            }

        }

        public void AddTableHeader(string header)
        {
            Row row = _table.AddRow();
            row.HeadingFormat = true;
            row.Format.Alignment = ParagraphAlignment.Left;
            row.Format.Font.Bold = true;
            row.Shading.Color = TableColors.HeaderColor;
            row.Cells[0].AddParagraph(header);
            row.Cells[0].Format.Font.Bold = false;
            row.Cells[0].Format.Alignment = ParagraphAlignment.Left;
            row.Cells[0].VerticalAlignment = VerticalAlignment.Bottom;
            row.Cells[0].MergeRight = _table.Columns.Count - 1;
        }

        private void CreateTable(Section section)
        {
            _table = section.AddTable();
            _table.Style = "Table";
            _table.Borders.Color = TableColors.BorderColor;
            _table.Borders.Width = 0.25;
            _table.Borders.Left.Width = 0.5;
            _table.Borders.Right.Width = 0.5;
            _table.Rows.LeftIndent = 0;
            _table.TopPadding = 5;
            _table.BottomPadding = 5;
        }

        private void AddColumns()
        {
            for (int i = 0; i < _columnNumber; i++)
            {
                Column column = _table.AddColumn(Unit.FromCentimeter(18.5 / _columnNumber));
                column.Format.Alignment = ParagraphAlignment.Left;
            }
        }
        protected void AddColumns(double[] columnWidths, int? columnNumber = null)
        {
            Debug.Assert(columnNumber == null || columnNumber.Value <= columnWidths.Length);
            for (int i = 0; i < (columnNumber ?? columnWidths.Length); i++)
            {
                Column column = _table.AddColumn(Unit.FromCentimeter(columnWidths[i]));
                column.Format.Alignment = ParagraphAlignment.Left;
            }
        }
    }
}
