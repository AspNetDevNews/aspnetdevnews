using AspNetDevNews.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AspNetDevNews.Services.Interfaces
{
    public interface ITwitterService
    {
        Task<IList<TwittedIssue>> Send(IList<Models.Issue> issues);
        Task<IList<TwittedPost>> Send(IList<Models.FeedItem> links);
        Task<IList<TwittedGitHubHostedDocument>> Send(IList<Models.GitHubHostedDocument> docs);
    }
}
