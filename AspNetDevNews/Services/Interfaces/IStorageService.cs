using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AspNetDevNews.Services.Interfaces
{
    public interface IStorageService
    {
        Task Store(List<Models.TwittedIssue> issues);
        Task Store(Exception exception, Models.Issue issue, string operation);
        Task ReportExection(DateTime StartedAt, DateTime EndedAt, int TwittedIssues, int CheckedRepositories);
        Task<List<Models.Issue>> RemoveExisting(List<Models.Issue> issues);
        Task<bool> Exists(Models.TwittedIssue issue);
    }
}
