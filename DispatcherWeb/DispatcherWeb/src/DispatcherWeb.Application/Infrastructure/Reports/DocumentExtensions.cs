using MigraDocCore.DocumentObjectModel;

namespace DispatcherWeb.Infrastructure.Reports
{
    public static class DocumentExtensions
    {
        public static void DefineStyles(this Document document)
        {
            // Get the predefined style Normal.
            Style style = document.Styles["Normal"];
            // Because all styles are derived from Normal, the next line changes the 
            // font of the whole document. Or, more exactly, it changes the font of
            // all styles and paragraphs that do not redefine the font.
            style.Font.Name = "Verdana";
            style.Font.Color = Colors.Black;

            style = document.Styles[StyleNames.Header];
            style.ParagraphFormat.AddTabStop("16cm", TabAlignment.Right);

            style = document.Styles[StyleNames.Footer];
            style.ParagraphFormat.AddTabStop("8cm", TabAlignment.Center);

            // Create a new style called Table based on style Normal
            style = document.Styles.AddStyle(Styles.Table, "Normal");
            //style.Font.Name = "Verdana";
            style.Font.Name = "Times New Roman";
            style.Font.Size = 9;

            style = document.Styles.AddStyle(Styles.ReportHeader, "Normal");
            style.Font.Size = 14;
            style.ParagraphFormat.SpaceBefore = "2mm";
            style.ParagraphFormat.SpaceAfter = "2mm";

            style = document.Styles.AddStyle(Styles.ReportHeader, "Normal");
            style.Font.Size = 16;
            style.ParagraphFormat.SpaceBefore = "0";
            style.ParagraphFormat.SpaceAfter = "5mm";

            style = document.Styles.AddStyle(Styles.CoverHeader, "Normal");
            style.Font.Size = 12;
            style.Font.Color = new Color(0, 0, 0);
            style.ParagraphFormat.Alignment = ParagraphAlignment.Center;
            style.ParagraphFormat.SpaceBefore = "1mm";
            style.ParagraphFormat.SpaceAfter = "1mm";

            style = document.Styles.AddStyle(Styles.H1, "Normal");
            style.Font.Size = 14;
            style.ParagraphFormat.SpaceBefore = "2mm";
            style.ParagraphFormat.SpaceAfter = "2mm";

            style = document.Styles.AddStyle(Styles.H3, "Normal");
            style.Font.Size = 12;
            style.ParagraphFormat.SpaceBefore = "1mm";
            style.ParagraphFormat.SpaceAfter = "1mm";

            style = document.Styles.AddStyle(Styles.Small, "Normal");
            style.Font.Size = 6;

            style = document.Styles.AddStyle(Styles.Criteria, "Normal");
            style.ParagraphFormat.SpaceBefore = Unit.FromCentimeter(1);
            style.ParagraphFormat.SpaceAfter = Unit.FromCentimeter(1);

        }
    }
}
