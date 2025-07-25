using System.IO;
using System.Linq;
using Abp.Extensions;
using DispatcherWeb.Infrastructure.MigraDoc;
using DispatcherWeb.Quotes.Dto;
using MigraDocCore.DocumentObjectModel;
using MigraDocCore.DocumentObjectModel.Shapes;
using MigraDocCore.DocumentObjectModel.Tables;

namespace DispatcherWeb.Quotes
{
    public static class QuoteReportGenerator
    {
        public static byte[] GenerateReport(QuoteReportDto model)
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
            style.Font.Size = Unit.FromPoint(9.5);

            var tableStyle = document.Styles.AddStyle("Table", StyleNames.Normal);
            tableStyle.Font.Name = "Times New Roman";
            tableStyle.Font.Size = Unit.FromPoint(9);
            tableStyle.ParagraphFormat.SpaceAfter = 0;

            style = document.Styles[StyleNames.Header];
            style.Font.Name = "Times New Roman";
            Paragraph paragraph = new Paragraph();
            paragraph.AddText("Page ");
            paragraph.AddPageField();
            paragraph.AddText(" of ");
            paragraph.AddNumPagesField();
            section.Headers.Primary.Add(paragraph);
            section.Headers.EvenPage.Add(paragraph.Clone());

            if (model.LogoBytes?.Length > 0)
            {
                var logo = section.AddImage(model.LogoBytes);
                logo.Height = Unit.FromCentimeter(3.2);
                //logo.Width = Unit.FromCentimeter(4.5);
                logo.LockAspectRatio = true;
                logo.RelativeVertical = RelativeVertical.Page;
                logo.RelativeHorizontal = RelativeHorizontal.Page;
                logo.WrapFormat.Style = WrapStyle.Through;
                logo.WrapFormat.DistanceLeft = Unit.FromCentimeter(11.5);
                logo.WrapFormat.DistanceTop = Unit.FromCentimeter(1.4);
            }

            paragraph = document.LastSection.AddParagraph("Customer:");
            paragraph.Format.Font.Bold = true;

            paragraph = document.LastSection.AddParagraph(model.CustomerName ?? "");
            paragraph = document.LastSection.AddParagraph(model.CustomerAddress1 ?? "");
            if (!model.CustomerAddress2.IsNullOrEmpty())
            {
                paragraph = document.LastSection.AddParagraph(model.CustomerAddress2 ?? "");
            }
            paragraph = document.LastSection.AddParagraph(model.CustomerCity + " " + model.CustomerState + " " + model.CustomerZipCode + " " + model.CustomerCountryCode);
            paragraph = document.LastSection.AddParagraph("Attn: ");
            paragraph.AddText(model.ContactAttn ?? "");
            paragraph = document.LastSection.AddParagraph(model.ContactPhoneNumber ?? "");
            paragraph.Format.SpaceAfter = Unit.FromCentimeter(1.5);

            paragraph = document.LastSection.AddParagraph("Quote Number: ");
            paragraph.AddText(model.QuoteId.ToString());
            paragraph.Format.AddTabStop(Unit.FromCentimeter(10));
            paragraph.AddTab();
            paragraph.AddText("Proposal Date: ");
            paragraph.AddText(model.QuoteProposalDate?.ToShortDateString() ?? "");

            paragraph = document.LastSection.AddParagraph("Quote Name: ");
            paragraph.AddText(model.QuoteName ?? "");
            paragraph.Format.AddTabStop(Unit.FromCentimeter(10));
            paragraph.AddTab();
            paragraph.AddText("Proposal Expires: ");
            paragraph.AddText(model.QuoteProposalExpiryDate?.ToShortDateString() ?? "");

            paragraph = document.LastSection.AddParagraph();
            paragraph = document.LastSection.AddParagraph();
            paragraph.Format.AddTabStop(Unit.FromCentimeter(10));
            paragraph.AddTab();
            paragraph.AddText("Salesperson:");

            paragraph = document.LastSection.AddParagraph("PO Number: ");
            paragraph.AddText(model.QuotePoNumber ?? "");
            paragraph.Format.AddTabStop(Unit.FromCentimeter(10));
            paragraph.AddTab();
            paragraph.AddText(model.UserFullName ?? "");

