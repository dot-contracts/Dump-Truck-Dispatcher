using System.Linq;
using DispatcherWeb.Infrastructure.MigraDoc;
using DispatcherWeb.Orders.Dto;
using MigraDocCore.DocumentObjectModel;
using MigraDocCore.DocumentObjectModel.Tables;

namespace DispatcherWeb.Orders.Reports
{
    public static class OrderSummaryReportGenerator
    {
        public static byte[] GenerateReport(OrderSummaryReportDto model)
        {
            Document document = new Document();

            Section section = document.AddSection();
            section.PageSetup = document.DefaultPageSetup.Clone();
            //section.PageSetup.Orientation = Orientation.Landscape;
            section.PageSetup.PageFormat = PageFormat.Letter;
            section.PageSetup.PageHeight = Unit.FromInch(8.5);
            section.PageSetup.PageWidth = Unit.FromInch(11);
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

            paragraph = document.LastSection.AddParagraph("Summary Order Report for " + model.Date.ToShortDateString());
            paragraph.Format.Font.Size = Unit.FromPoint(18);
            paragraph.Format.SpaceAfter = Unit.FromCentimeter(0.7);


            Table table = document.LastSection.AddTable();
            table.Style = "Table";
            table.Borders.Width = Unit.FromPoint(1);
            var tm = new TextMeasurement(TextMeasurementHelper.GetXGraphics(), document.Styles["Table"].Font.Clone());

            //Delivery Date
            table.AddColumn(Unit.FromCentimeter(1.8));
            //Clock in time
            //table.AddColumn(Unit.FromCentimeter(1.6));
            //Customer
            table.AddColumn(Unit.FromCentimeter(2.7));
            //Items
            table.AddColumn(Unit.FromCentimeter(7));
            //Load At
            table.AddColumn(Unit.FromCentimeter(2.6));
            //Deliver To
            table.AddColumn(Unit.FromCentimeter(2.7));
            if (!model.HidePrices)
            {
                //Total
                table.AddColumn(Unit.FromCentimeter(2.1));
            }
            //# Of Trucks
            table.AddColumn(Unit.FromCentimeter(1.2));
            //Trucks
            table.AddColumn(Unit.FromCentimeter(3.8));

            Row row = table.AddRow();
            row.Shading.Color = Colors.LightGray;
            //row.Format.Font.Size = Unit.FromPoint(9);
            //row.Format.Font.Bold = true;
            //row.Format.Alignment = ParagraphAlignment.Center;
            //row.Height = Unit.FromCentimeter(0.5);
            row.HeadingFormat = true;

            int i = 0;
            Cell cell = row.Cells[i++];
            cell.AddParagraph(model.UseShifts ? "Delivery Date / Shift" : "Delivery Date");
            cell = row.Cells[i++];
            cell.AddParagraph("Customer");
            cell = row.Cells[i++];
            cell.AddParagraph("Items");
            cell = row.Cells[i++];
            cell.AddParagraph("Load At");
            cell = row.Cells[i++];
            cell.AddParagraph("Deliver To");
            if (!model.HidePrices)
            {
                cell = row.Cells[i++];
                cell.AddParagraph("Total");
            }
            cell = row.Cells[i++];
            cell.AddParagraph("# Of Trucks");
            cell = row.Cells[i++];
            cell.AddParagraph("Trucks");

            if (model.Items.Any())
            {
                foreach (var item in model.Items)
                {
                    i = 0;
                    row = table.AddRow();
                    cell = row.Cells[i++];
                    paragraph = cell.AddParagraph(item.OrderDeliveryDate?.ToShortDateString() + (model.UseShifts ? $" {item.OrderShiftName}" : ""), tm);
                    cell = row.Cells[i++];
                    paragraph = cell.AddParagraph(item.CustomerName, tm);
                    cell = row.Cells[i++];
                    paragraph = cell.AddParagraph(OrderItemFormatter.GetItemWithQuantityFormatted(item.Item), tm);
                    cell = row.Cells[i++];
                    paragraph = cell.AddParagraph(item.LoadAtName, tm); //jobSite.WithMaxLength(50, true) - job site length was limited, but location name wasn't
                    cell = row.Cells[i++];
                    paragraph = cell.AddParagraph(item.DeliverToName, tm); //jobSite.WithMaxLength(50, true) - job site length was limited, but location name wasn't
                    if (!model.HidePrices)
                    {
                        cell = row.Cells[i++];
                        paragraph = cell.AddParagraph(item.Item.OrderLineTotal.ToString("C2", model.CurrencyCulture) ?? "", tm);
                    }
                    cell = row.Cells[i++];
                    paragraph = cell.AddParagraph(item.NumberOfTrucks?.ToString() ?? "", tm);
                    cell = row.Cells[i++];
                    paragraph = cell.AddParagraph(item.TrucksString, tm);
                }
            }
            else
            {
                table.AddRow();
            }

            table.SetEdge(0, 0, table.Columns.Count, table.Rows.Count, Edge.Box, BorderStyle.Single, 1, Colors.Black);


            return document.SaveToBytesArray();
        }
    }
}
