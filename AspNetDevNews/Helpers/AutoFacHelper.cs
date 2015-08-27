using AspNetDevNews.Services;
using AspNetDevNews.Services.AzureTableStorage;
using AspNetDevNews.Services.Feeds;
using AspNetDevNews.Services.Interfaces;
using Autofac;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AspNetDevNews.Helpers
{
    public static class AutoFacHelper
    {
        public static IContainer InitAutoFac() {

            var builder = new ContainerBuilder();
            builder.RegisterType<SettingsService>().As<ISettingsService>();
            builder.RegisterType<GitHubService>().As<IGitHubService>();
            builder.RegisterType<AzureTableStorageService>().As<IStorageService>();
            builder.RegisterType<TwitterService>().As<ITwitterService>();
            builder.RegisterType<FeedReaderService>().As<IFeedReaderService>();
            builder.RegisterType<SessionLogService>().As<ISessionLogger>().SingleInstance();

            builder.RegisterType<IssueReceiveService>();

            builder.RegisterType<GitHubService>();

            return builder.Build();
        }
    }
}
