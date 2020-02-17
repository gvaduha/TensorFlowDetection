using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace TensorSharpStresser
{
    [ApiController]
    [Route("[controller]")]
    public class TensorProcessorController : ControllerBase
    {
        //Must use DI for constructors like this
        //private ITensorProcessingResultSource _tps;
        //public TensorProcessorController(ITensorProcessingResultSource tps)
        //{
        //    _tps = tps;
        //}

        [HttpGet]
        public ServiceProcessingResult Get()
        {
            var res = ServiceLocatorAntiP.TensorProcessingResultSource.CurrentResult;
            return res;
        }

        //[HttpGet]
        //public IEnumerable<WeatherForecast> Get()
        //{
        //    var rng = new Random();
        //    var ret = Enumerable.Range(1, 5).Select(index => new WeatherForecast
        //    {
        //        Date = DateTime.Now.AddDays(index),
        //        TemperatureC = rng.Next(-20, 55),
        //        Summary = "XXX"
        //    })
        //    .ToArray();
        //    return ret;
        //}
    }
}
