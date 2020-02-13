using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TensorFlow;

namespace TensorSharpStresser
{
    struct DetectionResult
    {
        public Rectangle box;
        public float score;
        public int @class;
    }

    class ImageTensorProcessor
    {
        private readonly Guid _id;
        private readonly Size _imgSize;
        private readonly TFGraph _graph;
        private readonly TFSession _session;
        private readonly object _sessionLocker = new object();

        public ImageTensorProcessor(byte[] model, Size imgSize, string gpu = "/CPU:0")
        {
            _id = Guid.NewGuid();
            _imgSize = imgSize;
            _graph = new TFGraph();
            using (var device = _graph.WithDevice(gpu))
            {
                _graph.Import(model);
            }
            _session = new TFSession(_graph);
            Console.WriteLine($"=> Session for {gpu} created with: {String.Join(',', _session.ListDevices().Select(x => x.Name).ToList())}");
        }

        public virtual IReadOnlyCollection<DetectionResult> RunDetection(byte[] img)
        {
            TFTensor tensor = TFTensor.FromBuffer(new TFShape(1, _imgSize.Height, _imgSize.Width, 3), img, 0, img.Length);

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

            var boxes = (float[,,])output[0].GetValue();
            var scores = (float[,])output[1].GetValue();
            var classes = (float[,])output[2].GetValue();
            var xsize = boxes.GetLength(0);
            var ysize = boxes.GetLength(1);

            var results = new List<DetectionResult>(xsize * ysize);

            for (var i = 0; i < xsize; i++)
            {
                for (var j = 0; j < ysize; j++)
                {
                    var top = (int)(boxes[i, j, 0] * _imgSize.Height);
                    var left = (int)(boxes[i, j, 1] * _imgSize.Width);
                    var bottom = (int)(boxes[i, j, 2] * _imgSize.Height);
                    var right = (int)(boxes[i, j, 3] * _imgSize.Width);
                    float score = scores[i, j];
                    var @class = Convert.ToInt32(classes[i, j]);

                    results[i*ysize+j] = new DetectionResult { box = new Rectangle(top, left, bottom, right), score = score, @class = @class };
                }
            }

            return results;
        }
    }

    class Stresser : ImageTensorProcessor
    {
        private static readonly Random _rnd = new Random();

        private readonly int _imgByteLen;
        private readonly byte[] _buff;

        public Stresser(byte[] model, Size imgSize, string gpu = "/CPU:0")
            :base(model, imgSize, gpu)
        {
            _imgByteLen = imgSize.Width * imgSize.Height * 3;
            _buff = new byte[_imgByteLen];
        }

        public override IReadOnlyCollection<DetectionResult> RunDetection(byte[] img)
        {
            _rnd.NextBytes(_buff);
            return base.RunDetection(_buff);
        }

        public void RunDetectionAsync(int cycles)
        {
            Enumerable.Range(0, cycles).AsParallel().ForAll(_ => RunDetection());
        }
    }

    class Program
    {
        //static void Main(string[] args)
        //{
        //    byte[] model = File.ReadAllBytes(Settings.Default.ModelFile);
        //    var processors = Settings.Default.Processors.Split(',');

        //    var stressers = processors.Select(p => new Stresser(model, Settings.Default.ImgSize, p));
        //    var tasks = stressers.Select(s => new Task(()=>s.RunDetectionAsync(Settings.Default.StressCycles)));
        //    Task.WaitAll(tasks.ToArray());
        //}

        static void Main(string[] args)
        {
            byte[] model = File.ReadAllBytes(Settings.Default.ModelFile);

            using (var _graph = new TFGraph())
            {
                var X = _graph.Placeholder(TFDataType.Float, new TFShape(-1, 784));
                var Y = _graph.Placeholder(TFDataType.Float, new TFShape(-1, 10));

                var variablesAndGradients = new Dictionary<Variable, List<TFOutput>>();
                using (var device = _graph.WithDevice("/GPU:" + 1))
                {
                    //(costs[i], _, accuracys[i]) = CreateNetwork(graph, Xs[i], Ys[i], variablesAndGradients);
                    //foreach (var gv in sgd.ComputeGradient(costs[i], colocateGradientsWithOps: true))
                    //{
                    //    if (!variablesAndGradients.ContainsKey(gv.variable))
                    //        variablesAndGradients[gv.variable] = new List<TFOutput>();
                    //    variablesAndGradients[gv.variable].Add(gv.gradient);
                    //}
                    _graph.Import(model);

                    //_graph.DeviceName = "f";

                    Action<TFSession> runc = (s) =>
                    {
                        s.ListDevices().ToList().ForEach(x => Console.WriteLine(x.Name));

                        byte[] _buff = new byte[800 * 600 * 3];

                        Enumerable.Range(1, 1000).ToList().ForEach(i =>
                        {
                            new Random(DateTime.Now.Millisecond).NextBytes(_buff);
                            TFTensor tensor = TFTensor.FromBuffer(new TFShape(1, 800, 600, 3), _buff, 0, _buff.Length);

                            TFTensor[] output;
                            output = s.Run(new[] { _graph["image_tensor"][0] }, new[] { tensor }, new[]
                            {
                            _graph["detection_boxes"][0],
                            _graph["detection_scores"][0],
                            _graph["detection_classes"][0],
                            _graph["num_detections"][0]
                            });

                            //sesssion.GetRunner().AddTarget(graph.GetGlobalVariablesInitializer()).Run();
                            Console.WriteLine(i);
                        });
                    };

                    var t1 = new Task(() =>
                    {
                        using (var _session = new TFSession(_graph))
                        {
                            runc(_session);
                        }
                    });

                    var t2 = new Task(() =>
                    {
                        using (var _session = new TFSession(_graph))
                        {
                            runc(_session);
                        }
                    });
                    Task.WaitAll(new Task[] {t1, t2});
                }
            }
        }
    }
}
