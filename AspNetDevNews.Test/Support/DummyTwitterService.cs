using AspNetDevNews.Models;
using AspNetDevNews.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AspNetDevNews.Test.Support
{
    public class DummyTwitterService : ITwitterService
    {
        public List<TwittedIssue> Sent { get; set; }
        public async Task<IList<TwittedIssue>> SendIssues(IList<Issue> issues)
        {
           return Sent;
        }

        public Task<IList<FeedItem>> SendPosts(IList<FeedItem> links)
        {
            throw new NotImplementedException();
        }

        Task<IList<TwittedPost>> ITwitterService.SendPosts(IList<FeedItem> links)
        {
            throw new NotImplementedException();
        }

        public DummyTwitterService() {
            Sent = new List<TwittedIssue>();
        }
    }
}
