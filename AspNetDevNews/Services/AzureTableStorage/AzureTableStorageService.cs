﻿using AspNetDevNews.Models;
using AspNetDevNews.Services.Interfaces;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AspNetDevNews.Services.AzureTableStorage
{
    public class AzureTableStorageService: IStorageService
    {
        private ISettingsService Settings { get; set; }

        public AzureTableStorageService(ISettingsService settings) {
            if (settings == null)
                throw new ArgumentNullException( nameof(settings), "settings cannot be null");
            this.Settings = settings;
        }

        private CloudTableClient GetClient() {
            StorageCredentials creds = new StorageCredentials(this.Settings.TwittedIssuesATAccountName, 
                this.Settings.TwittedIssuesATAccountKey);
            CloudStorageAccount account = new CloudStorageAccount(creds, useHttps: true);

            CloudTableClient client = account.CreateCloudTableClient();
            return client;
        }

        #region get Storage Tables
        private CloudTable GetIssuesTable()
        {
            CloudTableClient client = GetClient();

            CloudTable table = client.GetTableReference("twittedIssues");

            table.CreateIfNotExists();
            return table;
        }

        private CloudTable GetLinksTable()
        {
            CloudTableClient client = GetClient();

            CloudTable table = client.GetTableReference("twittedLinks");

            table.CreateIfNotExists();
            return table;
        }

        private CloudTable GetExceptionsTable()
        {
            CloudTableClient client = GetClient();

            CloudTable table = client.GetTableReference("exceptions");
            table.CreateIfNotExists();
            return table;
        }

        private CloudTable GetExcecutionsTable()
        {
            CloudTableClient client = GetClient();

            CloudTable table = client.GetTableReference("executions");
            table.CreateIfNotExists();
            return table;
        }

        private CloudTable GetDocumentsTable()
        {
            CloudTableClient client = GetClient();

            CloudTable table = client.GetTableReference("twittedGitHubDocuments");
            table.CreateIfNotExists();
            return table;
        }

        #endregion

        #region insert and update entities
        public async Task Store(IList<TwittedIssue> issues)
        {
            if (issues == null || issues.Count == 0)
                return;

            try
            {
                var table = GetIssuesTable();
                TableBatchOperation batchOperation = new TableBatchOperation();

                foreach (var issue in issues)
                {
                    var twittedIssue = AutoMapper.Mapper.Map<TwittedIssueEntity>(issue);
                    batchOperation.Insert(twittedIssue);

                }
                if (batchOperation.Count() > 0)
                    await table.ExecuteBatchAsync(batchOperation);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public async Task Store(IList<TwittedPost> posts)
        {
            if (posts == null || posts.Count == 0)
                return;

            try
            {
                var table = GetLinksTable();
                TableBatchOperation batchOperation = new TableBatchOperation();

                foreach (var post in posts)
                {
                    //var twittedIssue = new TwittedLinkEntity(
                    //    TableStorageUtilities.EncodeToKey(post.Feed), 
                    //    TableStorageUtilities.EncodeToKey(post.Id));
                    //twittedIssue.Title = post.Title;
                    //twittedIssue.PublishDate = post.PublishDate;
                    //twittedIssue.Summary = post.Summary;
                    //twittedIssue.StatusId = post.StatusID.ToString(); // I have to save as string because

                    var twittedIssue = AutoMapper.Mapper.Map<TwittedLinkEntity>(post);
                    //TableOperation insertOperation = TableOperation.Insert(twittedIssue);
                    //TableOperation insertOperation = TableOperation.InsertOrReplace(entity);
                    //await table.ExecuteAsync(insertOperation);
                    //table.Execute(insertOperation);
                    batchOperation.Insert(twittedIssue);

                }
                if (batchOperation.Count() > 0)
                {
                    var result = await table.ExecuteBatchAsync(batchOperation);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public async Task Merge(IList<Issue> issues)
        {
            if (issues == null || issues.Count == 0)
                return;

            try
            {
                var table = GetIssuesTable();
                TableBatchOperation batchOperation = new TableBatchOperation();

                foreach (var issue in issues)
                {
                    // OK
                    //var twittedIssue = new IssueMergeEntity(issue.GetPartitionKey(), issue.GetRowKey());
                    //twittedIssue.Title = issue.Title;
                    //twittedIssue.UpdatedAt = issue.UpdatedAt;
                    //twittedIssue.State = issue.State;
                    //twittedIssue.Comments = issue.Comments;
                    //twittedIssue.ETag = "*";

                    var twittedIssue = AutoMapper.Mapper.Map<IssueMergeEntity>(issue);


                    //TableOperation insertOperation = TableOperation.Insert(twittedIssue);
                    //TableOperation insertOperation = TableOperation.Merge(twittedIssue);
                    //var result = await table.ExecuteAsync(insertOperation);
                    batchOperation.Merge(twittedIssue);

                }
                if (batchOperation.Count() > 0)
                {
                    var result = await table.ExecuteBatchAsync(batchOperation);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public async Task Store(IList<TwittedGitHubHostedDocument> documents)
        {
            if (documents == null || documents.Count == 0)
                return;

            try
            {
                var table = GetDocumentsTable();
                TableBatchOperation batchOperation = new TableBatchOperation();

                foreach (var document in documents)
                {
                    var twittedIssue = AutoMapper.Mapper.Map<TwittedGitHubHostedDocumentEntity>(document);
                    batchOperation.Insert(twittedIssue);
                }
                if (batchOperation.Count() > 0)
                {
                    var result = await table.ExecuteBatchAsync(batchOperation);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        #endregion
        public async Task Store(Exception exception, Issue issue, string operation)
        {
            if (exception == null )
                return;
            if (string.IsNullOrWhiteSpace(operation))
                return;

            try
            {
                var table = GetExceptionsTable();

                var storeException = new ExceptionEntity(operation, DateTime.Now.GetRowKey());
                storeException.TwitRowKey = issue.GetRowKey();
                storeException.TwitPartitionKey = issue.GetPartitionKey();
                storeException.CreatedAt = DateTime.Now;
                storeException.Exception = JsonConvert.SerializeObject(exception);
                storeException.Operation = operation;

                TableOperation insertOperation = TableOperation.Insert(storeException);
                var result = await table.ExecuteAsync(insertOperation);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
        public async Task Store(Exception exception, GitHubHostedDocument document, string operation)
        {
            if (exception == null)
                return;
            if (string.IsNullOrWhiteSpace(operation))
                return;

            try
            {
                var table = GetExceptionsTable();

                var storeException = new ExceptionEntity(operation, DateTime.Now.GetRowKey());
                storeException.TwitRowKey = document.GetRowKey();
                storeException.TwitPartitionKey = document.GetPartitionKey();
                storeException.CreatedAt = DateTime.Now;
                storeException.Exception = JsonConvert.SerializeObject(exception);
                storeException.Operation = operation;

                TableOperation insertOperation = TableOperation.Insert(storeException);
                var result = await table.ExecuteAsync(insertOperation);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public async Task ReportExecution(DateTime StartedAt, DateTime EndedAt, int TwittedIssues, int CheckedRepositories, int updatedIssues, int postedLinks)
        {

            try
            {
                var table = GetExcecutionsTable();

                var Report = new ExecutionEntity("execution", DateTime.Now.GetRowKey());
                Report.EndedAt = EndedAt;
                Report.StartedAt = StartedAt;
                Report.TwittedIsseues = TwittedIssues;
                Report.CheckedRepositories = CheckedRepositories;
                Report.UpdatedIssues = updatedIssues;
                Report.TwittedPosts = postedLinks;

                TableOperation insertOperation = TableOperation.Insert(Report);
                var result = await table.ExecuteAsync(insertOperation);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public async Task<bool> Exists(TwittedIssue issue) {
            try
            {
                var table = GetIssuesTable();

                TableOperation retrieveOperation = TableOperation.Retrieve<TwittedIssueEntity>(issue.GetPartitionKey(), issue.GetRowKey());

                TableResult query = await table.ExecuteAsync(retrieveOperation);

                if (query.Result != null)
                {
                    Console.WriteLine("Product: {0}", ((TwittedIssueEntity)query.Result).Title);
                    return true;
                }
                else
                {
                    Console.WriteLine("The Product was not found.");
                    return false;
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return false;
            }

        }

        public async Task<T> GetRecord<T>(string partitionKey, string rowKey) where T : ITableEntity
        {
            try
            {
                var table = GetIssuesTable();

                TableOperation retrieveOperation = TableOperation.Retrieve<T>(partitionKey, rowKey);

                TableResult query = await table.ExecuteAsync(retrieveOperation);

                if (query.Result != null)
                {
                    //Console.WriteLine("Product: {0}", ((T)query.Result).Title);
                    return (T)query.Result;
                }
                else
                {
                    Console.WriteLine("The Product was not found.");
                    return default(T);
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return default(T);
            }

        }

        public IList<Issue> GetBatchIssues(string organization, string repository, IList<string> rowKeys)
        {
            try
            {
                var table = GetIssuesTable();
                var dummy = new Issue();
                dummy.Organization = organization;
                dummy.Repository = repository;

                string filter = "(PartitionKey eq '" + dummy.GetPartitionKey() + "') and (";
                string rowFilter = string.Empty;
                foreach (var chiave in rowKeys) {
                    if (!string.IsNullOrWhiteSpace(rowFilter))
                        rowFilter += " or ";

                    rowFilter += " (RowKey eq '" + chiave + "') ";
                }
                filter += rowFilter + ")";

                TableQuery<TwittedIssueEntity> query = new TableQuery<TwittedIssueEntity>().Where(filter);
                var results = new List<Issue>();
                
                var alreadyStored = table.ExecuteQuery(query);
                foreach (TwittedIssueEntity entity in alreadyStored)
                {
                    // OK
                    //var issue = new Issue();
                    //issue.Body = entity.Body;
                    //issue.CreatedAt = entity.CreatedAt;
                    //issue.Labels = entity.Labels.Split(new char[] { ';' });
                    //issue.Number = Convert.ToInt32(entity.RowKey);

                    //var partitionFields = entity.PartitionKey.Split(new char[] { '+' });

                    //issue.Organization = partitionFields[0];
                    //issue.Repository = partitionFields[1];
                    //issue.Title = entity.Title;
                    //issue.UpdatedAt = entity.UpdatedAt;
                    //issue.Url = entity.Url;
                    //issue.State = entity.State;
                    //issue.Comments = entity.Comments;

                    var issue = AutoMapper.Mapper.Map<Issue>(entity);

                    results.Add(issue);
                }
                return results;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return new List<Models.Issue>();
            }
        }

        public IList<GitHubHostedDocument> GetBatchDocuments(string partitionKey, IList<string> rowKeys)
        {
            try
            {
                var table = GetDocumentsTable();
 
                string filter = "(PartitionKey eq '" + partitionKey + "') and (";
                string rowFilter = string.Empty;
                foreach (var chiave in rowKeys)
                {
                    if (!string.IsNullOrWhiteSpace(rowFilter))
                        rowFilter += " or ";

                    rowFilter += " (RowKey eq '" + chiave + "') ";
                }
                filter += rowFilter + ")";

                TableQuery<TwittedGitHubHostedDocumentEntity> query = new TableQuery<TwittedGitHubHostedDocumentEntity>().Where(filter);
                var results = new List<GitHubHostedDocument>();

                var alreadyStored = table.ExecuteQuery(query);
                foreach (TwittedGitHubHostedDocumentEntity entity in alreadyStored)
                {
                    var issue = AutoMapper.Mapper.Map<GitHubHostedDocument>(entity);

                    results.Add(issue);
                }
                return results;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return new List<Models.GitHubHostedDocument>();
            }
        }

        public async Task<IList<Issue>> GetRecentIssues(string organization, string repository, DateTimeOffset since)
        {
            try
            {
                var table = GetIssuesTable();
                var dummy = new Models.Issue();
                dummy.Organization = organization;
                dummy.Repository = repository;

                var partitionKey = dummy.GetPartitionKey();

                var recentIssuesQuery = (from entry in table.CreateQuery<TwittedIssueEntity>()
                    where entry.PartitionKey == partitionKey && entry.CreatedAt > since.DateTime
                                              select entry);
                // using async method raises an exception
                var recentIssues = recentIssuesQuery.ToList();

                var results = new List<Issue>();

                if (recentIssues.Any())
                {
                    foreach (TwittedIssueEntity entity in recentIssues)
                    {
                        // OK
                        //var issues = new Issue();
                        //issue.Body = product.Body;
                        //issue.CreatedAt = product.CreatedAt;
                        //issue.Labels = product.Labels.Split(new char[] { ';' });
                        //issue.Number = Convert.ToInt32(product.RowKey);

                        //var partitionFields = product.PartitionKey.Split(new char[] { '+' });

                        //issue.Organization = partitionFields[0];
                        //issue.Repository = partitionFields[1];
                        //issue.Title = product.Title;
                        //issue.UpdatedAt = product.UpdatedAt;
                        //issue.Url = product.Url;
                        //issue.State = product.State;
                        //issue.Comments = product.Comments;

                        var issue = AutoMapper.Mapper.Map<Issue>(entity);

                        results.Add(issue);
                    }
                    return results;
                }
                else
                {
                    Console.WriteLine("No inventory was not found.  Better order more!");
                    return new List<Models.Issue>();
                }

            }
            catch (Exception ex) {
                Console.WriteLine(ex);
                return new List<Models.Issue>();
            }
        }

        public IList<FeedItem> GetBatchWebLinks(string feed, IList<string> rowKeys)
        {
            try
            {
                var table = GetLinksTable();

                List<string> realKeys = new List<string>();
                for (int i = 0; i < rowKeys.Count; i++)
                    realKeys.Add(TableStorageUtilities.EncodeToKey(rowKeys[i]));
                feed = TableStorageUtilities.EncodeToKey(feed);

                TableQuery <TwittedLinkEntity> query = new TableQuery<TwittedLinkEntity>().Where(
                    TableStorageUtilities.GetTableQuerySetString(feed, realKeys));
                var results = new List<FeedItem>();

                var alreadyStored = table.ExecuteQuery(query);
                foreach (TwittedLinkEntity entity in alreadyStored)
                {
                    // OK
                    //var issue = new FeedItem();
                    //issue.Id =  TableStorageUtilities.DecodeFromKey(entity.RowKey);
                    //issue.PublishDate = entity.PublishDate;
                    //issue.Summary = entity.Summary;
                    //issue.Title = entity.Title;
                    //issue.Feed = feed;

                    var issue = AutoMapper.Mapper.Map<FeedItem>(entity);
                    results.Add(issue);
                }
                return results;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return new List<Models.FeedItem>();
            }
        }

        public async Task Store(Exception exception, string feedItem, FeedItem post, string operation)
        {
            try
            {
                var table = GetExceptionsTable();

                var storeException = new ExceptionEntity(operation, DateTime.Now.GetRowKey());
                storeException.TwitRowKey = TableStorageUtilities.EncodeToKey(post.Id);
                storeException.TwitPartitionKey = TableStorageUtilities.EncodeToKey(feedItem);
                storeException.CreatedAt = DateTime.Now;
                storeException.Exception = JsonConvert.SerializeObject(exception);
                storeException.Operation = operation;

                TableOperation insertOperation = TableOperation.Insert(storeException);
                var result = await table.ExecuteAsync(insertOperation);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}
