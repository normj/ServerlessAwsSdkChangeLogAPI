using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ServerlessAwsSdkChangeLogAPI.Common.Services;
using ServerlessAwsSdkChangeLogAPI.Web.Writers;

namespace ServerlessAwsSdkChangeLogAPI.Web.Controllers
{
    [Route("api/change-log")]
    public class AwsSdkChangeLogController : ControllerBase
    {
        private readonly IAwsSdkChangeLogService _awsSdkChangeLogService;
        private readonly ILogger<AwsSdkChangeLogController> _logger;
        private readonly IResponseWriterFactory _responseWriterFactory;

        public AwsSdkChangeLogController(ILogger<AwsSdkChangeLogController> logger, IAwsSdkChangeLogService awsSdkChangeLogService, IResponseWriterFactory responseWriterFactory)
        {
            this._logger = logger;
            this._awsSdkChangeLogService = awsSdkChangeLogService;
            this._responseWriterFactory = responseWriterFactory;
        }
        
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            _logger.LogInformation($"Getting the names of all of the services");
            var acceptedContentType = this.HttpContext.Request.Headers["Accept"];
            var (responseContentType, writerType) = DetermineResponseType(acceptedContentType);
            var services = await this._awsSdkChangeLogService.GetListOfServicesAsync();

            var writer = _responseWriterFactory.GetServiceListWriter(writerType);
            writer.Start();
            foreach(var service in services)
            {
                writer.WriteService(service);
            }
            var content = writer.Finish();

            return Content(content, responseContentType);
        }

        [HttpGet("{service}")]
        public async Task<IActionResult> Get(string service)
        {
            _logger.LogInformation($"Getting changes for service {service}");
            var acceptedContentType = this.HttpContext.Request.Headers["Accept"];
            var (responseContentType, writerType) = DetermineResponseType(acceptedContentType);
            var serviceReleases = await this._awsSdkChangeLogService.GetServiceAsync(service);

            var writer = _responseWriterFactory.GetServiceFeatureListWriter(writerType);
            writer.Start();
            foreach(var serviceRelease in serviceReleases)
            {
                writer.StartRelease(serviceRelease.Version, serviceRelease.ReleaseDate);
                writer.StartFeatures();
                foreach(var feature in serviceRelease.Features)
                {
                    writer.AddFeature(feature);
                }
                writer.EndFeatures();
                writer.EndRelease();
            }

            var content = writer.Finish();

            return Content(content, responseContentType);
        }
        
        public static (string ResponseContentType, ResponseWriterType WriterType) DetermineResponseType(string acceptedContentType)
        {
            
            if (!string.IsNullOrEmpty(acceptedContentType))
            {
                foreach (var token in acceptedContentType.Split(','))
                {
                    var tokenContentType = token.Trim();
                    if(string.Equals("text/plain", tokenContentType, StringComparison.OrdinalIgnoreCase))
                    {
                        return ("text/plain", ResponseWriterType.Text);
                    }
                    else if(string.Equals("application/json", tokenContentType, StringComparison.OrdinalIgnoreCase))
                    {
                        return ("application/json", ResponseWriterType.Json);
                    }
                }
            }            
            
            return ("text/plain", ResponseWriterType.Text);
        }        
    }
}