using AspNetDevNews.Helpers;
using AspNetDevNews.Services;
using AspNetDevNews.Services.Interfaces;
using Autofac;
using System;
using System.Linq;
using System.Threading.Tasks;

// use autofac to resolve TwitterContext, to make sendIssues routine testable, get tables in azure storage to make store testable

// azuretablestorage: store methods, checks that ExecuteBatchAsync isn't called  if list is empty
// public IList<FeedItem> GetBatchWebLinks(string feed, IList<string> rowKeys), test per controllare l'encoding dei dati che passa all'executequery
// tests for formatting of the tweet contents


namespace AspNetDevNews
{
    class Program
    {
        private static IContainer Container { get; set; }

        static void Main(string[] args)
        {
            //Merges().Wait();

            Work().Wait();
        }

        public static async Task Merges()
        {
            AutoMapperHelper.InitMappings();
            Container = AutoFacHelper.InitAutoFac();

            var service = Container.Resolve<IssueReceiveService>();
            var github = Container.Resolve<GitHubService>();

            var storage = Container.Resolve<IStorageService>();
            storage.GetRecentGitHubDocuments("aspnet", "Docs", new DateTimeOffset(DateTime.Today.AddDays(-1)));
            //var documents = await service.RecentGitHubDocuments("aspnet", "Docs");

            //var documents = await github.ExtractCommitDocuments("aspnet", "EntityFramework.Docs", new DateTimeOffset(DateTime.Today.AddMonths(-4)));
            //var documents = await github.ExtractCommitDocuments("dotnet", "core-docs", new DateTimeOffset(DateTime.Today.AddMonths(-4)));
            //documents = service.RemoveExisting(documents);
            // publish the new links
            //var twittedDocs = await service.Publish(documents);
            // store in the storage the data about the new issues
            //await service.Store(twittedDocs);

        }

        public static async Task Work() {

            AutoMapperHelper.InitMappings();
            Container = AutoFacHelper.InitAutoFac();

            var ghService = Container.Resolve<IssueReceiveService>();
            var logger = Container.Resolve<ISessionLogger>();
            var jobService = Container.Resolve<IJobService>();

            DateTime dtInizio = DateTime.Now;
            int twitted = 0;
            int checkedRepositories = 0;
            int updated = 0;
            int postedLink = 0;

            logger.StartSession();
            var docRepos = jobService.DocsRepo;

            foreach (var repo in docRepos)
            {
                logger.AddMessage("docRepos", "scanning", repo.Organization + " " + repo.Repository, MessageType.Info);

                // documents update processing
                var documents = await ghService.RecentGitHubDocuments(repo.Organization, repo.Repository);

                documents = ghService.RemoveExisting(documents);
                // publish the new links
                var twittedDocs = await ghService.Publish(documents);
                // store in the storage the data about the new issues
                await ghService.Store(twittedDocs);
            }

            // web posts processing
            foreach (var feed in jobService.Feeds)
            {
                logger.AddMessage("feeds", "scanning", feed, MessageType.Info);

                // get recent posts
                var links = await ghService.RecentPosts(feed);
                // check for posts already in archive and remove from the list
                links = ghService.RemoveExisting(links);
                // publish the new links
                var twittedPosts = await ghService.Publish(links);
                // store in the storage the data about the new issues
                await ghService.Store(twittedPosts);
                postedLink += twittedPosts.Count;
            }

            // github repositories processing
            foreach (var organization in jobService.Organizations) {
                var repositories = await ghService.Repositories(organization);
                checkedRepositories += repositories.Count();
                foreach (var repository in repositories) {
                    logger.AddMessage("repository", "scanning", organization + " " + repository, MessageType.Info);

                    // get recent created or modified issues
                    var gitHubIssues = await ghService.RecentGitHubIssues(organization, repository);
                    // if no issues are reported from github, go next repository
                    if (gitHubIssues == null || gitHubIssues.Count == 0)
                        continue;
                    // get the latest issues archived
                    var storageIssues = ghService.CheckInStorage(organization, repository, gitHubIssues);
                    // check for updates
                    var changed = ghService.IssuesToUpdate(gitHubIssues, storageIssues);
                    // if updated ones, merge the changes
                    await ghService.Merge(changed);
                    // remove from the list the ones already in the storage, keeping just the ones 
                    // I have to tweet and store
                    gitHubIssues = ghService.RemoveExisting(gitHubIssues);
                    // publish the new issues
                    var twittedIssues = await ghService.Publish(gitHubIssues);
                    // store in the storage the data about the new issues
                    await ghService.Store(twittedIssues);
                    updated += changed.Count;
                    twitted += twittedIssues.Count;
                }
            }

            DateTime dtFine = DateTime.Now;
            var stgService = Container.Resolve<IStorageService>();
            await stgService.ReportExecution(dtInizio, dtFine, twitted, checkedRepositories, updated, postedLink);
            logger.EndSession();
        }

    }
}
