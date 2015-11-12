using AspNetDevNews.Services.Interfaces;
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
        private ISessionLogger Logger { get; set; }

        public GitHubService(ISettingsService settings, ISessionLogger logger)
        {
            if (settings == null)
                throw new ArgumentNullException(nameof(settings), "cannot be null");
            if (logger == null)
                throw new ArgumentNullException(nameof(logger), "cannot be null");

            this.Settings = settings;
            this.Logger = logger;
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
            if (string.IsNullOrWhiteSpace(organization))
                throw new ArgumentNullException(nameof(organization), "cannot be null or empty");

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
                throw new ArgumentNullException(nameof(organization), "must be specified");
            if (string.IsNullOrWhiteSpace(repository))
                throw new ArgumentNullException(nameof(repository), "must be specified");

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
                    var issueToProcess = AutoMapper.Mapper.Map<Models.Issue>(issue);
                    issueToProcess.Organization = organization;
                    issueToProcess.Repository = repository;

                    issuesToProcess.Add(issueToProcess);
                }

                return issuesToProcess;
            }
            catch (Exception exc) {
                this.Logger.AddMessage("GetRecentIssues", $"exception {exc.Message} while reading recent Issues", organization + " " + repository, MessageType.Error);
                return new List<Models.Issue>();
            }
        }

        public async Task<IList<GitHubHostedDocument>> ExtractCommitDocuments(string organization, string repository) {
            if (string.IsNullOrWhiteSpace(organization))
                throw new ArgumentNullException(nameof(organization), "must be specified");
            if (string.IsNullOrWhiteSpace(repository))
                throw new ArgumentNullException(nameof(repository), "must be specified");

            var client = GetClient();
            var documents = new List<GitHubHostedDocument>();

            Octokit.CommitRequest request = new Octokit.CommitRequest();
            request.Since = this.Settings.Since;
            request.Sha = "master";

            // get all the latest commits for the master branch 
            var commits = await client.Repository.Commits.GetAll(organization, repository, request);
            // recover the data of the branch looking for the master
            var repo = await client.Repository.GetBranch(organization, repository, "master");

            var head = repo.Commit.Sha;

            foreach (var currentCommit in commits) {
                //var compareResult = await client.Repository.Commits.Compare(organization, repository, currentCommit.Sha, head);
                //head = currentCommit.Sha;

                var myCommit = await client.Repository.Commits.Get(organization, repository, currentCommit.Sha);
                if (myCommit.Files != null) {
                    Console.WriteLine(myCommit.Files.Count.ToString());

                    foreach (var file in myCommit.Files)
                    {
                        if (file.Filename.ToLower().EndsWith(".rst", StringComparison.Ordinal))
                        {
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

            }
            return documents;
        }

    }
}