            paragraph = document.LastSection.AddParagraph();
            paragraph.Format.AddTabStop(Unit.FromCentimeter(10));
            paragraph.AddTab();
            paragraph.AddText(model.UserEmail ?? "");
            paragraph.Format.SpaceAfter = Unit.FromCentimeter(0.7);



            Table table = document.LastSection.AddTable();
            table.Style = "Table";
            table.Borders.Width = Unit.FromPoint(1);
            var tm = new TextMeasurement(TextMeasurementHelper.GetXGraphics(), document.Styles["Table"].Font.Clone());

            //Code
            table.AddColumn(Unit.FromCentimeter(1.5));
            //Description
            table.AddColumn(Unit.FromCentimeter(2.2));
            if (model.ShowTruckCategories)
            {
                //Truck Categories
                table.AddColumn(Unit.FromCentimeter(2.2));
            }
            //Designation
            table.AddColumn(Unit.FromCentimeter(2.2));
            if (!model.HideLoadAt)
            {
                //Load At
                table.AddColumn(Unit.FromCentimeter(2.6));
            }
            //Deliver To
            table.AddColumn(Unit.FromCentimeter(2.6));
            //Qty
            table.AddColumn(Unit.FromCentimeter(3));
            //Rate
            table.AddColumn(Unit.FromCentimeter(2.5));

            Row row = table.AddRow();
            row.Shading.Color = Colors.LightGray;
            row.Format.Font.Size = Unit.FromPoint(7);
            row.Format.Font.Bold = true;
            row.Format.Alignment = ParagraphAlignment.Center;
            row.Height = Unit.FromCentimeter(0.4);
            row.HeadingFormat = true;

            int i = 0;
            Cell cell = row.Cells[i++];
            cell.AddParagraph(model.SeparateItems ? "Freight" : "Item");
            cell = row.Cells[i++];
            cell.AddParagraph(model.SeparateItems ? "Material" : "Description");
            if (model.ShowTruckCategories)
            {
                cell = row.Cells[i++];
                cell.AddParagraph("Truck Categories");
            }
            cell = row.Cells[i++];
            cell.AddParagraph("Designation");
            if (!model.HideLoadAt)
            {
                cell = row.Cells[i++];
                cell.AddParagraph("Load At");
            }
            cell = row.Cells[i++];
            cell.AddParagraph("Deliver To");
            cell = row.Cells[i++];
            cell.AddParagraph("Quantity");
            cell = row.Cells[i++];
            cell.AddParagraph("Rate");

            if (model.Items.Any())
            {
                foreach (var item in model.Items)
                {
                    i = 0;
                    row = table.AddRow();
                    cell = row.Cells[i++];
                    paragraph = cell.AddParagraph(item.FreightItemName, tm);
                    paragraph.Format.Alignment = ParagraphAlignment.Center;
                    cell = row.Cells[i++];
                    paragraph = cell.AddParagraph(model.SeparateItems ? item.MaterialItemName : item.FreightItemDescription, tm);
                    if (model.ShowTruckCategories)
                    {
                        cell = row.Cells[i++];
                        paragraph = cell.AddParagraph(string.Join(", ", item.QuoteLineVehicleCategories), tm);
                    }
                    cell = row.Cells[i++];
                    paragraph = cell.AddParagraph(item.DesignationName, tm);
                    if (!model.HideLoadAt)
                    {
                        cell = row.Cells[i++];
                        paragraph = cell.AddParagraph(item.LoadAtName, tm);
                        paragraph.Format.Alignment = ParagraphAlignment.Center;
                    }
                    cell = row.Cells[i++];
                    paragraph = cell.AddParagraph(item.DeliverToName, tm);
                    paragraph.Format.Alignment = ParagraphAlignment.Center;
                    cell = row.Cells[i++];
                    paragraph = cell.AddParagraph(item.QuantityFormatted, tm);
                    paragraph.Format.Alignment = ParagraphAlignment.Center;
                    cell = row.Cells[i++];
                    paragraph = cell.AddParagraph(item.Rate?.ToString("C2", model.CurrencyCulture) ?? "", tm);
                    paragraph.Format.Alignment = ParagraphAlignment.Center;
                    if (!string.IsNullOrEmpty(item.Note))
                    {
                        i = 0;
                        row = table.AddRow();
                        cell = row.Cells[i++];
                        cell.MergeRight = 5 + (model.HideLoadAt ? 0 : 1) + (model.ShowTruckCategories ? 1 : 0);
                        paragraph = cell.AddParagraph(item.Note, tm);
                    }
                }
            }
            else
            {
                table.AddRow();
            }

