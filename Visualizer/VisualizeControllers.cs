using System;
using System.Threading.Tasks;
using System.Web;
using gvaduha.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;

namespace Visualizer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CacheResetController : ControllerBase
    {
        [HttpGet]
        public ContentResult Get()
        {
            MySessionSigleton.Cache.Clear();
            return new ContentResult();
        }
    }

    [ApiController]
    [Route("[controller]")]
    public class FakeController : ControllerBase
    {
        [HttpGet("once/{video}")]
        public async Task<ContentResult> GetAsync(string video)
        {
            var key = $"{Request.HttpContext.Connection.RemoteIpAddress}:{video}";

            if (!MySessionSigleton.Cache.TryGetValue(key, out DetectedBoxPainter painter))
            {
                var vs = new VideoStreamSource(HttpUtility.UrlDecode(video));
                var drs = new FakeDetectionResultSource(vs);
                painter = new DetectedBoxPainter(drs);
                MySessionSigleton.Cache.Add(key, painter);
            }

            try
            {
                var data = await painter.GetNextImageAsync();

                var c = new ContentResult
                {
                    ContentType = "text/html",
                    Content = $"<img alt='frame' src='data:image/jpeg;base64,{data}'/>"
                };
                return c;
            }
            catch (Exception e)
            {
                return new ContentResult { ContentType = "text/plain", Content = e.ToString() };
            }
        }

        [HttpGet("{video}/{timeout}")]
        public ContentResult Get(string video, string timeout)
        {
            var c = new ContentResult
            {
                ContentType = "text/html",
                Content = $"<html><body><script>setTimeout(function(){{location.reload();}},{timeout});</script>{GetAsync(video).Result.Content.ToString()}</body></html>"
            };
            return c;
        }
    }
}
