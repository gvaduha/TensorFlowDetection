using System;
using System.Drawing;
using System.Runtime.ExceptionServices;
using System.Security;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;

namespace gvaduha.Common
{
    public interface IImageSource
    {
        string Uri { get; }
        Task<(int width, int height, byte[] data)> GetRawImageAsync();
        Task<Image<Bgr, byte>> GetImageAsync();
    }

    public class VideoStreamSource : IImageSource, IDisposable
    {
        private VideoCapture _videoCapture;

        public string Uri { get; }

        public VideoStreamSource(string uri)
        {
            Uri = uri;
            _videoCapture = new VideoCapture(uri);
            _videoCapture.Start();
        }

        //[SecurityCritical] //not allowed for async now
        [HandleProcessCorruptedStateExceptions]
        public Task<(int width, int height, byte[] data)> GetRawImageAsync()
        {
            return new Task<(int,int,byte[])>( () =>
            {
                try
                {
                    using var frame = _videoCapture.QueryFrame();
                    
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

        //[SecurityCritical] //not allowed for async now
        [HandleProcessCorruptedStateExceptions]
        public Task<Image<Bgr, byte>> GetImageAsync()
        {
            return Task.Run( () =>
            {
                try
                {
                    // using var frame = _videoCapture.QueryFrame(); //Primary way (not working!)
                    using Mat frame = new Mat(); _videoCapture.Read(frame); //ALT
                    return frame.ToImage<Bgr, byte>();
                }
                catch (AccessViolationException e)
                {
                    throw new ApplicationException($@"GetFrame: {e.Message} @cam: {Uri}", e);
                }
                catch (Exception e)
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

    public class WhiteNoiceImageSource : IImageSource
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

        public Task<(int width, int height, byte[] data)> GetRawImageAsync()
        {
            _rnd.NextBytes(_buff);
            return Task.FromResult((_imgSize.Width, _imgSize.Height, _buff));
        }

        public Task<Image<Bgr,byte>> GetImageAsync()
        {
            var (w, h, d) = GetRawImageAsync().Result;
            var img = new Image<Bgr, byte>(w, h);
            img.Bytes = d;
            return Task.FromResult(img);
        }
    }

}
