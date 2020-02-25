using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using gvaduha.Common;

namespace Visualizer
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            await WebServiceHostStartup.RunWebServiceHost();
        }
    }
}
