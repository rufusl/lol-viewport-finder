using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Linq;
using System.IO;

namespace LOLViewportFinder
{
    interface IImageProcessor
    {
        Bitmap ProcessImage(Bitmap img);
    }

    /// <summary>
    /// Crops an image to a normalized rectangle. Top left is (0,0), bottom right is (1,1). Full width/height is 1/1.
    /// </summary>
    class ImageCrop : IImageProcessor
    {
        private readonly float _normX;
        private readonly float _normY;
        private readonly float _normWidth;
        private readonly float _normHeight;

        public ImageCrop(float normX, float normY, float normWidth, float normHeight)
        {
            _normX = normX;
            _normY = normY;
            _normWidth = normWidth;
            _normHeight = normHeight;
        }

        public Bitmap ProcessImage(Bitmap img)
        {
            var cropArea = new Rectangle(
                (int)(img.Width * _normX), 
                (int)(img.Height * _normY), 
                (int)(img.Width * _normWidth),
                (int)(img.Height * _normHeight)    
            );
            return img.Clone(cropArea, img.PixelFormat);
        }
    }

    /// <summary>
    /// Filters out all pixels below a certain grey threshold. The result is a pure black/white only image.
    /// </summary>
    class BlackWhiteConverter : IImageProcessor
    {
        private byte _whiteThreshold;

        /// <summary>
        /// Creates a new <see cref="BlackWhiteConverter"/>.
        /// </summary>
        /// <param name="whiteThreshold">The minimum grey value to accept a pixel as "white". All pixels below that value are output as black.</param>
        public BlackWhiteConverter(byte whiteThreshold)
        {
            _whiteThreshold = whiteThreshold;
        }

        public Bitmap ProcessImage(Bitmap img)
        {
            Bitmap target = new Bitmap(img.Width, img.Height, PixelFormat.Format8bppIndexed);

            var imageData = ImageUtils.ReadPixels(img, out int bytesPerPixel, out int stride);

            var targetData = target.LockBits(new Rectangle(0, 0, target.Width, target.Height), ImageLockMode.ReadWrite, target.PixelFormat);
            var targetBuffer = targetData.Scan0;
            var pixelBuffer = new byte[bytesPerPixel];
            for (int i = 0; i < imageData.Length - bytesPerPixel; i += bytesPerPixel)
            {
                // Get the color of the current pixel and calculate average grey value.
                Buffer.BlockCopy(imageData, i, pixelBuffer, 0, bytesPerPixel);
                float avg = 0;
                for (int j = 0; j < pixelBuffer.Length; j++)
                {
                    avg += pixelBuffer[j];
                }
                avg /= bytesPerPixel;
                // We filter out pixels below a certain threshold.
                var isWhite = avg > _whiteThreshold;

                // Determine position in image.
                var x = (i % stride) / bytesPerPixel;
                var y = (i / stride);
                var bufferPos = targetBuffer + x + y * targetData.Stride;

                // White white or black to target image.
                Marshal.WriteInt32(bufferPos, isWhite ? byte.MaxValue : byte.MinValue);
            }
            target.UnlockBits(targetData);
            return target;
        }
    }

    /// <summary>
    /// Just calls the specified action with the current image and returns it.
    /// </summary>
    class Tap : IImageProcessor
    {
        private Action<Bitmap> _callback;

        public Tap(Action<Bitmap> callback)
        {
            _callback = callback;
        }

        public Bitmap ProcessImage(Bitmap img)
        {
            _callback(img);
            return img;
        }
    }
}
