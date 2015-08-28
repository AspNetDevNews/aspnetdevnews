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
using System.Text.RegularExpressions;

namespace AspNetDevNews.Services.Feeds
{
    public class FeedReaderService: IFeedReaderService
    {
        private ISettingsService Settings { get; set; }
        private ISessionLogger Logger { get; set; }

        public FeedReaderService(ISettingsService settings, ISessionLogger logger) {
            if (settings == null)
                throw new ArgumentNullException(nameof(settings), "cannot be null");
            if (logger == null)
                throw new ArgumentNullException(nameof(logger), "cannot be null");

            Settings = settings;
            Logger = logger;
        }

        public async Task<IList<FeedItem>> ReadFeed(string feedUrl)
        {
            if (string.IsNullOrWhiteSpace(feedUrl))
                return new List<FeedItem>();

            SyndicationFeed feed = await DownloadFeed(feedUrl);

            List<FeedItem> result = new List<FeedItem>();
            foreach (var item in feed.Items) {
                try
                {
                    var mioItem = AutoMapper.Mapper.Map<FeedItem>(item);
                    mioItem.Summary = Regex.Replace(mioItem.Summary, "<[^>]*>", "");
                    mioItem.Feed = feedUrl;

                    if (mioItem.PublishDate > this.Settings.Since)
                        result.Add(mioItem);
                }
                catch (Exception exc) {
                    this.Logger.AddMessage("ReadFeed", $"exception {exc.Message} while posting To Twitter", feedUrl, MessageType.Error);
                    var test = exc.Message;
                }
            }

            return result;
        }

        private async Task<SyndicationFeed> DownloadFeed(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return new SyndicationFeed();

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
                this.Logger.AddMessage("DownloadFeed", $"exception {exc.Message} while downloading Feed", url, MessageType.Error);
                return new SyndicationFeed();
            }
        }

        //private static void CleanItem(SyndicationItem item)
        //{
        //    string summary = item.Summary != null ? item.Summary.Text : ((TextSyndicationContent)item.Content).Text;
        //    summary = Regex.Replace(summary, "<[^>]*>", ""); // Strips out HTML
        //    item.Summary = new TextSyndicationContent(string.Join("", summary.Take(300)) + "...");
        //}
    }


}
