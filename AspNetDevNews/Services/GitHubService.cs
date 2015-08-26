using AspNetDevNews.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AspNetDevNews.Models;
using AspNetDevNews.Helpers;
using System.Reflection;

namespace AspNetDevNews.Services
{
    public class GitHubService : IGitHubService
    {
        private ISettingsService Settings { get; set; }

        // Devo togliere le referenze, poi lo posso cancellare
        //public GitHubService()
        //{
        //    this.Settings = new SettingsService();
        //}

        // Used by AutoFac
        public GitHubService(ISettingsService settings)
        {
            if (settings == null)
                throw new ArgumentNullException("settings cannot be null");
            this.Settings = settings;
        }

        private Octokit.GitHubClient GetClient()
        {
            var client = new Octokit.GitHubClient(new Octokit.ProductHeaderValue(this.Settings.GitHubAppName));
            var basicAuth = new Octokit.Credentials(this.Settings.GitHubUserId, this.Settings.GitHubPassword);
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
            if (string.IsNullOrWhiteSpace(organization))
                throw new ArgumentNullException(nameof(organization), "organization must be specified");
            if (string.IsNullOrWhiteSpace(repository))
                throw new ArgumentNullException(nameof(repository), "repository must be specified");

            var client = GetClient();
            var request = new Octokit.RepositoryIssueRequest();
            request.Since = since;

            try
            {
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
                    // OK
                    //var issueToProcess = new Models.Issue();
                    //issueToProcess.Title = issue.Title;
                    //issueToProcess.Url = issue.HtmlUrl.ToString();
                    //issueToProcess.Labels = issue.Labels.Select(lab => lab.Name).ToArray();
                    //issueToProcess.Organization = organization;
                    //issueToProcess.Repository = repository;
                    //issueToProcess.CreatedAt = issue.CreatedAt.LocalDateTime;
                    //issueToProcess.Number = issue.Number;
                    //issueToProcess.UpdatedAt = issue.UpdatedAt?.LocalDateTime;
                    //issueToProcess.Body = issue.Body;
                    //issueToProcess.State = issue.State == 0 ? "Open" : "Closed";
                    //issueToProcess.Comments = issue.Comments;

                    var issueToProcess = AutoMapper.Mapper.Map<Models.Issue>(issue);
                    issueToProcess.Organization = organization;
                    issueToProcess.Repository = repository;

                    issuesToProcess.Add(issueToProcess);
                }

                return issuesToProcess;
            }
            catch (Exception exc) {
                return new List<Models.Issue>();
            }
        }

        public async Task GetRecentReleases(string organization, string repository) {
            var client = GetClient();

            var merged = new List<Octokit.Release>();
            var releases = await client.Release.GetAll( organization, repository);

            foreach (var release in releases) {
            }

            foreach (var merge in merged) {
            }
        }

        public async Task<IList<GitHubHostedDocument>> ExtractCommitDocuments(string organization, string repository) {

            var client = GetClient();
            var documents = new List<GitHubHostedDocument>();

            Octokit.CommitRequest request = new Octokit.CommitRequest();
//            request.Since = new DateTimeOffset(DateTime.Now.AddDays(-7));
            request.Since = this.Settings.Since;
            request.Sha = "master";

            // get all the latest commits for the master branch 
            var commits = await client.Repository.Commits.GetAll(organization, repository, request);
            // recover the data of the branch looking for the master
            var repo = await client.Repository.GetBranch(organization, repository, "master");

            var head = repo.Commit.Sha;

            foreach (var currentCommit in commits) {
                var compareResult = await client.Repository.Commits.Compare(organization, repository, currentCommit.Sha, head);
                head = currentCommit.Sha;

                foreach (var file in compareResult.Files) {
                    if (file.Filename.ToLower().EndsWith(".rst", StringComparison.Ordinal)) { 
                        var fileData = new GitHubHostedDocument();
                        fileData.Commit = currentCommit.Sha;
                        fileData.FileName = file.Filename;
                        fileData.Status = file.Status;
                        fileData.TsCommit = currentCommit.Commit.Committer.Date;
                        fileData.Organization = organization;
                        fileData.Repository = repository;

                        documents.Add(fileData);
                    }
                }
            }
            return documents;
        }

    }
}
