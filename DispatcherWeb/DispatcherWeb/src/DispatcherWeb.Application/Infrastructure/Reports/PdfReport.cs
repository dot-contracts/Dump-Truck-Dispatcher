using MigraDocCore.DocumentObjectModel;

namespace DispatcherWeb.Infrastructure.Reports
{
    public class PdfReport : IReport
    {
        private readonly Document _document;
        private readonly string _localDateTimeString;
        private Section _section;

        public PdfReport(Document document, string localDateTimeString)
        {
            _document = document;
            _localDateTimeString = localDateTimeString;
        }

        public Document Document => _document;
        public Section Section => _section;

        public void AddSection(Orientation orientation = Orientation.Portrait)
        {
            AddSection(Unit.FromMillimeter(12), Unit.FromMillimeter(20), Unit.FromMillimeter(12), Unit.FromMillimeter(20), orientation);
        }

        public void AddSection(Unit leftMargin, Unit topMargin, Unit rightMargin, Unit bottomMargin, Orientation orientation = Orientation.Portrait)
        {
            _section = _document.AddSection();
            _section.PageSetup.PageFormat = PageFormat.A4;
            _section.PageSetup.LeftMargin = leftMargin;
            _section.PageSetup.TopMargin = topMargin;
            _section.PageSetup.RightMargin = rightMargin;
            _section.PageSetup.BottomMargin = bottomMargin;
            _section.PageSetup.Orientation = orientation;

            // Create a paragraph with centered page number. See definition of style "Footer".
            Paragraph paragraph = new Paragraph();
            paragraph.Format.Font.Size = 10;
            paragraph.AddText(_localDateTimeString ?? "");
            paragraph.AddTab();
            paragraph.AddTab();
            //paragraph.AddText(_footerText ?? "");
            paragraph.AddText("Page ");
            //paragraph.AddTab();
            paragraph.AddPageField();
            paragraph.AddText(" of ");
            paragraph.AddNumPagesField();

            paragraph.Format.TabStops.ClearAll();
            paragraph.Format.TabStops.AddTabStop(Unit.FromCentimeter(5));
            paragraph.Format.TabStops.AddTabStop(
                GetCurrentPageWidth() - _section.PageSetup.LeftMargin - _section.PageSetup.RightMargin,
                TabAlignment.Right
            );

            // Add paragraph to footer for odd pages.
            _section.Footers.Primary.Add(paragraph);
            // Add clone of paragraph to footer for odd pages. Cloning is necessary because an object must
            // not belong to more than one other object. If you forget cloning an exception is thrown.
            //section.Footers.EvenPage.Add(paragraph.Clone());
        }

        private Unit GetCurrentPageWidth()
        {
            Unit pageWidth, pageHeight;
            PageSetup.GetPageSize(PageFormat.A4, out pageWidth, out pageHeight);

            return _section.PageSetup.Orientation == Orientation.Landscape ? pageHeight : pageWidth;
        }

        public void AddReportHeader(string text)
        {
            var paragraph = _section.AddParagraph();
            paragraph.Style = Styles.ReportHeader;
            paragraph.AddFormattedText(text, TextFormat.Bold);
        }

        public void AddHeader(string text)
        {
            var paragraph = _section.AddParagraph();
            paragraph.Style = Styles.H1;
            paragraph.AddFormattedText(text, TextFormat.Bold);
        }

        public void AddText(string text)
        {
            _section.AddParagraph(text);
        }

        public void AddEmptyLine()
        {
            _section.AddParagraph();
        }

    }
}
