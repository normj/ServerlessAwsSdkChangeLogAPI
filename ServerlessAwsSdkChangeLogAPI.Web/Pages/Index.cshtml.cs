using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

using ServerlessAwsSdkChangeLogAPI.Common.Services;



namespace ServerlessAwsSdkChangeLogAPI.Web.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly IAwsSdkChangeLogService _awsSdkChangeLogService;

        public IList<AwsSdkChangeLogService.ReleaseEntry> LastWeekOfReleases { get; } = new List<AwsSdkChangeLogService.ReleaseEntry>();

        public IndexModel(ILogger<IndexModel> logger, IAwsSdkChangeLogService awsSdkChangeLogService)
        {
            _logger = logger;
            this._awsSdkChangeLogService = awsSdkChangeLogService;
        }

        public async Task OnGet()
        {
            var maxDate = DateTime.Today.AddDays(-7);
            this.LastWeekOfReleases.Clear();
            foreach(var release in (await _awsSdkChangeLogService.EnumerableReleases()))
            {
                if (release.Date.Date < maxDate)
                    break;

                this.LastWeekOfReleases.Add(release);
            }
        }
    }
}