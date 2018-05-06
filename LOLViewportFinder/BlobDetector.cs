using System.Collections.Generic;
using System.Drawing;

namespace LOLViewportFinder
{

    class BlobDetector
    {
        private readonly byte _blobColorValue;
        private readonly int _minBlobPixels;

        public BlobDetector(byte blobColorValue, int minBlobPixels)
        {
            _blobColorValue = blobColorValue;
            _minBlobPixels = minBlobPixels;
        }

        public IReadOnlyList<Blob> FindBlobs(Bitmap img)
        {
            var width = img.Width;
            var height = img.Height;
            var imgData = ImageUtils.ReadPixels(img, out int bytesPerPixel, out int stride);

            var imageMap = new byte[img.Width, img.Height];
            for (int i = 0; i < imgData.Length; i++)
            {
                var x = (i % stride) / bytesPerPixel;
                var y = (i / stride);
                imageMap[x, y] = imgData[i];
            }

            var visited = new HashSet<PixelLocation>(PixelLocation.EqualityComparer);
            var blobs = new List<Blob>();
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
                        var blob = CollectContiguousBlob(imageMap, c, visited);
                        if (blob.Pixels.Count > _minBlobPixels)
                            blobs.Add(blob);
                    }
                }
            }

            return blobs;
        }

        private Blob CollectContiguousBlob(byte[,] imageMap, PixelLocation pxLocation, HashSet<PixelLocation> visited)
        {
            var width = imageMap.GetLength(0);
            var height = imageMap.GetLength(1);
            var blob = new Blob();
            blob.Pixels.Add(pxLocation);

            var pending = new Queue<PixelLocation>();

            AddNeighbouringPixels(pxLocation, pending, visited, 0, 0, width, height);

            while (pending.Count > 0)
            {
                var p = pending.Dequeue();

                if (imageMap[p.X, p.Y] == _blobColorValue)
                {
                    blob.Pixels.Add(p);
                    AddNeighbouringPixels(p, pending, visited, 0, 0, width, height);
                }
            }
            return blob;
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
