using Octokit;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AspNetDevNews.Services
{
    public class GitHubService
    {
        public IEnumerable<string> Organizations {
            get { return new List<string> { "aspnet", "nuget" }; }
        }

        public IEnumerable<string> Labels
        {
            //get { return new List<string> { "enhancement" }; }
            
            get { return new List<string> { "Announcement", "Breaking Change", "Feedback Wanted", "Up for Grabs" }; }
        }


        public DateTimeOffset Since {
//            get { return DateTime.Now.AddMinutes(-15); }
            get { return DateTime.Now.AddDays(-7); }
        }

        private GitHubClient GetClient() {
            var client = new GitHubClient(new ProductHeaderValue(ConfigurationSettings.AppSettings["GitHubAppName"]));
            var basicAuth = new Credentials(ConfigurationSettings.AppSettings["GitHubUserId"], ConfigurationSettings.AppSettings["GitHubPassword"]); // NOTE: not real credentials
            client.Credentials = basicAuth;
            return client;
        }

        public async Task<IEnumerable<string>> Repositories( string organization)  {

            var repositoriesNames = new List<string>();
            var client = GetClient();

            var repositories = await client.Repository.GetAllForOrg(organization);
            foreach (var repository in repositories) {
                repositoriesNames.Add(repository.Name);
            }
            return repositoriesNames;
        }

        public async Task<List<Models.Issue>> RecentIssues(string organization, string repository) {

            var client = GetClient();
            var request = new RepositoryIssueRequest();
            request.Since = this.Since;

            try
            {
                var queryResult = await client.Issue.GetAllForRepository(organization, repository, request);
                var issues = new List<Issue>();
                foreach (var issue in queryResult)
                {
                    if (issue.PullRequest != null)
                        continue;

                    foreach (var refLabel in Labels)
                    {
                        if (issue.Labels.Any(lab => lab.Name == refLabel))
                        {
                            issues.Add(issue);
                        }
                    }
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
            catch (Exception exc) {
                var stgService = new ATStorageService();
                stgService.Store(exc, null, "RecentIssues");
                return new List<Models.Issue>();
            }
        }
    }
}
