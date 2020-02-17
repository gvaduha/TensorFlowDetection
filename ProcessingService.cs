using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace TensorSharpStresser
{
    class TensorProcessingService
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

        public struct ServiceProcessingResult
        {
            public Guid ServiceId;
            public DateTime TimeStamp;
            public List<ImageProcessorResult> ImageProcessorResults;
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
            IEnumerable<ImageProcessorResult> RunSingleCycle()
            {
                _cancellation.ThrowIfCancellationRequested();
                var results = new ConcurrentBag<IEnumerable<ImageProcessorResult>>();
                var taskPack = _tps.Select(p => new Task(() => results.Add(p.RunDetectionCycle()))).ToList();
                Task.WaitAll(taskPack.ToArray());
                return results.SelectMany(x => x);
            }

            var steps = Enumerable.Range(0, (int)_processCycles).Select(x =>
            new
            {
                CycleNumber = x,
                Result = new ServiceProcessingResult { ServiceId = Id, TimeStamp = DateTime.UtcNow, ImageProcessorResults = RunSingleCycle().ToList() }
            });

            steps.ToList().ForEach(x => CurrentResult = x.Result);
        }
    }
}
