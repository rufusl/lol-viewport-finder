using System.Collections.Generic;

namespace LOLViewportFinder
{
    struct PixelLocation
    {
        private class PixelLocationEqualityComparer : IEqualityComparer<PixelLocation>
        {
            public bool Equals(PixelLocation p1, PixelLocation p2)
            {
                return p1.GetHashCode() == p2.GetHashCode();
            }

            public int GetHashCode(PixelLocation obj)
            {
                return obj.GetHashCode();
            }
        }
        public static readonly IEqualityComparer<PixelLocation> EqualityComparer = new PixelLocationEqualityComparer();

        public int X { get; }
        public int Y { get; }

        private int _hashCode;

        public PixelLocation(int x, int y)
        {
            X = x;
            Y = y;

            // Assume images are never going to get bigger than short.MaxValue (32k).
            // Higher 16 bits of an integer store X, lower 16 bits Y.
            _hashCode = ((short)x << (sizeof(short) * 8)) + (short)y;
        }

        public bool Equals(PixelLocation other)
        {
            return X == other.X && Y == other.Y;
        }

        public override bool Equals(object obj)
        {
            if (obj is PixelLocation)
                return Equals((PixelLocation)obj);
            else
                return false;
        }

        public override int GetHashCode()
        {
            return _hashCode;
        }
    }
}
