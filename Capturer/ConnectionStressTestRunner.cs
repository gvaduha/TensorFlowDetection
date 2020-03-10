using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using Emgu.CV;

namespace Capturer
{
    /// <summary>
    /// This test has shown than EMGU CV has serious performance problems
    /// potentially with locks in FFMpeg network layer
    /// We can't control cams connection timeout so nonexitent cam freezes
    /// creation loop and prevent to initialize good cams.
    /// Also all cam creation process queued allegedly with network ops
    /// </summary>
    class ImageGrabber
    {
        public static async Task<ImageGrabber> CreateAsync(string uri)
        {
            try
            {
                Console.WriteLine($"Create  {uri}");
                var vc = await Task.Run(() => new VideoCapture(uri, VideoCapture.API.Ffmpeg));
                Console.WriteLine($"CreatED {uri} with {vc.BackendName}");
                var ig = new ImageGrabber(vc);
                Console.WriteLine($"CreatED {uri} with {vc.BackendName} [{ig._id}]");
                return ig;
            }
            catch (Exception e)
            {
                Console.WriteLine($"FAULT {uri}: {e}");
                return NullImageGrabber;
            }
        }

        public static ImageGrabber Create(string uri)
        {
            Console.WriteLine($"Create  {uri}");
            var vc = new VideoCapture(uri);
            var ig = new ImageGrabber(vc);
            Console.WriteLine($"CreatED {uri} with {vc.BackendName} [{ig._id}]");
            return ig;
        }

        private readonly VideoCapture _vc;
        private Guid _id;

        protected ImageGrabber(VideoCapture vc)
        {
            _id = Guid.NewGuid();
            _vc = vc;
            _vc.ImageGrabbed += ImageGrabbed;
        }

        private ImageGrabber() { }
        public static ImageGrabber NullImageGrabber = new ImageGrabber();

        private void ImageGrabbed(object sender, EventArgs e)
        {
            try
            {
                Mat m = new Mat();
                _vc.Read(m);
                Console.WriteLine($"{_id.ToString().Substring(0,8)}:{m.Width}x{m.Height}");
            }
            catch (Exception exception)
            {
                Console.WriteLine($"{_id.ToString().Substring(0,8)}: FAULT {exception.Message}");
            }
        }

        public void Start()
        {
            _vc?.Start();
        }
    }

    static class ConnectionStressTestRunner
    {
        static readonly List<string> Cams = new List<string>
        {
            "rtsp://cam1/",
            "rtsp://cam2/",
            "rtsp://nonexistent",
            "rtsp://nonexistent",
            "rtsp://cam1:666/",
            "rtsp://cam2:666/",
        };

        public static async Task RunAsync()
        {

            var watch = System.Diagnostics.Stopwatch.StartNew();

            var tasks = Cams.Select(async x => await ImageGrabber.CreateAsync(x));
            Console.WriteLine($"ImageGrabber.CreateAsync collection: {watch.ElapsedMilliseconds}");

            var results = await Task.WhenAll(tasks.ToList());
            Console.WriteLine($"Task.WhenAll CreateAsync collection: {watch.ElapsedMilliseconds}");

            results.ToList().ForEach(x => x.Start());
            Console.WriteLine($"ForEach Start capturing  collection: {watch.ElapsedMilliseconds}");

            Console.WriteLine("Press enter to quit");
            Console.ReadLine();
        }

        public static void Run()
        {

            var watch = System.Diagnostics.Stopwatch.StartNew();

            var results = Cams.Select(ImageGrabber.Create);
            Console.WriteLine($"Create cams collection: {watch.ElapsedMilliseconds}");

            results.ToList().ForEach(x => x.Start());
            Console.WriteLine($"Start capturing  collection: {watch.ElapsedMilliseconds}");

            //Console.WriteLine("Press enter to quit");
            //Console.ReadLine();
        }
    }
}
