using System;
using System.Drawing;
using Emgu.CV;
using Emgu.CV.CvEnum;

namespace Capturer
{
    class Capture : IDisposable
    {
        VideoCapture _vsrc;
        VideoWriter _sink;

        static int FourCC(string code) => VideoWriter.Fourcc(code[0], code[1], code[2], code[3]);
        
        public Capture(string videoUri, string fileUri, string codec = "MPEG")
        {
            _vsrc = new VideoCapture(videoUri);
            _vsrc.ImageGrabbed += ImageGrabbed;
            _sink = new VideoWriter(fileUri, FourCC(codec), 1,
                new Size((int)_vsrc.GetCaptureProperty(CapProp.FrameWidth), (int)_vsrc.GetCaptureProperty(CapProp.FrameHeight)),
                _vsrc.GetCaptureProperty(CapProp.Monochrome) == 0);
        }

        public Capture CaptureConfig(Action<VideoCapture> fun)
        {
            fun(_vsrc);
            return this;
        }

        private void ImageGrabbed(object sender, EventArgs e)
        {
            var framepos = _vsrc.GetCaptureProperty(CapProp.PosFrames);
            Console.WriteLine($"{DateTime.Now.Second}=>{framepos}:{_vsrc.GetCaptureProperty(CapProp.PosMsec)}:{_vsrc.GetCaptureProperty(CapProp.PosAviRatio)}");
            if (framepos % 25 == 0)
            {
                Console.WriteLine($"======================> writing {framepos}");
                using var m = new Mat();
                _vsrc.Read(m);
                _sink.Write(m);
            }
        }

        public void Start() => _vsrc.Start();
        public void Stop() => _vsrc.Stop();

        public void Dispose()
        {
            _vsrc.Stop();
            _vsrc.Dispose();
            _sink.Dispose();
        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            using var x = new Capture(@"C:\rusalhell.mp4", @"C:\videos\xxx.avi");
            x.Start();
            Console.ReadLine();
        }
    }
}
