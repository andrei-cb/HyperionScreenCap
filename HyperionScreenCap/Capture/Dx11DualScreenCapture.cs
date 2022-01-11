using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;

namespace HyperionScreenCap.Capture
{
    class Dx11DualScreenCapture : IScreenCapture
    {
        private DX11ScreenCapture _capture1;
        private DX11ScreenCapture _capture2;

        private int _adapterIndex;
        private int _monitorIndex1;
        private int _monitorIndex2;
        private int _scalingFactor;
        private int _maxFps;
        private int _frameCaptureTimeout;

        private int _minCaptureTime;
        private Stopwatch _captureTimer;

        public int CaptureWidth { get; private set; }

        public int CaptureHeight { get; private set; }

        public Dx11DualScreenCapture(int adapterIndex, int monitorIndex1, int monitorIndex2, int scalingFactor, int maxFps, int frameCaptureTimeout)
        {
            _adapterIndex = adapterIndex;
            _monitorIndex1 = monitorIndex1;
            _monitorIndex2 = monitorIndex2;
            _scalingFactor = scalingFactor;
            _maxFps = maxFps;
            _frameCaptureTimeout = frameCaptureTimeout;
        }

        public byte[] Capture()
        {
            _captureTimer.Restart();

            var image1 = _capture1.Capture();
            var image2 = _capture2.Capture();

            var bitmap1 = ImageFromRawArray(image1, _capture1.CaptureWidth, _capture1.CaptureHeight, PixelFormat.Format24bppRgb);
            var bitmap2 = ImageFromRawArray(image2, _capture2.CaptureWidth, _capture2.CaptureHeight, PixelFormat.Format24bppRgb);

            // reduce image1 size to match image2 height
            if(bitmap1.Height > bitmap2.Height)
            {
                var original = bitmap1;
                bitmap1 = new Bitmap(original, new Size(_capture1.CaptureWidth, CaptureHeight));
                original.Dispose();
            }
            // reduce image2 size to match image height
            else if(bitmap1.Height < bitmap2.Height)
            {
                var original = bitmap2;
                bitmap2 = new Bitmap(original, new Size(_capture2.CaptureWidth, CaptureHeight));
                original.Dispose();
            }

            using (Bitmap mergedBitmap = new Bitmap(CaptureWidth, CaptureHeight, PixelFormat.Format24bppRgb))
            {
                using (Graphics g = Graphics.FromImage(mergedBitmap))
                {
                    g.DrawImage(bitmap1, 0, 0);
                    g.DrawImage(bitmap2, bitmap1.Width, 0);

                    bitmap1.Dispose();
                    bitmap2.Dispose();

                    _captureTimer.Stop();

                    return BitmapToByteArray(mergedBitmap);
                }
            }
        }
 
        public void DelayNextCapture()
        {
            int remainingFrameTime = _minCaptureTime - (int)_captureTimer.ElapsedMilliseconds;
            if (remainingFrameTime > 0)
            {
                Thread.Sleep(remainingFrameTime);
            }
        }

        public void Dispose()
        {
            _capture1.Dispose();
            _capture2.Dispose();
        }

        public void Initialize()
        {
            _capture1 = new DX11ScreenCapture(_adapterIndex, _monitorIndex1, _scalingFactor, _maxFps, _frameCaptureTimeout);
            _capture2 = new DX11ScreenCapture(_adapterIndex, _monitorIndex2, _scalingFactor, _maxFps, _frameCaptureTimeout);

            _capture1.Initialize();
            _capture2.Initialize();

            CaptureWidth = (_capture1.CaptureWidth + _capture2.CaptureWidth);
            CaptureHeight = Math.Min(_capture1.CaptureHeight, _capture2.CaptureHeight);

            _captureTimer = new Stopwatch();
            _minCaptureTime = 1000 / _maxFps;
        }

        public bool IsDisposed()
        {
            return (_capture1 == null || _capture2 == null) || (_capture1.IsDisposed() && _capture2.IsDisposed());
        }

        private static Image ImageFromRawArray(byte[] arr, int width, int height, PixelFormat pixelFormat)
        {
            var output = new Bitmap(width, height, pixelFormat);
            var rect = new Rectangle(0, 0, width, height);
            var bmpData = output.LockBits(rect, ImageLockMode.ReadWrite, output.PixelFormat);

            // Row-by-row copy
            var arrRowLength = width * Image.GetPixelFormatSize(output.PixelFormat) / 8;
            var ptr = bmpData.Scan0;
            for (var i = 0; i < height; i++)
            {
                Marshal.Copy(arr, i * arrRowLength, ptr, arrRowLength);
                ptr += bmpData.Stride;
            }

            output.UnlockBits(bmpData);
            return output;
        }

        private static byte[] BitmapToByteArray(Bitmap bitmap)
        {
            BitmapData bmpdata = null;

            try
            {
                bmpdata = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, bitmap.PixelFormat);
                int numbytes = bmpdata.Stride * bitmap.Height;
                byte[] bytedata = new byte[numbytes];
                IntPtr ptr = bmpdata.Scan0;

                Marshal.Copy(ptr, bytedata, 0, numbytes);

                return bytedata;
            }
            finally
            {
                if (bmpdata != null)
                    bitmap.UnlockBits(bmpdata);
            }

        }
    }
}
