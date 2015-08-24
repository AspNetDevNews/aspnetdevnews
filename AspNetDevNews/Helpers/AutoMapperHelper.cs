using AspNetDevNews.Models;
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
            AutoMapper.Mapper.CreateMap<TwittedIssue, TwittedIssueEntity>()
                .ForMember(dest => dest.PartitionKey, opts => opts.MapFrom(src => src.GetPartitionKey()))
                .ForMember(dest => dest.RowKey, opts => opts.MapFrom(src => src.GetRowKey()))
                .ForMember(dest => dest.Labels, opts => opts.MapFrom(src => string.Join(";", src.Labels)))
                .ForMember(dest => dest.StatusId, opts => opts.MapFrom(src => src.StatusID.ToString()))
                .ForMember(dest => dest.TwittedAt, opts => opts.MapFrom(src => DateTime.Now));

            AutoMapper.Mapper.CreateMap<TwittedIssueEntity, Issue>()
                .ForMember(dest => dest.Labels, opts => opts.MapFrom(src => src.Labels.Split(new char[] { ';' })))
                .ForMember(dest => dest.Number, opts => opts.MapFrom(src => Convert.ToInt32(src.RowKey)))
                .ForMember(dest => dest.Organization, opts => opts.MapFrom(src => src.PartitionKey.Split(new char[] { '+' })[0]))
                .ForMember(dest => dest.Repository, opts => opts.MapFrom(src => src.PartitionKey.Split(new char[] { '+' })[1]));

            AutoMapper.Mapper.CreateMap<Issue, IssueMergeEntity>()
                .ForMember(dest => dest.PartitionKey, opts => opts.MapFrom(src => src.GetPartitionKey()))
                .ForMember(dest => dest.RowKey, opts => opts.MapFrom(src => src.GetRowKey()))
                .ForMember(dest => dest.ETag, opts => opts.MapFrom(src => "*"));

            AutoMapper.Mapper.CreateMap<TwittedLinkEntity, FeedItem>()
                .ForMember(dest => dest.Id, opts => opts.MapFrom(src => TableStorageUtilities.DecodeFromKey(src.RowKey)))
                .ForMember(dest => dest.Feed, opts => opts.MapFrom(src => TableStorageUtilities.DecodeFromKey(src.PartitionKey)));

            AutoMapper.Mapper.CreateMap<FeedItem, TwittedLinkEntity>()
                .ForMember(dest => dest.PartitionKey, opts => opts.MapFrom(src => TableStorageUtilities.EncodeToKey(src.Feed)))
                .ForMember(dest => dest.RowKey, opts => opts.MapFrom(src => TableStorageUtilities.EncodeToKey(src.Id)));

            AutoMapper.Mapper.CreateMap<SyndicationItem, FeedItem>()
                .ForMember(dest => dest.Id, opts => opts.ResolveUsing<PublishDateResolver>())
                .ForMember(dest => dest.PublishDate, opts =>
                    opts.MapFrom(src => src.PublishDate.Year != 1 ? src.PublishDate.DateTime : src.LastUpdatedTime.DateTime))
                .ForMember(dest => dest.Summary, opts => opts.MapFrom(src => src.Summary != null ? src.Summary.Text : string.Empty))
                .ForMember(dest => dest.Title, opts => opts.MapFrom(src => src.Title.Text));

            AutoMapper.Mapper.CreateMap<Octokit.Issue, Models.Issue>()
                .ForMember(dest => dest.Url, opts => opts.MapFrom(src => src.HtmlUrl.ToString()))
                .ForMember(dest => dest.Labels, opts => opts.MapFrom(src => src.Labels.Select(lab => lab.Name).ToArray()))
                .ForMember(dest => dest.CreatedAt, opts => opts.MapFrom(src => src.CreatedAt.LocalDateTime))
                .ForMember(dest => dest.UpdatedAt, opts => opts.MapFrom(src => src.UpdatedAt != null ? (DateTimeOffset?)src.UpdatedAt.Value.LocalDateTime : null))
                .ForMember(dest => dest.State, opts => opts.MapFrom(src => src.State == 0 ? "Open" : "Closed"));

            AutoMapper.Mapper.CreateMap<Models.Issue, TwittedIssue>();

            AutoMapper.Mapper.CreateMap<Models.Issue, TwittedPost>();
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
