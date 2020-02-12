using System;
using System.Collections.Generic;
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

        public Stresser(byte[] model, Size imgSize, string gpu = "/CPU:0")
        {
            _id = Guid.NewGuid();
            _imgSize = imgSize;
            _imgByteLen = _imgSize.Width * _imgSize.Height * 3;
            _buff = new byte[_imgByteLen];
            _graph = new TFGraph();
            using (var device = _graph.WithDevice(gpu))
            {
                _graph.Import(model);
            }
            _session = new TFSession(_graph);
            Console.WriteLine($"=> Session for {gpu} created with: {String.Join(',',_session.ListDevices().Select(x=>x.Name).ToList())}");
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
                //using (var device = _graph.WithDevice("/GPU:" + 7))
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

                    using (var _session = new TFSession(_graph))
                    {
                        _session.ListDevices().ToList().ForEach(x => Console.WriteLine(x.Name));

                        byte[] _buff = new byte[800 * 600 * 3];

                        Enumerable.Range(1, 1000).ToList().ForEach(_ =>
                        {
                            new Random(DateTime.Now.Millisecond).NextBytes(_buff);
                            TFTensor tensor = TFTensor.FromBuffer(new TFShape(1, 800, 600, 3), _buff, 0, _buff.Length);

                            TFTensor[] output;
                            output = _session.Run(new[] { _graph["image_tensor"][0] }, new[] { tensor }, new[]
                            {
                            _graph["detection_boxes"][0],
                            _graph["detection_scores"][0],
                            _graph["detection_classes"][0],
                            _graph["num_detections"][0]
                            });

                            //sesssion.GetRunner().AddTarget(graph.GetGlobalVariablesInitializer()).Run();
                        });
                    }
                }
            }
        }
    }
}
