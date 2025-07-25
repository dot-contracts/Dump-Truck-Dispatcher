using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Dependency;
using Abp.Extensions;
using DispatcherWeb.Infrastructure.MigraDoc;
using DispatcherWeb.Orders.Dto;
using MigraDocCore.DocumentObjectModel;
using MigraDocCore.DocumentObjectModel.Shapes;
using MigraDocCore.DocumentObjectModel.Tables;
using static DispatcherWeb.Orders.Dto.WorkOrderReportDto;

namespace DispatcherWeb.Orders.Reports
{
    public class WorkOrderReportGenerator : ITransientDependency
    {
        private readonly OrderTaxCalculator _orderTaxCalculator;

        public WorkOrderReportGenerator(OrderTaxCalculator orderTaxCalculator)
        {
            _orderTaxCalculator = orderTaxCalculator;
        }

        public async Task<Document> GenerateReport(WorkOrderReportCollectionDto collectionModel)
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

            var firstOrder = true;
            var orderDetailsSpacing = Unit.FromCentimeter(0.1);

            foreach (var model in collectionModel.WorkOrderReports)
            {
                if (firstOrder)
                {
                    firstOrder = false;
                }
                else
                {
                    section.AddPageBreak();
                }

                string taxWarning = null;
                const string taxWarningAsterisks = "**";

                if (model.UseActualAmount)
                {
                    model.Items.ForEach(x =>
                    {
                        x.FreightPrice = x.IsFreightTotalOverridden
                            ? x.FreightPrice
                            : decimal.Round((x.FreightPricePerUnit ?? 0) * (x.ActualFreightQuantity ?? 0), 2);

                        x.MaterialPrice = x.IsMaterialTotalOverridden
                            ? x.MaterialPrice
                            : decimal.Round((x.MaterialPricePerUnit ?? 0) * (x.ActualMaterialQuantity ?? 0), 2);
                    });

                    var taxCalculationType = await _orderTaxCalculator.GetTaxCalculationTypeAsync();

                    if (taxCalculationType == TaxCalculationType.NoCalculation)
                    {
                        //keep the full sales tax for the office
                    }

                    await _orderTaxCalculator.CalculateTotalsAsync(model, model.Items);
                }

                Table table = document.LastSection.AddTable();
                if (model.DebugLayout)
                {
                    table.Borders.Width = Unit.FromPoint(1);
                }

                //text
                table.AddColumn(Unit.FromCentimeter(13));
                //logo
                table.AddColumn(Unit.FromCentimeter(4.21));

                int j = 0;
                Row row = table.AddRow();
                Cell cell = row.Cells[j++];

                paragraph = cell.AddParagraph(model.UseReceipts ? "Receipt" : model.ShowDeliveryInfo ? "Delivery Report" : model.OrderIsPending ? "Quote" : "Work Order");
                paragraph.Format.Font.Size = Unit.FromPoint(18);
                paragraph.Format.SpaceAfter = Unit.FromCentimeter(0.7);

                var secondColumnMargin = Unit.FromCentimeter(7.5);

                if (model.AuthorizationCaptureDateTime.HasValue)
                {
                    var date = model.AuthorizationCaptureDateTime?.ConvertTimeZoneTo(model.TimeZone).ToString("g");

                    var paidImage = section.AddImage(collectionModel.PaidImageBytes);
                    paidImage.Height = Unit.FromCentimeter(2.1);
                    paidImage.Width = Unit.FromCentimeter(4.5);
                    //paidImage.LockAspectRatio = true;
                    paidImage.RelativeVertical = RelativeVertical.Page;
                    paidImage.RelativeHorizontal = RelativeHorizontal.Page;
                    paidImage.WrapFormat.Style = WrapStyle.Through;
                    paidImage.WrapFormat.DistanceLeft = Unit.FromCentimeter(15.5);
                    paidImage.WrapFormat.DistanceTop = Unit.FromCentimeter(6);

                    var paidInfo = section.AddTextFrame();
                    paidInfo.Width = Unit.FromCentimeter(6);
                    paidInfo.Height = Unit.FromCentimeter(5);
                    paidInfo.RelativeVertical = RelativeVertical.Page;
                    paidInfo.RelativeHorizontal = RelativeHorizontal.Page;
                    paidInfo.WrapFormat.Style = WrapStyle.Through;
                    paidInfo.WrapFormat.DistanceLeft = Unit.FromCentimeter(15.9);
                    paidInfo.WrapFormat.DistanceTop = Unit.FromCentimeter(8);

                    paragraph = paidInfo.AddParagraph("Amount: " + model.AuthorizationCaptureSettlementAmount?.ToString("C2", model.CurrencyCulture));
                    paragraph = paidInfo.AddParagraph(date);
                    paragraph = paidInfo.AddParagraph("Id: " + model.AuthorizationCaptureTransactionId);
                }
                else if (model.AuthorizationDateTime.HasValue && model.ShowPaymentStatus)
                {
                    var date = model.AuthorizationDateTime?.ConvertTimeZoneTo(model.TimeZone).ToString("g");
                    document.LastSection.AddParagraph("Authorized " + date);
                }

                paragraph = cell.AddParagraph("Order Number: " + model.Id);
                paragraph.Format.SpaceAfter = orderDetailsSpacing;

                if (model.ShowSpectrumNumber)
                {
                    paragraph = cell.AddParagraph(model.SpectrumNumberLabel + ": "); //Spectrum #
                    paragraph.AddText(model.SpectrumNumber ?? "");
                    paragraph.Format.SpaceAfter = orderDetailsSpacing;
                }

                if (!string.IsNullOrEmpty(model.CustomerAccountNumber))
                {
                    paragraph = cell.AddParagraph("Account #: ");
                    paragraph.AddText(model.CustomerAccountNumber ?? "");
                    paragraph.Format.SpaceAfter = orderDetailsSpacing;
                }

                if (model.ShowOfficeName)
                {
                    paragraph.Format.AddTabStop(secondColumnMargin);
                    paragraph.AddTab();
                    paragraph.AddText("Office: ");
                    paragraph.AddText(model.OfficeName ?? "");
                }

                paragraph = cell.AddParagraph("Delivery Date: ");
                paragraph.AddText(model.OrderDeliveryDate?.ToShortDateString() ?? "");
                paragraph.Format.SpaceAfter = orderDetailsSpacing;

                if (model.OrderShift.HasValue)
                {
                    paragraph.Format.AddTabStop(secondColumnMargin);
                    paragraph.AddTab();
                    paragraph.AddText("Shift: ");
                    paragraph.AddText(model.OrderShiftName);
                }

                paragraph = cell.AddParagraph("Customer: ");
                paragraph.AddText(model.CustomerName ?? "");
                paragraph.Format.SpaceAfter = orderDetailsSpacing;

                paragraph = cell.AddParagraph("Contact: ");
                paragraph.AddText(model.ContactFullDetails ?? "");
                paragraph.Format.SpaceAfter = orderDetailsSpacing;

                paragraph = cell.AddParagraph("PO Number: ");
                paragraph.AddText(model.PoNumber ?? "");
                paragraph.Format.SpaceAfter = orderDetailsSpacing;

                if (!model.HidePrices && model.SplitRateColumn)
                {
                    paragraph = cell.AddParagraph("Material Total: ");
                    paragraph.AddText(model.MaterialTotal.ToString("C2", model.CurrencyCulture) ?? "");
                    paragraph.Format.AddTabStop(secondColumnMargin);
                    paragraph.AddTab();
                    paragraph.AddText("Freight Total: ");
                    paragraph.AddText(model.FreightTotal.ToString("C2", model.CurrencyCulture) ?? "");
                    paragraph.Format.SpaceAfter = orderDetailsSpacing;
                }

                if (!model.HidePrices)
                {
                    paragraph = cell.AddParagraph("Sales Tax: ");

                    if (taxWarning.IsNullOrEmpty())
                    {
                        paragraph.AddText(model.SalesTax.ToString(Utilities.NumberFormatWithoutRounding) ?? "");
                    }
                    else
                    {
                        paragraph.AddText(taxWarningAsterisks);
                    }

                    paragraph.Format.AddTabStop(secondColumnMargin);
                    paragraph.AddTab();
                    paragraph.AddText("Total: ");
                    paragraph.AddText(model.CodTotal.ToString("C2", model.CurrencyCulture) ?? "");
                    paragraph.Format.SpaceAfter = orderDetailsSpacing;
                }

                if (!model.ShowDeliveryInfo)
                {
                    paragraph = cell.AddParagraph("Charge To: ");
                    paragraph.AddText(model.ChargeTo ?? "");
                    paragraph.Format.SpaceAfter = orderDetailsSpacing;
                }

                //#11605
                //if (model.UseActualAmount) //this flag is set to true for backoffice reports
                //{
                //    paragraph = cell.AddParagraph("Trucks: ");
                //    paragraph.AddText(JoinTrucks(model.GetNonLeasedTrucks(), model.ShowDriverNamesOnPrintedOrder));
                //    var leasedTrucks = model.GetLeasedTrucks();
                //    if (leasedTrucks.Any())
                //    {
                //        paragraph = cell.AddParagraph("Lease Hauler Trucks: ");
                //        paragraph.AddText(JoinTrucks(leasedTrucks, model.ShowDriverNamesOnPrintedOrder));
                //    }
                //    paragraph.Format.SpaceAfter = Unit.FromCentimeter(1);
                //}
                //else
                //{
                //    paragraph = cell.AddParagraph("Trucks: ");
                //    paragraph.AddText(JoinTrucks(model.GetAllTrucks(), model.ShowDriverNamesOnPrintedOrder));
                //    paragraph.Format.SpaceAfter = Unit.FromCentimeter(1);
                //}

                paragraph.Format.SpaceAfter = Unit.FromCentimeter(1);


                cell = row.Cells[j++];
                if (model.LogoBytes?.Length > 0)
                {
                    paragraph = cell.AddParagraph();
                    var logo = paragraph.AddImage(model.LogoBytes);
                    //logo.Height = Unit.FromCentimeter(3.2);
                    logo.Width = Unit.FromCentimeter(4.21);
                    logo.LockAspectRatio = true;
                    //cell.Format.Alignment = ParagraphAlignment.Left; //default
                    paragraph.Format.Alignment = ParagraphAlignment.Right;
                }


                table = null;
                if (model.Items.Any())
                {
                    foreach (var item in model.Items)
                    {
                        if (table == null)
                        {
                            table = CreateNewOrderLineTable(document, model);
                        }

                        var tm = new TextMeasurement(TextMeasurementHelper.GetXGraphics(), document.Styles["Table"].Font.Clone());

                        int i = 0;
                        row = table.AddRow();
                        cell = row.Cells[i++];
                        if (model.ShowDeliveryInfo)
                        {
                            paragraph = cell.AddParagraph(item.JobNumber, tm);
                        }
                        else
                        {
                            paragraph = cell.AddParagraph(item.LineNumber.ToString(), tm);
                        }
                        paragraph.Format.Alignment = ParagraphAlignment.Center;
                        cell = row.Cells[i++];
                        paragraph = cell.AddParagraph(item.GetItemFormatted(collectionModel.SeparateItems), tm);
                        if (model.ShowTruckCategories)
                        {
                            cell = row.Cells[i++];
                            paragraph = cell.AddParagraph(string.Join(", ", item.OrderLineVehicleCategories), tm);
                        }
                        if (!model.UseReceipts)
                        {
                            cell = row.Cells[i++];
                            paragraph = cell.AddParagraph(item.DesignationName, tm);
                        }
                        if (model.ShowLoadAtOnPrintedOrder)
                        {
                            cell = row.Cells[i++];
                            paragraph = cell.AddParagraph(item.LoadAtName, tm);
                            paragraph.Format.Alignment = ParagraphAlignment.Center;
                        }
                        cell = row.Cells[i++];
                        paragraph = cell.AddParagraph(item.DeliverToName, tm);
                        paragraph.Format.Alignment = ParagraphAlignment.Center;
                        cell = row.Cells[i++];
                        var formattedQuantity = model.UseActualAmount
                            ? item.GetQuantityFormatted(item.ActualMaterialQuantity, item.ActualFreightQuantity)
                            : item.GetQuantityFormatted(item.MaterialQuantity, item.FreightQuantity);
                        paragraph = cell.AddParagraph(formattedQuantity, tm);
                        paragraph.Format.Alignment = ParagraphAlignment.Center;
                        if (!model.HidePrices)
                        {
                            if (model.SplitRateColumn)
                            {
                                cell = row.Cells[i++];
                                if (!item.IsMaterialTotalOverridden)
                                {
                                    paragraph = cell.AddParagraph(item.MaterialPricePerUnit?.ToString(Utilities.GetCurrencyFormatWithoutRounding(model.CurrencyCulture)) ?? "", tm);
                                    paragraph.Format.Alignment = ParagraphAlignment.Center;
                                }
                                cell = row.Cells[i++];
                                if (!item.IsFreightTotalOverridden)
                                {
                                    paragraph = cell.AddParagraph(item.FreightPricePerUnit?.ToString(Utilities.GetCurrencyFormatWithoutRounding(model.CurrencyCulture)) ?? "", tm);
                                    paragraph.Format.Alignment = ParagraphAlignment.Center;
                                }
                            }
                            else
                            {
                                cell = row.Cells[i++];
                                if (!item.IsMaterialTotalOverridden && !item.IsFreightTotalOverridden)
                                {
                                    paragraph = cell.AddParagraph(item.Rate?.ToString(Utilities.GetCurrencyFormatWithoutRounding(model.CurrencyCulture)) ?? "", tm);
                                    paragraph.Format.Alignment = ParagraphAlignment.Center;
                                }
                            }
                            cell = row.Cells[i++];
                            paragraph = cell.AddParagraph((item.FreightPrice + item.MaterialPrice).ToString("C2", model.CurrencyCulture) ?? "", tm);
                            if (item.IsMaterialTotalOverridden || item.IsFreightTotalOverridden)
                            {
                                if (!model.UseReceipts)
                                {
                                    cell.Format.Shading.Color = Colors.MistyRose;
                                }
                            }
                            paragraph.Format.Alignment = ParagraphAlignment.Center;
                        }

                        if (!model.UseReceipts)
                        {
                            cell = row.Cells[i++];
                            paragraph = cell.AddParagraph(item.TimeOnJob?.ConvertTimeZoneTo(model.TimeZone).ToString("t") ?? "", tm);
                            paragraph.Format.Alignment = ParagraphAlignment.Center;
                            if (item.IsTimeStaggered)
                            {
                                var staggeredIcon = paragraph.AddImage(collectionModel.StaggeredTimeImageBytes);
                                staggeredIcon.Height = Unit.FromCentimeter(0.5);
                                staggeredIcon.LockAspectRatio = true;
                            }
                        }

                        if (!string.IsNullOrEmpty(item.Note))
                        {
                            i = 0;
                            row = table.AddRow();
                            cell = row.Cells[i++];
                            cell.MergeRight = table.Columns.Count - 1;
                            paragraph = cell.AddParagraph(item.Note, tm);
                        }

                        var deliveryInfoItems = model.DeliveryInfoItems
                            ?.Where(x => x.OrderLineId == item.OrderLineId)
                            .OrderBy(x => x.Load?.DeliveryTime)
                            .ThenBy(x => x.DriverName)
                            .ToList();

                        if (model.ShowDeliveryInfo && deliveryInfoItems?.Any() == true)
                        {
                            paragraph = document.LastSection.AddParagraph();
                            paragraph.Format.LineSpacingRule = LineSpacingRule.Exactly;
                            paragraph.Format.LineSpacing = Unit.FromMillimeter(0.0);

                            table = document.LastSection.AddTable();
                            table.Style = "Table";
                            table.Borders.Width = Unit.FromPoint(1);

                            var showTravelTime = model.IncludeTravelTime && item.FreightUomBaseId == UnitOfMeasureBaseEnum.Hours;

                            //Truck number
                            table.AddColumn(Unit.FromCentimeter(2));
                            //Driver
                            table.AddColumn(Unit.FromCentimeter(2));
                            //Load time
                            table.AddColumn(Unit.FromCentimeter(3.3));
                            //Delivery time
                            table.AddColumn(Unit.FromCentimeter(3.3));
                            //Ticket number
                            table.AddColumn(Unit.FromCentimeter(3));
                            //Qty
                            table.AddColumn(Unit.FromCentimeter(1.5));
                            //UOM
                            table.AddColumn(Unit.FromCentimeter(1.7));
                            if (showTravelTime)
                            {
                                //Travel time
                                table.AddColumn(Unit.FromCentimeter(2));
                            }
                            //18.8
                            row = table.AddRow();
                            row.Shading.Color = Colors.LightGray;
                            row.Format.Font.Size = Unit.FromPoint(9);
                            row.Format.Font.Bold = true;
                            row.Format.Alignment = ParagraphAlignment.Center;
                            row.Height = Unit.FromCentimeter(0.5);
                            row.HeadingFormat = true;

                            i = 0;
                            cell = row.Cells[i++];
                            cell.AddParagraph("Truck number");
                            cell = row.Cells[i++];
                            cell.AddParagraph("Driver");
                            cell = row.Cells[i++];
                            cell.AddParagraph("Load time");
                            cell = row.Cells[i++];
                            cell.AddParagraph("Delivery time");
                            cell = row.Cells[i++];
                            cell.AddParagraph("Ticket number");
                            cell = row.Cells[i++];
                            cell.AddParagraph("Qty");
                            cell = row.Cells[i++];
                            cell.AddParagraph("Unit");
                            if (showTravelTime)
                            {
                                cell = row.Cells[i++];
                                cell.AddParagraph("Travel time");
                            }

                            foreach (var ticket in deliveryInfoItems)
                            {
                                i = 0;
                                row = table.AddRow();
                                cell = row.Cells[i++];
                                paragraph = cell.AddParagraph(ticket.TruckNumber, tm);
                                paragraph.Format.Alignment = ParagraphAlignment.Center;
                                cell = row.Cells[i++];
                                paragraph = cell.AddParagraph(ticket.DriverName, tm);
                                paragraph.Format.Alignment = ParagraphAlignment.Center;
                                cell = row.Cells[i++];
                                paragraph = cell.AddParagraph(ticket.Load?.LoadTime?.ConvertTimeZoneTo(model.TimeZone).ToString("g") ?? "", tm);
                                paragraph.Format.Alignment = ParagraphAlignment.Center;
                                cell = row.Cells[i++];
                                paragraph = cell.AddParagraph(ticket.Load?.DeliveryTime?.ConvertTimeZoneTo(model.TimeZone).ToString("g") ?? "", tm);
                                paragraph.Format.Alignment = ParagraphAlignment.Center;
                                cell = row.Cells[i++];
                                paragraph = cell.AddParagraph(ticket.TicketNumber, tm);
                                paragraph.Format.Alignment = ParagraphAlignment.Center;
                                cell = row.Cells[i++];
                                paragraph = cell.AddParagraph((ticket.MaterialQuantity ?? ticket.FreightQuantity ?? 0).ToString(Utilities.NumberFormatWithoutRounding) ?? "", tm);
                                paragraph.Format.Alignment = ParagraphAlignment.Center;
                                cell = row.Cells[i++];
                                paragraph = cell.AddParagraph(ticket.MaterialUomName ?? ticket.FreightUomName, tm);
                                paragraph.Format.Alignment = ParagraphAlignment.Center;

                                if (showTravelTime)
                                {
                                    cell = row.Cells[i++];
                                    paragraph = cell.AddParagraph((ticket.Load?.TravelTime)?.ToString(Utilities.NumberFormatWithoutRounding) ?? "", tm);
                                    paragraph.Format.Alignment = ParagraphAlignment.Center;
                                }

                                if (model.ShowSignatureColumn && ticket.Load?.SignatureBytes?.Length > 0)
                                {
                                    row = table.AddRow();
                                    row.Borders.Visible = false;
                                    cell = row.Cells[0];

                                    var logo = cell.AddImage(ticket.Load?.SignatureBytes);
                                    //logo.Height = Unit.FromCentimeter(3.2);
                                    logo.Width = Unit.FromCentimeter(4);
                                    logo.LockAspectRatio = true;
                                    paragraph = cell.AddParagraph(ticket.Load?.SignatureName, tm);
                                }
                            }

                            table.SetEdge(0, 0, table.Columns.Count, table.Rows.Count, Edge.Box, BorderStyle.Single, 1, Colors.Black);

                            table = null;

                            paragraph = document.LastSection.AddParagraph();
                        }
                    }
                }

                if (table != null)
                {
                    table.SetEdge(0, 0, table.Columns.Count, table.Rows.Count, Edge.Box, BorderStyle.Single, 1, Colors.Black);
                }

                if (!model.HidePrices && model.Items.Any() && !model.ShowDeliveryInfo)
                {
                    var tm = new TextMeasurement(TextMeasurementHelper.GetXGraphics(), document.Styles["Table"].Font.Clone());
                    if (table == null)
                    {
                        table = CreateNewOrderLineTable(document, model);
                    }

                    var totalColumn = table.FindColumnByComment("Total");

                    row = table.AddRow();
                    cell = row.Cells[0];
                    cell.MergeRight = totalColumn.Index - 1;
                    cell.Borders.Visible = false;
                    cell.Borders.Left.Visible = true;
                    cell.Borders.Bottom.Visible = true;
                    cell.Borders.Top.Visible = true;
                    paragraph = cell.AddParagraph("Total:", tm);
                    paragraph.Format.Alignment = ParagraphAlignment.Right;
                    cell = row.Cells[totalColumn.Index];
                    cell.Borders.Visible = false;
                    cell.Borders.Right.Visible = true;
                    cell.Borders.Bottom.Visible = true;
                    cell.Borders.Top.Visible = true;
                    paragraph = cell.AddParagraph(model.Items.Sum(x => x.FreightPrice + x.MaterialPrice).ToString("C2", model.CurrencyCulture) ?? "", tm);
                    paragraph.Format.Alignment = ParagraphAlignment.Center;
                }

                paragraph = document.LastSection.AddParagraph("Comments: ");
                paragraph.Format.SpaceBefore = Unit.FromCentimeter(0.7);
                paragraph = document.LastSection.AddParagraph(model.Directions ?? "");

                if (!taxWarning.IsNullOrEmpty())
                {
                    paragraph = document.LastSection.AddParagraph(taxWarningAsterisks + taxWarning);
                    paragraph.Format.SpaceBefore = Unit.FromCentimeter(0.7);
                }

                if (model.IncludeTickets && model.DeliveryInfoItems?.Any() == true)
                {
                    var ticketPhotoWidthCm = AppConsts.TicketPhotoWidthCm;
                    foreach (var ticket in model.DeliveryInfoItems)
                    {
                        if (!(ticket.TicketPhotoBytes?.Length > 0))
                        {
                            continue;
                        }

                        paragraph = document.LastSection.AddParagraph();
                        paragraph.Format.SpaceBefore = Unit.FromCentimeter(0.5);

                        if (ticket.TicketPhotoFilename?.EndsWith(".pdf") == true)
                        {
                            var ticketNumber = string.IsNullOrEmpty(ticket.TicketNumber) ? string.Empty : $" {ticket.TicketNumber}";
                            var unableToPrintTicketImageWarning = $"Unable to include ticket{ticketNumber} because it is pdf instead of image.";

                            if (!collectionModel.ConvertPdfTicketImages)
                            {
                                paragraph = document.LastSection.AddParagraph(unableToPrintTicketImageWarning);
                                continue;
                            }

                            var imageBytes = ReportExtensions.ConvertPdfTicketImageToJpg(ticket.TicketPhotoBytes, ticketPhotoWidthCm);
                            if (!(imageBytes?.Length > 0))
                            {
                                paragraph = document.LastSection.AddParagraph(unableToPrintTicketImageWarning);
                                continue;
                            }

                            ticket.TicketPhotoBytes = imageBytes;
                        }

                        var ticketImage = paragraph.AddImage(ticket.TicketPhotoBytes);
                        //ticketImage.Width = Unit.FromInch(8.5) - Unit.FromCentimeter(3); //full width
                        ticketImage.Width = Unit.FromCentimeter(ticketPhotoWidthCm);
                        ticketImage.LockAspectRatio = true;
                    }
                }
            }

