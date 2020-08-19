# TensorFlowDetection

[![License](http://img.shields.io/badge/license-mit-blue.svg?style=flat-square)](https://raw.githubusercontent.com/json-iterator/go/master/LICENSE)
[![Build Status](https://travis-ci.org/gvaduha/TensorFlowDetection.svg?branch=master)](https://travis-ci.org/gvaduha/TensorFlowDetection)

Capturer - imbecile video to file capture
SharpStresser - run video streams images on object detection graph with multi gpu, expose results of detection on web service endpoint
 Detection Results Endpoint: http://localhost:5000/TensorProcessor
Visualizer - visualize object detection on video stream
 use:
 * video with fake boxes: https://localhost:6001/fake/rtsp%3A%2F%2F192.168.0.1%2F , https://localhost:6001/fake/file%3A%2F%2F%2FC%3A%2Ftest.mp4
 *


https://www.tensorflow.org/install/lang_c - C packages of libtensorflow for differnt platforms and devices


# Service in NetStandard 2.0

## csproj

<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>netstandard2.0</TargetFramework>
    <AspNetCoreHostingModel>InProcess</AspNetCoreHostingModel>
    <RootNamespace></RootNamespace>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.AspNetCore" Version="2.2.0" />
    <PackageReference Include="Microsoft.AspNetCore.Mvc" Version="2.2.0" />
    <PackageReference Include="NLog" Version="4.6.8" />
  </ItemGroup>
</Project>

## code
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        // GET api/values
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public ActionResult<string> Get(int id)
        {
            return "value";
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }

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
        public static async Task RunWebServiceHost<T>() where T : class
        {
            var host = WebHost.CreateDefaultBuilder()
                .UseKestrel(o => { o.ListenAnyIP(9999); })
                .UseStartup<T>()
                .Build();

            await host.RunAsync();
        }
    }

    SelfHostedWebService.RunWebServiceHost<WebServiceStartup>());
