using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AspNetDevNews.Services.Interfaces
{
    public interface IStorageService
    {
        Task Store(IList<Models.TwittedIssue> issues);
        Task Merge(IList<Models.Issue> issues);
        Task Store(Exception exception, Models.Issue issue, string operation);

        Task ReportExection(DateTime StartedAt, DateTime EndedAt, int TwittedIssues, int CheckedRepositories);
        Task<IList<Models.Issue>> GetBatchIssues(string organization, string repository, IList<string> rowKeys);
        Task<bool> Exists(Models.TwittedIssue issue);
        Task<IList<Models.Issue>> GetRecentIssues(string organization, string repository, DateTimeOffset since);
    }
}
