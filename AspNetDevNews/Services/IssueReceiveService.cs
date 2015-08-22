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

        internal async Task<int> UpdateExistings(IEnumerable<Models.Issue> issues, IEnumerable<Models.Issue> lastStored)
        {
            List<Models.Issue> toUpdate = new List<Models.Issue>();
            foreach (var stgIssue in lastStored) {
                foreach (var githubIssue in issues)
                {
                    bool updated = false;
                    if (stgIssue.GetPartitionKey() == githubIssue.GetPartitionKey() && stgIssue.GetRowKey() == githubIssue.GetRowKey()) {
                        if (stgIssue.Title != githubIssue.Title) {
                            stgIssue.Title = githubIssue.Title;
                            updated = true;
                        }

                        if (stgIssue.UpdatedAt != githubIssue.UpdatedAt)
                        {
                            stgIssue.UpdatedAt = githubIssue.UpdatedAt;
                            updated = true;
                        }

                        if (stgIssue.State != githubIssue.State)
                        {
                            stgIssue.State = githubIssue.State;
                            updated = true;
                        }

                        if (stgIssue.Comments != githubIssue.Comments)
                        {
                            stgIssue.Comments = githubIssue.Comments;
                            updated = true;
                        }

                        //if (stgIssue.Labels != githubIssue.Labels)
                        //{
                        //    stgIssue.Labels = githubIssue.Labels;
                        //    updated = true;
                        //}

                        if (updated)
                            toUpdate.Add(stgIssue); 
                    }

                }

            }
            if (toUpdate.Count() > 0)
                this.StorageService.Store(toUpdate);

            return toUpdate.Count(); 
        }

        public IEnumerable<string> Labels
        {
            get { return new List<string> { "Announcement", "Breaking Change", "Feedback Wanted", "Up for Grabs" }; }
        }


        public async Task<IEnumerable<string>> Repositories( string organization)  {

            return await this.GitHubService.Repositories(organization);
        }

        public async Task<List<Models.Issue>> RecentIssues(string organization, string repository) {

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
    }
}
