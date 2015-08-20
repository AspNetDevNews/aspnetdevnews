﻿using AspNetDevNews.Models;
using AspNetDevNews.Services.Interfaces;
using LinqToTwitter;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;

namespace AspNetDevNews.Services
{
    public class TwitterService
    {
        public TwitterService() {
            this.Settings = new SettingsService();
            this.Storage = new AzureTableStorageService();
        }

        public TwitterService(ISettingsService settings, IStorageService storage) {
            this.Settings = settings;
            this.Storage = storage;
        }

        private ISettingsService Settings { get; set; } 
        private IStorageService Storage { get; set; }

        public async Task<List<TwittedIssue>> SendIssues(IEnumerable<Models.Issue> issues) {

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
                    string title = issue.Title.Trim();
                    if (!title.EndsWith("."))
                        title += ".";
                    string test = "[" + issue.Repository + "]: " + title + " " + issue.Url;

                    try
                    {
                        var tweet = await twitterCtx.TweetAsync(test);

                        var twittedIssue = new TwittedIssue();

                        twittedIssue.Title = issue.Title;
                        twittedIssue.Url = issue.Url;
                        twittedIssue.Labels = issue.Labels;
                        twittedIssue.Organization = issue.Organization;
                        twittedIssue.Repository = issue.Repository;
                        twittedIssue.CreatedAt = issue.CreatedAt;
                        twittedIssue.Number = issue.Number;
                        twittedIssue.UpdatedAt = issue.UpdatedAt;

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
    }
}
