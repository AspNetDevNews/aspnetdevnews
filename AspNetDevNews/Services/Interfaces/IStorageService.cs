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
        Task Store(IList<Models.TwittedGitHubHostedDocument> documents);

        Task Merge(IList<Models.Issue> issues);

        //Task Store(Exception exception, Models.Issue issue, string operation);
        //Task Store(Exception exception, Models.FeedItem post, string operation);
        //Task Store(Exception exception, Models.GitHubHostedDocument post, string operation);
        Task Store(Exception exception, Interfaces.ITableStorageKeyGet record, string operation);


        Task ReportExecution(DateTime StartedAt, DateTime EndedAt, int TwittedIssues, int CheckedRepositories, int UpdatedIssues, int postedLinks);
        Task<bool> Exists(Models.TwittedIssue issue);
        IList<Models.Issue> GetRecentIssues(string organization, string repository, DateTimeOffset since);

        IList<Models.Issue> GetBatchIssues(string organization, string repository, IList<string> rowKeys);
        IList<Models.FeedItem> GetBatchWebLinks(string feed, IList<string> rowKeys);
        IList<Models.GitHubHostedDocument> GetBatchDocuments(string organization, string repository, IList<string> rowKeys);
    }
}
