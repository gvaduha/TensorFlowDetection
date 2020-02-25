using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using TensorFlow;

namespace gvaduha.Common
{
    public struct BBox
    {
        public int Top { get; set; }
        public int Left { get; set; }
        public int Bottom { get; set; }
        public int Right { get; set; }
        public BBox(int top, int left, int bottom, int right)
        {
            Top = top;
            Left = left;
            Bottom = bottom;
            Right = right;
        }
    }

    public struct DetectionResult
    {
        public int Class { get; set; }
        public float Score { get; set; }
        public BBox Box { get; set; }
    }

    public struct ImageProcessorResult
    {
        public string Uri { get; set; }
        public DateTime TimeStamp { get; set; }
        public List<DetectionResult> DetectionResults { get; set; }
    }

    public interface IImageSource
    {
        string Uri { get; }
        Task<(int width, int height, byte[] data)> GetRawImage();
    }

    public class ImageTensorProcessor
    {
        private readonly Guid _id;
        private readonly TFGraph _graph;
        private readonly TFSession _session;
        private readonly object _sessionLocker = new object();
        private List<IImageSource> _imageSources;

        public ImageTensorProcessor(byte[] model, IEnumerable<IImageSource> imageSources, string device = "/CPU:0")
        {
            _id = Guid.NewGuid();
            _imageSources = imageSources.ToList();

            _graph = new TFGraph();
            var options = new TFImportGraphDefOptions();
            //options.SetDefaultDevice(device);
            _graph.Import(model, options);

            TFSessionOptions TFOptions = new TFSessionOptions();

            unsafe
            {
                byte[] GPUConfig = { 0x38, 0x1 };

                fixed (void* ptr = &GPUConfig[0])
                {
                    TFOptions.SetConfig(new IntPtr(ptr), GPUConfig.Length);
                }
            }
            _session = new TFSession(_graph, TFOptions);

            Console.WriteLine($"=> Session for {device} created with: {String.Join(',', _session.ListDevices().Select(x => x.Name).ToList())}");
        }

        public async Task<IReadOnlyCollection<ImageProcessorResult>> RunDetectionCycle()
        {
            var results = new ConcurrentBag<ImageProcessorResult>();
            var tasks = _imageSources.Select(async s =>
            {
                try
                {
                    (var width, var height, var img) = await s.GetRawImage();

                    results.Add(new ImageProcessorResult
                    {
                        Uri = s.Uri,
                        TimeStamp = DateTime.UtcNow,
                        DetectionResults = RunDetection(width, height, img)
                    });
                }
                catch (ApplicationException e)
                {
                    Console.WriteLine($"FAULT @{s.Uri}: {e}");
                }
            });

            await Task.WhenAll(tasks.ToArray());

            return results.ToArray();
        }

        protected List<DetectionResult> RunDetection(int imgWidth, int imgHeight, byte[] img)
        {
            Debug.Assert(img.Length == imgWidth * imgHeight * 3);

            TFTensor tensor = TFTensor.FromBuffer(new TFShape(1, imgWidth, imgHeight, 3), img, 0, img.Length);

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
            var ysize = Math.Min(boxes.GetLength(1), 5); //HACK: too many results

            var results = new List<DetectionResult>();

            for (var i = 0; i < xsize; i++)
            {
                for (var j = 0; j < ysize; j++)
                {
                    var top = (int)(boxes[i, j, 0] * imgHeight);
                    var left = (int)(boxes[i, j, 1] * imgWidth);
                    var bottom = (int)(boxes[i, j, 2] * imgHeight);
                    var right = (int)(boxes[i, j, 3] * imgWidth);
                    float score = scores[i, j];
                    var @class = Convert.ToInt32(classes[i, j]);

                    //if (score < 0.03) break; //HACK: too many results

                    results.Add(new DetectionResult
                    {
                        Box = new BBox(top, left, bottom, right),
                        Score = score,
                        Class = @class
                    });
                }
            }

            return results;
        }
    }

    class ParallelPairStresser
    {
        private List<ImageTensorProcessor> _processors;

        public ParallelPairStresser(IEnumerable<ImageTensorProcessor> processors)
        {
            _processors = processors.ToList();
        }

        public void Run(int stressCycles)
        {
            var taskGroups = Enumerable.Range(0, stressCycles).Select(_ => _processors.Select(p => new Task(() => p.RunDetectionCycle())));
            taskGroups.ToList().ForEach(group => Task.WaitAll(group.ToArray()));
        }
    }

    //class BatchImageProcessor


    static class WeirdTrials
    {
        //static void Test()
        //{
        //    byte[] model = File.ReadAllBytes(Settings.Default.ModelFile);

        //    using (var _graph = new TFGraph())
        //    {
        //        var X = _graph.Placeholder(TFDataType.Float, new TFShape(-1, 784));
        //        var Y = _graph.Placeholder(TFDataType.Float, new TFShape(-1, 10));

        //        var variablesAndGradients = new Dictionary<Variable, List<TFOutput>>();
        //        using (var device = _graph.WithDevice("/GPU:" + 1))
        //        {
        //            //(costs[i], _, accuracys[i]) = CreateNetwork(graph, Xs[i], Ys[i], variablesAndGradients);
        //            //foreach (var gv in sgd.ComputeGradient(costs[i], colocateGradientsWithOps: true))
        //            //{
        //            //    if (!variablesAndGradients.ContainsKey(gv.variable))
        //            //        variablesAndGradients[gv.variable] = new List<TFOutput>();
        //            //    variablesAndGradients[gv.variable].Add(gv.gradient);
        //            //}
        //            _graph.Import(model);

        //            //_graph.DeviceName = "f";

        //            Action<TFSession> runc = (s) =>
        //            {
        //                s.ListDevices().ToList().ForEach(x => Console.WriteLine(x.Name));

        //                byte[] _buff = new byte[800 * 600 * 3];

        //                Enumerable.Range(1, 1000).ToList().ForEach(i =>
        //                {
        //                    new Random(DateTime.UtcNow.Millisecond).NextBytes(_buff);
        //                    TFTensor tensor = TFTensor.FromBuffer(new TFShape(1, 800, 600, 3), _buff, 0, _buff.Length);

        //                    TFTensor[] output;
        //                    output = s.Run(new[] { _graph["image_tensor"][0] }, new[] { tensor }, new[]
        //                    {
        //                    _graph["detection_boxes"][0],
        //                    _graph["detection_scores"][0],
        //                    _graph["detection_classes"][0],
        //                    _graph["num_detections"][0]
        //                    });

        //                    //sesssion.GetRunner().AddTarget(graph.GetGlobalVariablesInitializer()).Run();
        //                    Console.WriteLine(i);
        //                });
        //            };

        //            var t1 = new Task(() =>
        //            {
        //                using (var _session = new TFSession(_graph))
        //                {
        //                    runc(_session);
        //                }
        //            });

        //            var t2 = new Task(() =>
        //            {
        //                using (var _session = new TFSession(_graph))
        //                {
        //                    runc(_session);
        //                }
        //            });
        //            Task.WaitAll(new Task[] {t1, t2});
        //        }
        //    }
        //}
    }
}
