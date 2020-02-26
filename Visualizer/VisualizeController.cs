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
    public class VisualizeController : ControllerBase
    {
        public VisualizeController()
        {
        }

        [HttpGet("{video}")]
        public ContentResult Get(string video)
        {
            var painter = new DetectedBoxPainter(HttpUtility.UrlDecode(video));

            var c = new ContentResult();
            c.ContentType = "text/html";
            c.Content = $"<html><body><img alt='frame' src='data:image/jpeg;base64,{painter.GetNextImage()}'/></body></html>";
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
