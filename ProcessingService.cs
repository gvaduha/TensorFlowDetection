using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace TensorSharpStresser
{
    public struct ServiceProcessingResult
    {
        public Guid ServiceId { get; set; }
        public DateTime TimeStamp { get; set; }
        public List<ImageProcessorResult> ImageProcessorResults { get; set; }
    }

    public interface ITensorProcessingResultSource
    {
        ServiceProcessingResult CurrentResult { get; }
    }

    //I'm too lazy to use DI at this point
    public static class ServiceLocatorAntiP
    {
        public static ITensorProcessingResultSource TensorProcessingResultSource { get; set; }
    }

    class TensorProcessingService : ITensorProcessingResultSource
    {
        public Guid Id { get; } = Guid.NewGuid();

        private IEnumerable<ImageTensorProcessor> _tps;
        private uint _processCycles;
        private CancellationToken _cancellation;

        public TensorProcessingService(IEnumerable<ImageTensorProcessor> tps, uint processCycles, CancellationToken cancellation)
        {
            _processCycles = processCycles;
            _tps = tps;
            _cancellation = cancellation;
        }

        public static ServiceProcessingResult UndefinedResult = new ServiceProcessingResult();

        private ServiceProcessingResult _currentResult = UndefinedResult;
        private object _resultLock = new object();
        public ServiceProcessingResult CurrentResult
        {
            get => _currentResult;
            set { lock (_resultLock) _currentResult = value; }
        }

        public void Run()
        {
            async Task<IEnumerable<ImageProcessorResult>> RunSingleCycle()
            {
                _cancellation.ThrowIfCancellationRequested();
                var results = new ConcurrentBag<IEnumerable<ImageProcessorResult>>();
                var taskPack = _tps.Select(async x => results.Add(await x.RunDetectionCycle()));
                await Task.WhenAll(taskPack.ToArray());
                return results.SelectMany(x => x);
            };

            try
            {
                CurrentResult = new ServiceProcessingResult
                {
                    ServiceId = Id,
                    TimeStamp = DateTime.UtcNow,
                    ImageProcessorResults = RunSingleCycle().Result.ToList()
                };
            }
            catch (Exception e)
            {
                Console.WriteLine($"OOPS!: {e}");
            }

            //var steps = Enumerable.Range(0, (int)_processCycles).Select(async x =>
            //new
            //{
            //    CycleNumber = x,
            //    Result = new ServiceProcessingResult { ServiceId = Id, TimeStamp = DateTime.UtcNow, ImageProcessorResults = (await RunSingleCycle()).ToList() }
            //});

            ////steps.ToList().ForEach(async x => CurrentResult = (await x).Result);
            //steps.ToList().ForEach(x => CurrentResult = x.Result.Result);
        }
    }
}
