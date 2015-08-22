using AspNetDevNews.Models;
using AspNetDevNews.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AspNetDevNews.Test.Support
{
    public class DummyStorageService : IStorageService
    {
        public List<Issue> RecentIssues { get; set; }
        public List<Issue> Cleaned { get; set; }
        public Task<bool> Exists(TwittedIssue issue)
        {
            throw new NotImplementedException();
        }

        public async Task<IList<Issue>> GetRecentIssues(string organization, string repository, DateTimeOffset since)
        {
            return RecentIssues;
        }

        public Task<IList<Issue>> RemoveExisting(IList<Issue> issues)
        {
            throw new NotImplementedException();
        }

        #region Store methods do nothing
        public async Task ReportExection(DateTime StartedAt, DateTime EndedAt, int TwittedIssues, int CheckedRepositories)
        {
            return;
        }

        public async Task Merge(IList<Issue> issues)
        {
            return;
        }

        public async Task Store(List<TwittedIssue> issues)
        {
            return;
        }

        public async Task Store(Exception exception, Issue issue, string operation)
        {
            return;
        }

        #endregion
    }

}