            return document;
        }

        private static Table CreateNewOrderLineTable(Document document, WorkOrderReportDto model)
        {
            Column column;
            Table table = document.LastSection.AddTable();
            table.Style = "Table";
            table.Borders.Width = Unit.FromPoint(1);

            if (model.ShowDeliveryInfo)
            {
                //Job #
                table.AddColumn(Unit.FromCentimeter(1.1));
            }
            else
            {
                //Line #
                table.AddColumn(Unit.FromCentimeter(0.8));
            }
            //Item
            table.AddColumn(Unit.FromCentimeter(!model.HidePrices && model.SplitRateColumn ? 2.2 : 3.3));
            if (model.ShowTruckCategories)
            {
                //Truck Categories
                table.AddColumn(Unit.FromCentimeter(1.8));
            }
            if (!model.UseReceipts)
            {
                //Designation
                table.AddColumn(Unit.FromCentimeter(1.8));
            }
            if (model.ShowLoadAtOnPrintedOrder)
            {
                //Quarry/Load At
                table.AddColumn(Unit.FromCentimeter(2.2));
            }
            //Deliver To
            table.AddColumn(Unit.FromCentimeter(2.2));
            //Quantity
            table.AddColumn(Unit.FromCentimeter(2.5));
            if (!model.HidePrices)
            {
                if (model.SplitRateColumn)
                {
                    //Material Rate
                    table.AddColumn(Unit.FromCentimeter(1.4));
                    //Freight Rate
                    table.AddColumn(Unit.FromCentimeter(1.4));
                }
                else
                {
                    //Rate
                    table.AddColumn(Unit.FromCentimeter(1.4));
                }
                //Total
                column = table.AddColumn(Unit.FromCentimeter(2.2));
                column.Comment = "Total";
            }
            if (!model.UseReceipts)
            {
                //Time on Job
                table.AddColumn(Unit.FromCentimeter(1.3));
            }

            var row = table.AddRow();
            row.Shading.Color = Colors.LightGray;
            row.Format.Font.Size = Unit.FromPoint(9);
            row.Format.Font.Bold = true;
            row.Format.Alignment = ParagraphAlignment.Center;
            row.Height = Unit.FromCentimeter(0.5);
            row.HeadingFormat = true;

            int i = 0;
            Cell cell;

            if (model.ShowDeliveryInfo)
            {
                cell = row.Cells[i++];
                cell.AddParagraph("Job #");
            }
            else
            {
                cell = row.Cells[i++];
                cell.AddParagraph("Line #");
            }
            cell = row.Cells[i++];
            cell.AddParagraph("Item");
            if (model.ShowTruckCategories)
            {
                cell = row.Cells[i++];
                cell.AddParagraph("Truck Categories");
            }
            if (!model.UseReceipts)
            {
                cell = row.Cells[i++];
                cell.AddParagraph("Designation");
            }
            if (model.ShowLoadAtOnPrintedOrder)
            {
                cell = row.Cells[i++];
                cell.AddParagraph("Load At");
            }
            cell = row.Cells[i++];
            cell.AddParagraph("Deliver To");
            cell = row.Cells[i++];
            cell.AddParagraph("Quantity");
            if (!model.HidePrices)
            {
                if (model.SplitRateColumn)
                {
                    cell = row.Cells[i++];
                    cell.AddParagraph("Material Rate");
                    cell = row.Cells[i++];
                    cell.AddParagraph("Freight Rate");
                }
                else
                {
                    cell = row.Cells[i++];
                    cell.AddParagraph("Rate");
                }
                cell = row.Cells[i++];
                cell.AddParagraph("Total");
            }
            if (!model.UseReceipts)
            {
                cell = row.Cells[i++];
                cell.AddParagraph("Time on Job");
            }

            return table;
        }

        private static string JoinTrucks(IEnumerable<TruckDriverDto> trucks, bool showDriverNamesOnPrintedOrder)
        {
            return showDriverNamesOnPrintedOrder
                ? string.Join(", ", trucks)
                : string.Join(", ", trucks.Select(x => x.TruckCode));
        }
    }
}
