using System.Collections.Generic;
using System.Drawing;

namespace LOLViewportFinder
{
    /// <summary>
    /// Finds areas of connected pixels of the same color.
    /// </summary>
    class BlobDetector
    {
        private readonly byte _blobColorValue;
        private readonly int _minBlobPixels;

        /// <summary>
        /// Creates a new <see cref="BlobDetector"/>.
        /// </summary>
        /// <param name="blobColorValue">The color value to group.</param>
        /// <param name="minBlobPixels">The minimum number of pixels to form a valid blob.</param>
        public BlobDetector(byte blobColorValue, int minBlobPixels)
        {
            _blobColorValue = blobColorValue;
            _minBlobPixels = minBlobPixels;
        }

        public IEnumerable<Blob> FindBlobs(Bitmap img)
        {
            var width = img.Width;
            var height = img.Height;
            var imgData = ImageUtils.ReadPixels(img, out int bytesPerPixel, out int stride);

            // Copy image to two-dimensional array for easier and more efficient navigation through the image.
            var imageMap = new byte[img.Width, img.Height];
            for (int i = 0; i < imgData.Length; i++)
            {
                var x = (i % stride) / bytesPerPixel;
                var y = (i / stride);
                imageMap[x, y] = imgData[i];
            }

            // the set of pixels that we already processed
            var visited = new HashSet<PixelLocation>(PixelLocation.EqualityComparer);
            // Iterate column-wise.
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    var c = new PixelLocation(x, y);
                    if (visited.Contains(c))
                        continue;

                    visited.Add(c);

                    var pixelValue = imageMap[x, y];
                    if (pixelValue == _blobColorValue)
                    {
                        // Once we find a matching pixel, we start blob detection.
                        var blob = CollectContiguousBlob(imageMap, c, visited);
                        if (blob.Pixels.Count > _minBlobPixels)
                            yield return blob;
                    }
                }
            }
        }

        /// <summary>
        /// Starts at a position and finds all adjacent pixels of the same color. Search if performed in all directions (also diagonal).
        /// </summary>
        /// <param name="imageMap">The input image to collect blobs from.</param>
        /// <param name="pxLocation">The initial pixel to start collecting blob pixels from.</param>
        /// <param name="visited">The set of pixels that we already checked.</param>
        /// <returns>A <see cref="Blob"/> of connected pixels of the same color.</returns>
        private Blob CollectContiguousBlob(byte[,] imageMap, PixelLocation pxLocation, HashSet<PixelLocation> visited)
        {
            var width = imageMap.GetLength(0);
            var height = imageMap.GetLength(1);
            var connectedPixels = new List<PixelLocation>();
            connectedPixels.Add(pxLocation);

            var pending = new Queue<PixelLocation>();

            AddNeighbouringPixels(pxLocation, pending, visited, 0, 0, width - 1, height - 1);

            while (pending.Count > 0)
            {
                var p = pending.Dequeue();

                if (imageMap[p.X, p.Y] == _blobColorValue)
                {
                    connectedPixels.Add(p);
                    AddNeighbouringPixels(p, pending, visited, 0, 0, width - 1, height - 1);
                }
            }
            return new Blob(connectedPixels);
        }

        private void AddNeighbouringPixels(
            PixelLocation fromLocation, 
            Queue<PixelLocation> toQueue, 
            HashSet<PixelLocation> visited,
            int minX, int minY, int maxX, int maxY)
        {
            
            void enqueueIfNotVisitedAndNotOutOfBounds(int x, int y)
            {
                if (x < minX || x > maxX || y < minY || y > maxY)
                    return;

                var pxLocation = new PixelLocation(x, y);
                if (!visited.Contains(pxLocation))
                {
                    // Mark as visited already to prevent it from being enqueued again.
                    visited.Add(pxLocation);
                    toQueue.Enqueue(pxLocation);
                }
            };

            // Right
            enqueueIfNotVisitedAndNotOutOfBounds(fromLocation.X + 1, fromLocation.Y);
            // Bottom Right
            enqueueIfNotVisitedAndNotOutOfBounds(fromLocation.X + 1, fromLocation.Y + 1);
            // Bottom
            enqueueIfNotVisitedAndNotOutOfBounds(fromLocation.X, fromLocation.Y + 1);
            // Bottom Left
            enqueueIfNotVisitedAndNotOutOfBounds(fromLocation.X - 1, fromLocation.Y + 1);
            // Top
            enqueueIfNotVisitedAndNotOutOfBounds(fromLocation.X, fromLocation.Y - 1);
            // Top Right
            enqueueIfNotVisitedAndNotOutOfBounds(fromLocation.X + 1, fromLocation.Y - 1);


            // Left and top left are never needing when traversing columnwise from the top left of the image
            // to the bottom right.
        }
    }
}
