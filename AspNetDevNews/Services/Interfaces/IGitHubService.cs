using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AspNetDevNews.Services.Interfaces
{
    public interface IGitHubService
    {
        Task<IEnumerable<string>> Repositories(string organization);
        Task<IEnumerable<Models.Issue>> GetRecentIssues(string organization, string repository, DateTimeOffset since);
        Task<IList<Models.GitHubHostedDocument>> ExtractCommitDocuments(string organization, string repository);
    }
}