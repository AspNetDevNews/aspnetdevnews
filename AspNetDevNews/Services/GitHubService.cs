using AspNetDevNews.Services.Interfaces;
using Octokit;
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
            if (string.IsNullOrWhiteSpace(organization))
                throw new ArgumentNullException(nameof(organization), "organization must be specified");
            if (string.IsNullOrWhiteSpace(repository))
                throw new ArgumentNullException(nameof(repository), "repository must be specified");

            var client = GetClient();
            var request = new RepositoryIssueRequest();
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

            var merged = new List<Release>();
            var releases = await client.Release.GetAll( organization, repository);

            foreach (var release in releases) {
            }

            foreach (var merge in merged) {
            }
        }

        private static bool SetAllowUnsafeHeaderParsing(bool value)
        {
            //Get the assembly that contains the internal class  
            Assembly aNetAssembly = Assembly.GetAssembly(typeof(System.Net.Configuration.SettingsSection));
            if (aNetAssembly != null)
            {
                //Use the assembly in order to get the internal type for the internal class  
                Type aSettingsType = aNetAssembly.GetType("System.Net.Configuration.SettingsSectionInternal");
                if (aSettingsType != null)
                {
                    //Use the internal static property to get an instance of the internal settings class.  
                    //If the static instance isn't created allready the property will create it for us.  
                    object anInstance = aSettingsType.InvokeMember("Section",
                      BindingFlags.Static | BindingFlags.GetProperty | BindingFlags.NonPublic, null, null, new object[] { });

                    if (anInstance != null)
                    {
                        //Locate the private bool field that tells the framework is unsafe header parsing should be allowed or not  
                        FieldInfo aUseUnsafeHeaderParsing = aSettingsType.GetField("useUnsafeHeaderParsing", BindingFlags.NonPublic | BindingFlags.Instance);
                        if (aUseUnsafeHeaderParsing != null)
                        {
                            aUseUnsafeHeaderParsing.SetValue(anInstance, value);
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        // http://dejanstojanovic.net/aspnet/2014/october/the-server-committed-a-protocol-violation-section-responsestatusline/
        public async Task<IList<string>> ExtractCommitDocuments(string organization, string repository, string commit) {
            var client = GetClient();
            var documents = new List<string>();

            SetAllowUnsafeHeaderParsing(true);

            var commitData = await client.GitDatabase.Commit.Get(organization, repository, commit);

            var repo = await client.Repository.GetBranch(organization, repository, "master");
            repo.Commit.Url.ToString();

            var myCommit = RestHelper.Get<string>("https://api.github.com/repos/aspnet/Docs/commits/", "cb5fc228c857cfabbc128385a628b95f89232469");

            ////commitData.Tree
            //var tree = await client.GitDatabase.Tree.Get(organization, repository, commitData.Tree.Sha);
            //foreach (var elemento in tree.Tree) {
            //    if (elemento.Type == TreeType.Blob && elemento.Path.ToLower().EndsWith(".rst"))
            //        documents.Add(elemento.Path);
            //    if (elemento.Type == TreeType.Tree) {
            //        var test = await client.GitDatabase.Tree.Get(organization, repository, elemento.Sha);
            //    }
            //}

            //var blob = await client.GitDatabase.Blob.Get(organization, repository, commitData.Tree.Sha);
            var deploy = await client.Deployment.GetAll(organization, repository);

            return null;
        }

    }
}
