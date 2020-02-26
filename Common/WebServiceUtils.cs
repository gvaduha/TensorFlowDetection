using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace gvaduha.Common
{
    public static class SelfHostedWebService
    {
        public static async Task RunWebServiceHost<T>() where T : class
        {
            //var config = new ConfigurationBuilder()
            //    //.AddCommandLine(args)
            //    //.AddEnvironmentVariables(prefix: "ASPNETCORE_")
            //    .Build();

            var host = new WebHostBuilder()
                //.UseConfiguration(config)
                .UseKestrel()
                //.UseContentRoot(System.IO.Directory.GetCurrentDirectory())
                //.UseIISIntegration()
                .UseStartup<T>()
                .Build();

            await host.RunAsync();
        }
    }

    public class DefaultWebServiceHostStartup
    {
        public DefaultWebServiceHostStartup(IConfiguration configuration)
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
}
