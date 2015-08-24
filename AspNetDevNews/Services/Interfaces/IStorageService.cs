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
        Task Store(IList<Models.TwittedPost> posts);

        Task Merge(IList<Models.Issue> issues);
        Task Store(Exception exception, Models.Issue issue, string operation);
        Task Store(Exception exception, string feedItem, Models.FeedItem post, string operation);

        Task ReportExecution(DateTime StartedAt, DateTime EndedAt, int TwittedIssues, int CheckedRepositories, int UpdatedIssues, int postedLinks);
        IList<Models.Issue> GetBatchIssues(string organization, string repository, IList<string> rowKeys);
        Task<bool> Exists(Models.TwittedIssue issue);
        Task<IList<Models.Issue>> GetRecentIssues(string organization, string repository, DateTimeOffset since);
        IList<Models.FeedItem> GetBatchWebLinks(string feed, IList<string> rowKeys);
    }
}
