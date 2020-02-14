using System;
using System.Collections.Generic;
using System.Text;

namespace TensorSharpStresser
{
    struct ImageProcessorResult
    {
        Guid sourceId;
        DateTime timeStamp;
        List<DetectionResult> results;
    }

    class ResultAggregator
    {
        public ResultAggregator() { }
    }

    class ServiceStuff
    {
    }
}
