using AspNetDevNews.Helpers;
using AspNetDevNews.Models;
using AspNetDevNews.Services;
using AspNetDevNews.Services.AzureTableStorage;
using AspNetDevNews.Services.Feeds;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// Add AutoFac
// use autofac to resolve TwitterContext, to make sendIssues routine testable, get tables in azure storage to make store testable
// add logger as instance and store everything in a blob

// azuretablestorage: store methods, checks that ExecuteBatchAsync isn't called  if list is empty
// more parameters validation in azurestorageservice

namespace AspNetDevNews
{
    class Program
    {
        static void Main(string[] args)
        {
//            Merges().Wait();

            Work().Wait();
        }

        public static async Task Merges()
        {
            AutoMapperHelper.InitMappings();

            var lista = new List<Models.TwittedPost>();
            var issue = new Models.TwittedPost();
            issue.Feed = "mio Feed";
            issue.Id = "http://loclahost";
            issue.PublishDate = DateTime.Now;
            issue.StatusID = 12;
            issue.Summary = "sommario";
            issue.Title = "titolo";

            lista.Add(issue);
            var twService = new AzureTableStorageService();
            await twService.Store(lista);

            return;
            var feedService = new FeedReaderService();
            //await feedService.ReadFeeds();

            var gitHubService = new GitHubService();
            await gitHubService.ExtractCommitDocuments("aspnet", "Docs", "cb5fc228c857cfabbc128385a628b95f89232469");
        }

        public static async Task Work() {

            AutoMapperHelper.InitMappings();
            var ghService = new IssueReceiveService();

            DateTime dtInizio = DateTime.Now;
            int twitted = 0;
            int checkedRepositories = 0;
            int updated = 0;
            int postedLink = 0;

            //// web posts processing
            foreach (var feed in ghService.Feeds)
            {
                // get recent posts
                var links = await ghService.RecentPosts(feed);
                // check for posts already in archive and remove from the list
                links = await ghService.RemoveExisting(links);
                // publish the new links
                var twittedPosts = await ghService.PublishNewPosts(links);
                // store in the storage the data about the new issues
                await ghService.StorePublishedPosts(twittedPosts);
                postedLink += twittedPosts.Count;
            }

            // github repositories processing
            foreach (var organization in ghService.Organizations) {
                var repositories = await ghService.Repositories(organization);
                checkedRepositories += repositories.Count();
                foreach (var repository in repositories) {
                    // get recent created or modified issues
                    var gitHubIssues = await ghService.RecentGitHubIssues(organization, repository);
                    // if no issues are reported from github, go next repository
                    if (gitHubIssues == null || gitHubIssues.Count == 0)
                        continue;
                    // get the latest issues archived
                    var storageIssues = await ghService.CheckInStorage(organization, repository, gitHubIssues);
                    // check for updates
                    var changed = await ghService.IssuesToUpdate(gitHubIssues, storageIssues);
                    // if updated ones, merge the changes
                    await ghService.Merge(changed);
                    // remove from the list the ones already in the storage, keeping just the ones 
                    // I have to tweet and store
                    gitHubIssues = await ghService.RemoveExisting(gitHubIssues);
                    // publish the new issues
                    var twittedIssues = await ghService.PublishNewIssues(gitHubIssues);
                    // store in the storage the data about the new issues
                    await ghService.StorePublishedIssues(twittedIssues);
                    updated += changed.Count;
                    twitted += twittedIssues.Count;
                }
            }

            DateTime dtFine = DateTime.Now;
            var stgService = new AzureTableStorageService();
            await stgService.ReportExecution(dtInizio, dtFine, twitted, checkedRepositories, updated, postedLink);
        }

    }
}
