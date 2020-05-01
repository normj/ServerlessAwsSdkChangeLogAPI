using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ServerlessAwsSdkChangeLogAPI.Common.Services;

namespace ServerlessAwsSdkChangeLogAPI.Web.Controllers
{
    [Route("api/change-log")]
    public class AwsSdkChangeLogController : ControllerBase
    {
        private readonly IAwsSdkChangeLogService _awsSdkChangeLogService;
        private readonly ILogger<AwsSdkChangeLogController> _logger;

        public AwsSdkChangeLogController(ILogger<AwsSdkChangeLogController> logger, IAwsSdkChangeLogService awsSdkChangeLogService)
        {
            this._logger = logger;
            this._awsSdkChangeLogService = awsSdkChangeLogService;
        }
        
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            _logger.LogInformation($"Getting the names of all of the services");
            var acceptedContentType = this.HttpContext.Request.Headers["Accept"];
            var (responseContentType, writerType) = DetermineResponseType(acceptedContentType);
            var content = await this._awsSdkChangeLogService.GetListOfServicesAsync(writerType);

            return Content(content, responseContentType);
        }

        [HttpGet("{service}")]
        public async Task<IActionResult> Get(string service)
        {
            _logger.LogInformation($"Getting changes for service {service}");
            var acceptedContentType = this.HttpContext.Request.Headers["Accept"];
            var (responseContentType, writerType) = DetermineResponseType(acceptedContentType);
            var content = await this._awsSdkChangeLogService.GetServiceAsync(service, writerType);

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