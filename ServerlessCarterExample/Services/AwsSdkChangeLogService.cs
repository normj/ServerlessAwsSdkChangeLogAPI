using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Linq;
using System.Threading.Tasks;
using System.Text;
using System.IO;
using Newtonsoft.Json.Linq;

namespace ServerlessCarterExample.Services
{
    public interface IAwsSdkChangeLogService
    {
        Task<string> GetLatestReleaseAsync();

        Task<string> GetServiceAsync(string serviceName, int maxEntries = 10);
    }


    public class AwsSdkChangeLogService : IAwsSdkChangeLogService
    {
        const string CHANGE_LOG_URL = "https://raw.githubusercontent.com/aws/aws-sdk-net/master/SDK.CHANGELOG.md";
        const int REFRESH_INTERVAL_IN_MINUTES = 5;

        HttpClient _httpClient;
        static DateTime _nextFetchTime;
        static string _changeLogContent;

        public AwsSdkChangeLogService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string> GetLatestReleaseAsync()
        {
            var sb = new StringBuilder();
            using var reader = new StringReader(await GetChangeLogTextAsync());

            string line;
            while(!string.IsNullOrEmpty((line = reader.ReadLine())))
            {
                sb.AppendLine(line);
            }

            return sb.ToString();
        }

        public async Task<string> GetServiceAsync(string serviceName, int maxEntries = 10)
        {
            var sb = new StringBuilder();

            var foundServiceEntries = 0;
            foreach(var release in EnumerableReleases(await GetChangeLogTextAsync()))
            {
                
                if(release.Services.TryGetValue(serviceName, out var service))
                {
                    if (sb.Length > 0)
                        sb.AppendLine();

                    sb.AppendLine($"Version {service.Version} released {release.Date}");
                    sb.AppendLine(service.Features.ToString());

                    foundServiceEntries++;
                    if (foundServiceEntries == maxEntries)
                        break;
                }
            }

            return sb.ToString();
        }

        
        IEnumerable<ReleaseEntry> EnumerableReleases(string changeLog)
        {
            Func<string, DateTime> extractDateFromLine = (line) =>
            {
                int startPos = line.IndexOf("(");
                int endPos = line.IndexOf(")");
                string strDate = line.Substring(startPos + 1, endPos - startPos - 1);


                if (DateTime.TryParse(strDate, out var date))
                    return date;

                return DateTime.MinValue;
            };

            Func<string, (string name, string version)> extractServiceInformation = (line) =>
            {
                var tokens = line.Substring(1).Trim().Split(' ');
                if (tokens.Length != 2)
                    return ("Unknown", "0.0.0.0");


                string name = tokens[0];
                string version = tokens[1].Replace("(", "").Replace(")", "");
                return (name, version);
            };

            using var reader = new StringReader(changeLog);

            ReleaseEntry currentReleaseEntry = null;
            ServiceEntry currentServiceEntry = null;
            string line;
            while (((line = reader.ReadLine())) != null)
            {
                if(line.StartsWith("###"))
                {
                    if(currentReleaseEntry != null)
                    {
                        yield return currentReleaseEntry;
                    }

                    currentReleaseEntry = new ReleaseEntry
                    {
                        Date = extractDateFromLine(line)
                    };
                }
                // Empty space at the start of the document
                else if (currentReleaseEntry == null)
                {
                    continue;
                }
                else if(line.StartsWith("* "))
                {
                    var info = extractServiceInformation(line);
                    currentServiceEntry = new ServiceEntry
                    {
                        Name = info.name,
                        Version = info.version
                    };
                    currentReleaseEntry.Services[currentServiceEntry.Name] = currentServiceEntry;
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
                _changeLogContent = await _httpClient.GetStringAsync(CHANGE_LOG_URL);
                _nextFetchTime = DateTime.Now.AddMinutes(REFRESH_INTERVAL_IN_MINUTES);
            }

            return _changeLogContent;
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
