using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading;

namespace LOLViewportFinder.CaptureDemo
{
    class CaptureRectDraw
    {
        public event Action<Bitmap> OnImageCaptured;

        public CaptureRectDraw()
        {
        }

        public CancellationTokenSource Start()
        {
            var cts = new CancellationTokenSource();
            SearchDrawRectWindow(cts.Token);
            return cts;
        }

        private void SearchDrawRectWindow(CancellationToken ct)
        {
            Task.Run(() =>
            {
                while (!ct.IsCancellationRequested)
                {
                    var handle = Win32.FindWindow(null, "RectDraw");
                    if (handle != IntPtr.Zero)
                    {
                        Console.WriteLine("RectDraw window found. Capturing...");
                        StartCapturing(handle, ct);
                    }
                    else
                    {
                        Thread.Sleep(1000);
                    }
                }
            }, ct);
        }

        private void StartCapturing(IntPtr handle, CancellationToken ct)
        {
            var stop = false;
            while (!stop && !ct.IsCancellationRequested)
            {
                try
                {
                    var capture = CaptureWindow(handle);
                    OnImageCaptured?.Invoke(capture);
                }
                catch(Exception e)
                {
                    Console.WriteLine($"Capturing error: {e.Message}. Stopping capture and trying to find window again.");
                    stop = true;
                }

            }
        }

        public Bitmap CaptureWindow(IntPtr handle)
        {
            Win32.RECT clientRect = new Win32.RECT();
            Win32.GetClientRect(handle, out clientRect);
            var topLeft = new Win32.POINT()
            {
                x = clientRect.left,
                y = clientRect.top
            };
            Win32.ClientToScreen(handle, ref topLeft);
            var bottomRight = new Win32.POINT()
            {
                x = clientRect.right,
                y = clientRect.bottom
            };
            Win32.ClientToScreen(handle, ref bottomRight);

            int width = clientRect.right - clientRect.left;
            int height = clientRect.bottom - clientRect.top;

            var bitmap = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            Graphics.FromImage(bitmap).CopyFromScreen(
                topLeft.x, 
                topLeft.y,
                0, 
                0, 
                new Size(width, height), 
                CopyPixelOperation.SourceCopy
            );
            return bitmap;
        }
    }

}