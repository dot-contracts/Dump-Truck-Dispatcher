using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using Abp.IO.Extensions;

namespace DispatcherWeb.Images
{
    public static class ImageHelper
    {
        public static Image ResizePreservingRatio(Image image, int newWidth, int newHeight)
        {
            int sourceWidth = image.Width;
            int sourceHeight = image.Height;
            int sourceX = 0;
            int sourceY = 0;
            int destX = 0;
            int destY = 0;

            float nPercent = 0;
            float nPercentW = 0;
            float nPercentH = 0;

            nPercentW = ((float)newWidth / (float)sourceWidth);
            nPercentH = ((float)newHeight / (float)sourceHeight);
            if (nPercentH < nPercentW)
            {
                nPercent = nPercentH;
                destX = System.Convert.ToInt16((newWidth - (sourceWidth * nPercent)) / 2);
            }
            else
            {
                nPercent = nPercentW;
                destY = System.Convert.ToInt16((newHeight - (sourceHeight * nPercent)) / 2);
            }

            int destWidth = (int)(sourceWidth * nPercent);
            int destHeight = (int)(sourceHeight * nPercent);

            Bitmap bmPhoto = new Bitmap(newWidth, newHeight, PixelFormat.Format24bppRgb);
            bmPhoto.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            Graphics grPhoto = Graphics.FromImage(bmPhoto);
            grPhoto.Clear(Color.White);
            grPhoto.InterpolationMode = InterpolationMode.HighQualityBicubic;

            grPhoto.DrawImage(image,
                new Rectangle(destX, destY, destWidth, destHeight),
                new Rectangle(sourceX, sourceY, sourceWidth, sourceHeight),
                GraphicsUnit.Pixel);

            grPhoto.Dispose();
            return bmPhoto;
        }

        //public static Image GetImageFromBytes(byte[] data)
        //{
        //	return (Bitmap)((new ImageConverter()).ConvertFrom(data));
        //}

        public static byte[] ResizeImageToMaxWidth(byte[] imageBytes, int maxWidth)
        {
            using (var ms = new MemoryStream(imageBytes))
            {
                var image = Image.FromStream(ms);
                if (image.Width <= maxWidth)
                {
                    return imageBytes;
                }

                var newImage = ResizePreservingRatio(image, maxWidth, (int)(image.Height * ((float)maxWidth / image.Width)));

                using (var saveStream = new MemoryStream())
                {
                    newImage.Save(saveStream, image.RawFormat);
                    saveStream.Seek(0, SeekOrigin.Begin);
                    return saveStream.GetAllBytes();
                }
            }
        }
    }
}
