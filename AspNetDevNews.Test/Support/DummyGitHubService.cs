using AspNetDevNews.Models;
using AspNetDevNews.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AspNetDevNews.Test.Support
{
    public class DummyGitHubService : IGitHubService
    {
        public List<Issue> RecentIssues { get; set; }
        public async Task<IEnumerable<Issue>> GetRecentIssues(string organization, string repository, DateTimeOffset since)
        {
            return this.RecentIssues;
        }

        public Task<IEnumerable<string>> Repositories(string organization)
        {
            throw new NotImplementedException();
        }

        public Task GetRecentReleases(string organization, string repository)
        {
            throw new NotImplementedException();
        }

        public DummyGitHubService() {
            RecentIssues = new List<Issue>();
        }
    }
}
