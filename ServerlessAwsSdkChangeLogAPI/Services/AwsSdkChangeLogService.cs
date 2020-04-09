using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Linq;
using System.Threading.Tasks;
using System.Text;
using System.IO;
using Microsoft.Extensions.Logging;

namespace ServerlessAwsSdkChangeLogAPI.Services
{
    public interface IAwsSdkChangeLogService
    {
        Task<string> GetListOfServicesAsync();
        Task<string> GetServiceAsync(string serviceName);
    }


    public class AwsSdkChangeLogService : IAwsSdkChangeLogService
    {
        const string UnknownPlaceHolder = "unknown";
        const string ChangeLogUrl = "https://raw.githubusercontent.com/aws/aws-sdk-net/master/SDK.CHANGELOG.md";
        const int RefreshIntervalInMinutes = 5;

        readonly HttpClient _httpClient;
        static DateTime _nextFetchTime;
        static string _changeLogContent;

        private ILogger<AwsSdkChangeLogService> _logger;

        public AwsSdkChangeLogService(ILogger<AwsSdkChangeLogService> logger, HttpClient httpClient)
        {
            _logger = logger;
            _httpClient = httpClient;
        }

        public async Task<string> GetListOfServicesAsync()
        {
            var setOfServices = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach(var release in EnumerableReleases(await GetChangeLogTextAsync()))
            {
                foreach (var service in release.Services.Keys)
                {
                    if (!setOfServices.Contains(service))
                    {
                        setOfServices.Add(service.ToLower());
                    }
                }
            }

            var sb = new StringBuilder();
            foreach (var service in setOfServices.OrderBy(x => x))
            {
                sb.AppendLine(service);
            }

            return sb.ToString();
        }

        public async Task<string> GetServiceAsync(string serviceName)
        {
            var sb = new StringBuilder();

            foreach(var release in EnumerableReleases(await GetChangeLogTextAsync()))
            {
                
                if(release.Services.TryGetValue(serviceName, out var service))
                {
                    if (sb.Length > 0)
                        sb.AppendLine();

                    sb.AppendLine($"Version {service.Version} released {release.Date.ToShortDateString()}");
                    sb.AppendLine(service.Features.ToString());
                }
            }

            return sb.ToString();
        }

        
        IEnumerable<ReleaseEntry> EnumerableReleases(string changeLog)
        {
            using var reader = new StringReader(changeLog);

            ReleaseEntry currentReleaseEntry = null;
            ServiceEntry currentServiceEntry = null;
            string line;
            while (((line = reader.ReadLine())) != null)
            {
                if(line.StartsWith("###"))
                {
                    if(currentReleaseEntry != null && currentReleaseEntry.Services.Count > 0)
                    {
                        yield return currentReleaseEntry;
                    }

                    currentReleaseEntry = new ReleaseEntry
                    {
                        Date = ExtractDateFromLine(line)
                    };
                }
                // Empty space at the start of the document
                else if (currentReleaseEntry == null)
                {
                    continue;
                }
                else if(line.StartsWith("* "))
                {
                    var info = ExtractServiceInformation(line);
                    currentServiceEntry = new ServiceEntry
                    {
                        Name = info.name,
                        Version = info.version
                    };

                    if (!string.Equals(UnknownPlaceHolder, currentServiceEntry.Name))
                    {
                        currentReleaseEntry.Services[currentServiceEntry.Name] = currentServiceEntry;
                    }
                }
                else if(currentServiceEntry != null && line.Trim().StartsWith("*"))
                {
                    currentServiceEntry.Features.AppendLine(line.Trim());
                }
            }
        }


        private async Task<string> GetChangeLogTextAsync()
        {
            if(_changeLogContent == null || _nextFetchTime < DateTime.Now)
            {
                _changeLogContent = await _httpClient.GetStringAsync(ChangeLogUrl);
                _nextFetchTime = DateTime.Now.AddMinutes(RefreshIntervalInMinutes);
                _logger.LogInformation($"Fetched changed log from GitHub repo. Next refresh will be {_nextFetchTime}");
            }

            return _changeLogContent;
        }
        
        
        public DateTime ExtractDateFromLine(string line)
        {
            var startPos = line.IndexOf("(");
            var endPos = line.IndexOf(")");
            if (startPos == -1 || endPos == -1 || endPos < startPos)
            {
                _logger.LogWarning($"Failed to parse date from line: \"{line}\"");
                return DateTime.MinValue;
            }

            var strDate = line.Substring(startPos + 1, endPos - startPos - 1);

            // Chop off the time component
            if (strDate.Length > 10) 
                strDate = strDate.Substring(0, 10);

            if (!DateTime.TryParse(strDate, out var date))
            {
                _logger.LogWarning($"Failed to parse date from line: \"{line}\"");
                return DateTime.MinValue;
            }
                
            return date;
        }

        public (string name, string version) ExtractServiceInformation(string line)
        {
            if (line.StartsWith("* Core ") && !line.Contains('(') )
            {
                var tokens = line.Substring(2).Split(' ');
                if (tokens.Length != 2)
                {
                    return (UnknownPlaceHolder, "0.0.0.0");
                }
                
                return (tokens[0], tokens[1]);
            }
            
            int openParenPos = line.LastIndexOf('(');
            int closeParenPos = line.LastIndexOf(')');
            
            if(openParenPos == -1 || closeParenPos == -1 || closeParenPos < openParenPos)
            {
                return (UnknownPlaceHolder, "0.0.0.0");
            }

            var version = line.Substring(openParenPos + 1, closeParenPos - openParenPos - 1);
            var name = line.Substring(0, openParenPos - 1).Trim();
            if (name.StartsWith("* "))
            {
                name = name.Substring(2);
            }
            
            return (name, version);
        }        


        class ReleaseEntry
        {
            public DateTime Date { get; set; }
            public IDictionary<string, ServiceEntry> Services { get; set; } = new Dictionary<string, ServiceEntry>(StringComparer.OrdinalIgnoreCase);
        }

        class ServiceEntry
        {
            public string Name { get; set; }
            public string Version { get; set; }
            public StringBuilder Features { get; set; } = new StringBuilder();
        }

    }
}
