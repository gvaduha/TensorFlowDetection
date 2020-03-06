using System;
using System.Drawing;
using System.Threading;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;

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
            _sink = new VideoWriter(fileUri, FourCC(codec), 0.25,
                new Size((int)_vsrc.GetCaptureProperty(CapProp.FrameWidth), (int)_vsrc.GetCaptureProperty(CapProp.FrameHeight)),
                _vsrc.GetCaptureProperty(CapProp.Monochrome) == 0);
            _sink.Set(VideoWriter.WriterProperty.NStripes, 1);
        }

        public Capture CaptureConfig(Action<VideoCapture> fun)
        {
            fun(_vsrc);
            return this;
        }

        private uint n = 1;

        private void ImageGrabbed(object sender, EventArgs e)
        {
            var framepos = _vsrc.GetCaptureProperty(CapProp.PosFrames);
            var text = $"#{n:####} TID:{Thread.CurrentThread.ManagedThreadId} Sec{DateTime.Now.Second}=>F#:[{framepos}] Pos:[{_vsrc.GetCaptureProperty(CapProp.PosMsec)}] AviPos:[{_vsrc.GetCaptureProperty(CapProp.PosAviRatio):#.##########}] FRAME:";
            Console.WriteLine(text);
            if (framepos % 25 == 0)
            {
                Console.WriteLine($"======================> writing {framepos}");
                using var m = new Mat();
                _vsrc.Read(m);
                if (m == null) Console.WriteLine("***************************************************");
                ////////////////////////////////
                Mat resm = null;
                {
                    var img = m.ToImage<Bgr, byte>();
                    img.Draw(text, new Point(10, 150), FontFace.HersheySimplex, 1, new Bgr(Color.Red), 2);
                    resm = img.Mat;
                }
                ////////////////////////////////
                _sink.Write(resm);
            }
            n++;
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
            using var x = new Capture(@"earth480x270.mp4", @"out.avi");
            x.Start();
            Console.ReadLine();
        }
    }
}
