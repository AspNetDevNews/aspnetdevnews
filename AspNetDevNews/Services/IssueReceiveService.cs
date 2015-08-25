using AspNetDevNews.Models;
using AspNetDevNews.Services.AzureTableStorage;
using AspNetDevNews.Services.Feeds;
using AspNetDevNews.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AspNetDevNews.Services
{
    public class IssueReceiveService
    {
        private IGitHubService GitHubService { get; set; }
        private IStorageService StorageService { get; set; }
        private ISettingsService SettingsService { get; set; }
        private ITwitterService TwitterService { get; set; }
        private IFeedReaderService FeedReaderService { get; set; }


        // da rimuovere
        public IssueReceiveService()
        {
            //this.SettingsService = new SettingsService();
            //this.GitHubService = new GitHubService();
            //this.FeedReaderService = new FeedReaderService();

            //this.StorageService = new AzureTableStorageService();
            //this.TwitterService = new TwitterService();
        }

        public IssueReceiveService(IGitHubService gitHubService, IStorageService storageService, ISettingsService settingsService, 
            ITwitterService twitterService, IFeedReaderService feedReaderService ) {
            if (gitHubService == null)
                throw new ArgumentNullException(nameof(gitHubService), "gitHubService cannot be null");
            if (storageService == null)
                throw new ArgumentNullException(nameof(storageService), "storageService cannot be null");
            if (settingsService == null)
                throw new ArgumentNullException(nameof(settingsService), "settingsService cannot be null");
            if (twitterService == null)
                throw new ArgumentNullException(nameof(twitterService), "twitterService cannot be null");
            if (feedReaderService == null)
                throw new ArgumentNullException(nameof(feedReaderService), "feedReaderService cannot be null");

            this.GitHubService = gitHubService;
            this.StorageService = storageService;
            this.SettingsService = settingsService;
            this.TwitterService = twitterService;
            this.FeedReaderService = feedReaderService;
        }

        public IEnumerable<string> Organizations {
            get { return new List<string> { "aspnet", "nuget" }; }
        }

        public IEnumerable<string> Feeds {
            get { return new List<string> { "http://webdevblogs.azurewebsites.net/master.xml" }; }
        }

        public async Task<IList<Issue>> IssuesToUpdate(IList<Issue> issues, IList<Issue> lastStored)
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
            get { return new List<string> { "Announcement", "Breaking Change", "Feedback Wanted", "Up for Grabs", "up-for-grabs", "help wanted", "feedback-requested" }; }
        }

        public async Task<IEnumerable<string>> Repositories( string organization)  {
            if (string.IsNullOrWhiteSpace(organization))
                throw new ArgumentNullException(nameof(organization), "organization must be specified");

            return await this.GitHubService.Repositories(organization);
        }

        public async Task<IList<Issue>> RecentGitHubIssues(string organization, string repository) {
            if (string.IsNullOrWhiteSpace(organization))
                throw new ArgumentNullException(nameof(organization), "organization must be specified");
            if (string.IsNullOrWhiteSpace(repository))
                throw new ArgumentNullException(nameof(repository), "repository must be specified");

            try
            {
                List<Models.Issue> issuesToProcess = new List<Models.Issue>();
                var recentIssues = await this.GitHubService.GetRecentIssues(organization, repository, this.SettingsService.Since);
                foreach (var issue in recentIssues)
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

        public async Task<IList<Issue>> CheckInStorage(string organization, string repository, IList<Issue> issuesToCheck)
        {
            if (string.IsNullOrWhiteSpace(organization))
                throw new ArgumentNullException(nameof(organization), "organization must be specified");
            if (string.IsNullOrWhiteSpace(repository))
                throw new ArgumentNullException(nameof(repository), "repository must be specified");
            if (issuesToCheck == null || issuesToCheck.Count == 0)
                return new List<Models.Issue>();

            //return await this.StorageService.GetRecentIssues(organization, repository, this.SettingsService.Since);
            List<string> RowKeysToScan = new List<string>();
            foreach (var issue in issuesToCheck)
            {
                if (issue.Organization != organization || issue.Repository != repository)
                    throw new ApplicationException("Can process only issues from the same repository");
                RowKeysToScan.Add(issue.GetRowKey());
            }

            try
            {
                return this.StorageService.GetBatchIssues(organization, repository, RowKeysToScan);
            }
            catch (Exception ex)
            {
                return new List<Models.Issue>();
            }

        }

        public async Task Merge(IList<Issue> issues) {
            if (issues == null || issues.Count == 0)
                return; 
            await this.StorageService.Merge(issues);
        }

        public async Task<IList<Models.Issue>> RemoveExisting(IList<Issue> issues)
        {
            List<Models.Issue> result = new List<Models.Issue>();
            if (issues == null || issues.Count == 0)
                return result;

            var organization = issues[0].Organization;
            var repository = issues[0].Repository;

            List<string> RowKeysToScan = new List<string>();
            foreach (var issue in issues) {
                if (issue.Organization != organization || issue.Repository != repository)
                    throw new ApplicationException("Can process only issues from the same repository");
                RowKeysToScan.Add(issue.GetRowKey());
            }

            try
            {

                var issuesFound = this.StorageService.GetBatchIssues(organization, repository, RowKeysToScan);

                foreach (var issue in issues)
                {
                    bool inArchive = false;
                    foreach (var issueFound in issuesFound) {
                        if (issue.GetPartitionKey() == issueFound.GetPartitionKey() && issue.GetRowKey() == issueFound.GetRowKey())
                            inArchive = true;
                    }
                    if (!inArchive)
                        result.Add(issue);
                }
                return result;
            }
            catch (Exception ex)
            {
                return new List<Models.Issue>();
            }

        }

        public async Task<IList<Models.FeedItem>> RemoveExisting(IList<FeedItem> issues)
        {
            List<FeedItem> result = new List<FeedItem>();
            if (issues == null || issues.Count == 0)
                return result;

            string partitionKey = issues[0].Feed;

            List<string> RowKeysToScan = new List<string>();
            foreach (var issue in issues)
            {
                if (issue.Feed != partitionKey)
                    throw new ApplicationException("Can process only posts from the same feed");
                RowKeysToScan.Add(issue.Id);
            }

            try
            {
                var issuesFound = this.StorageService.GetBatchWebLinks(partitionKey, RowKeysToScan);

                foreach (var issue in issues)
                {
                    bool inArchive = false;
                    foreach (var issueFound in issuesFound)
                    {
                        if (issue.Id == issueFound.Id )
                            inArchive = true;
                    }
                    if (!inArchive)
                        result.Add(issue);
                }
                return result;
            }
            catch (Exception ex)
            {
                return new List<FeedItem>();
            }

        }


        public async Task<IList<Models.TwittedIssue>> PublishNewIssues(IList<Models.Issue> issues) {
            if (issues == null || issues.Count == 0)
                return new List<Models.TwittedIssue>();
            return await this.TwitterService.SendIssues(issues);
        }

        public async Task StorePublishedIssues(IList<Models.TwittedIssue> twittedIssues) {
            if (twittedIssues == null || twittedIssues.Count == 0)
                return;
            await this.StorageService.Store(twittedIssues);
        }

        public async Task<int> ProcessRepository(string organization, string repository) {
            if (string.IsNullOrWhiteSpace(organization))
                throw new ArgumentNullException(nameof(organization), "organization must be specified");
            if (string.IsNullOrWhiteSpace(repository))
                throw new ArgumentNullException(nameof(repository), "repository must be specified");

            // get recent created or modified issues
            var issues = await RecentGitHubIssues(organization, repository);
            // if no issues are reported from github, go next repository
            if (issues == null || issues.Count == 0)
                return 0;
            // get the latest issues archived
            var lastStored = await CheckInStorage(organization, repository, issues);
            // check for updates
            var changed = await IssuesToUpdate(issues, lastStored);
            // if updated ones, merge the changes
            await Merge(changed);
            // remove from the list the ones already in the storage, keeping just the ones 
            // I have to tweet and store
            issues = await RemoveExisting(issues);
            // publish the new issues
            var twittedIssues = await PublishNewIssues(issues);
            // store in the storage the data about the new issues
            await StorePublishedIssues(twittedIssues);
            return twittedIssues.Count;

        }

        public async Task<IList<FeedItem>> RecentPosts(string feedUrl) {
            if (string.IsNullOrWhiteSpace(feedUrl))
                return new List<FeedItem>();

            return await this.FeedReaderService.ReadFeed(feedUrl);
        }

        public async Task<IList<TwittedPost>> PublishNewPosts(IList<FeedItem> posts)
        {
            if (posts == null || posts.Count == 0)
                return new List<TwittedPost>();
            return await this.TwitterService.SendPosts(posts);
        }

        public async Task StorePublishedPosts(IList<Models.TwittedPost> twittedPosts)
        {
            if (twittedPosts == null || twittedPosts.Count == 0)
                return;
            await this.StorageService.Store(twittedPosts);
        }

    }
}
