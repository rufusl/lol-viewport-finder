using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;

namespace LOLViewportFinder
{
    static class ImageUtils
    {
        private static ImageCodecInfo _jpegCodec;

        public static Bitmap ReadImage(string localPathOrUrl, string cacheDir)
        {
            var isLocal = File.Exists(localPathOrUrl) || (new Uri(localPathOrUrl).IsFile);
            if (isLocal)
                return ReadLocalImage(localPathOrUrl);
            else
                return ReadRemoteImage(localPathOrUrl, cacheDir);
        }

        private static Bitmap ReadRemoteImage(string url, string cacheDir)
        {
            var uri = new Uri(url);
            var localPath = Path.Combine(cacheDir, Path.GetFileName(uri.LocalPath));
            if (!File.Exists(localPath))
            {
                if (!Directory.Exists(cacheDir))
                    Directory.CreateDirectory(cacheDir);

                using (WebClient client = new WebClient())
                {
                    client.DownloadFile(uri, localPath);
                }
            }
            return ReadLocalImage(localPath);
        }

        private static Bitmap ReadLocalImage(string path)
        {
            return (Bitmap)Bitmap.FromFile(path);
        }

        public static void SaveImageAsJpg(Bitmap img, string filename)
        {
            var encoderParams = new EncoderParameters(1);
            encoderParams.Param[0] = new EncoderParameter(Encoder.Quality, 90L);
            img.Save(filename, GetJpegCodec(), encoderParams);
        }

        private static ImageCodecInfo GetJpegCodec()
        {
            if (_jpegCodec == null)
            {
                _jpegCodec = ImageCodecInfo.GetImageEncoders().FirstOrDefault(c => c.FormatID.Equals(ImageFormat.Jpeg.Guid));
            }
            return _jpegCodec;
        }

        public static byte[] ReadPixels(Bitmap img)
        {
            return ReadPixels(img, out int bytesPerPixel, out int stride);
        }

        public static byte[] ReadPixels(Bitmap img, out int bytesPerPixel, out int stride)
        {
            var bitmapData = img.LockBits(new Rectangle(0, 0, img.Width, img.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, img.PixelFormat);
            
            bytesPerPixel = Image.GetPixelFormatSize(bitmapData.PixelFormat) / 8;
            // Bitmap pads the stride, we want the non-padded data so calculate the actual stride.
            var actualStride = bitmapData.Width * bytesPerPixel;

            byte[] imageData = new byte[actualStride * bitmapData.Height];

            var bitmapPtr = bitmapData.Scan0;
            var imageDataPtr = 0;
            // Copy row by row to get rid of the stride padding.
            for (int i = 0; i < bitmapData.Height; i++)
            {
                Marshal.Copy(bitmapPtr, imageData, imageDataPtr, actualStride);
                // Advance pointer to bitmap by source/padded stride.
                bitmapPtr += bitmapData.Stride;
                // Advance pointer to output data by actual stride.
                imageDataPtr += actualStride;
            }
            img.UnlockBits(bitmapData);

            stride = actualStride;
            return imageData;
        }

        public static Bitmap CreateBlobsHighlightImage(int width, int height, IEnumerable<Blob> blobs)
        {
            Bitmap target = new Bitmap(width, height, PixelFormat.Format8bppIndexed);

            var targetData = target.LockBits(new Rectangle(0, 0, target.Width, target.Height), ImageLockMode.ReadWrite, target.PixelFormat);
            var allBlobPixels = new byte[targetData.Height * targetData.Stride];
            foreach (var b in blobs)
            {
                foreach (var p in b.Pixels)
                {
                    allBlobPixels[p.Y * targetData.Stride + p.X] = byte.MaxValue;
                }

                Marshal.Copy(allBlobPixels, 0, targetData.Scan0, allBlobPixels.Length);
                
            }
            target.UnlockBits(targetData);
            return target;
        }

    }
}