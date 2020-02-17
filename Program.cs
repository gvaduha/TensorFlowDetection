using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace TensorSharpStresser
{
    class Program
    {
        private static CancellationTokenSource _cts = new CancellationTokenSource();

        private static void CtrlcHandler(object sender, ConsoleCancelEventArgs args)
        {
            if (ConsoleSpecialKey.ControlC == args.SpecialKey)
                _cts.Cancel();
        }

        // Return ((Task * procNum) * stressCyclesNum)
        static IReadOnlyCollection<ImageTensorProcessor> PrepareTensorProcessors()
        {
            byte[] model = File.ReadAllBytes(Settings.Default.ModelFile);

            // Image sources pack (cam | whitenoice * SourcesPerProcessor)
            var imgSrcpack = Enumerable.Range(0, Settings.Default.SourcesPerProcessor)
                .Select(_ => Settings.Default.WhiteNoiceSources
                                ? new WhiteNoiceImageSource(Settings.Default.ImgSize) as IImageSource
                                : new VideoFileSource(Settings.Default.VideoSource));

            // Image tensor processors (num of TensorProcessors)
            var procUnits = Settings.Default.TensorProcessors.Split(',');
            var imgProcessors = procUnits.Select(p => new ImageTensorProcessor(model, imgSrcpack, p));

            return imgProcessors.ToArray();
        }

        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += (s, e) => Console.WriteLine("*** Crash! *** UnhandledException");
            TaskScheduler.UnobservedTaskException += (s, e) => Console.WriteLine("*** Crash! *** UnobservedTaskException");
            
            var tps = PrepareTensorProcessors();
            var aggr = new TensorProcessingService(tps, Settings.Default.StressCycles, _cts.Token);

            var wsTask = Task.Run(() => WebServiceHostStartup.RunWebServiceHost());
            var tensorTask = Task.Run(() => aggr.Run());

            Console.CancelKeyPress += new ConsoleCancelEventHandler(CtrlcHandler);

            Task.WaitAll(new Task[] { wsTask, tensorTask });
        }
    }
}
