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
using Carter;

using ServerlessAwsSdkChangeLogAPI.Services;

namespace ServerlessAwsSdkChangeLogAPI
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCarter();
            services.AddHttpClient<IAwsSdkChangeLogFetcherService, AwsSdkChangeLogFetcherService>();
            services.AddScoped<IAwsSdkChangeLogService, AwsSdkChangeLogService>();
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseRouting();
            app.UseEndpoints(builder => builder.MapCarter());
        }
    }
}
