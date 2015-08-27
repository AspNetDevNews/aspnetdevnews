using AspNetDevNews.Models;
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
        private ISessionLogger LoggerService { get; set; }


        // remove it
        public IssueReceiveService()
        {
        }

        public IssueReceiveService(IGitHubService gitHubService, IStorageService storageService, ISettingsService settingsService, 
            ITwitterService twitterService, IFeedReaderService feedReaderService, ISessionLogger loggerService) {
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
            if (loggerService == null)
                throw new ArgumentNullException(nameof(loggerService), "loggerService cannot be null");

            this.GitHubService = gitHubService;
            this.StorageService = storageService;
            this.SettingsService = settingsService;
            this.TwitterService = twitterService;
            this.FeedReaderService = feedReaderService;
            this.LoggerService = loggerService;
        }

        public IEnumerable<string> Organizations {
            get { return new List<string> { "aspnet", "nuget" }; }
        }

        public IEnumerable<string> Feeds {
            get { return new List<string> { "http://webdevblogs.azurewebsites.net/master.xml" }; }
        }

        public IList<Issue> IssuesToUpdate(IList<Issue> issues, IList<Issue> lastStored)
        {
            List<Models.Issue> toUpdate = new List<Models.Issue>();

            if ((issues == null) || (lastStored == null))
                return toUpdate;

            foreach (var stgIssue in lastStored) {
                foreach (var githubIssue in issues)
                {
                    if (stgIssue.GetPartitionKey() == githubIssue.GetPartitionKey() && stgIssue.GetRowKey() == githubIssue.GetRowKey()) {
                        bool updated = false;

                        if (stgIssue.Title != githubIssue.Title) {
                            this.LoggerService.AddMessage("IssuesToUpdate", 
                                $"update in property Title of issue {stgIssue.Organization} {stgIssue.Repository} {stgIssue.Number}", 
                                $"storage Value : {stgIssue.Title} github value : {githubIssue.Title}", MessageType.Warning);
                            updated = true;
                        }
                        else if (stgIssue.UpdatedAt != githubIssue.UpdatedAt) {
                            this.LoggerService.AddMessage("IssuesToUpdate",
                                $"update in property UpdatedAt of issue {stgIssue.Organization} {stgIssue.Repository} {stgIssue.Number}",
                                $"storage Value : {stgIssue.UpdatedAt} github value : {githubIssue.UpdatedAt}", MessageType.Warning);
                            updated = true;
                        }
                        else if (stgIssue.State != githubIssue.State) {
                            this.LoggerService.AddMessage("IssuesToUpdate",
                                $"update in property State of issue {stgIssue.Organization} {stgIssue.Repository} {stgIssue.Number}",
                                $"storage Value : {stgIssue.State} github value : {githubIssue.State}", MessageType.Warning);
                            updated = true;
                        }
                        else if (stgIssue.Comments != githubIssue.Comments) {
                            this.LoggerService.AddMessage("IssuesToUpdate",
                                $"update in property Comments of issue {stgIssue.Organization} {stgIssue.Repository} {stgIssue.Number}",
                                $"storage Value : {stgIssue.Comments} github value : {githubIssue.Comments}", MessageType.Warning);
                            updated = true;
                        }

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

        public IList<GitHubRepo> DocsRepo {
            get {
                List<GitHubRepo> repos = new List<GitHubRepo>();
                repos.Add(new GitHubRepo { Organization = "aspnet", Repository = "Docs" });
                repos.Add(new GitHubRepo { Organization = "aspnet", Repository = "EntityFramework.Docs" });
                repos.Add(new GitHubRepo { Organization = "dotnet", Repository = "core-docs" });
                return repos;
            }
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

        #region check for new contents
        public async Task<IList<Issue>> RecentGitHubIssues(string organization, string repository)
        {
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
                this.LoggerService.AddMessage("RecentGitHubIssues", 
                    "found " + issuesToProcess.Count + " issues", string.Empty, MessageType.Info);
                return issuesToProcess;
            }
            catch (Exception exc)
            {
                this.LoggerService.AddMessage("RecentGitHubIssues", "exception " + exc.Message + " while receiving issues", string.Empty, MessageType.Error);
                await this.StorageService.Store(exc, null, "RecentIssues");
                return new List<Models.Issue>();
            }
        }

        public async Task<IList<FeedItem>> RecentPosts(string feedUrl)
        {
            if (string.IsNullOrWhiteSpace(feedUrl))
                return new List<FeedItem>();
            var feedItems = await this.FeedReaderService.ReadFeed(feedUrl);

            this.LoggerService.AddMessage("RecentPosts",
                "found " + feedItems.Count + " posts", string.Empty, MessageType.Info);

            return feedItems;
        }

        public async Task<IList<GitHubHostedDocument>> RecentGitHubDocuments(string organization, string repository)
        {
            if (string.IsNullOrWhiteSpace(organization))
                throw new ArgumentNullException(nameof(organization), "organization must be specified");
            if (string.IsNullOrWhiteSpace(repository))
                throw new ArgumentNullException(nameof(repository), "repository must be specified");

            var documents = await this.GitHubService.ExtractCommitDocuments(organization, repository);

            this.LoggerService.AddMessage("RecentGitHubDocuments",
                "found " + documents.Count + " documents", string.Empty, MessageType.Info);

            return documents;
        }

        #endregion

        public IList<Issue> CheckInStorage(string organization, string repository, IList<Issue> issuesToCheck)
        {
            if (string.IsNullOrWhiteSpace(organization))
                throw new ArgumentNullException(nameof(organization), "organization must be specified");
            if (string.IsNullOrWhiteSpace(repository))
                throw new ArgumentNullException(nameof(repository), "repository must be specified");
            if (issuesToCheck == null || issuesToCheck.Count == 0)
                return new List<Models.Issue>();

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
            catch (Exception exc)
            {
                this.LoggerService.AddMessage("CheckInStorage", "exception " + exc.Message + " while reading from storage", string.Empty, MessageType.Error);
                return new List<Models.Issue>();
            }

        }

        public async Task Merge(IList<Issue> issues) {
            if (issues == null || issues.Count == 0)
                return; 
            await this.StorageService.Merge(issues);
        }

        #region remove from the list of the new contents the ones already in archive
        public IList<Models.Issue> RemoveExisting(IList<Issue> issues)
        {
            List<Models.Issue> result = new List<Models.Issue>();
            if (issues == null || issues.Count == 0)
                return result;

            var organization = issues[0].Organization;
            var repository = issues[0].Repository;

            List<string> RowKeysToScan = new List<string>();
            foreach (var issue in issues)
            {
                if (issue.Organization != organization || issue.Repository != repository)
                    throw new ApplicationException("Can process only issues from the same repository");
                RowKeysToScan.Add(issue.GetRowKey());
            }

            try
            {

                var issuesFound = this.StorageService.GetBatchIssues(organization, repository, RowKeysToScan);

                return RemoveStoredItems(issues, issuesFound);

                //foreach (var issue in issues)
                //{
                //    bool inArchive = false;
                //    foreach (var issueFound in issuesFound)
                //    {
                //        if (issue.GetPartitionKey() == issueFound.GetPartitionKey() && issue.GetRowKey() == issueFound.GetRowKey())
                //            inArchive = true;
                //    }
                //    if (!inArchive)
                //        result.Add(issue);
                //}
                //return result;
            }
            catch (Exception exc)
            {
                this.LoggerService.AddMessage("RemoveExisting", "exception " + exc.Message + " while receiving issues", "Issue", MessageType.Error);
                return new List<Models.Issue>();
            }

        }

        public IList<Models.FeedItem> RemoveExisting(IList<FeedItem> issues)
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

                return RemoveStoredItems(issues, issuesFound);

                //foreach (var issue in issues)
                //{
                //    bool inArchive = false;
                //    foreach (var issueFound in issuesFound)
                //    {
                //        if (issue.Id == issueFound.Id)
                //            inArchive = true;
                //    }
                //    if (!inArchive)
                //        result.Add(issue);
                //}
                //return result;
            }
            catch (Exception exc)
            {
                this.LoggerService.AddMessage("RemoveExisting", "exception " + exc.Message + " while receiving issues", "FeedItem", MessageType.Error);
                return new List<FeedItem>();
            }

        }

        public IList<Models.GitHubHostedDocument> RemoveExisting(IList<GitHubHostedDocument> issues)
        {
            List<GitHubHostedDocument> result = new List<GitHubHostedDocument>();
            if (issues == null || issues.Count == 0)
                return result;

            string organization = issues[0].Organization;
            string repository = issues[0].Repository;

            List<string> RowKeysToScan = new List<string>();
            foreach (var issue in issues)
            {
                if (issue.Repository != repository && issue.Organization != organization)
                    throw new ApplicationException("Can process only posts from the same repository");
                RowKeysToScan.Add(issue.GetRowKey());
            }

            try
            {
                var issuesFound = this.StorageService.GetBatchDocuments(organization, repository, RowKeysToScan);

                return RemoveStoredItems(issues, issuesFound);
                //foreach (var issue in issues)
                //{
                //    bool inArchive = false;
                //    foreach (var issueFound in issuesFound)
                //    {
                //        if (issue.GetRowKey() == issueFound.GetRowKey())
                //            inArchive = true;
                //    }
                //    if (!inArchive)
                //        result.Add(issue);
                //}
                //return result;
            }
            catch (Exception exc)
            {
                this.LoggerService.AddMessage("RemoveExisting", "exception " + exc.Message + " while receiving issues", "GitHubHostedDocument", MessageType.Error);
                return new List<GitHubHostedDocument>();
            }

        }

        private IList<T> RemoveStoredItems<T>(IList<T> incomingElements, IList<T> storedElements) where T : ITableStorageKeyGet
        {
            var result = new List<T>();
            foreach (var issue in incomingElements)
            {
                bool inArchive = false;
                foreach (var issueFound in storedElements)
                {
                    if (issue.GetRowKey() == issueFound.GetRowKey())
                        inArchive = true;
                }
                if (!inArchive)
                    result.Add(issue);
            }
            return result;

        }

        #endregion

        #region publish to twitter
        public async Task<IList<Models.TwittedIssue>> Publish(IList<Models.Issue> issues)
        {
            if (issues == null || issues.Count == 0)
                return new List<Models.TwittedIssue>();
            return await this.TwitterService.Send(issues);
        }

        public async Task<IList<TwittedPost>> Publish(IList<FeedItem> posts)
        {
            if (posts == null || posts.Count == 0)
                return new List<TwittedPost>();
            return await this.TwitterService.Send(posts);
        }

        public async Task<IList<TwittedGitHubHostedDocument>> Publish(IList<GitHubHostedDocument> posts)
        {
            if (posts == null || posts.Count == 0)
                return new List<TwittedGitHubHostedDocument>();
            return await this.TwitterService.Send(posts);
        }

        #endregion

        #region store published items
        public async Task Store(IList<Models.TwittedIssue> twittedIssues)
        {
            if (twittedIssues == null || twittedIssues.Count == 0)
                return;
            await this.StorageService.Store(twittedIssues);
        }

        public async Task Store(IList<Models.TwittedPost> twittedPosts)
        {
            if (twittedPosts == null || twittedPosts.Count == 0)
                return;
            await this.StorageService.Store(twittedPosts);
        }

        public async Task Store(IList<Models.TwittedGitHubHostedDocument> twittedDocuments)
        {
            if (twittedDocuments == null || twittedDocuments.Count == 0)
                return;
            await this.StorageService.Store(twittedDocuments);
        }

        #endregion

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
            var lastStored = CheckInStorage(organization, repository, issues);
            // check for updates
            var changed = IssuesToUpdate(issues, lastStored);
            // if updated ones, merge the changes
            await Merge(changed);
            // remove from the list the ones already in the storage, keeping just the ones 
            // I have to tweet and store
            issues = RemoveExisting(issues);
            // publish the new issues
            var twittedIssues = await Publish(issues);
            // store in the storage the data about the new issues
            await Store(twittedIssues);
            return twittedIssues.Count;

        }

    }
}
