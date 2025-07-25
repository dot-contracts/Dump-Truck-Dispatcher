using System;
using System.Linq;
using DispatcherWeb.Dispatching.Dto;
using DispatcherWeb.Infrastructure.MigraDoc;
using DispatcherWeb.Tickets;
using MigraDocCore.DocumentObjectModel;
using MigraDocCore.DocumentObjectModel.Tables;

namespace DispatcherWeb.Dispatching.Reports
{
    public class DriverActivityDetailReportGenerator
    {
        public Document GenerateReport(DriverActivityDetailReportDto input)
        {
            Document document = new Document();

            Section section = document.AddSection();
            section.PageSetup = document.DefaultPageSetup.Clone();
            //section.PageSetup.Orientation = Orientation.Landscape;
            section.PageSetup.PageFormat = PageFormat.Letter;
            section.PageSetup.PageHeight = Unit.FromInch(11);
            section.PageSetup.PageWidth = Unit.FromInch(8.5);
            section.PageSetup.TopMargin = Unit.FromCentimeter(1.5);
            section.PageSetup.LeftMargin = Unit.FromCentimeter(1.5);
            section.PageSetup.BottomMargin = Unit.FromCentimeter(1.5);
            section.PageSetup.RightMargin = Unit.FromCentimeter(1.5);
            section.PageSetup.HeaderDistance = Unit.FromCentimeter(0.6);

            Style style = document.Styles[StyleNames.Normal];
            style.Font.Name = "Times New Roman";
            style.Font.Size = Unit.FromPoint(12);
            style.ParagraphFormat.SpaceAfter = Unit.FromCentimeter(0.2);

            var tableStyle = document.Styles.AddStyle("Table", StyleNames.Normal);
            tableStyle.Font.Name = "Times New Roman";
            tableStyle.Font.Size = Unit.FromPoint(11);
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

            var firstPage = true;

            foreach (var model in input.Pages)
            {
                if (firstPage)
                {
                    firstPage = false;
                }
                else
                {
                    section.AddPageBreak();
                }


                paragraph = document.LastSection.AddParagraph("Driver Activity Report");
                paragraph.Format.Font.Size = Unit.FromPoint(18);
                paragraph.Format.SpaceAfter = Unit.FromCentimeter(0.7);
                paragraph.Format.Alignment = ParagraphAlignment.Center;

                var firstColumnMargin = Unit.FromCentimeter(1);
                var secondColumnMargin = Unit.FromCentimeter(7.5);


                paragraph = document.LastSection.AddParagraph();
                paragraph.AddFormattedText("Date: ", TextFormat.Bold);
                paragraph.AddText(model.Date.ToString("d"));
                paragraph.Format.AddTabStop(secondColumnMargin);
                paragraph.AddTab();
                paragraph.AddFormattedText("Scheduled Start Time: ", TextFormat.Bold);
                paragraph.AddText(model.ScheduledStartTime?.ToShortTimeString() ?? "-");

                paragraph = document.LastSection.AddParagraph();
                paragraph.AddFormattedText("Name: ", TextFormat.Bold);
                paragraph.AddText(model.DriverName ?? "");
                paragraph.Format.AddTabStop(secondColumnMargin);
                paragraph.AddTab();
                paragraph.AddText(model.CarrierName ?? "");

                var elapsedTime = TimeSpan.FromSeconds(0);
                foreach (var employeeTime in model.EmployeeTimes)
                {
                    paragraph = document.LastSection.AddParagraph();
                    paragraph.AddFormattedText("Clock-in: ", TextFormat.Bold);
                    paragraph.AddText(employeeTime.ClockInTime.ConvertTimeZoneTo(input.Timezone).ToString("t") ?? "");
                    paragraph.Format.AddTabStop(secondColumnMargin);
                    paragraph.AddSpace(12);
                    paragraph.AddText(employeeTime.TimeClassificationName);
                    paragraph.AddTab();
                    paragraph.AddFormattedText("Clock-out: ", TextFormat.Bold);
                    paragraph.AddText(employeeTime.ClockOutTime?.ConvertTimeZoneTo(input.Timezone).ToString("t") ?? "");
                    if (employeeTime.ClockOutTime.HasValue)
                    {
                        elapsedTime += employeeTime.ClockOutTime.Value - employeeTime.ClockInTime;
                    }
                }

                paragraph = document.LastSection.AddParagraph();
                paragraph.AddFormattedText("Elapsed Time: ", TextFormat.Bold);
                paragraph.AddText(Math.Round(elapsedTime.TotalHours, 2).ToString("0.##") + " Hours");




                if (model.Loads.Any())
                {
                    foreach (var loadGroup in model.Loads.GroupBy(x => new
                    {
                        x.CustomerName,
                        x.JobNumber,
                        x.LoadAtName,
                        x.DeliverToName,
                        x.TruckCode,
                        x.TrailerTruckCode,
                        x.VehicleCategory,
                        x.QuantityOrdered,
                        UomName = x.GetAmountTypeToUse().useMaterial ? (x.MaterialUomName ?? x.FreightUomName) : x.FreightUomName,
                        x.ItemName,
                    }))
                    {
                        paragraph = document.LastSection.AddParagraph();
                        paragraph.Format.SpaceBefore = Unit.FromCentimeter(0.7);
                        paragraph.Format.AddTabStop(firstColumnMargin);
                        paragraph.AddTab();
                        paragraph.AddFormattedText("Customer: ", TextFormat.Bold);
                        paragraph.AddText(loadGroup.Key.CustomerName ?? "");

                        paragraph.Format.AddTabStop(secondColumnMargin);
                        paragraph.AddTab();
                        paragraph.AddFormattedText("Job Number: ", TextFormat.Bold);
                        paragraph.AddText(loadGroup.Key.JobNumber ?? "");

                        paragraph = document.LastSection.AddParagraph();
                        paragraph.Format.AddTabStop(firstColumnMargin);
                        paragraph.AddTab();
                        paragraph.AddFormattedText("Load At: ", TextFormat.Bold);
                        paragraph.AddText(loadGroup.Key.LoadAtName ?? "");

                        paragraph = document.LastSection.AddParagraph();
                        paragraph.Format.AddTabStop(firstColumnMargin);
                        paragraph.AddTab();
                        paragraph.AddFormattedText("Deliver To: ", TextFormat.Bold);
                        paragraph.AddText(loadGroup.Key.DeliverToName ?? "");

                        paragraph = document.LastSection.AddParagraph();
                        paragraph.Format.AddTabStop(firstColumnMargin);
                        paragraph.AddTab();
                        paragraph.AddFormattedText("Truck: ", TextFormat.Bold);
                        paragraph.AddText(loadGroup.Key.TruckCode ?? "");

                        if (loadGroup.Key.TrailerTruckCode != null)
                        {
                            paragraph = document.LastSection.AddParagraph();
                            paragraph.Format.AddTabStop(firstColumnMargin);
                            paragraph.AddTab();
                            paragraph.AddFormattedText("Trailer: ", TextFormat.Bold);
                            paragraph.AddText(loadGroup.Key.VehicleCategory + " " + loadGroup.Key.TrailerTruckCode);
                        }

                        paragraph = document.LastSection.AddParagraph();
                        paragraph.Format.LeftIndent = firstColumnMargin;
                        paragraph.AddFormattedText("Item: ", TextFormat.Bold);
                        paragraph.AddText(loadGroup.Key.ItemName ?? "");

                        paragraph.Format.AddTabStop(firstColumnMargin);
                        paragraph.AddTab();
                        paragraph.AddFormattedText("Quantity Ordered: ", TextFormat.Bold);
                        paragraph.AddText(loadGroup.Key.QuantityOrdered.HasValue ? loadGroup.Key.QuantityOrdered?.ToString(Utilities.NumberFormatWithoutRounding) + " " + loadGroup.Key.UomName : "");

                        paragraph.Format.AddTabStop(firstColumnMargin);
                        paragraph.AddTab();
                        paragraph.AddFormattedText("Quantity Delivered: ", TextFormat.Bold);
                        decimal? quantityDelivered = 0;
                        foreach (var load in loadGroup)
                        {
                            quantityDelivered += load.QuantityDelivered;
                        }
                        paragraph.AddText(quantityDelivered.HasValue ? quantityDelivered?.ToString(Utilities.NumberFormatWithoutRounding) + " " + loadGroup.Key.UomName : "");

                        paragraph.Format.SpaceAfter = Unit.FromCentimeter(0.7);


                        Table table = document.LastSection.AddTable();
                        table.Style = "Table";
                        table.Borders.Width = Unit.FromPoint(1);
                        table.Rows.LeftIndent = firstColumnMargin;
                        var tm = new TextMeasurement(TextMeasurementHelper.GetXGraphics(), document.Styles["Table"].Font.Clone());

                        //Load Time
                        table.AddColumn(Unit.FromCentimeter(3.2)); //1.9 is still too small to fit 12:34 PM on one line
                        //Load Ticket
                        table.AddColumn(Unit.FromCentimeter(3.2));
                        //Delivery Time
                        table.AddColumn(Unit.FromCentimeter(3.2));
                        //Cycle Time
                        table.AddColumn(Unit.FromCentimeter(3.2));
                        //Quantity
                        table.AddColumn(Unit.FromCentimeter(3.2));

                        Row row = table.AddRow();
                        row.Shading.Color = Colors.LightGray;
                        row.Format.Font.Size = Unit.FromPoint(9);
                        row.Format.Font.Bold = true;
                        row.Format.Alignment = ParagraphAlignment.Center;
                        row.Height = Unit.FromCentimeter(0.5);
                        row.HeadingFormat = true;

                        int i = 0;
                        Cell cell = row.Cells[i++];
                        cell.AddParagraph("Load Time");
                        cell = row.Cells[i++];
                        cell.AddParagraph("Load Ticket");
                        cell = row.Cells[i++];
                        cell.AddParagraph("Delivery Time");
                        cell = row.Cells[i++];
                        cell.AddParagraph("Cycle Time");
                        cell = row.Cells[i++];
                        cell.AddParagraph("Quantity");

                        foreach (var load in loadGroup)
                        {
                            i = 0;
                            row = table.AddRow();
                            cell = row.Cells[i++];
                            paragraph = cell.AddParagraph(load.LoadTime?.ConvertTimeZoneTo(input.Timezone).ToShortTimeString(), tm);
                            cell = row.Cells[i++];
                            paragraph = cell.AddParagraph(load.LoadTicket, tm);
                            paragraph.Format.Alignment = ParagraphAlignment.Center;
                            cell = row.Cells[i++];
                            paragraph = cell.AddParagraph(load.DeliveryTime?.ConvertTimeZoneTo(input.Timezone).ToShortTimeString(), tm);
                            paragraph.Format.Alignment = ParagraphAlignment.Center;
                            cell = row.Cells[i++];
                            paragraph = cell.AddParagraph(load.CycleTime?.ToString("h\\:mm") ?? "", tm);
                            paragraph.Format.Alignment = ParagraphAlignment.Center;
                            cell = row.Cells[i++];
                            var uomName = load.GetAmountTypeToUse().useMaterial ? (load.MaterialUomName ?? load.FreightUomName) : load.FreightUomName;
                            paragraph = cell.AddParagraph(load.QuantityDelivered?.ToString(Utilities.NumberFormatWithoutRounding) + " " + uomName ?? "", tm);
                        }

                        table.SetEdge(0, 0, table.Columns.Count, table.Rows.Count, Edge.Box, BorderStyle.Single, 1, Colors.Black);
                    }
                }
            }

            return document;
        }
    }
}
