using System;
using System.Drawing;
using System.Runtime.ExceptionServices;
using System.Security;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.CvEnum;

namespace TensorSharpStresser
{
    class VideoFileSource : IImageSource, IDisposable
    {
        private VideoCapture _videoCapture;

        public string Uri { get; }

        public VideoFileSource(string uri)
        {
            Uri = uri;
            _videoCapture = new VideoCapture(uri);
            _videoCapture.Start();
        }

        //[SecurityCritical] //not allowed for async now
        [HandleProcessCorruptedStateExceptions]
        public async Task<(int width, int height, byte[] data)> GetRawImage()
        {
            return await Task.Run( () =>
            {
                try
                {
                    using var frame = new Mat();
                    
                    //if (!_videoCapture.Retrieve(frame)) -from net cam source
                    //    throw new ApplicationException($@"Frame cannot be retrived {Uri}");

                    _videoCapture.Read(frame);

                    using var rgbFrame = new Mat();
                    CvInvoke.CvtColor(frame, rgbFrame, ColorConversion.Bgr2Rgb);
                    return (frame.Width, frame.Height, rgbFrame.GetRawData());
                }
                catch (AccessViolationException e)
                {
                    throw new ApplicationException($@"GetFrame: {e.Message} @cam: {Uri}", e);
                }
            });
        }

        [SecurityCritical]
        [HandleProcessCorruptedStateExceptions]
        public void Dispose()
        {
            try
            {
                _videoCapture.Stop();
                _videoCapture.Dispose();
            }
            catch (AccessViolationException e)
            {
                throw new ApplicationException($@"Dispose: {e.Message} @cam: {Uri}", e);
            }
        }
    }

    class WhiteNoiceImageSource : IImageSource
    {
        private static readonly Random _rnd = new Random();
        private readonly Size _imgSize;
        private readonly byte[] _buff;

        public WhiteNoiceImageSource(Size imgSize)
        {
            _imgSize = imgSize;
            _buff = new byte[imgSize.Width * imgSize.Height * 3];
        }

        public string Uri { get; } = Guid.NewGuid().ToString();

        public Task<(int width, int height, byte[] data)> GetRawImage()
        {
            _rnd.NextBytes(_buff);
            return Task.FromResult((_imgSize.Width, _imgSize.Height, _buff));
        }
    }

}
