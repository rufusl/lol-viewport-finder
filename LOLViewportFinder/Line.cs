namespace LOLViewportFinder
{
    class Line
    {
        public PixelLocation Start { get; }
        public PixelLocation End { get; }

        public Line(int startX, int startY, int endX, int endY)
        {
            Start = new PixelLocation(startX, startY);
            End = new PixelLocation(endX, endY);
        }

        public override string ToString()
        {
            return $"{{{Start.X}, {Start.Y}}} - {{{End.X}, {End.Y}}}";
        }
    }
}
