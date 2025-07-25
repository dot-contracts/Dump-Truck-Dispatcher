using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Dependency;
using DispatcherWeb.Infrastructure.MigraDoc;
using DispatcherWeb.Tickets.Dto;
using MigraDocCore.DocumentObjectModel;
using MigraDocCore.DocumentObjectModel.Tables;

namespace DispatcherWeb.Tickets.Reports
{
    public class TicketPrintOutGenerator : ITransientDependency
    {
        public TicketPrintOutGenerator()
        {
        }

        public Task<Document> GenerateReport(List<TicketPrintOutDto> modelList)
        {
            Document document = new Document();

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
            section.PageSetup.HeaderDistance = Unit.FromCentimeter(0.6);

            Style style = document.Styles[StyleNames.Normal];
            style.Font.Name = "Times New Roman";
            style.Font.Size = Unit.FromPoint(11);
            style.ParagraphFormat.SpaceAfter = Unit.FromCentimeter(0.2);

            var headerStyle = document.Styles[StyleNames.Header];
            headerStyle.Font.Name = "Times New Roman";
            headerStyle.Font.Size = Unit.FromPoint(10);

            Paragraph paragraph;
            Table table;
            Row row;
            Cell cell;

            var firstTicket = true;

            foreach (var model in modelList)
            {
                if (firstTicket)
                {
                    firstTicket = false;
                }
                else
                {
                    section.AddPageBreak();
                }

                table = document.LastSection.AddTable();
                table.Style = "Table";
                if (model.DebugLayout)
                {
                    table.Borders.Width = Unit.FromPoint(1);
                }

                //18.6cm total width
                //Legal Name, Legal Address and Billing Phone Number
                table.AddColumn(Unit.FromCentimeter(10));
                //logo
                table.AddColumn(Unit.FromCentimeter(8.6));

                row = table.AddRow();
                row.Format.Alignment = ParagraphAlignment.Left;
                row.Format.Font.Size = Unit.FromPoint(10);

                int i = 0;

                cell = row.Cells[i++];
                paragraph = cell.AddParagraph(model.LegalName ?? "");
                paragraph.Format.Font.Size = Unit.FromPoint(14);
                paragraph.Format.Font.Bold = true;
                //paragraph.Format.SpaceAfter = Unit.FromCentimeter(0.7);
                paragraph = cell.AddParagraph(model.LegalAddress ?? "");
                paragraph.Format.Font.Size = Unit.FromPoint(10);
                paragraph = cell.AddParagraph(model.BillingPhoneNumber ?? "");
                paragraph.Format.Font.Size = Unit.FromPoint(10);


                cell = row.Cells[i++];
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

                paragraph.Format.SpaceAfter = Unit.FromCentimeter(1.5);

                var secondColumnMargin = Unit.FromCentimeter(4);

                paragraph = document.LastSection.AddParagraph("Ticket Number: ");
                paragraph.Format.AddTabStop(secondColumnMargin);
                paragraph.AddTab();
                paragraph.AddText(model.TicketNumber ?? "");

                paragraph = document.LastSection.AddParagraph("Date Time: ");
                paragraph.Format.AddTabStop(secondColumnMargin);
                paragraph.AddTab();
                paragraph.AddText(model.TicketDateTime?.ToShortDateString() ?? "");

                paragraph = document.LastSection.AddParagraph("Sold To: ");
                paragraph.Format.AddTabStop(secondColumnMargin);
                paragraph.AddTab();
                paragraph.AddText(model.CustomerName ?? "");

                if (model.SeparateItems)
                {
                    paragraph = document.LastSection.AddParagraph("Freight Item: ");
                    paragraph.Format.AddTabStop(secondColumnMargin);
                    paragraph.AddTab();
                    paragraph.AddText(model.FreightItemName ?? "");

                    paragraph = document.LastSection.AddParagraph("Material Item: ");
                    paragraph.Format.AddTabStop(secondColumnMargin);
                    paragraph.AddTab();
                    paragraph.AddText(model.MaterialItemName ?? "");
                }
                else
                {
                    paragraph = document.LastSection.AddParagraph("Item: ");
                    paragraph.Format.AddTabStop(secondColumnMargin);
                    paragraph.AddTab();
                    paragraph.AddText(model.FreightItemName ?? "");
                }

                paragraph = document.LastSection.AddParagraph("Freight Quantity: ");
                paragraph.Format.AddTabStop(secondColumnMargin);
                paragraph.AddTab();
                paragraph.AddText(model.FreightQuantity + " " + model.FreightUomName ?? "");

                paragraph = document.LastSection.AddParagraph("Material Quantity: ");
                paragraph.Format.AddTabStop(secondColumnMargin);
                paragraph.AddTab();
                paragraph.AddText(model.MaterialQuantity + " " + model.MaterialUomName ?? "");

                paragraph = document.LastSection.AddParagraph("Note: ");
                paragraph.Format.AddTabStop(secondColumnMargin);
                paragraph.AddTab();
                paragraph.AddText(model.Note ?? "");
            }

            return Task.FromResult(document);
        }
    }
}
