using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
}
