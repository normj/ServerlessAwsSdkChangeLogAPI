using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Carter;
using Carter.Request;
using Microsoft.AspNetCore.Http;
using ServerlessCarterExample.Services;

namespace ServerlessCarterExample.Features
{
    public class AwsSdkChangeLogModule : CarterModule
    {
        public AwsSdkChangeLogModule(IAwsSdkChangeLogService awsSdkChangeLogService)
        {
            this.Get("/awssdk", async (req, res) => 
            {
                var content = await awsSdkChangeLogService.GetLatestReleaseAsync();
                res.StatusCode = 200;                
                await res.WriteAsync(content);                
            });

            this.Get("/awssdk/{service}", async (ctx) =>
            {
                var service = ctx.Request.RouteValues.As<string>("service");
                if(string.IsNullOrEmpty(service))
                {
                    ctx.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                }

                var content = await awsSdkChangeLogService.GetServiceAsync(service);
                ctx.Response.StatusCode = 200;
                await ctx.Response.WriteAsync(content);
            });
        }
    }
}
