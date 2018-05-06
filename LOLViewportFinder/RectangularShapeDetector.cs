using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace LOLViewportFinder
{
    /// <summary>
    /// Tries to find a rectangular shape in a list of lines. The shape must not be closed, i.e. two lines are enough.
    /// The rectangular shape is then defined by two lines that begin/end at the same point and that are orthogonal to each other.
    /// </summary>
    class RectangularShapeDetector
    {
        private int _minSideLengthSq;
        private int _edgeConnectionTolerancePx;

        /// <summary>
        /// Creates a new <see cref="RectangularShapeDetector"/>.
        /// </summary>
        /// <param name="minSideLength">The minimum length of a line to be considered a side of the rectangle.</param>
        /// <param name="edgeConnectionTolerancePx">The max allowed distance two line edge points may be offset by to consider them as connected.</param>
        public RectangularShapeDetector(int minSideLength, int edgeConnectionTolerancePx)
        {
            _minSideLengthSq = minSideLength * minSideLength;
            _edgeConnectionTolerancePx = edgeConnectionTolerancePx;
        }

        public bool FormsRectangularShape(IReadOnlyList<PixelLine> lines)
        {
            // We need the line vectors later so we can precalculate them in (PixelLine, Vector) tuples.
            var linesWithVectors = lines.
                Select(l => (Line: l, V: new Vector2(l.End.X - l.Start.X, l.End.Y - l.Start.Y))).
                // Filter out lines below the minimum required length.
                Where(lineWithVector => lineWithVector.V.LengthSquared() >= _minSideLengthSq).
                ToArray();

            if (linesWithVectors.Length < 2)
            {
                return false;
            }

            for (int i = 0; i < linesWithVectors.Length; i++)
            {
                var l1 = linesWithVectors[i];

                for (int j = i + 1; j < linesWithVectors.Length; j++)
                {
                    var l2 = linesWithVectors[j];

                    if (AreConnected(l1.Line, l2.Line))
                    {
                        var dot = Vector2.Dot(l1.V, l2.V);
                        // Two perpendicular lines that are connected at their ends are enough.
                        // We can have the situation that the minimap's viewport overlaps two edges,
                        // resulting in a viewport rectangle that consists of two lines only.
                        if (dot < double.Epsilon)
                            return true;
                    }
                }
            }

            return false;
        }

        private bool AreConnected(PixelLine line1, PixelLine line2)
        {
            return AreConnected(line1.Start, line2.Start) ||
                AreConnected(line1.Start, line2.End) ||
                AreConnected(line1.End, line2.Start) ||
                AreConnected(line1.End, line2.End);
        }

        private bool AreConnected(PixelLocation p1, PixelLocation p2)
        {
            return AreClose(p1.X, p2.X) && AreClose(p1.Y, p2.Y);
        }

        private bool AreClose(int i1, int i2)
        {
            return Math.Abs(i1 - i2) < _edgeConnectionTolerancePx;
        }
    }
}
