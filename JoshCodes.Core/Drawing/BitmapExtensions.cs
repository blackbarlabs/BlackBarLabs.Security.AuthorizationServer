using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;

namespace JoshCodes.Core.Drawing
{
    public static class BitmapExtensions
    {
        /// <summary>
        /// Resize the image to the specified width and height.
        /// </summary>
        /// <param name="image">The image to resize.</param>
        /// <param name="width">The width to resize to.</param>
        /// <param name="height">The height to resize to.</param>
        /// <returns>The resized image.</returns>
        public static System.Drawing.Bitmap Resize(this System.Drawing.Image image, int width, int height,
            InterpolationMode interpolationMode = InterpolationMode.HighQualityBicubic,
            SmoothingMode smoothingMode = SmoothingMode.HighQuality)
        {
            //a holder for the result
            Bitmap result = new Bitmap(width, height);
            // set the resolutions the same to avoid cropping due to resolution differences
            result.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            //use a graphics object to draw the resized image into the bitmap
            using (Graphics graphics = Graphics.FromImage(result))
            {
                //set the resize quality modes to high quality
                graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                graphics.InterpolationMode = interpolationMode;
                graphics.SmoothingMode = smoothingMode;
                //draw the image into the target bitmap
                graphics.DrawImage(image, 0, 0, result.Width, result.Height);
            }

            //return the resulting bitmap
            return result;
        }

        public static Stream Save(this Bitmap bitmap, ImageFormat format)
        {
            var stream = new MemoryStream();
            bitmap.Save(stream, format);
            stream.Position = 0;
            return stream;
        }
    }
}
