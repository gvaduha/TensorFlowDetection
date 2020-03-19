using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NLog;

namespace gvaduha
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
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();
        }
    }

    public static class SelfHostedWebService
    {
        public static async Task RunWebServiceHost<T>(int port, CancellationToken cst) where T : class
        {
            var host = WebHost.CreateDefaultBuilder()
                .UseKestrel(o => { o.ListenAnyIP(port); })
                .UseStartup<T>()
                .Build();

            await host.RunAsync(cst);
        }
    }

    public static class ObjectDetectorServerFacade
    {
        private static CancellationTokenSource _cts = new CancellationTokenSource();
        private static ObjectDetectorService _objectDetectorService;
        private static string[] _gpus;
        private static Logger Logger;

        private static void Log(Exception e) => Logger.Error($"{nameof(ObjectDetectorServerFacade)}: {e.ToOneLineString()}");

        private static bool SafeExecute(Action a)
        {
            try
            {
                a();
                return true;
            }
            catch (Exception e)
            {
                Log(e);
                return false;
            }
        }

        public static bool Configure(ObjectDetectorConfig objectDetectorConfig)
        {
            return SafeExecute(() =>
            {
                var objectDetectorWorker = new ObjectDetectionWorker(objectDetectorConfig, _gpus);
                _objectDetectorService = new ObjectDetectorService(objectDetectorWorker, _cts.Token, Logger);
            });
        }

        public static bool Start()
        {
            return SafeExecute(() =>
            {
                _objectDetectorService.Start();
            });
        }

        public static bool Stop()
        {
            return SafeExecute(() =>
            {
                _objectDetectorService.Stop();
            });
        }

        public static async void StartWebService(int port, IEnumerable<string> gpus)
        {
            _gpus = gpus.ToArray();
            await SelfHostedWebService.RunWebServiceHost<WebServiceStartup>(port, _cts.Token);
        }

        public static void StopWebService()
        {
            _cts.Cancel();
        }
    }
}
