using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LOLViewportFinder
{
    class Program
    {
        static string[] inputFiles = new[]
        {
            "1.jpg", "2.jpg", "3.jpg", "4.jpg", "5.jpg", "6.jpg", "7.jpg", "8.jpg", "9.jpg", "10.jpg", "hires_1.jpg",
        };

        static void Main(string[] args)
        {
            // TODO: Add console logs.
            foreach (var imgFile in inputFiles)
            {
                ProcessImage(imgFile);
            }
        }

        private static void ProcessImage(string imgFile)
        {
            // TODO: Optimize by copying less images around?
            // TODO: Shrink images for better performance?
            // TODO: Add better debug output.

            var img = LoadFile(imgFile);
            var croppedImage = Crop(img);
            var whiteImage = WhiteFilter(croppedImage);
            var blobDetector = new BlobDetector(byte.MaxValue, 20);
            var blobs = blobDetector.FindBlobs(whiteImage);

            var lineDetector = new LineDetector(5);
            var rectDetector = new RectangularShapeDetector(10, 4);
            Rectangle? rectBlobAreaAbs = null;
            RectangleF? rectBlobArea = null;
            var rectBlob = blobs.FirstOrDefault(b =>
            {
                var lines = lineDetector.DetectLines(b);
                if (rectDetector.FormsRectangularShape(lines))
                {
                    var rectInMiniMapAbs = GeometryUtils.FindBoundingBox(lines);
                    rectBlobAreaAbs = MiniMapToAbsoluteCoordinates(img, rectInMiniMapAbs);
                    rectBlobArea = GeometryUtils.MakeRelativeRect(rectInMiniMapAbs, whiteImage.Width, whiteImage.Height);
                    return true;
                }
                else
                {
                    return false;
                }
            });

            Console.WriteLine($"File {imgFile}:");
            if (rectBlob != null)
            {
                Console.WriteLine($"Detected MiniMap Viewport at {rectBlobAreaAbs.ToString()}, relative position: {rectBlobArea.ToString()}");
            }
            else
            {
                Console.Error.WriteLine("No MiniMap Viewport found.");
            }
        }

        private static Bitmap LoadFile(string imgFile)
        {
            return ImageUtils.ReadImage(@"C:\Users\rufus\Downloads\images\" + imgFile);
        }

        private static Bitmap Crop(Bitmap image)
        {
            // The mini map's relative location and size is always the same on all screen sizes.
            // These values are taken from the full hd resolution.
            var filter = new ImageCrop(1640f / 1920f, 800f / 1080f, 280f / 1920f, 280f / 1080f);
            return filter.ProcessImage(image);
        }

        private static Rectangle MiniMapToAbsoluteCoordinates(Bitmap img, Rectangle rect)
        {
            int miniMapX = (int)((1640f / 1920f) * img.Width);
            int miniMapY = (int)((800f / 1080f) * img.Height);

            return new Rectangle(
                miniMapX + rect.X,
                miniMapY + rect.Y,
                rect.Width,
                rect.Height
            );
        }

        private static Bitmap WhiteFilter(Bitmap image)
        {
            var filter = new WhiteFilter(220);
            return filter.ProcessImage(image);
        }
    }
}
