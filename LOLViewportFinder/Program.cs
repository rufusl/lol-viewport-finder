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
        static readonly string _consoleClear = new string(' ', Console.BufferWidth - 1);

        static void Main(string[] args)
        {
            _enableDebugOutput = args.Length > 0 && args.Any(a => a == "-d");
            if (args.Length > 0 && args.Any(a => a == "--rectDraw"))
            {
                RectDrawDemo();
            }
            else
            {
                ProcessInputImages(args);
            }
        }

        static void ProcessInputImages(string[] args)
        {
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
                    PresentDebugInfoWithOriginalFile(imgFilePathOrUrl, whiteRectDetector.LastDebugInfo);
                }

                Console.WriteLine();
            }
        }

        static void PresentDebugInfoWithOriginalFile(string imgFilePathOrUrl, WhiteRectangleDetector.IDebugInformation debugInfo)
        {
            var path = File.Exists(imgFilePathOrUrl) ? imgFilePathOrUrl : new Uri(imgFilePathOrUrl).LocalPath;
            var filebase = Path.GetFileNameWithoutExtension(path);
            PresentDebugInfo(filebase, debugInfo);
        }

        static void PresentDebugInfo(string filebase, WhiteRectangleDetector.IDebugInformation debugInfo)
        {
            // Save images.
            if (!Directory.Exists(OutDir))
                Directory.CreateDirectory(OutDir);

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

        static void RectDrawDemo()
        {
            var whiteRectDetector = new WhiteRectangleDetector(new RectangleF(0, 0, 1, 1), _enableDebugOutput);
            WhiteRectangleDetector.RectangleDetectionResult lastResult = null;

            var capturer = new CaptureDemo.CaptureRectDraw();
            capturer.OnImageCaptured += img =>
            {
                try
                {
                    var result = whiteRectDetector.FindWhiteRectangle(img);
                    if (lastResult != result)
                    {
                        // Clear console line.
                        Console.Write($"\r{_consoleClear}");
                    }

                    if (result != null)
                    {
                        Console.Write($"\rFound white rectangle at {result.AbsolutePosition.ToString()}");
                        if (_enableDebugOutput)
                            PresentDebugInfo("rectDraw", whiteRectDetector.LastDebugInfo);
                    }
                    else
                    {
                        Console.Write("\rWhite rectangle not found.");
                    }
                    lastResult = result;
                }
                finally
                {
                    img.Dispose();
                }

                System.Threading.Thread.Sleep(10);
            };
            var cts = capturer.Start();

            Console.WriteLine("Waiting for RectDraw window. Press <escape> to exit.");
            while(Console.ReadKey().Key != ConsoleKey.Escape)
            {
            }

            cts.Cancel();
        }
    }
}
