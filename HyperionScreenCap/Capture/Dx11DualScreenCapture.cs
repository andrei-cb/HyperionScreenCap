using System;
using System.Diagnostics;
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

            byte[] bytes = new byte[image1.Length + image2.Length];

            for(var y = 0; y < CaptureHeight; y++)
            {
                for(var x = 0; x < CaptureWidth; x++)
                {
                    if(x < _capture1.CaptureWidth)
                    {
                        bytes[y * 3 * CaptureWidth + x * 3] = image1[y * 3 * _capture1.CaptureWidth + x * 3];
                        bytes[y * 3 * CaptureWidth + x * 3 + 1] = image1[y * 3 * _capture1.CaptureWidth + x * 3 + 1];
                        bytes[y * 3 * CaptureWidth + x * 3 + 2] = image1[y * 3 * _capture1.CaptureWidth + x * 3 + 2];
                    }
                    else
                    {
                        bytes[y * 3 * CaptureWidth + x * 3] = image2[y * 3 * _capture2.CaptureWidth + x * 3 - _capture1.CaptureWidth * 3];
                        bytes[y * 3 * CaptureWidth + x * 3 + 1] = image2[y * 3 * _capture2.CaptureWidth + x * 3 - _capture1.CaptureWidth * 3 + 1];
                        bytes[y * 3 * CaptureWidth + x * 3 + 2] = image2[y * 3 * _capture2.CaptureWidth + x * 3 - _capture1.CaptureWidth * 3 + 2];
                    }
                }
            }  

            _captureTimer.Stop();

            return bytes;
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
            CaptureHeight = (_capture1.CaptureHeight + _capture2.CaptureHeight) / 2;

            _captureTimer = new Stopwatch();
            _minCaptureTime = 1000 / _maxFps;
        }

        public bool IsDisposed()
        {
            return (_capture1 == null || _capture2 == null) || (_capture1.IsDisposed() && _capture2.IsDisposed());
        }
    }
}
