using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LOLViewportFinder
{
    class GeometryUtils
    {
        public static Rectangle FindBoundingBox(IReadOnlyList<Line> lines)
        {
            int minX = int.MaxValue, minY = int.MaxValue, maxX = int.MinValue, maxY = int.MinValue;
            foreach (var l in lines)
            {
                if (l.Start.X < minX)
                {
                    minX = l.Start.X;
                }
                else if (l.Start.X > maxX)
                {
                    maxX = l.Start.X;
                }

                if (l.Start.Y < minY)
                {
                    minY = l.Start.Y;
                }
                else if (l.Start.Y > maxY)
                {
                    maxY = l.Start.Y;
                }

                if (l.End.X < minX)
                {
                    minX = l.End.X;
                }
                else if (l.End.X > maxX)
                {
                    maxX = l.End.X;
                }

                if (l.End.Y < minY)
                {
                    minY = l.End.Y;
                }
                else if (l.End.Y > maxY)
                {
                    maxY = l.End.Y;
                }
            }

            return new Rectangle(minX, minY, maxX - minX, maxY - minY);
        }

        public static RectangleF MakeRelativeRect(Rectangle absoluteRect, int imgWidth, int imgHeight)
        {
            return new RectangleF(
                (float)absoluteRect.X / imgWidth,
                (float)absoluteRect.Y / imgHeight,
                (float)absoluteRect.Width / imgWidth,
                (float)absoluteRect.Height / imgHeight
            );
        }
    }
}
