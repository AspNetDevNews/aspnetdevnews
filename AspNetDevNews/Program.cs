﻿using AspNetDevNews.Helpers;
using AspNetDevNews.Models;
using AspNetDevNews.Services;
using AspNetDevNews.Services.AzureTableStorage;
using AspNetDevNews.Services.Feeds;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AspNetDevNews
{
    class Program
    {
        static void Main(string[] args)
        {
            //Test().Wait();

            Work().Wait();
        }

        public static async Task Merges()
        {
            var feedService = new FeedReaderService();
            //await feedService.ReadFeeds();

            //var gitHubService = new GitHubService();
            //await gitHubService.GetRecentMerges("aspnet", "Docs", "aspnet:master");
        }

        public static async Task Test()
        {
            AutoMapperHelper.InitMappings();

            var test = new TwittedIssue();
            test.Body = "corpo";
            test.Comments = 1;
            test.CreatedAt = DateTime.Now;
            test.Labels = new string[] { "test1", "test2" };
            test.Number = 101;
            test.Organization = "org";
            test.Repository = "repo";
            test.State = "open";
            test.StatusID = 1212;
            test.Title = "titolo";
            test.UpdatedAt = DateTime.Now.AddHours(1);
            test.Url = "http://localhost";

            var lista = new List<TwittedIssue>();
            lista.Add(test);
            var stg = new AzureTableStorageService();
            stg.Store(lista);
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
            //foreach (var feed in ghService.Feeds) { 
            //    // get recent posts
            //    var links = await ghService.RecentPosts(feed);
            //    // check for posts already in archive and remove from the list
            //    links = await ghService.RemoveExisting(links);
            //    // publish the new links
            //    var twittedPosts = await ghService.PublishNewPosts(links);
            //    // store in the storage the data about the new issues
            //    await ghService.StorePublishedPosts(twittedPosts);
            //    postedLink += twittedPosts.Count;
            //}

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
