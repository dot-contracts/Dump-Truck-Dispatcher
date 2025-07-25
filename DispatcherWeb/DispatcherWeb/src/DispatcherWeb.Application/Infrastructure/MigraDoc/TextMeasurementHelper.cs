using System;
using MigraDocCore.DocumentObjectModel;
using MigraDocCore.DocumentObjectModel.MigraDoc.DocumentObjectModel.Shapes;
using MigraDocCore.DocumentObjectModel.Shapes;
using MigraDocCore.DocumentObjectModel.Tables;
using PdfSharpCore.Drawing;
using PdfSharpCore.Utils;

namespace DispatcherWeb.Infrastructure.MigraDoc
{
    public static class TextMeasurementHelper
    {
        public static XGraphics GetXGraphics()
        {
            return XGraphics.CreateMeasureContext(new XSize(2000, 2000), XGraphicsUnit.Point, XPageDirection.Downwards);
        }

        public static Image AddImage(this Cell cell, byte[] imageBytes)
        {
            var imageSource = GetImageSourceFromByteArray(imageBytes);
            var image = cell.AddImage(imageSource);
            return image;
        }

        public static Image AddImage(this Paragraph paragraph, byte[] imageBytes)
        {
            var imageSource = GetImageSourceFromByteArray(imageBytes);
            var image = paragraph.AddImage(imageSource);
            return image;
        }

        public static Image AddImage(this Section section, byte[] imageBytes)
        {
            var imageSource = GetImageSourceFromByteArray(imageBytes);
            var image = section.AddImage(imageSource);
            return image;
        }

        private static ImageSource.IImageSource GetImageSourceFromByteArray(byte[] imageBytes)
        {
            ImageSource.ImageSourceImpl ??= new ImageSharpImageSource<SixLabors.ImageSharp.PixelFormats.Rgba32>();
            var imageName = $"{Guid.NewGuid()}.png";
            var imageSource = ImageSource.FromBinary(imageName, () => imageBytes, 100);
            return imageSource;
        }
    }
}
