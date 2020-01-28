using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TensorFlow;

namespace TensorSharpStresser
{
    class Stresser
    {
        private readonly Guid _id;
        private readonly Random _rnd = new Random();
        private readonly Size _imgSize;
        private readonly int _imgByteLen;
        private readonly byte[] _buff;
        private readonly TFGraph _graph;
        private readonly TFSession _session;
        private readonly object _sessionLocker = new object();

        public Stresser(byte[] model, Size imgSize)
        {
            _id = Guid.NewGuid();
            _imgSize = imgSize;
            _imgByteLen = _imgSize.Width * _imgSize.Height * 3;
            _buff = new byte[_imgByteLen];
            _graph = new TFGraph();
            _graph.Import(model);
            _session = new TFSession(_graph);
        }

        public void RunDetection()
        {
            _rnd.NextBytes(_buff);
            TFTensor tensor = TFTensor.FromBuffer(new TFShape(1, _imgSize.Height, _imgSize.Width, 3), _buff, 0, _buff.Length);

            TFTensor[] output;
            lock (_sessionLocker)
            {
                output = _session.Run(new[] { _graph["image_tensor"][0] }, new[] { tensor }, new[]
                {
                    _graph["detection_boxes"][0],
                    _graph["detection_scores"][0],
                    _graph["detection_classes"][0],
                    _graph["num_detections"][0]
                });
            }
        }

        public void RunDetectionAsync(int cycles)
        {
            Enumerable.Range(0, cycles).AsParallel().ForAll(_ => RunDetection());
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            byte[] model = File.ReadAllBytes(Settings.Default.ModelFile);
            var stresser = new Stresser(model, Settings.Default.ImgSize);
            stresser.RunDetectionAsync(Settings.Default.StressCycles);
        }
    }
}
