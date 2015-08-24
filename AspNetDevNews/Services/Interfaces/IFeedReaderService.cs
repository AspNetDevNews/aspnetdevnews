using AspNetDevNews.Services.Feeds;
using AspNetDevNews.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AspNetDevNews.Services.Interfaces
{
    public interface IFeedReaderService
    {
        Task<IList<FeedItem>> ReadFeed( string feedUrl );
    }
}
