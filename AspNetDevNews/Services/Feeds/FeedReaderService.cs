using AspNetDevNews.Services.Interfaces;
using AspNetDevNews.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.ServiceModel.Syndication;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace AspNetDevNews.Services.Feeds
{
    public class FeedReaderService: IFeedReaderService
    {
        private ISettingsService Settings { get; set; }
        private ISessionLogger Logger { get; set; }

        public FeedReaderService(ISettingsService settings, ISessionLogger logger) {
            if (settings == null)
                throw new ArgumentNullException(nameof(settings), "Settings cannot be null");
            if (logger == null)
                throw new ArgumentNullException(nameof(logger), "logger cannot be null");

            Settings = settings;
            Logger = logger;
        }

        public async Task<IList<FeedItem>> ReadFeed(string feedUrl)
        {
            SyndicationFeed feed = await DownloadFeed(feedUrl);

            List<FeedItem> result = new List<FeedItem>();
            foreach (var item in feed.Items) {
                try
                {
                    var mioItem = AutoMapper.Mapper.Map<FeedItem>(item);
                    mioItem.Feed = feedUrl;

                    if (mioItem.PublishDate > this.Settings.Since)
                        result.Add(mioItem);
                }
                catch (Exception exc) {
                    this.Logger.AddMessage("ReadFeed", "exception " + exc.Message + " while posting To Twitter", feedUrl, MessageType.Error);
                    var test = exc.Message;
                }
            }

            return result;
        }


        //private async Task DownloadFeeds()
        //{
        //    var rss = new SyndicationFeed(config.AppSettings["title"], config.AppSettings["description"], null);

        //    foreach (var key in config.AppSettings.AllKeys.Where(key => key.StartsWith("feed:")))
        //    {
        //        SyndicationFeed feed = await DownloadFeed(config.AppSettings[key]);
        //        rss.Items = rss.Items.Union(feed.Items).GroupBy(i => i.Title.Text).Select(i => i.First()).OrderByDescending(i => i.PublishDate.Date);
        //    }

        //    using (XmlWriter writer = XmlWriter.Create(_masterFile))
        //        rss.SaveAsRss20(writer);

        //    using (XmlWriter writer = XmlWriter.Create(_feedFile))
        //    {
        //        rss.Items = rss.Items.Take(10);
        //        rss.SaveAsRss20(writer);
        //    }
        //}

        private async Task<SyndicationFeed> DownloadFeed(string url)
        {
            try
            {
                using (WebClient client = new WebClient())
                {
                    var stream = await client.OpenReadTaskAsync(url);
                    var settings = new XmlReaderSettings();
                    settings.DtdProcessing = DtdProcessing.Parse;
                    return SyndicationFeed.Load(XmlReader.Create(stream, settings));
                }
            }
            catch (Exception exc)
            {
                this.Logger.AddMessage("DownloadFeed", "exception " + exc.Message + " while downloading Feed", url, MessageType.Error);
                return new SyndicationFeed();
            }
        }

        //public IEnumerable<SyndicationItem> GetData()
        //{
        //    using (XmlReader reader = XmlReader.Create(_masterFile))
        //    {
        //        var count = int.Parse(config.AppSettings["postsPerPage"]);
        //        var items = SyndicationFeed.Load(reader).Items.Skip((_page - 1) * count).Take(count);
        //        return items.Select(item => { CleanItem(item); return item; });
        //    }
        //}

        //private static void CleanItem(SyndicationItem item)
        //{
        //    string summary = item.Summary != null ? item.Summary.Text : ((TextSyndicationContent)item.Content).Text;
        //    summary = Regex.Replace(summary, "<[^>]*>", ""); // Strips out HTML
        //    item.Summary = new TextSyndicationContent(string.Join("", summary.Take(300)) + "...");
        //}
    }


}
