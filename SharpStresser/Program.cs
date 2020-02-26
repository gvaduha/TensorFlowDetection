using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using gvaduha.Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace SharpStresser
{
    public class WebServiceStartup
    {
        public WebServiceStartup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            //if (env.IsDevelopment())
            //{
            //    app.UseDeveloperExceptionPage();
            //}

            //app.UseHttpsRedirection();

            app.UseRouting();

            //app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }

    class Program
    {
        private static CancellationTokenSource _cts = new CancellationTokenSource();

        private static void CtrlcHandler(object sender, ConsoleCancelEventArgs args)
        {
            if (ConsoleSpecialKey.ControlC == args.SpecialKey)
            {
                Console.WriteLine("Waiting for worker tasks...");
                _cts.Cancel();
                args.Cancel = true;
            }
        }

        // Return ((Task * procNum) * stressCyclesNum)
        static IReadOnlyCollection<BatchImagesTensorProcessor> PrepareTensorProcessors()
        {
            byte[] model = File.ReadAllBytes(Settings.Default.ModelFile);

            // Image sources pack (cam | whitenoice * SourcesPerProcessor)
            var imgSrcpack = Enumerable.Range(0, Settings.Default.SourcesPerProcessor)
                .Select(_ => Settings.Default.WhiteNoiceSources
                                ? new WhiteNoiceImageSource(Settings.Default.ImgSize) as IImageSource
                                : new VideoFileSource(Settings.Default.VideoSource));

            // Image tensor processors (num of TensorProcessors)
            var procUnits = Settings.Default.TensorProcessors.Split(',');
            var imgProcessors = procUnits.Select(p => new BatchImagesTensorProcessor(model, imgSrcpack, p));

            return imgProcessors.ToArray();
        }

        static void Main(string[] args)
        {
            Console.CancelKeyPress += new ConsoleCancelEventHandler(CtrlcHandler);

            AppDomain.CurrentDomain.UnhandledException += (s, e) => Console.WriteLine("*** Crash! *** UnhandledException");
            TaskScheduler.UnobservedTaskException += (s, e) => Console.WriteLine("*** Crash! *** UnobservedTaskException");
            
            var tps = PrepareTensorProcessors();
            var aggr = new TensorProcessingService(tps, Settings.Default.StressCycles, _cts.Token);

            ServiceLocatorAntiP.TensorProcessingResultSource = aggr as ITensorProcessingResultSource;

            var wsTask = Task.Run(() => SelfHostedWebService.RunWebServiceHost<WebServiceStartup>());
            var tensorTask = Task.Run(() => aggr.Run());

            Task.WaitAll(new Task[] { wsTask, tensorTask });
        }
    }
}
