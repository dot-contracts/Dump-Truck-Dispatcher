using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DispatcherWeb.Dto;
using MigraDocCore.DocumentObjectModel;
using MigraDocCore.DocumentObjectModel.Tables;
using MigraDocCore.Rendering;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using PdfSharpCore.Pdf.IO;
using PDFtoImage;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Chars = MigraDocCore.DocumentObjectModel.Chars;

namespace DispatcherWeb
{
    public static class ReportExtensions
    {
        public static Paragraph AddParagraph(this Cell cell, string paragraphText, TextMeasurement tm)
        {
            if (tm.Font.Size == 0)
            {
                throw new System.Exception("TextMeasurement font size was not explicitly set");
            }
            return cell.AddParagraph(AdjustIfTooWideToFitIn(tm, cell, paragraphText ?? ""));
        }

        public static Column FindColumnByComment(this Table table, string comment)
        {
            for (int i = 0; i < table.Columns.Count; i++)
            {
                if (table.Columns[i].Comment == comment)
                {
                    return table.Columns[i];
                }
            }

            return null;
        }

        public static byte[] SaveToBytesArray(this Document document)
        {
            using (var stream = new MemoryStream())
            {
                document.SaveToMemoryStream(stream);
                return stream.ToArray();
            }
        }

        public static MemoryStream SaveToMemoryStream(this Document document, MemoryStream stream)
        {
            document.UseCmykColor = true;

            var renderer = new PdfDocumentRenderer(true)
            {
                Document = document,
            };

            renderer.RenderDocument();

            renderer.PdfDocument.Save(stream, false);
            renderer.PdfDocument.Close();
            renderer.PdfDocument.Dispose();
            stream.Flush();
            stream.Seek(0, SeekOrigin.Begin);
            return stream;
        }

        public static int ConvertCmToPixels(double centimeters, double dpi = 96.0)
        {
            const double cmPerInch = 2.54;
            return (int)(centimeters / cmPerInch * dpi);
        }

        public static byte[] ConvertPdfTicketImageToJpg(byte[] pdfBytes, double ticketPhotoWidthCm)
        {
            const int dpi = 200;
            return ConvertPdfPageToJpg(
                pdfBytes: pdfBytes,
                widthInPixels: ConvertCmToPixels(ticketPhotoWidthCm, dpi),
                pageIndex: 0,
                quality: 70
            );
        }

        public static byte[] ConvertPdfPageToJpg(byte[] pdfBytes, int widthInPixels, int pageIndex = 0, int quality = 85)
        {
            var bitmapImage = Conversion.ToImage(pdfBytes, pageIndex, options: new RenderOptions
            {
                Width = widthInPixels,
                WithAspectRatio = true,
            });
            using (var imageStream = new MemoryStream())
            {
                if (bitmapImage.Encode(imageStream, SkiaSharp.SKEncodedImageFormat.Jpeg, quality))
                {
                    return imageStream.ToArray();
                }
            }

            return null;
        }

        public static byte[] GenerateImagesPdf(IReadOnlyCollection<FileBytesDto> files)
        {
            var outputDocument = new PdfDocument();

            foreach (var file in files)
            {
                var ext = Path.GetExtension(file.FileName)?.ToLowerInvariant();

                if (IsImageExtension(ext))
                {
                    var fixedBytes = FixImageOrientation(file.FileBytes);
                    var page = outputDocument.AddPage();

                    using var imgStream = new MemoryStream(fixedBytes);
                    using var xImage = XImage.FromStream(() => imgStream);

                    double targetWidthCm = AppConsts.TicketPhotoFullPageWidthCm;
                    double targetWidthPt = targetWidthCm * 28.35;
                    double aspectRatio = xImage.PixelHeight / (double)xImage.PixelWidth;

                    double width = targetWidthPt;
                    double height = width * aspectRatio;

                    double x = (page.Width - width) / 2;
                    double y = (page.Height - height) / 2;

                    using var gfx = XGraphics.FromPdfPage(page);
                    gfx.DrawImage(xImage, x, y, width, height);
                }
                else if (ext == ".pdf")
                {
                    using var pdfStream = new MemoryStream(file.FileBytes);
                    var inputPdf = PdfReader.Open(pdfStream, PdfDocumentOpenMode.Import);

                    for (int i = 0; i < inputPdf.PageCount; i++)
                    {
                        outputDocument.AddPage(inputPdf.Pages[i]);
                    }
                }
            }

            using var outputStream = new MemoryStream();
            outputDocument.Save(outputStream);
            return outputStream.ToArray();
        }

        private static bool IsImageExtension(string ext)
        {
            return new[] { ".jpg", ".jpeg", ".png", ".bmp", ".gif", ".tif", ".tiff" }.Contains(ext);
        }

        private static byte[] FixImageOrientation(byte[] imageBytes)
        {
            using var image = Image.Load<Rgba32>(imageBytes, out IImageFormat format);

            image.Mutate(x => x.AutoOrient());

            using var ms = new MemoryStream();
            image.Save(ms, format);
            return ms.ToArray();
        }

        private static string AdjustIfTooWideToFitIn(TextMeasurement tm, Cell cell, string text)
        {
            Column column = cell.Column;
            Unit rightPadding = Unit.FromMillimeter(1.2);
            Unit availableWidth = column.Width - column.Table.Borders.Width - cell.Borders.Width - rightPadding;

            if (cell.MergeRight > 0)
            {
                for (var i = 0; i < cell.MergeRight; i++)
                {
                    availableWidth += cell.Table.Columns[cell.Column.Index + i + 1].Width;
                }
                availableWidth -= Unit.FromMillimeter(3);
            }

            var tooWideWords = text.Split(" ".ToCharArray()).Distinct().Where(s => TooWide(s, availableWidth, tm));

            var adjusted = new StringBuilder(text);
            foreach (string word in tooWideWords)
            {
                var replacementWord = MakeFit(word, availableWidth, tm);
                adjusted.Replace(word, replacementWord);
            }

            return adjusted.ToString();
        }

        private static bool TooWide(string word, Unit width, TextMeasurement tm)
        {
            double f = tm.MeasureString(word, UnitType.Point).Width;
            return f > width.Point;
        }

        /// <summary>
        /// Makes the supplied word fit into the available width
        /// </summary>
        /// <returns>modified version of the word with inserted Returns at appropriate points</returns>
        private static string MakeFit(string word, Unit width, TextMeasurement tm)
        {
            var adjustedWord = new StringBuilder();
            var current = string.Empty;
            foreach (char c in word)
            {
                if (TooWide(current + c, width, tm))
                {
                    adjustedWord.Append(current);
                    adjustedWord.Append(Chars.CR);
                    current = c.ToString();
                }
                else
                {
                    current += c;
                }
            }
            adjustedWord.Append(current);

            return adjustedWord.ToString();
        }
    }
}
