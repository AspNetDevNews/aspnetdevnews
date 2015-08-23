using AspNetDevNews.Models;
using AspNetDevNews.Services;
using AspNetDevNews.Services.Interfaces;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AspNetDevNews.Services.AzureTableStorage
{
    public class ReadOnlyAzureStorageTableService : IStorageService
    {
        private AzureTableStorageService AzureStorage { get; set;  }
        public ReadOnlyAzureStorageTableService() {
            Existing = new List<Issue>();
            RecentIssues = new List<Issue>();
            Cleaned = new List<Issue>();
            AzureStorage = new AzureTableStorageService();
        }
        public List<Issue> RecentIssues { get; set; }
        public List<Issue> Cleaned { get; set; }
        public List<Issue> Existing { get; set; }
        public async Task<bool> Exists(TwittedIssue issue)
        {
            return await AzureStorage.Exists(issue);
        }

        public async Task<IList<Issue>> GetRecentIssues(string organization, string repository, DateTimeOffset since)
        {
            return await AzureStorage.GetRecentIssues(organization, repository, since );
        }
        public async Task<IList<Issue>> GetBatchIssues(string organization, string repository, IList<string> rowKeys)
        {
            return await AzureStorage.GetBatchIssues(organization, repository, rowKeys);
        }

        #region Store methods do nothing
        public async Task ReportExecution(DateTime StartedAt, DateTime EndedAt, int TwittedIssues, int CheckedRepositories, int updatedIssues)
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
