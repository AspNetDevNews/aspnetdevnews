using AspNetDevNews.Models;
using LinqToTwitter;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AspNetDevNews.Services
{
    public class TwitterService
    {

        public async Task<List<TwittedIssue>> SendIssues(IEnumerable<Models.Issue> issues) {

            var authorizer = new SingleUserAuthorizer
                 {
                     CredentialStore = new SingleUserInMemoryCredentialStore
                     {
                         ConsumerKey = ConfigurationSettings.AppSettings["TwitterConsumerKey"],
                         ConsumerSecret = ConfigurationSettings.AppSettings["TwitterConsumerSecret"],
                         AccessToken = ConfigurationSettings.AppSettings["TwitterAccessToken"],
                         AccessTokenSecret = ConfigurationSettings.AppSettings["TwitterAccessTokenSecret"]
                     }
                 };
            using (var twitterCtx = new TwitterContext(authorizer))
            {
                List<TwittedIssue> twittedIssues = new List<TwittedIssue>();

                foreach (var issue in issues) {
                    string test = "[" + issue.Repository + "] [" + issue.Title + "] " + issue.Url;

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
                        var stgService = new ATStorageService();
                        stgService.Store(exc, issue, "SendIssues");
                    }
                    //tweet.
                }
                //Console.WriteLine(tweet.StatusID);
                return twittedIssues;
}

        }
    }
}
