using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace LOLViewportFinder
{
    class RectangularShapeDetector
    {
        private int _minSideLengthSq;
        private int _edgeConnectionThreshold;

        public RectangularShapeDetector(int minSideLength, int edgeConnectionThreshold)
        {
            _minSideLengthSq = minSideLength * minSideLength;
            _edgeConnectionThreshold = edgeConnectionThreshold;
        }

        public bool FormsRectangularShape(IReadOnlyList<Line> lines)
        {
            var linesWithVectors = lines.
                Select(l => (Line: l, V: new Vector2(l.End.X - l.Start.X, l.End.Y - l.Start.Y))).
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

        private bool AreConnected(Line line1, Line line2)
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
            return Math.Abs(i1 - i2) < _edgeConnectionThreshold;
        }
    }
}
