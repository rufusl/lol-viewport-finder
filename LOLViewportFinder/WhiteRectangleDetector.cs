using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LOLViewportFinder
{
    /// <summary>
    /// Tries to find a white rectangle in an image.
    /// </summary>
    class WhiteRectangleDetector
    {
        public class RectangleDetectionResult
        {
            /// <summary>
            /// Gets the absolute position of the detected rectangle, relative to the input image's origin (top left).
            /// </summary>
            public Rectangle AbsolutePosition { get; }
            /// <summary>
            /// Gets the position of the detected rectangle, normalized to the detection area inside the image. (0,0) is top left, (1,1) is bottom right
            /// of the detection area.
            /// </summary>
            public RectangleF NormalizedToDetectionAreaPosition { get; }

            public RectangleDetectionResult(Rectangle absolutePosition, RectangleF normalizedToDetectionAreaPosition)
            {
                AbsolutePosition = absolutePosition;
                NormalizedToDetectionAreaPosition = normalizedToDetectionAreaPosition;
            }
        }

        public interface IDebugInformation
        {
            Bitmap CropImage { get; }
            Bitmap BWImage { get; }
            Bitmap BlobsDetectionResult { get; }
            IDictionary<string, double> Timings { get; }
        }

        private class DebugInformation : IDebugInformation
        {
            public Bitmap CropImage { get; set; }
            public Bitmap BWImage { get; set; }
            public Bitmap BlobsDetectionResult { get; set; }
            public IDictionary<string, double> Timings { get { return _timings;  } }

            private readonly Stopwatch _stopwatch = new Stopwatch();
            private readonly Dictionary<string, double> _timings = new Dictionary<string, double>();

            public void StartTiming()
            {
                _stopwatch.Restart();
            }

            public void StopTiming(string tag)
            {
                _stopwatch.Stop();
                var elapsed = _stopwatch.Elapsed;
                _timings.Add(tag, elapsed.TotalMilliseconds);
            }
        }

        private bool _lastDebugInfoEnabled;
        private DebugInformation _lastDebugInfo;
        /// <summary>
        /// Gets debug information for the last <see cref="FindWhiteRectangle(Bitmap, string)"/> call. 
        /// This is null unless the enableDebugInfo flag was set and a call to <see cref="FindWhiteRectangle(Bitmap, string)"/> was made.
        /// </summary>
        internal IDebugInformation LastDebugInfo { get => _lastDebugInfo; }

        private RectangleF _normalizedDetectionArea;
        private List<IImageProcessor> _preprocessorPipeline;

        private BlobDetector _blobDetector;
        private LineDetector _lineDetector;
        private RectangularShapeDetector _rectDetector;

        /// <summary>
        /// Creates a new <see cref="WhiteRectangleDetector"/>.
        /// </summary>
        /// <param name="normalizedDetectionArea">The normalized area (relative to the input images) in which to try to find a rectangle.</param>
        /// <param name="enableDebugInfo">If true, <see cref="LastDebugInfo"/> will be populated.</param>
        public WhiteRectangleDetector(RectangleF normalizedDetectionArea, bool enableDebugInfo = false)
        {
            _normalizedDetectionArea = normalizedDetectionArea;
            _lastDebugInfoEnabled = enableDebugInfo;

            // First crop the image to the detection area (minimap),
            _preprocessorPipeline = new List<IImageProcessor>();
            if (enableDebugInfo)
            {
                _preprocessorPipeline.Add(new Tap((i) =>
                {
                    _lastDebugInfo.StartTiming();
                }));
            }
            _preprocessorPipeline.Add(new ImageCrop(normalizedDetectionArea.X, normalizedDetectionArea.Y, normalizedDetectionArea.Width, normalizedDetectionArea.Height));
            if (enableDebugInfo)
            {
                _preprocessorPipeline.Add(new Tap(img =>
                {
                    _lastDebugInfo.StopTiming("PreProcess: Crop");
                    _lastDebugInfo.CropImage = img;
                    _lastDebugInfo.StartTiming();
                }));
            }
            // then filter out all pixels below a certain threshold to get only "white" pixels.
            _preprocessorPipeline.Add(new BlackWhiteConverter(220));
            if (enableDebugInfo)
            {
                _preprocessorPipeline.Add(new Tap(img =>
                {
                    _lastDebugInfo.StopTiming("PreProcess: BW");
                    _lastDebugInfo.BWImage = img;
                }));
            }

            _blobDetector = new BlobDetector(byte.MaxValue, 20);
            _lineDetector = new LineDetector(5);
            _rectDetector = new RectangularShapeDetector(10, 4);
        }

        public RectangleDetectionResult FindWhiteRectangle(Bitmap inputImage)
        {
            if (_lastDebugInfoEnabled)
            {
                _lastDebugInfo = new DebugInformation();
            }

            var preprocessed = PreProcessImage(inputImage);

            if (_lastDebugInfoEnabled)
            {
                _lastDebugInfo.StartTiming();
            }

            var blobs = _blobDetector.FindBlobs(preprocessed);

            if (_lastDebugInfoEnabled)
            {
                _lastDebugInfo.BlobsDetectionResult = ImageUtils.CreateBlobsHighlightImage(
                    _lastDebugInfo.BWImage.Width,
                    _lastDebugInfo.BWImage.Height,
                    blobs
                );
                _lastDebugInfo.StartTiming();
            }

            foreach (var b in blobs)
            {
                var lines = _lineDetector.DetectLines(b);

                if (_rectDetector.FormsRectangularShape(lines))
                {
                    // The detected rectangle in pixels, relative to the detection area (minimap).
                    var rectInDetectionAreaInPixels = GeometryUtils.FindBoundingBox(lines);
                    // The detected rectangle in pixels, relative to (0,0) of the input image.
                    var rectInImageAbsolute = NormalizedDetetionAreaCoordinatesToAbsoluteCoordinates(inputImage, rectInDetectionAreaInPixels);
                    // The detected rectangle, normalized to the detection area (minimap).
                    var rectInDetectionAreaNormalized = GeometryUtils.NormalizeRect(
                        rectInDetectionAreaInPixels, 
                        (int)(_normalizedDetectionArea.Width * inputImage.Width), 
                        (int)(_normalizedDetectionArea.Height * inputImage.Height)
                    );

                    if (_lastDebugInfoEnabled)
                        _lastDebugInfo.StopTiming("RectangleDetection");
                    return new RectangleDetectionResult(rectInImageAbsolute, rectInDetectionAreaNormalized);
                }
            }

            if (_lastDebugInfoEnabled)
                _lastDebugInfo.StopTiming("RectangleDetection");
            // No matching blob.
            return null;
        }

        private Bitmap PreProcessImage(Bitmap inputImage)
        {
            return _preprocessorPipeline.Aggregate(inputImage, (current, imgProcessor) => imgProcessor.ProcessImage(current));
        }

        private Rectangle NormalizedDetetionAreaCoordinatesToAbsoluteCoordinates(Bitmap img, Rectangle rect)
        {
            int miniMapX = (int)(_normalizedDetectionArea.X * img.Width);
            int miniMapY = (int)(_normalizedDetectionArea.Y * img.Height);

            return new Rectangle(
                miniMapX + rect.X,
                miniMapY + rect.Y,
                rect.Width,
                rect.Height
            );
        }
    }
}
