using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using TensorFlow;

namespace gvaduha.Common
{
    public class TensorProcessor
    {
        private readonly TFGraph _graph;
        private readonly TFSession _session;
        private readonly object _sessionLocker = new object();

        public TensorProcessor(byte[] model, string device = "/CPU:0")
        {
            _graph = new TFGraph();
            var options = new TFImportGraphDefOptionsExt();
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

        public List<DetectionResult> RunDetection(int imgWidth, int imgHeight, byte[] img)
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

    public class BatchImagesTensorProcessor
    {
        private readonly Guid _id;
        private List<IImageSource> _imageSources;
        private TensorProcessor _tp;

        public BatchImagesTensorProcessor(IEnumerable<IImageSource> imageSources, TensorProcessor tp)
        {
            _id = Guid.NewGuid();
            _imageSources = imageSources.ToList();
            _tp = tp;
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
                        DetectionResults = _tp.RunDetection(width, height, img)
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
    }
}
