using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using gvaduha.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace SharpStresser
{
    [ApiController]
    [Route("[controller]")]
    public class TensorProcessorController : ControllerBase
    {
        [HttpGet]
        public ServiceProcessingResult Get()
        {
            var res = ServiceLocatorAntiP.TensorProcessingResultSource.CurrentResult;
            return res;
        }
    }


    [ApiController]
    [Route("ctrl")]
    public class DetectorServerController : ControllerBase
    {
        [HttpPost]
        [Route("start")]
        public bool Start()
        {
            return false;
        }
        [HttpPost]
        [Route("stop")]
        public bool Stop()
        {
            return false;
        }
    }
}
