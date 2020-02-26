using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace gvaduha.Common
{
    class ParallelPairStresser
    {
        private List<BatchImagesTensorProcessor> _processors;

        public ParallelPairStresser(IEnumerable<BatchImagesTensorProcessor> processors)
        {
            _processors = processors.ToList();
        }

        public void Run(int stressCycles)
        {
            var taskGroups = Enumerable.Range(0, stressCycles).Select(_ => _processors.Select(p => new Task(async () => await p.RunDetectionCycle())));
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