            table.SetEdge(0, 0, table.Columns.Count, table.Rows.Count, Edge.Box, BorderStyle.Single, 1, Colors.Black);
            //table.Format.SpaceAfter = Unit.FromCentimeter(0.7);


            paragraph = document.LastSection.AddParagraph("Notes: ");
            paragraph.Format.SpaceBefore = Unit.FromCentimeter(0.7);
            paragraph.Format.KeepWithNext = true;
            paragraph = document.LastSection.AddParagraph();
            paragraph.Format.KeepWithNext = true;
            paragraph = document.LastSection.AddParagraph();
            paragraph.Format.KeepWithNext = true;

            paragraph = document.LastSection.AddParagraph(model.QuoteNotesFormatted ?? "");
            paragraph.Format.SpaceAfter = Unit.FromCentimeter(0.2);

            // Signatures

            table = document.LastSection.AddTable();
            table.KeepTogether = true;
            //table.Style = "Table";
            //table.Borders.Width = Unit.FromPoint(1);
            table.AddColumn(Unit.FromCentimeter(3));
            table.AddColumn(Unit.FromCentimeter(8.5));
            table.AddColumn(Unit.FromCentimeter(2));
            table.AddColumn(Unit.FromCentimeter(1.2));
            table.AddColumn(Unit.FromCentimeter(4.1));

            row = table.AddRow();
            row.KeepWith = 1;
            //row.Height = Unit.FromCentimeter(1);
            row.VerticalAlignment = VerticalAlignment.Bottom;
            i = 0;
            cell = row.Cells[i++];
            paragraph = cell.AddParagraph(model.UserFullName);
            paragraph.Format.SpaceBefore = Unit.FromCentimeter(2);
            cell = row.Cells[i++];
            cell.Borders.Bottom.Visible = true;
            if (model.SignatureBytes?.Length > 0)
            {
#pragma warning disable CA1416 // Validate platform compatibility
                using (var ms = new MemoryStream(model.SignatureBytes))
                using (var signatureImage = System.Drawing.Image.FromStream(ms))
                {
                    var signature = cell.AddImage(model.SignatureBytes);
                    signature.LockAspectRatio = true;
                    var ratio = (decimal)signatureImage.Width / signatureImage.Height;
                    if (ratio > 3)
                    {
                        signature.Width = Unit.FromCentimeter(6);
                    }
                    else
                    {
                        signature.Height = Unit.FromCentimeter(2);
                    }
                }
#pragma warning restore CA1416 // Validate platform compatibility
            }

            cell = row.Cells[i++];
            cell = row.Cells[i++];
            paragraph = cell.AddParagraph("Date");
            cell = row.Cells[i++];
            cell.Borders.Bottom.Visible = true;
            paragraph = cell.AddParagraph(model.Today.ToShortDateString());

            row = table.AddRow();
            row.Height = Unit.FromCentimeter(2);
            row.VerticalAlignment = VerticalAlignment.Bottom;
            i = 0;
            cell = row.Cells[i++];
            paragraph = cell.AddParagraph("Customer Signature");
            cell = row.Cells[i++];
            cell.Borders.Bottom.Visible = true;

            cell = row.Cells[i++];
            cell = row.Cells[i++];
            paragraph = cell.AddParagraph("Date");
            cell = row.Cells[i++];
            cell.Borders.Bottom.Visible = true;


            // Second Page

            if (!string.IsNullOrEmpty(model.QuoteGeneralTermsAndConditions))
            {
                section.AddPageBreak();
                paragraph = document.LastSection.AddParagraph();
                paragraph.Format.Font.Size = Unit.FromPoint(7.5);

                paragraph.AddLineBreak();
                paragraph.AddLineBreak();
                paragraph.AddLineBreak();
                paragraph.AddText(model.QuoteGeneralTermsAndConditions);
                paragraph.AddLineBreak();
                paragraph.AddLineBreak();
            }

            return document.SaveToBytesArray();
        }
    }
}
