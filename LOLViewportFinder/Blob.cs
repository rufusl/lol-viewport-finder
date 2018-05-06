using System.Collections.Generic;

namespace LOLViewportFinder
{
    class Blob
    {
        public IReadOnlyList<PixelLocation> Pixels { get; }

        public Blob(IReadOnlyList<PixelLocation> pixels)
        {
            Pixels = pixels;
        }
    }
}
