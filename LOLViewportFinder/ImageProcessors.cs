using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace LOLViewportFinder
{
    interface IImageProcessor
    {
        Bitmap ProcessImage(Bitmap img);
    }

    class ImageCrop : IImageProcessor
    {
        private readonly float _relX;
        private readonly float _relY;
        private readonly float _relWidth;
        private readonly float _relHeight;

        public ImageCrop(float relX, float relY, float relWidth, float relHeight)
        {
            _relX = relX;
            _relY = relY;
            _relWidth = relWidth;
            _relHeight = relHeight;
        }

        public Bitmap ProcessImage(Bitmap img)
        {
            var cropArea = new Rectangle(
                (int)(img.Width * _relX), 
                (int)(img.Height * _relY), 
                (int)(img.Width * _relWidth),
                (int)(img.Height * _relHeight)    
            );
            return img.Clone(cropArea, img.PixelFormat);
        }
    }

    class WhiteFilter : IImageProcessor
    {
        private byte _whiteThreshold;

        public WhiteFilter(byte whiteThreshold)
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
                // We filter out pixel below a certain threshold.
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
}
