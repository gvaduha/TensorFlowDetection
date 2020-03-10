using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.Structure;

namespace gvaduha.Common
{
    public interface IDetectionResultSource// : IEnumerable<DetectionResult>
    {
        Task<(Image<Bgr, byte> image, IReadOnlyCollection<DetectionResult> detectionResults)> GetNextResultsAsync();
    }

    public class LocalTensorDetectionResultSource : IDetectionResultSource
    {
        private TensorProcessor _tp;
        private IImageSource _ims;

        public LocalTensorDetectionResultSource(IImageSource ims, TensorProcessor tp)
        {
            _ims = ims;
            _tp = tp;
        }

        public async Task<(Image<Bgr,byte>, IReadOnlyCollection<DetectionResult>)> GetNextResultsAsync()
        {
            var image = await _ims.GetImageAsync();
            //HACK: Fake implementation
            return (image, Enumerable.Empty<DetectionResult>().ToList().AsReadOnly());
        }
    }

    public class FakeDetectionResultSource : IDetectionResultSource
    {
        private Random _rnd = new Random();
        private IImageSource _ims;

        public FakeDetectionResultSource(IImageSource ims)
        {
            _ims = ims;
        }

        public async Task<(Image<Bgr, byte>, IReadOnlyCollection<DetectionResult>)> GetNextResultsAsync()
        {
            var dr = Enumerable.Range(0, _rnd.Next(0, 10)).Select(_ =>
            {
                var top = _rnd.Next(0, 500);
                var left = _rnd.Next(0, 700);
                return new DetectionResult
                {
                    Class = _rnd.Next(1, 5),
                    Score = _rnd.Next(1, 100) / 100f,
                    Box = new BBox(top, left, top + _rnd.Next(10, 100), left + _rnd.Next(10, 100))
                };
            });
            var img = await _ims.GetImageAsync();
            return (img, dr.ToList().AsReadOnly());
        }
    }

    //public class RemoteTensorDetectionResultSource : IDetectionResultSource
    //{

    //}

    //public class StoredDetectionResultSource : IDetectionResultSource
    //{

    //}
}
