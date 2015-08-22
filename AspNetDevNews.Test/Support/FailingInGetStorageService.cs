using AspNetDevNews.Models;
using AspNetDevNews.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AspNetDevNews.Test.Support
{
    public class FailingInGetStorageService : IStorageService
    {
        public FailingInGetStorageService() {
            Existing = new List<Issue>();
            RecentIssues = new List<Issue>();
            Cleaned = new List<Issue>();
        }
        public List<Issue> RecentIssues { get; set; }
        public List<Issue> Cleaned { get; set; }
        public List<Issue> Existing { get; set; }
        public Task<bool> Exists(TwittedIssue issue)
        {
            throw new NotImplementedException();
        }

        public async Task<IList<Issue>> GetRecentIssues(string organization, string repository, DateTimeOffset since)
        {
            return RecentIssues;
        }
        public async Task<IList<Issue>> GetBatchIssues(string organization, string repository, IList<string> rowKeys)
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

        public async Task Store(IList<TwittedIssue> issues)
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
