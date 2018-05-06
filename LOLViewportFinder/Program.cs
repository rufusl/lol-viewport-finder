using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LOLViewportFinder
{
    class Program
    {
        const string CacheDir = "inputCache";
        const string OutDir = "output";
        static bool _enableDebugOutput = false;

        static void Main(string[] args)
        {
            _enableDebugOutput = args.Length > 0 && args[0] == "-d";

            var inputFiles = File.ReadAllLines("inputFiles.txt")
                .Where(inputFile => !string.IsNullOrWhiteSpace(inputFile))
                .Select(inputFile => inputFile.Trim())
                .Where(inputFile => inputFile[0] != '#');

            var minimapArea = new RectangleF(1640f / 1920f, 800f / 1080f, 280f / 1920f, 280f / 1080f);
            var whiteRectDetector = new WhiteRectangleDetector(minimapArea, _enableDebugOutput);

            foreach (var imgFilePathOrUrl in inputFiles)
            {
                Console.WriteLine($"Processing file {imgFilePathOrUrl}");
                var img = ImageUtils.ReadImage(imgFilePathOrUrl, "inputCache");
                var result = whiteRectDetector.FindWhiteRectangle(img);
                if (result != null)
                {
                    Console.WriteLine($"Detected MiniMap Viewport at {result.AbsolutePosition.ToString()}");
                    Console.WriteLine($"relative position in MiniMap: {result.NormalizedToDetectionAreaPosition.ToString()}");
                }
                else
                {
                    Console.Error.WriteLine("No MiniMap Viewport found.");
                }

                if (_enableDebugOutput)
                {
                    PresentDebugInfo(imgFilePathOrUrl, whiteRectDetector.LastDebugInfo);
                }

                Console.WriteLine();
            }
        }

        static void PresentDebugInfo(string imgFilePathOrUrl, WhiteRectangleDetector.IDebugInformation debugInfo)
        {
            // Save images.
            if (!Directory.Exists(OutDir))
                Directory.CreateDirectory(OutDir);

            var path = File.Exists(imgFilePathOrUrl) ? imgFilePathOrUrl : new Uri(imgFilePathOrUrl).LocalPath;
            var filebase = Path.GetFileNameWithoutExtension(path);
            ImageUtils.SaveImageAsJpg(debugInfo.CropImage, Path.Combine(OutDir, $"{filebase}_0_crop.jpg"));
            ImageUtils.SaveImageAsJpg(debugInfo.BWImage, Path.Combine(OutDir, $"{filebase}_1_bw.jpg"));
            ImageUtils.SaveImageAsJpg(debugInfo.BlobsDetectionResult, Path.Combine(OutDir, $"{filebase}_2_blobs.jpg"));

            // Print timing stats.
            Console.WriteLine("Timing stats:");
            foreach(var kv in debugInfo.Timings)
            {
                Console.WriteLine($"{kv.Key}: {kv.Value}ms");
            }
            Console.WriteLine($"Total: {debugInfo.Timings.Values.Sum()}ms");
        }
    }
}
