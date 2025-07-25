using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Dependency;
using DispatcherWeb.Infrastructure.MigraDoc;
using DispatcherWeb.Invoices.Dto;
using MigraDocCore.DocumentObjectModel;
using MigraDocCore.DocumentObjectModel.Tables;

namespace DispatcherWeb.Invoices.Reports
{
    public class InvoicePrintOutGenerator9 : ITransientDependency
    {
        public InvoicePrintOutGenerator9()
        {
        }

        public Task<Document> GenerateReport(List<InvoicePrintOutDto> modelList)
        {
            Document document = new Document();

            Style style = document.Styles[StyleNames.Normal];
            style.Font.Name = "Times New Roman";
            style.Font.Size = Unit.FromPoint(11);
            style.ParagraphFormat.SpaceAfter = Unit.FromCentimeter(0.2);

            var tableStyle = document.Styles.AddStyle("Table", StyleNames.Normal);
            tableStyle.Font.Name = "Times New Roman";
            tableStyle.Font.Size = Unit.FromPoint(10);
            tableStyle.ParagraphFormat.SpaceAfter = 0;

            Paragraph paragraph;
            Table table;
            Row row;
            Cell cell;

            var firstInvoice = true;

            foreach (var model in modelList)
            {
                Section section = document.AddSection();
                section.PageSetup = document.DefaultPageSetup.Clone();
                //section.PageSetup.Orientation = Orientation.Landscape;
                section.PageSetup.PageFormat = PageFormat.Letter;
                section.PageSetup.PageHeight = Unit.FromInch(11); //27.94cm
                section.PageSetup.PageWidth = Unit.FromInch(8.5); //21.59cm -3cm margin = 18.6cm total
                section.PageSetup.TopMargin = Unit.FromCentimeter(1.5);
                section.PageSetup.LeftMargin = Unit.FromCentimeter(1.5);
                section.PageSetup.BottomMargin = Unit.FromCentimeter(1.5);
                section.PageSetup.RightMargin = Unit.FromCentimeter(1.5);
                section.PageSetup.FooterDistance = Unit.FromCentimeter(0.6);
                section.PageSetup.StartingNumber = 1;

                var footerStyle = document.Styles[StyleNames.Footer];
                footerStyle.Font.Name = "Times New Roman";
                footerStyle.Font.Size = Unit.FromPoint(10);
                paragraph = new Paragraph();
                paragraph.AddText("Page ");
                paragraph.AddPageField();
                paragraph.AddText(" of ");
                paragraph.AddNumPagesField();
                section.Footers.Primary.Add(paragraph);
                section.Footers.EvenPage.Add(paragraph.Clone());

                if (firstInvoice)
                {
                    firstInvoice = false;
                }
                else
                {
                    section.AddPageBreak();
                }

                //string taxWarning = null;
                //const string taxWarningAsterisks = "**";


                table = document.LastSection.AddTable();
                table.Style = "Table";
                if (model.DebugLayout)
                {
                    table.Borders.Width = Unit.FromPoint(1);
                }
                //todo, since we have different font sizes on a page
                var tm = new TextMeasurement(TextMeasurementHelper.GetXGraphics(), document.Styles["Table"].Font.Clone());

                //18.6cm total width
                //logo
                table.AddColumn(Unit.FromCentimeter(9.3));
                //Legal Name and Legal Address, Phone
                table.AddColumn(Unit.FromCentimeter(9.3));

                row = table.AddRow();
                row.Format.Font.Size = Unit.FromPoint(10);

                int i = 0;

                cell = row.Cells[i++];
                if (model.LogoBytes?.Length > 0)
                {
                    paragraph = cell.AddParagraph();
                    var logo = paragraph.AddImage(model.LogoBytes);
                    logo.Height = Unit.FromCentimeter(1.7);
                    //logo.Width = Unit.FromCentimeter(4.21);
                    logo.LockAspectRatio = true;
                }

                cell = row.Cells[i++];
                cell.Format.Alignment = ParagraphAlignment.Right; //.Left is default 
                //paragraph = cell.AddParagraph(model.LegalName ?? "");
                paragraph = cell.AddParagraph(model.LegalAddress ?? "");
                paragraph = cell.AddParagraph(model.LegalPhoneNumber ?? "");
                paragraph.Format.SpaceAfter = Unit.FromCentimeter(1.5);



                i = 0;
                row = table.AddRow();
                row.Format.Font.Size = Unit.FromPoint(11);
                cell = row.Cells[i++];
                paragraph = cell.AddParagraph(model.CustomerName ?? "");
                paragraph = cell.AddParagraph(model.BillingAddress ?? "");


                cell = row.Cells[i++];
                cell.Format.Alignment = ParagraphAlignment.Right;
                paragraph = cell.AddParagraph($"Invoice#: {model.NumberPrefix}{model.InvoiceId}");
                paragraph = cell.AddParagraph($"Issue Date: {model.IssueDate?.ToShortDateString()}");
                paragraph = cell.AddParagraph($"Terms: {model.Terms?.GetDisplayName()}");
                paragraph = cell.AddParagraph($"Due Date: {model.DueDate?.ToShortDateString()}");
                paragraph.Format.SpaceAfter = Unit.FromCentimeter(1);



                if (model.InvoiceLines.Any())
                {
                    var groupedInvoiceLines = model.InvoiceLines
                        .GroupBy(x => new
                        {
                            x.JobNumber,
                            x.PoNumber,
                            x.DeliverToFormatted,
                            x.LoadAtFormatted,
                            x.ItemName,
                        })
                        .Select(x => new
                        {
                            x.Key.JobNumber,
                            x.Key.PoNumber,
                            x.Key.DeliverToFormatted,
                            x.Key.LoadAtFormatted,
                            x.Key.ItemName,
                            InvoiceLines = x.ToList(),
                        })
                        .ToList();

                    foreach (var group in groupedInvoiceLines)
                    {
                        table = document.LastSection.AddTable();
                        table.Style = "Table";
                        if (model.DebugLayout)
                        {
                            table.Borders.Width = Unit.FromPoint(1);
                        }
                        //18.6cm total width

                        //Job Number
                        //Delivery Location
                        //Material
                        table.AddColumn(Unit.FromCentimeter(9.3));
                        //PO Number
                        //Load At
                        table.AddColumn(Unit.FromCentimeter(9.3));

                        row = table.AddRow();
                        row.Format.Font.Size = Unit.FromPoint(11);

                        i = 0;
                        cell = row.Cells[i++];
                        paragraph = cell.AddParagraph($"Job Number: {group.JobNumber ?? ""}");
                        paragraph.Format.Font.Bold = true;
                        paragraph = cell.AddParagraph($"Delivery Location: {group.DeliverToFormatted ?? ""}");
                        paragraph = cell.AddParagraph($"Material: {group.ItemName ?? ""}");
                        paragraph.Format.Font.Bold = true;
                        paragraph.Format.SpaceAfter = Unit.FromCentimeter(0.3);

                        cell = row.Cells[i++];
                        paragraph = cell.AddParagraph($"PO Number: {group.PoNumber ?? ""}");
                        paragraph = cell.AddParagraph($"Load At: {group.LoadAtFormatted ?? ""}");
                        paragraph.Format.SpaceAfter = Unit.FromCentimeter(0.3);


                        table = document.LastSection.AddTable();
                        table.Style = "Table";
                        table.Format.Alignment = ParagraphAlignment.Right;
                        table.Borders.Width = Unit.FromPoint(1);

                        //18.6cm total width
                        //Ticket #
                        table.AddColumn(Unit.FromCentimeter(1.5));
                        //Truck
                        table.AddColumn(Unit.FromCentimeter(1.5));
                        //Date
                        table.AddColumn(Unit.FromCentimeter(1.8));
                        //Qty
                        table.AddColumn(Unit.FromCentimeter(1.5));
                        //Haul Rate
                        table.AddColumn(Unit.FromCentimeter(1.6));
                        //Mat./Dump Rate
                        table.AddColumn(Unit.FromCentimeter(1.7));
                        //Mat. Total
                        table.AddColumn(Unit.FromCentimeter(1.7));
                        //Haul Total
                        table.AddColumn(Unit.FromCentimeter(1.7));
                        //Other Fees
                        table.AddColumn(Unit.FromCentimeter(1.8));
                        //Tax Total
                        table.AddColumn(Unit.FromCentimeter(1.9));
                        //Amount
                        table.AddColumn(Unit.FromCentimeter(1.9));


                        row = table.AddRow();
                        row.Shading.Color = Colors.LightGray;
                        row.Format.Font.Size = Unit.FromPoint(9);
                        row.Format.Font.Bold = true;
                        row.Format.Alignment = ParagraphAlignment.Center;
                        row.Height = Unit.FromCentimeter(0.5);
                        row.HeadingFormat = true;

                        i = 0;
                        cell = row.Cells[i++];
                        cell.AddParagraph("Ticket #");
                        cell = row.Cells[i++];
                        cell.AddParagraph("Truck");
                        cell = row.Cells[i++];
                        cell.AddParagraph("Date");
                        cell = row.Cells[i++];
                        cell.AddParagraph("Qty");
                        cell = row.Cells[i++];
                        cell.AddParagraph("Haul Rate");
                        cell = row.Cells[i++];
                        cell.AddParagraph("Mat./Dump Rate");
                        cell = row.Cells[i++];
                        cell.AddParagraph("Mat. Total");
                        cell = row.Cells[i++];
                        cell.AddParagraph("Haul Total");
                        cell = row.Cells[i++];
                        cell.AddParagraph("Other Fees");
                        cell = row.Cells[i++];
                        cell.AddParagraph("Tax Total");
                        cell = row.Cells[i++];
                        cell.AddParagraph("Total");


                        if (group.InvoiceLines.Any())
                        {
                            foreach (var invoiceLine in group.InvoiceLines)
                            {
                                i = 0;
                                row = table.AddRow();
                                cell = row.Cells[i++];
                                paragraph = cell.AddParagraph(invoiceLine.TicketNumber, tm);
                                cell = row.Cells[i++];
                                paragraph = cell.AddParagraph(invoiceLine.TruckCode, tm);
                                cell = row.Cells[i++];
                                paragraph = cell.AddParagraph(invoiceLine.DeliveryDateTime?.ToShortDateString(), tm);
                                cell = row.Cells[i++];
                                paragraph = cell.AddParagraph(invoiceLine.Quantity.ToString(Utilities.NumberFormatWithoutRounding), tm);
                                cell = row.Cells[i++];
                                paragraph = cell.AddParagraph((invoiceLine.FreightRate ?? 0).ToString(Utilities.GetCurrencyFormatWithoutRounding(model.CurrencyCulture)), tm);
                                cell = row.Cells[i++];
                                paragraph = cell.AddParagraph((invoiceLine.MaterialRate ?? 0).ToString(Utilities.GetCurrencyFormatWithoutRounding(model.CurrencyCulture)), tm);
                                cell = row.Cells[i++];
                                paragraph = cell.AddParagraph(invoiceLine.MaterialExtendedAmount.ToString(Utilities.GetCurrencyFormatWithoutRounding(model.CurrencyCulture)), tm);
                                cell = row.Cells[i++];
                                paragraph = cell.AddParagraph(invoiceLine.FreightExtendedAmount.ToString(Utilities.GetCurrencyFormatWithoutRounding(model.CurrencyCulture)), tm);
                                cell = row.Cells[i++];
                                paragraph = cell.AddParagraph(invoiceLine.FuelSurcharge.ToString("C2", model.CurrencyCulture), tm);
                                cell = row.Cells[i++];
                                paragraph = cell.AddParagraph(invoiceLine.Tax.ToString("C2", model.CurrencyCulture), tm);
                                cell = row.Cells[i++];
                                paragraph = cell.AddParagraph((invoiceLine.Subtotal + invoiceLine.FuelSurcharge + invoiceLine.Tax).ToString("C2", model.CurrencyCulture), tm);
                            }
                        }
                        else
                        {
                            table.AddRow();
                        }

                        table.SetEdge(0, 0, table.Columns.Count, table.Rows.Count, Edge.Box, BorderStyle.Single, 1, Colors.Black);



                        //Job Total
                        table = document.LastSection.AddTable();
                        table.Style = "Table";
                        table.Format.Alignment = ParagraphAlignment.Right;
                        table.Borders.Visible = false;
                        table.Format.Font.Bold = true;

                        //"Invoice Total"
                        table.AddColumn(Unit.FromCentimeter(4.8));
                        //Qty Total
                        table.AddColumn(Unit.FromCentimeter(1.5));
                        //Haul Rate and Mat./Dump Rate
                        table.AddColumn(Unit.FromCentimeter(3.3));
                        //Mat. Total
                        table.AddColumn(Unit.FromCentimeter(1.7));
                        //Haul Total
                        table.AddColumn(Unit.FromCentimeter(1.7));
                        //Other Fees
                        table.AddColumn(Unit.FromCentimeter(1.8));
                        //Tax Total
                        table.AddColumn(Unit.FromCentimeter(1.9));
                        //Amount Total
                        table.AddColumn(Unit.FromCentimeter(1.9));

                        row = table.AddRow();
                        row.Format.SpaceBefore = Unit.FromCentimeter(0.3);
                        i = 0;
                        cell = row.Cells[i++];
                        cell.Format.Alignment = ParagraphAlignment.Left;
                        paragraph = cell.AddParagraph("Job Total:");
                        cell = row.Cells[i++];
                        paragraph = cell.AddParagraph(group.InvoiceLines.Sum(x => x.Quantity).ToString(Utilities.NumberFormatWithoutRounding), tm);
                        cell = row.Cells[i++];
                        paragraph = cell.AddParagraph();
                        cell = row.Cells[i++];
                        paragraph = cell.AddParagraph(group.InvoiceLines.Sum(x => x.MaterialExtendedAmount).ToString(Utilities.GetCurrencyFormatWithoutRounding(model.CurrencyCulture)), tm);
                        cell = row.Cells[i++];
                        paragraph = cell.AddParagraph(group.InvoiceLines.Sum(x => x.FreightExtendedAmount).ToString(Utilities.GetCurrencyFormatWithoutRounding(model.CurrencyCulture)), tm);
                        cell = row.Cells[i++];
                        paragraph = cell.AddParagraph(group.InvoiceLines.Sum(x => x.FuelSurcharge).ToString("C2", model.CurrencyCulture), tm);
                        cell = row.Cells[i++];
                        paragraph = cell.AddParagraph(group.InvoiceLines.Sum(x => x.Tax).ToString("C2", model.CurrencyCulture), tm);
                        cell = row.Cells[i++];
                        var totalWithTaxAndFuelSurcharge = group.InvoiceLines.Sum(x => x.Subtotal + x.Tax + x.FuelSurcharge);
                        paragraph = cell.AddParagraph(totalWithTaxAndFuelSurcharge.ToString("C2", model.CurrencyCulture), tm);

                        paragraph = document.LastSection.AddParagraph();
                        paragraph.Format.SpaceAfter = Unit.FromCentimeter(0.5);
                    }



                    //Invoice Total
                    var quantityTotal = groupedInvoiceLines.Sum(x => x.InvoiceLines.Sum(y => y.Quantity));
                    var materialTotal = groupedInvoiceLines.Sum(x => x.InvoiceLines.Sum(y => y.MaterialExtendedAmount));
                    var freightTotal = groupedInvoiceLines.Sum(x => x.InvoiceLines.Sum(y => y.FreightExtendedAmount));
                    var fuelSurchargeTotal = groupedInvoiceLines.Sum(x => x.InvoiceLines.Sum(y => y.FuelSurcharge));
                    var taxTotal = groupedInvoiceLines.Sum(x => x.InvoiceLines.Sum(y => y.Tax));

                    table = document.LastSection.AddTable();
                    table.Style = "Table";
                    table.Format.Alignment = ParagraphAlignment.Right;
                    table.Borders.Visible = false;
                    table.Format.Font.Bold = true;

                    //"Invoice Total"
                    table.AddColumn(Unit.FromCentimeter(4.8));
                    //Qty Total
                    table.AddColumn(Unit.FromCentimeter(1.5));
                    //Haul Rate and Mat./Dump Rate
                    table.AddColumn(Unit.FromCentimeter(3.3));
                    //Mat. Total
                    table.AddColumn(Unit.FromCentimeter(1.7));
                    //Haul Total
                    table.AddColumn(Unit.FromCentimeter(1.7));
                    //Other Fees
                    table.AddColumn(Unit.FromCentimeter(1.8));
                    //Tax Total
                    table.AddColumn(Unit.FromCentimeter(1.9));
                    //Amount Total
                    table.AddColumn(Unit.FromCentimeter(1.9));

                    row = table.AddRow();
                    i = 0;
                    cell = row.Cells[i++];
                    cell.Format.Alignment = ParagraphAlignment.Left;
                    paragraph = cell.AddParagraph("Invoice Total:");

                    cell = row.Cells[i++];
                    paragraph = cell.AddParagraph(quantityTotal.ToString(Utilities.NumberFormatWithoutRounding), tm);

                    cell = row.Cells[i++];
                    paragraph = cell.AddParagraph();

                    cell = row.Cells[i++];
                    paragraph = cell.AddParagraph(materialTotal.ToString(Utilities.GetCurrencyFormatWithoutRounding(model.CurrencyCulture)), tm);

                    cell = row.Cells[i++];
                    paragraph = cell.AddParagraph(freightTotal.ToString(Utilities.GetCurrencyFormatWithoutRounding(model.CurrencyCulture)), tm);

                    cell = row.Cells[i++];
                    paragraph = cell.AddParagraph(fuelSurchargeTotal.ToString("C2", model.CurrencyCulture), tm);

                    cell = row.Cells[i++];
                    paragraph = cell.AddParagraph(taxTotal.ToString("C2", model.CurrencyCulture), tm);

                    cell = row.Cells[i++];
                    paragraph = cell.AddParagraph(model.TotalAmount.ToString("C2", model.CurrencyCulture));




                    //Invoice footer
                    row = table.AddRow();
                    row.Format.SpaceBefore = Unit.FromCentimeter(0.8);
                    //row.Borders.Visible = true;
                    cell = row.Cells[0];
                    cell.MergeRight = 6;
                    paragraph = cell.AddParagraph("Transaction Count This Invoice:");
                    cell = row.Cells[7];
                    paragraph = cell.AddParagraph(model.InvoiceLines.Count.ToString());

                    row = table.AddRow();
                    cell = row.Cells[0];
                    cell.MergeRight = 6;
                    paragraph = cell.AddParagraph("Total Taxes This Invoice:");
                    cell = row.Cells[7];
                    paragraph = cell.AddParagraph(model.Tax.ToString("C2", model.CurrencyCulture));

                    row = table.AddRow();
                    cell = row.Cells[0];
                    cell.MergeRight = 6;
                    paragraph = cell.AddParagraph("Total Fees This Invoice:");
                    cell = row.Cells[7];
                    paragraph = cell.AddParagraph(fuelSurchargeTotal.ToString("C2", model.CurrencyCulture), tm);

                    row = table.AddRow();
                    row.Format.Font.Size = Unit.FromPoint(14);
                    row.Format.Font.Bold = true;
                    cell = row.Cells[0];
                    cell.MergeRight = 6;
                    paragraph = cell.AddParagraph("Total Amount Due This Invoice (Incl. Tax&Fees):");
                    cell = row.Cells[7];
                    paragraph = cell.AddParagraph(model.TotalAmount.ToString("C2", model.CurrencyCulture));
                }



                // Second Page
                if (!string.IsNullOrEmpty(model.TermsAndConditions))
                {
                    section.AddPageBreak();
                    paragraph = document.LastSection.AddParagraph();
                    paragraph.Format.Font.Size = Unit.FromPoint(7.5);

                    paragraph.AddLineBreak();
                    paragraph.AddLineBreak();
                    paragraph.AddLineBreak();
                    paragraph.AddText(model.TermsAndConditions);
                    paragraph.AddLineBreak();
                    paragraph.AddLineBreak();
                }

                //if (!taxWarning.IsNullOrEmpty())
                //{
                //    paragraph = document.LastSection.AddParagraph(taxWarningAsterisks + taxWarning);
                //    paragraph.Format.SpaceBefore = Unit.FromCentimeter(0.7);
                //}
            }

            return Task.FromResult(document);
        }
    }
}
