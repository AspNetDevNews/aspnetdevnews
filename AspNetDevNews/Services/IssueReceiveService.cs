using AspNetDevNews.Services.Interfaces;
using Octokit;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using AspNetDevNews.Models;

namespace AspNetDevNews.Services
{
    public class IssueReceiveService
    {
        private IGitHubService GitHubService { get; set; }
        private IStorageService StorageService { get; set; }
        private ISettingsService SettingsService { get; set; }

        public IssueReceiveService() {
            this.GitHubService = new GitHubService();
            this.StorageService = new AzureTableStorageService();
            this.SettingsService = new SettingsService();
        }

        public IssueReceiveService(IGitHubService gitHubService, IStorageService storageService, ISettingsService settingsService ) {
            if (gitHubService == null)
                throw new ArgumentNullException("gitHubService cannot be null");
            if (storageService == null)
                throw new ArgumentNullException("storageService cannot be null");
            if (settingsService == null)
                throw new ArgumentNullException("settingsService cannot be null");

            this.GitHubService = gitHubService;
            this.StorageService = storageService;
            this.SettingsService = settingsService;
        }

        public IEnumerable<string> Organizations {
            get { return new List<string> { "aspnet", "nuget" }; }
        }

        public async Task<IList<Models.Issue>> IssuesToUpdate(IList<Models.Issue> issues, IList<Models.Issue> lastStored)
        {
            List<Models.Issue> toUpdate = new List<Models.Issue>();

            if ((issues == null) || (lastStored == null))
                return toUpdate;

            foreach (var stgIssue in lastStored) {
                foreach (var githubIssue in issues)
                {
                    if (stgIssue.GetPartitionKey() == githubIssue.GetPartitionKey() && stgIssue.GetRowKey() == githubIssue.GetRowKey()) {
                        bool updated = false;

                        if (stgIssue.Title != githubIssue.Title) 
                            updated = true;
                        else if (stgIssue.UpdatedAt != githubIssue.UpdatedAt)
                            updated = true;
                        else if (stgIssue.State != githubIssue.State)
                            updated = true;
                        else if (stgIssue.Comments != githubIssue.Comments)
                            updated = true;

                        //if (stgIssue.Labels != githubIssue.Labels)
                        //{
                        //    stgIssue.Labels = githubIssue.Labels;
                        //    updated = true;
                        //}

                        if (updated) 
                            toUpdate.Add(githubIssue);
                    }
                }

            }

            return toUpdate; 
        }

        public IList<string> Labels
        {
            get { return new List<string> { "Announcement", "Breaking Change", "Feedback Wanted", "Up for Grabs" }; }
        }

        public async Task<IEnumerable<string>> Repositories( string organization)  {
            if (string.IsNullOrWhiteSpace(organization))
                throw new ArgumentNullException("organization must be specified");

            return await this.GitHubService.Repositories(organization);
        }

        public async Task<IList<Models.Issue>> RecentGitHubIssues(string organization, string repository) {
            if (string.IsNullOrWhiteSpace(organization))
                throw new ArgumentNullException("organization must be specified");
            if (string.IsNullOrWhiteSpace(repository))
                throw new ArgumentNullException("organization must be specified");

            try
            {
                List<Models.Issue> issuesToProcess = new List<Models.Issue>();
                foreach (var issue in await this.GitHubService.GetRecentIssues(organization, repository, this.SettingsService.Since))
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

        public async Task<IList<Models.Issue>> RecentStorageIssues(string organization, string repository)
        {
            if (string.IsNullOrWhiteSpace(organization))
                throw new ArgumentNullException("organization must be specified");
            if (string.IsNullOrWhiteSpace(repository))
                throw new ArgumentNullException("organization must be specified");

            return await this.StorageService.GetRecentIssues(organization, repository, this.SettingsService.Since);
        }

        public async Task Merge(IList<Models.Issue> issues) {
            if (issues == null || issues.Count == 0)
                return; 
            await this.StorageService.Merge(issues);
        }
    }
}
