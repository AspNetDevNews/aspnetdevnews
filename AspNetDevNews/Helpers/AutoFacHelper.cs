using AspNetDevNews.Services;
using AspNetDevNews.Services.AzureTableStorage;
using AspNetDevNews.Services.Feeds;
using AspNetDevNews.Services.Interfaces;
using Autofac;

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
            builder.RegisterType<JobService>().As<IJobService>();
            builder.RegisterType<SessionLogService>().As<ISessionLogger>().SingleInstance();

            builder.RegisterType<IssueReceiveService>();
            builder.RegisterType<GitHubService>();
            //builder.RegisterType<JobService>();
            
            return builder.Build();
        }
    }
}
