using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using gvaduha.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Visualizer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class FakeController : ControllerBase
    {
        [HttpGet("once/{video}")]
        public async Task<ContentResult> GetAsync(string video)
        {
            var vs = new VideoStreamSource(HttpUtility.UrlDecode(video));
            var drs = new FakeDetectionResultSource(vs);
            var painter = new DetectedBoxPainter(drs);

            var c = new ContentResult
            {
                ContentType = "text/html",
                Content = $"<img alt='frame' src='data:image/jpeg;base64,{await painter.GetNextImageAsync()}'/>"
            };
            return c;
        }

        [HttpGet("{video}")]
        public ContentResult Get(string video)
        {
            var c = new ContentResult
            {
                ContentType = "text/html",
                Content = $"<html><body><script>setTimeout(function(){{location.reload();}},1000);</script>FUUUUU!!!!{DateTime.Now}</body></html>"
            };
            return c;
        }
    }
}
