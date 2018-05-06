using System;
using System.Collections.Generic;
using System.Linq;

namespace LOLViewportFinder
{
    class LineDetector
    {
        private class AxisAlignedLine
        {
            public int Start;
            public int End;

            public int OtherAxisValue;
        }

        private int _minLineLengthPx;

        public LineDetector(int minLineLengthPx)
        {
            _minLineLengthPx = minLineLengthPx;
        }

        public IReadOnlyList<Line> DetectLines(Blob blob)
        {
            if (blob.Pixels.Count < 1)
            {
                return new List<Line>();
            }

            var openHorizontalLines = new List<AxisAlignedLine>
            {
                new AxisAlignedLine()
                {
                    Start = blob.Pixels[0].X,
                    End = blob.Pixels[0].X,

                    OtherAxisValue = blob.Pixels[0].Y
                }
            };
            var openVerticalLines = new List<AxisAlignedLine>
            {
                new AxisAlignedLine()
                {
                    Start = blob.Pixels[0].Y,
                    End = blob.Pixels[0].Y,

                    OtherAxisValue = blob.Pixels[0].X
                }
            };

            for (int i = 1; i < blob.Pixels.Count; i++)
            {
                var px = blob.Pixels[i];
                var newLine = true;
                foreach(var hl in openHorizontalLines)
                {
                    if (hl.OtherAxisValue != px.Y)
                        continue;

                    if (hl.Start == px.X + 1)
                    {
                        // Start is right of px, move to left.
                        hl.Start = px.X;
                        newLine = false;
                    }
                    else if (hl.End == px.X - 1)
                    {
                        // End is left of px, move to right.
                        hl.End = px.X;
                        newLine = false;
                    }
                }

                foreach (var vl in openVerticalLines)
                {
                    if (vl.OtherAxisValue != px.X)
                        continue;

                    if (vl.Start == px.Y + 1)
                    {
                        // Start is bottom of px, move to top.
                        vl.Start = px.Y;
                        newLine = false;
                    }
                    else if (vl.End == px.Y - 1)
                    {
                        // End is top of px, move to bottom.
                        vl.End = px.Y;
                        newLine = false;
                    }
                }

                if (newLine)
                {
                    openHorizontalLines.Add(new AxisAlignedLine()
                    {
                        Start = px.X,
                        End = px.X,

                        OtherAxisValue = px.Y
                    });
                    openVerticalLines.Add(new AxisAlignedLine()
                    {
                        Start = px.Y,
                        End = px.Y,

                        OtherAxisValue = px.X
                    });
                }
            }

            var foundLines = openHorizontalLines.
                Where(l => Math.Abs(l.Start - l.End) > _minLineLengthPx).
                Select(l => new Line(l.Start, l.OtherAxisValue, l.End, l.OtherAxisValue)).
                Concat(openVerticalLines.
                    Where(l => Math.Abs(l.Start - l.End) > _minLineLengthPx).
                    Select(l => new Line(l.OtherAxisValue, l.Start, l.OtherAxisValue, l.End))
                ).ToList();

            return foundLines;
        }
    }
}
