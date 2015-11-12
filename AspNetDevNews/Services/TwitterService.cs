using AspNetDevNews.Models;
using AspNetDevNews.Services.Interfaces;
using LinqToTwitter;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AspNetDevNews.Services
{
    public class TwitterService: ITwitterService
    {
        public TwitterService(ISettingsService settings, IStorageService storage, ISessionLogger logger) {
            if (settings == null)
                throw new ArgumentNullException(nameof(settings), "cannot be null");
            if (storage == null)
                throw new ArgumentNullException(nameof(storage), "cannot be null");
            if (logger == null)
                throw new ArgumentNullException(nameof(logger), "cannot be null");

            this.Settings = settings;
            this.Storage = storage;
            this.Logger = logger;
        }

        private ISettingsService Settings { get; set; } 
        private IStorageService Storage { get; set; }
        private ISessionLogger Logger { get; set; }

        public async Task<IList<TwittedIssue>> Send(IList<Models.Issue> issues) {
            return await Send<TwittedIssue, Issue>(issues, "SendIssues");
        }

        public async Task<IList<TwittedPost>> Send( IList<FeedItem> links) {
            return await Send<TwittedPost, FeedItem>(links, "SendPosts");
        }

        public async Task<IList<TwittedGitHubHostedDocument>> Send(IList<Models.GitHubHostedDocument> docs)
        {
            return await Send<TwittedGitHubHostedDocument, GitHubHostedDocument>(docs, "SendDocuments");
        }

        protected virtual TwitterContext GetTwitterContext() {
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
            return new TwitterContext(authorizer);
        }

        private async Task<IList<TwittedType>> Send<TwittedType, ContentType>(IList<ContentType> docs, string operation) 
            where ContentType : ITableStorageKeyGet, IIsTweetable
            where TwittedType : IHasTweetInfo
        {
            if (string.IsNullOrWhiteSpace(operation))
                return new List<TwittedType>();
            if (docs == null || docs.Count == 0)
                return new List<TwittedType>();

            using (var twitterCtx = GetTwitterContext())
            {
                List<TwittedType> twittedIssues = new List<TwittedType>();
                List<string> twittedMessages = new List<string>();

                this.Logger.AddMessage("SendTweet","documents to tweet: " + docs.Count, string.Empty, MessageType.Info);

                foreach (var document in docs)
                {
                    try
                    {
                        var message = document.GetTwitterText();
                        if (twittedMessages.Contains(message))
                            continue;
                        var tweet = await twitterCtx.TweetAsync(message);
                        //var tweet = new Status();
                        //tweet.StatusID = 11;

                        this.Logger.AddMessage("SendTweet", "tweeted : " + document.GetPartitionKey() + " "
                            + document.GetRowKey() + " " + document.GetTwitterText(), string.Empty, MessageType.Info);

                        var twittedIssue = AutoMapper.Mapper.Map<TwittedType>(document);
                        twittedIssue.StatusID = tweet.StatusID;

                        twittedIssues.Add(twittedIssue);

                        twittedMessages.Add(message);
                    }
                    catch (Exception exc)
                    {
                        this.Logger.AddMessage("Send", $"exception {exc.Message} while posting To Twitter", operation, MessageType.Error);

                        await Storage.Store(exc, document, operation);
                    }
                }
                return twittedIssues;
            }
        }
    }
}
