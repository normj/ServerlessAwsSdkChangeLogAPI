using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Carter;
using Carter.Request;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using ServerlessCarterExample.Services;

namespace ServerlessCarterExample.Features
{
    public class AwsSdkChangeLogModule : CarterModule
    {
        public AwsSdkChangeLogModule(ILogger<AwsSdkChangeLogModule> logger, IAwsSdkChangeLogService awsSdkChangeLogService)
        {
            this.Get("/", async (req, res) => 
            {
                try
                {
                    var content = await awsSdkChangeLogService.GetListOfServicesAsync();
                    res.StatusCode = 200;                
                    await res.WriteAsync(content);
                }
                catch (Exception e)
                {
                    logger.LogError("Error getting list of services", e);
                    res.StatusCode = 500;
                }
            });

            this.Get("/{service}", async (ctx) =>
            {
                try
                {
                    var service = ctx.Request.RouteValues.As<string>("service");
                    if(string.IsNullOrEmpty(service))
                    {
                        ctx.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    }

                    var content = await awsSdkChangeLogService.GetServiceAsync(service);
                    ctx.Response.StatusCode = 200;
                    await ctx.Response.WriteAsync(content);
                }
                catch (Exception e)
                {
                    logger.LogError("Error getting list of services", e);
                    ctx.Response.StatusCode = 500;
                }
            });
        }
    }
}
