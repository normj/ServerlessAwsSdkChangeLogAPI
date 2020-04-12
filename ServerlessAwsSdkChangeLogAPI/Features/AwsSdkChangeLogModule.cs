using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Carter;
using Carter.Request;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using ServerlessAwsSdkChangeLogAPI.Services;

namespace ServerlessAwsSdkChangeLogAPI.Features
{
    public class AwsSdkChangeLogModule : CarterModule
    {
        public AwsSdkChangeLogModule(ILogger<AwsSdkChangeLogModule> logger, IAwsSdkChangeLogService awsSdkChangeLogService)
        {
            this.Get("/", async (req, res) => 
            {
                try
                {
                    var acceptedContentType = req.Headers["Accept"];
                    var responseInfo = DetermineResponseType(acceptedContentType);
                    var content = await awsSdkChangeLogService.GetListOfServicesAsync(responseInfo.WriterType);
                    res.StatusCode = 200;
                    res.ContentType = responseInfo.ResponseContentType;
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

                    var acceptedContentType = ctx.Request.Headers["Accept"];
                    var responseInfo = DetermineResponseType(acceptedContentType);
                    var content = await awsSdkChangeLogService.GetServiceAsync(service, responseInfo.WriterType);
                    ctx.Response.StatusCode = 200;
                    ctx.Response.ContentType = responseInfo.ResponseContentType;
                    await ctx.Response.WriteAsync(content);
                }
                catch (Exception e)
                {
                    logger.LogError("Error getting list of services", e);
                    ctx.Response.StatusCode = 500;
                }
            });
        }

        private (string ResponseContentType, ResponseWriterType WriterType) DetermineResponseType(string acceptedContentType)
        {
            if (!string.IsNullOrEmpty(acceptedContentType))
            {
                if(string.Equals("text/plain", acceptedContentType, StringComparison.OrdinalIgnoreCase))
                {
                    return ("text/plain", ResponseWriterType.Text);
                }
                else if(string.Equals("application/json", acceptedContentType, StringComparison.OrdinalIgnoreCase))
                {
                    return ("application/json", ResponseWriterType.Json);
                } 
            }            
            
            return ("text/plain", ResponseWriterType.Text);
        }
    }
}
