using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace ServerlessAwsSdkChangeLogAPI.Common.Services
{
    public interface IAwsSdkChangeLogFetcherService
    {
        Task<string> GetChangeLogTextAsync();
    }
    
    public class AwsSdkChangeLogFetcherService : IAwsSdkChangeLogFetcherService
    {
        const int RefreshIntervalInMinutes = 5;
        const string ChangeLogUrl = "https://raw.githubusercontent.com/aws/aws-sdk-net/master/SDK.CHANGELOG.md";
        
        static DateTime _nextFetchTime;
        static string _changeLogContent;
        
        readonly HttpClient _httpClient;
        private readonly ILogger<AwsSdkChangeLogService> _logger;

        public AwsSdkChangeLogFetcherService(ILogger<AwsSdkChangeLogService> logger, HttpClient httpClient)
        {
            _logger = logger;
            _httpClient = httpClient;
        }
        
        public async Task<string> GetChangeLogTextAsync()
        {
            if(_changeLogContent == null || _nextFetchTime < DateTime.Now)
            {
                
                _changeLogContent = await _httpClient.GetStringAsync(ChangeLogUrl);
                _nextFetchTime = DateTime.Now.AddMinutes(RefreshIntervalInMinutes);
                _logger.LogInformation($"Fetched changed log from GitHub repo. Next refresh will be {_nextFetchTime}");
            }

            return _changeLogContent;
        }        
    }
}