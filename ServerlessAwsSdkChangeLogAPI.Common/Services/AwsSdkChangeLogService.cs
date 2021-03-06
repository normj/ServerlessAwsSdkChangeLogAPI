﻿using System;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.Net.Http;
using System.Linq;
using System.Threading.Tasks;
using System.Text;
using System.IO;
using Microsoft.Extensions.Logging;

namespace ServerlessAwsSdkChangeLogAPI.Common.Services
{
    public interface IAwsSdkChangeLogService
    {
        Task<IEnumerable<string>> GetListOfServicesAsync();
        Task<IEnumerable<AwsSdkChangeLogService.ServiceRelease>> GetServiceAsync(string serviceName);

        Task<IEnumerable<AwsSdkChangeLogService.ReleaseEntry>> EnumerableReleases();
    }


    public class AwsSdkChangeLogService : IAwsSdkChangeLogService
    {
        const string UnknownPlaceHolder = "unknown";

        readonly IAwsSdkChangeLogFetcherService _logFetcher;

        private readonly ILogger<AwsSdkChangeLogService> _logger;

        public AwsSdkChangeLogService(ILogger<AwsSdkChangeLogService> logger, IAwsSdkChangeLogFetcherService logFetcher)
        {
            _logger = logger;
            _logFetcher = logFetcher;
        }

        public async Task<IEnumerable<string>> GetListOfServicesAsync()
        {
            var setOfServices = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach(var release in EnumerableReleases(await _logFetcher.GetChangeLogTextAsync()))
            {
                foreach (var service in release.Services.Keys)
                {
                    if (!setOfServices.Contains(service))
                    {
                        setOfServices.Add(service.ToLower());
                    }
                }
            }

            return setOfServices.OrderBy(x => x);
        }

        public async Task<IEnumerable<ServiceRelease>> GetServiceAsync(string serviceName)
        {
            var serviceReleases = new List<ServiceRelease>();
            
            foreach(var release in EnumerableReleases(await _logFetcher.GetChangeLogTextAsync()))
            {
                
                if(release.Services.TryGetValue(serviceName, out var service))
                {
                    var sr = new ServiceRelease
                    {
                        Version = service.Version,
                        ReleaseDate = release.Date
                    };
                    foreach (var feature in service.Features)
                    {
                        sr.Features.Add(feature);
                    }

                    serviceReleases.Add(sr);
                }
            }

            return serviceReleases;
        }

        public async Task<IEnumerable<ReleaseEntry>> EnumerableReleases()
        {
            var changeLog = await _logFetcher.GetChangeLogTextAsync();
            return EnumerableReleases(changeLog);
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
                    int pos = line.IndexOf('*');
                    currentServiceEntry.Features.Add(line.Substring(pos + 1).Trim());
                }
            }
        }


        public DateTime ExtractDateFromLine(string line)
        {
            var startPos = line.IndexOf("(");
            var endPos = line.IndexOf(")");
            if (startPos == -1 || endPos == -1 || endPos < startPos)
            {
                _logger?.LogWarning($"Failed to parse date from line: \"{line}\"");
                return DateTime.MinValue;
            }

            var strDate = line.Substring(startPos + 1, endPos - startPos - 1);

            // Chop off the time component
            if (strDate.Length > 10) 
                strDate = strDate.Substring(0, 10);

            if (!DateTime.TryParse(strDate, out var date))
            {
                _logger?.LogWarning($"Failed to parse date from line: \"{line}\"");
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

            if (!char.IsLetter(name[0]) || name.Contains(' '))
            {
                return (UnknownPlaceHolder, "0.0.0.0");
            }
            
            return (name, version);
        }        


        public class ReleaseEntry
        {
            public DateTime Date { get; set; }
            public IDictionary<string, ServiceEntry> Services { get; set; } = new Dictionary<string, ServiceEntry>(StringComparer.OrdinalIgnoreCase);
        }

        public class ServiceEntry
        {
            public string Name { get; set; }
            public string Version { get; set; }
            public IList<string> Features { get; set; } = new List<string>();
        }


        public class ServiceRelease
        {
            public DateTime ReleaseDate { get; set; }
            public string Version { get; set; }
            public IList<string> Features { get; } = new List<string>();
        }

    }
}
