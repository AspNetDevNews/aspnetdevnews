using AspNetDevNews.Services.Interfaces;
using Octokit;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;

namespace AspNetDevNews.Services
{
    public class IssueReceiveService
    {
        private IGitHubService GitHubService { get; set; }
        private IStorageService StorageService { get; set; }

        public IssueReceiveService() {
            this.GitHubService = new GitHubService();
            this.StorageService = new AzureTableStorageService();
        }

        public IssueReceiveService(IGitHubService gitHubService, IStorageService storageService) {
            if (gitHubService == null)
                throw new ArgumentNullException("gitHubService cannot be null");
            if (storageService == null)
                throw new ArgumentNullException("storageService cannot be null");

            this.GitHubService = gitHubService;
            this.StorageService = storageService;
        }

        public IEnumerable<string> Organizations {
            get { return new List<string> { "aspnet", "nuget" }; }
        }

        public IEnumerable<string> Labels
        {
            get { return new List<string> { "Announcement", "Breaking Change", "Feedback Wanted", "Up for Grabs" }; }
        }


        public DateTimeOffset Since {
            get { return DateTime.Now.AddDays(-7); }
        }

        public async Task<IEnumerable<string>> Repositories( string organization)  {

            return await this.GitHubService.Repositories(organization);
        }

        public async Task<List<Models.Issue>> RecentIssues(string organization, string repository) {

            try
            {
                List<Models.Issue> issuesToProcess = new List<Models.Issue>();
                foreach (var issue in await this.GitHubService.GetRecentIssues(organization, repository, this.Since))
                {
                    foreach (var refLabel in Labels)
                    {
                        if (issue.Labels.Contains(refLabel))
                            issuesToProcess.Add(issue);
                    }
                }
                return issuesToProcess;
            }
            catch (Exception exc)
            {
                await this.StorageService.Store(exc, null, "RecentIssues");
                return new List<Models.Issue>();
            }
        }
    }
}
