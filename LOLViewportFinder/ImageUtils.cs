using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

namespace LOLViewportFinder
{
    static class ImageUtils
    {
        // TODO: Read bitmap from URL? Cache?

        public static Bitmap ReadImage(string path)
        {
            return (Bitmap)Bitmap.FromFile(path);
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

        public static Bitmap CreateBlobsHighlightImage(Bitmap originalImg, IEnumerable<Blob> blobs)
        {
            Bitmap target = new Bitmap(originalImg.Width, originalImg.Height, PixelFormat.Format8bppIndexed);

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