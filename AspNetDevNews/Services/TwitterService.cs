﻿using AspNetDevNews.Models;
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
                throw new ArgumentNullException(nameof(settings), "Settings cannot be null");
            if (storage == null)
                throw new ArgumentNullException(nameof(storage), "Storage cannot be null");
            if (logger == null)
                throw new ArgumentNullException(nameof(logger), "logger cannot be null");

            this.Settings = settings;
            this.Storage = storage;
            this.Logger = logger;
        }

        private ISettingsService Settings { get; set; } 
        private IStorageService Storage { get; set; }
        private ISessionLogger Logger { get; set; }

        public async Task<IList<TwittedIssue>> Send(IList<Models.Issue> issues) {
            return await Send<TwittedIssue, Issue>(issues, "SendIssues");

            //var authorizer = new SingleUserAuthorizer
            //     {
            //         CredentialStore = new SingleUserInMemoryCredentialStore
            //         {
            //             ConsumerKey = this.Settings.TwitterConsumerKey,
            //             ConsumerSecret = this.Settings.TwitterConsumerSecret,
            //             AccessToken = this.Settings.TwitterAccessToken,
            //             AccessTokenSecret = this.Settings.TwitterAccessTokenSecret
            //         }
            //     };
            //using (var twitterCtx = new TwitterContext(authorizer))
            //{
            //    List<TwittedIssue> twittedIssues = new List<TwittedIssue>();
            //    List<string> twittedMessages = new List<string>();

            //    foreach (var issue in issues) {
            //        try
            //        {
            //            var message = issue.GetTwitterText();
            //            if (twittedMessages.Contains(message))
            //                continue;

            //            var tweet = await twitterCtx.TweetAsync(issue.GetTwitterText());

            //            var twittedIssue = AutoMapper.Mapper.Map<TwittedIssue>(issue);
            //            twittedIssue.StatusID = tweet.StatusID;

            //            twittedIssues.Add(twittedIssue);

            //            twittedMessages.Add(message);
            //        }
            //        catch (Exception exc) {
            //            await Storage.Store(exc, issue, "SendIssues");
            //        }
            //    }
            //    return twittedIssues;
            //}
        }

        public async Task<IList<TwittedPost>> Send( IList<FeedItem> links) {
            return await Send<TwittedPost, FeedItem>(links, "SendPosts");

            //var authorizer = new SingleUserAuthorizer
            //{
            //    CredentialStore = new SingleUserInMemoryCredentialStore
            //    {
            //        ConsumerKey = this.Settings.TwitterConsumerKey,
            //        ConsumerSecret = this.Settings.TwitterConsumerSecret,
            //        AccessToken = this.Settings.TwitterAccessToken,
            //        AccessTokenSecret = this.Settings.TwitterAccessTokenSecret
            //    }
            //};
            //using (var twitterCtx = new TwitterContext(authorizer))
            //{
            //    List<TwittedPost> twittedIssues = new List<TwittedPost>();
            //    List<string> twittedMessages = new List<string>();

            //    foreach (var post in links)
            //    {
            //        try
            //        {
            //            var message = post.GetTwitterText();
            //            if (twittedMessages.Contains(message))
            //                continue;

            //            var tweet = await twitterCtx.TweetAsync(post.GetTwitterText());

            //            var twittedIssue = AutoMapper.Mapper.Map<TwittedPost>(post);
            //            twittedIssue.StatusID = tweet.StatusID;

            //            twittedIssues.Add(twittedIssue);

            //            twittedMessages.Add(message);
            //        }
            //        catch (Exception exc)
            //        {
            //            await Storage.Store(exc, post, "SendPosts");
            //        }
            //    }
            //    return twittedIssues;
            //}
        }

        public async Task<IList<TwittedGitHubHostedDocument>> Send(IList<Models.GitHubHostedDocument> docs)
        {
            return await Send<TwittedGitHubHostedDocument, GitHubHostedDocument>(docs, "SendDocuments");

            //var authorizer = new SingleUserAuthorizer
            //{
            //    CredentialStore = new SingleUserInMemoryCredentialStore
            //    {
            //        ConsumerKey = this.Settings.TwitterConsumerKey,
            //        ConsumerSecret = this.Settings.TwitterConsumerSecret,
            //        AccessToken = this.Settings.TwitterAccessToken,
            //        AccessTokenSecret = this.Settings.TwitterAccessTokenSecret
            //    }
            //};
            //using (var twitterCtx = new TwitterContext(authorizer))
            //{
            //    List<TwittedGitHubHostedDocument> twittedIssues = new List<TwittedGitHubHostedDocument>();
            //    List<string> twittedMessages = new List<string>();

            //    foreach (var document in docs)
            //    {
            //        try
            //        {
            //            var message = document.GetTwitterText();
            //            if (twittedMessages.Contains(message))
            //                continue;

            //            var tweet = await twitterCtx.TweetAsync(document.GetTwitterText());

            //            var twittedIssue = AutoMapper.Mapper.Map<TwittedGitHubHostedDocument>(document);
            //            twittedIssue.StatusID = tweet.StatusID;

            //            twittedIssues.Add(twittedIssue);

            //            twittedMessages.Add(message);
            //        }
            //        catch (Exception exc)
            //        {
            //            await Storage.Store(exc, document, "SendDocuments");
            //        }
            //    }
            //    return twittedIssues;
            //}
        }

        public TwitterContext GetTwitterContext() {
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
            using (var twitterCtx = GetTwitterContext())
            {
                List<TwittedType> twittedIssues = new List<TwittedType>();
                List<string> twittedMessages = new List<string>();

                foreach (var document in docs)
                {
                    try
                    {
                        var message = document.GetTwitterText();
                        if (twittedMessages.Contains(message))
                            continue;
                        var tweet = await twitterCtx.TweetAsync(message);

                        var twittedIssue = AutoMapper.Mapper.Map<TwittedType>(document);
                        twittedIssue.StatusID = tweet.StatusID;

                        twittedIssues.Add(twittedIssue);

                        twittedMessages.Add(message);
                    }
                    catch (Exception exc)
                    {
                        this.Logger.AddMessage("Send", "exception " + exc.Message + " while posting To Twitter", operation, MessageType.Error);

                        await Storage.Store(exc, document, operation);
                    }
                }
                return twittedIssues;
            }
        }


    }
}
