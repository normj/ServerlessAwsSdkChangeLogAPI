using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Amazon.XRay.Recorder.Handlers.System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ServerlessAwsSdkChangeLogAPI.Common.Services;
using ServerlessAwsSdkChangeLogAPI.Web.Writers;

namespace ServerlessAwsSdkChangeLogAPI.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddRazorPages();

            // Add Http Client for IAwsSdkChangeLogFetcherService and enable X-Ray for the out going HTTP requests
            services.AddHttpClient<IAwsSdkChangeLogFetcherService, AwsSdkChangeLogFetcherService>()
                    .ConfigurePrimaryHttpMessageHandler(() => new HttpClientXRayTracingHandler(new HttpClientHandler()));
            
            services.AddSingleton<IResponseWriterFactory, ResponseWriterFactory>();
            services.AddScoped<IAwsSdkChangeLogService, AwsSdkChangeLogService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseXRay("ServerlessAwsSdkChangeLogAPI.Web");

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapRazorPages();
            });
        }
    }
}