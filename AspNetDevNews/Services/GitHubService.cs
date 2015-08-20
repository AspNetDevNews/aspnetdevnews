using AspNetDevNews.Services.Interfaces;
using Octokit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AspNetDevNews.Models;

namespace AspNetDevNews.Services
{
    public class GitHubService : IGitHubService
    {
        private ISettingsService Settings { get; set; }

        public GitHubService()
        {
            this.Settings = new SettingsService();
        }

        public GitHubService(ISettingsService settings)
        {
            this.Settings = settings;
        }

        private GitHubClient GetClient()
        {
            var client = new GitHubClient(new ProductHeaderValue(this.Settings.GitHubAppName));
            var basicAuth = new Credentials(this.Settings.GitHubUserId, this.Settings.GitHubPassword);
            client.Credentials = basicAuth;
            return client;
        }

        public async Task<IEnumerable<string>> Repositories(string organization)
        {
            var repositoriesNames = new List<string>();
            var client = GetClient();

            var repositories = await client.Repository.GetAllForOrg(organization);
            foreach (var repository in repositories)
                repositoriesNames.Add(repository.Name);
            return repositoriesNames;
        }

        public async Task<IEnumerable<Models.Issue>> GetRecentIssues(string organization, string repository, DateTimeOffset since)
        {
            var client = GetClient();
            var request = new RepositoryIssueRequest();
            request.Since = since;

            var queryResult = await client.Issue.GetAllForRepository(organization, repository, request);
            var issues = new List<Octokit.Issue>();
            foreach (var issue in queryResult)
            {
                if (issue.PullRequest != null)
                    continue;

                issues.Add(issue);
            }
            var issuesToProcess = new List<Models.Issue>();
            foreach (var issue in issues)
            {
                var issueToProcess = new Models.Issue();
                issueToProcess.Title = issue.Title;
                issueToProcess.Url = issue.HtmlUrl.ToString();
                issueToProcess.Labels = issue.Labels.Select(lab => lab.Name).ToArray();
                issueToProcess.Organization = organization;
                issueToProcess.Repository = repository;
                issueToProcess.CreatedAt = issue.CreatedAt.LocalDateTime;
                issueToProcess.Number = issue.Number;
                issueToProcess.UpdatedAt = issue.UpdatedAt?.LocalDateTime;

                issuesToProcess.Add(issueToProcess);
            }

            return issuesToProcess;
        }
    }
}
