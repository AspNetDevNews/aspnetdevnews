using AspNetDevNews.Models;
using AspNetDevNews.Services.Interfaces;
using AspNetDevNews.Services.AzureTableStorage;
using LinqToTwitter;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;

namespace AspNetDevNews.Services
{
    public class TwitterService: ITwitterService
    {
        public TwitterService() {
            this.Settings = new SettingsService();
            this.Storage = new AzureTableStorageService();
        }

        public TwitterService(ISettingsService settings, IStorageService storage) {
            if (Settings == null)
                throw new ArgumentNullException("Settings cannot be null");
            if (Storage == null)
                throw new ArgumentNullException("Storage cannot be null");

            this.Settings = settings;
            this.Storage = storage;
        }

        private ISettingsService Settings { get; set; } 
        private IStorageService Storage { get; set; }

        public async Task<IList<TwittedIssue>> SendIssues(IList<Models.Issue> issues) {

            var authorizer = new SingleUserAuthorizer
                 {
                     CredentialStore = new SingleUserInMemoryCredentialStore
                     {
                         ConsumerKey = this.Settings.TwitterConsumerKey,
                         ConsumerSecret = this.Settings.TwitterConsumerSecret,
                         AccessToken = this.Settings.TwitterAccessToken,
                         AccessTokenSecret = this.Settings.TwitterAccessTokenSecret
                     }
                 };
            using (var twitterCtx = new TwitterContext(authorizer))
            {
                List<TwittedIssue> twittedIssues = new List<TwittedIssue>();

                foreach (var issue in issues) {
                    try
                    {
                        var tweet = await twitterCtx.TweetAsync(issue.GetTwitterText());

                        // OK
                        //twittedIssue.Title = issue.Title;
                        //twittedIssue.Url = issue.Url;
                        //twittedIssue.Labels = issue.Labels;
                        //twittedIssue.Organization = issue.Organization;
                        //twittedIssue.Repository = issue.Repository;
                        //twittedIssue.CreatedAt = issue.CreatedAt;
                        //twittedIssue.Number = issue.Number;
                        //twittedIssue.UpdatedAt = issue.UpdatedAt;
                        //twittedIssue.StatusID = tweet.StatusID;
                        //twittedIssue.Body = issue.Body;
                        //twittedIssue.Comments = issue.Comments;

                        var twittedIssue = AutoMapper.Mapper.Map<TwittedIssue>(issue);
                        twittedIssue.StatusID = tweet.StatusID;

                        twittedIssues.Add(twittedIssue);
                    }
                    catch (Exception exc) {
                        await Storage.Store(exc, issue, "SendIssues");
                    }
                }
                return twittedIssues;
            }
        }

        public async Task<IList<TwittedPost>> SendPosts( IList<FeedItem> links) {
            var authorizer = new SingleUserAuthorizer
            {
                CredentialStore = new SingleUserInMemoryCredentialStore
                {
                    ConsumerKey = this.Settings.TwitterConsumerKey,
                    ConsumerSecret = this.Settings.TwitterConsumerSecret,
                    AccessToken = this.Settings.TwitterAccessToken,
                    AccessTokenSecret = this.Settings.TwitterAccessTokenSecret
                }
            };
            using (var twitterCtx = new TwitterContext(authorizer))
            {
                List<TwittedPost> twittedIssues = new List<TwittedPost>();

                foreach (var post in links)
                {
                    try
                    {
                        var tweet = await twitterCtx.TweetAsync(post.GetTwitterText());
                        // OK
                        //var twittedIssue = new TwittedPost();
                        //twittedIssue.Title = post.Title;
                        //twittedIssue.Id = post.Id;
                        //twittedIssue.PublishDate = post.PublishDate;
                        //twittedIssue.Summary = post.Summary;
                        //twittedIssue.Title = post.Title;
                        //twittedIssue.StatusID = tweet.StatusID;
                        //twittedIssue.Feed = post.Feed;

                        var twittedIssue = AutoMapper.Mapper.Map<TwittedPost>(post);
                        twittedIssue.StatusID = tweet.StatusID;

                        twittedIssues.Add(twittedIssue);
                    }
                    catch (Exception exc)
                    {
                        await Storage.Store(exc, post.Feed, post, "SendPosts");
                    }
                }
                return twittedIssues;
            }
        }
    }
}
