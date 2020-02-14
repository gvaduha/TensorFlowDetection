using System;
using System.Runtime.ExceptionServices;
using System.Security;
using Emgu.CV;
using Emgu.CV.CvEnum;

namespace TensorSharpStresser
{
    class VideoSource : IImageSource, IDisposable
    {
        private VideoCapture _videoCapture;

        public string Uri { get; }

        public VideoSource(string uri)
        {
            Uri = uri;
            _videoCapture = new VideoCapture(uri);
            _videoCapture.Start();
        }

        [SecurityCritical]
        [HandleProcessCorruptedStateExceptions]
        public (int width, int height, byte[] data) GetRawImage()
        {
            try
            {
                using (var frame = new Mat())
                {
                    if (!_videoCapture.Retrieve(frame))
                        throw new ApplicationException($@"Frame cannot be retrived {Uri}");

                    using (var rgbFrame = new Mat())
                    {
                        CvInvoke.CvtColor(frame, rgbFrame, ColorConversion.Bgr2Rgb);
                        return (frame.Width, frame.Height, rgbFrame.GetRawData());
                    }

                }
            }
            catch (AccessViolationException e)
            {
                throw new ApplicationException($@"GetFrame: {e.Message} @cam: {Uri}", e);
            }
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
}
