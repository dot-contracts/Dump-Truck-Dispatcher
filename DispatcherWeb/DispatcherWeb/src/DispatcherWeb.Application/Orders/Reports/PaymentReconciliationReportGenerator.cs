using System.Linq;
using DispatcherWeb.Infrastructure.MigraDoc;
using DispatcherWeb.Orders.Dto;
using MigraDocCore.DocumentObjectModel;
using MigraDocCore.DocumentObjectModel.Tables;

namespace DispatcherWeb.Orders.Reports
{
    public static class PaymentReconciliationReportGenerator
    {
        public static byte[] GenerateReport(PaymentReconciliationReportDto model)
        {
            Document document = new Document();

            Section section = document.AddSection();
            section.PageSetup = document.DefaultPageSetup.Clone();
            section.PageSetup.Orientation = Orientation.Landscape;
            section.PageSetup.PageFormat = PageFormat.Letter;
            section.PageSetup.PageHeight = Unit.FromInch(11);
            section.PageSetup.PageWidth = Unit.FromInch(8.5);
            section.PageSetup.TopMargin = Unit.FromCentimeter(1.2);
            section.PageSetup.LeftMargin = Unit.FromCentimeter(1.2);
            section.PageSetup.BottomMargin = Unit.FromCentimeter(1.2);
            section.PageSetup.RightMargin = Unit.FromCentimeter(1.2);
            section.PageSetup.HeaderDistance = Unit.FromCentimeter(0.6);

            Style style = document.Styles[StyleNames.Normal];
            style.Font.Name = "Times New Roman";
            style.Font.Size = Unit.FromPoint(12);
            style.ParagraphFormat.SpaceAfter = Unit.FromCentimeter(0.2);

            var tableStyle = document.Styles.AddStyle("Table", StyleNames.Normal);
            tableStyle.Font.Name = "Times New Roman";
            tableStyle.Font.Size = Unit.FromPoint(9.75); //11
            tableStyle.ParagraphFormat.SpaceAfter = 0;

            var headerStyle = document.Styles[StyleNames.Header];
            headerStyle.Font.Name = "Times New Roman";
            headerStyle.Font.Size = Unit.FromPoint(10);
            Paragraph paragraph = new Paragraph();
            paragraph.AddText("Page ");
            paragraph.AddPageField();
            paragraph.AddText(" of ");
            paragraph.AddNumPagesField();
            section.Headers.Primary.Add(paragraph);
            section.Headers.EvenPage.Add(paragraph.Clone());

            paragraph = document.LastSection.AddParagraph($"Payment Reconciliation Report for {model.StartDate.ToShortDateString()} through {model.EndDate.ToShortDateString()}");
            paragraph.Format.Font.Size = Unit.FromPoint(18);
            paragraph.Format.SpaceAfter = Unit.FromCentimeter(0.0);

            var itemGroups = model.Items.OrderBy(x => x.TransactionDate).GroupBy(x => x.OfficeId).ToList();

            if (itemGroups.Any())
            {

                foreach (var group in itemGroups)
                {
                    var officeName = group.FirstOrDefault()?.OfficeName;
                    if (!string.IsNullOrEmpty(officeName))
                    {
                        paragraph = document.LastSection.AddParagraph($"{officeName} Office");
                        paragraph.Format.Font.Size = Unit.FromPoint(14);
                        paragraph.Format.SpaceBefore = Unit.FromCentimeter(0.6);
                        paragraph.Format.SpaceAfter = Unit.FromCentimeter(0.4);
                        paragraph.Format.KeepWithNext = true;
                    }

                    Table table = document.LastSection.AddTable();
                    table.Style = "Table";
                    table.Borders.Width = Unit.FromPoint(1);
                    var tm = new TextMeasurement(TextMeasurementHelper.GetXGraphics(), document.Styles["Table"].Font.Clone());

                    //Job Site was removed, so now 3.8 cm can be added to the remaining columns
                    //Customer
                    table.AddColumn(Unit.FromCentimeter(3.7));
                    //Delivery Date
                    table.AddColumn(Unit.FromCentimeter(1.9));
                    //Transaction Type
                    table.AddColumn(Unit.FromCentimeter(1.9));
                    //Card Type
                    table.AddColumn(Unit.FromCentimeter(1));
                    //Card Last 4
                    table.AddColumn(Unit.FromCentimeter(1.2));
                    //Authorization Amount
                    table.AddColumn(Unit.FromCentimeter(2.6));
                    //Capture Amount
                    table.AddColumn(Unit.FromCentimeter(2.6));
                    //TransactionDate
                    table.AddColumn(Unit.FromCentimeter(3.2));
                    //TransactionId
                    table.AddColumn(Unit.FromCentimeter(2.2));
                    //BatchSummaryId
                    table.AddColumn(Unit.FromCentimeter(1.6));

                    Row row = table.AddRow();
                    row.Shading.Color = Colors.LightGray;
                    //row.Format.Font.Size = Unit.FromPoint(9);
                    //row.Format.Font.Bold = true;
                    //row.Format.Alignment = ParagraphAlignment.Center;
                    //row.Height = Unit.FromCentimeter(0.5);
                    row.HeadingFormat = true;

                    int i = 0;
                    Cell cell = row.Cells[i++];
                    cell.AddParagraph("Customer");
                    cell = row.Cells[i++];
                    cell.AddParagraph("Delivery Date");
                    cell = row.Cells[i++];
                    cell.AddParagraph("Transaction Type");
                    cell = row.Cells[i++];
                    cell.AddParagraph("Card Type");
                    cell = row.Cells[i++];
                    cell.AddParagraph("Card Last 4");
                    cell = row.Cells[i++];
                    cell.AddParagraph("Authorization Amount");
                    cell = row.Cells[i++];
                    cell.AddParagraph("Capture Amount");
                    cell = row.Cells[i++];
                    cell.AddParagraph("Transaction Date");
                    cell = row.Cells[i++];
                    cell.AddParagraph("TransactionId");
                    cell = row.Cells[i++];
                    cell.AddParagraph("Batch Summary Id");

                    if (group.Any())
                    {
                        foreach (var item in group)
                        {
                            i = 0;
                            row = table.AddRow();
                            cell = row.Cells[i++];
                            paragraph = cell.AddParagraph(item.CustomerName, tm);
                            cell = row.Cells[i++];
                            paragraph = cell.AddParagraph(item.DeliveryDate.ToShortDateString(), tm);
                            cell = row.Cells[i++];
                            paragraph = cell.AddParagraph(item.TransactionType, tm);
                            cell = row.Cells[i++];
                            paragraph = cell.AddParagraph(item.CardType, tm);
                            cell = row.Cells[i++];
                            paragraph = cell.AddParagraph(item.CardLast4, tm);
                            cell = row.Cells[i++];
                            paragraph = cell.AddParagraph(item.AuthorizationAmount?.ToString("C2", item.CurrencyCulture), tm);
                            cell = row.Cells[i++];
                            paragraph = cell.AddParagraph(item.CaptureAmount?.ToString("C2", item.CurrencyCulture), tm);
                            cell = row.Cells[i++];
                            paragraph = cell.AddParagraph(item.TransactionDate?.ConvertTimeZoneTo(item.TimeZone).ToString("g"), tm);
                            cell = row.Cells[i++];
                            paragraph = cell.AddParagraph(item.TransactionId, tm);
                            cell = row.Cells[i++];
                            paragraph = cell.AddParagraph(item.BatchSummaryId, tm);
                        }
                    }
                    else
                    {
                        table.AddRow();
                    }

                    table.SetEdge(0, 0, table.Columns.Count, table.Rows.Count, Edge.Box, BorderStyle.Single, 1, Colors.Black);
                }
            }
            else
            {
                paragraph = document.LastSection.AddParagraph($"No records for {model.OfficeName} Office");
                paragraph.Format.Font.Size = Unit.FromPoint(14);
                paragraph.Format.SpaceBefore = Unit.FromCentimeter(0.6);
                paragraph.Format.SpaceAfter = Unit.FromCentimeter(0.4);
            }

            return document.SaveToBytesArray();
        }
    }
}
