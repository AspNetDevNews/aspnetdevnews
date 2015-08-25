using AspNetDevNews.Services.AzureTableStorage;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Text;
using System.Threading.Tasks;

namespace AspNetDevNews.Helpers
{
    public static class AutoMapperHelper
    {
        public static void InitMappings() {
            // Source, Desc

            // OK
            Mapper.CreateMap<Models.TwittedIssue, TwittedIssueEntity>()
                .ForMember(dest => dest.PartitionKey, opts => opts.MapFrom(src => src.GetPartitionKey()))
                .ForMember(dest => dest.RowKey, opts => opts.MapFrom(src => src.GetRowKey()))
                .ForMember(dest => dest.Labels, opts => opts.MapFrom(src => string.Join(";", src.Labels)))
                .ForMember(dest => dest.StatusId, opts => opts.MapFrom(src => src.StatusID.ToString()))
                .ForMember(dest => dest.TwittedAt, opts => opts.MapFrom(src => DateTime.Now));
            // OK
            Mapper.CreateMap<Models.Issue, IssueMergeEntity>()
                .ForMember(dest => dest.PartitionKey, opts => opts.MapFrom(src => src.GetPartitionKey()))
                .ForMember(dest => dest.RowKey, opts => opts.MapFrom(src => src.GetRowKey()))
                .ForMember(dest => dest.ETag, opts => opts.MapFrom(src => "*"));

            Mapper.CreateMap<Models.TwittedPost, TwittedLinkEntity>()
                .ForMember(dest => dest.PartitionKey, opts => opts.MapFrom(src => TableStorageUtilities.EncodeToKey(src.Feed)))
                .ForMember(dest => dest.RowKey, opts => opts.MapFrom(src => TableStorageUtilities.EncodeToKey(src.Id)));

            // OK
            Mapper.CreateMap<TwittedIssueEntity, Models.Issue>()
                .ForMember(dest => dest.Labels, opts => opts.MapFrom(src => src.Labels.Split(new char[] { ';' })))
                .ForMember(dest => dest.Number, opts => opts.MapFrom(src => Convert.ToInt32(src.RowKey)))
                .ForMember(dest => dest.Organization, opts => opts.MapFrom(src => src.PartitionKey.Split(new char[] { '+' })[0]))
                .ForMember(dest => dest.Repository, opts => opts.MapFrom(src => src.PartitionKey.Split(new char[] { '+' })[1]));
            // OK
            Mapper.CreateMap<TwittedLinkEntity, Models.FeedItem>()
                .ForMember(dest => dest.Id, opts => opts.MapFrom(src => TableStorageUtilities.DecodeFromKey(src.RowKey)))
                .ForMember(dest => dest.Feed, opts => opts.MapFrom(src => TableStorageUtilities.DecodeFromKey(src.PartitionKey)));
            //OK
            Mapper.CreateMap<SyndicationItem, Models.FeedItem>()
                .ForMember(dest => dest.Id, opts => opts.ResolveUsing<PublishDateResolver>())
                .ForMember(dest => dest.PublishDate, opts =>
                    opts.MapFrom(src => src.PublishDate.Year != 1 ? src.PublishDate.DateTime : src.LastUpdatedTime.DateTime))
                .ForMember(dest => dest.Summary, opts => opts.MapFrom(src => src.Summary != null ? src.Summary.Text : string.Empty))
                .ForMember(dest => dest.Title, opts => opts.MapFrom(src => src.Title.Text));
            // OK
            Mapper.CreateMap<Octokit.Issue, Models.Issue>()
                .ForMember(dest => dest.Url, opts => opts.MapFrom(src => src.HtmlUrl.ToString()))
                .ForMember(dest => dest.Labels, opts => opts.MapFrom(src => src.Labels.Select(lab => lab.Name).ToArray()))
                .ForMember(dest => dest.CreatedAt, opts => opts.MapFrom(src => src.CreatedAt.LocalDateTime))
                .ForMember(dest => dest.UpdatedAt, opts => opts.MapFrom(src => src.UpdatedAt != null ? (DateTime?)src.UpdatedAt.Value.LocalDateTime : null))
                .ForMember(dest => dest.State, opts => opts.MapFrom(src => src.State == 0 ? "Open" : "Closed"));
            // OK
            Mapper.CreateMap<Models.Issue, Models.TwittedIssue>();
            // OK
            Mapper.CreateMap<Models.FeedItem, Models.TwittedPost>();
        }

        public class PublishDateResolver : ValueResolver<SyndicationItem, string>
        {
            protected override string ResolveCore(SyndicationItem src)
            {
                if (src.Id.ToLower().StartsWith("http"))
                    return src.Id;
                else if (src.Links.Count > 0)
                    return src.Links[0].Uri.ToString();
                else
                    return string.Empty;
            }
        }
    }
}
