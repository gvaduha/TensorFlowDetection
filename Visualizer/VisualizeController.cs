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
    //[Route("[controller]")]
    [Route("/")]
    public class VisualizeController : ControllerBase
    {
        public VisualizeController()
        {
        }

        [HttpGet("fake/{video}")]
        public async Task<ContentResult> GetAsync(string video)
        {
            var vs = new VideoStreamSource(HttpUtility.UrlDecode(video));
            var drs = new FakeDetectionResultSource(vs);
            var painter = new DetectedBoxPainter(drs);

            var c = new ContentResult
            {
                ContentType = "text/html",
                Content = $"<html><body><img alt='frame' src='data:image/jpeg;base64,{await painter.GetNextImageAsync()}'/></body></html>"
            };
            return c;
        }

        //[HttpGet("shit")]
        //public ContentResult GetShit()
        //{
        //    var c = new ContentResult();
        //    c.ContentType = "text/html";
        //    c.Content = "<html><body>Test</body></html>";
        //    return c;
        //}


        // GET api/authors/RickAndMSFT
        //[HttpGet("{alias}")]
        //[HttpGet("syncsale")]
    }
}
