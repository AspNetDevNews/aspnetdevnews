using AspNetDevNews.Models;
using AspNetDevNews.Services.Interfaces;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
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

        private CloudBlobClient GetBlobClient()
        {
            StorageCredentials creds = new StorageCredentials(this.Settings.TwittedIssuesATAccountName,
                this.Settings.TwittedIssuesATAccountKey);
            CloudStorageAccount account = new CloudStorageAccount(creds, useHttps: true);

            return account.CreateCloudBlobClient();
        }

        private CloudBlobContainer GetLogContainer() {
            CloudBlobClient client = GetBlobClient();

            CloudBlobContainer container = client.GetContainerReference("sessionLog");

            container.CreateIfNotExists();
            return container;
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
            //if (issues == null || issues.Count == 0)
            //    return;

            //try
            //{
            //    var table = GetIssuesTable();
            //    TableBatchOperation batchOperation = new TableBatchOperation();

            //    foreach (var issue in issues)
            //    {
            //        var twittedIssue = AutoMapper.Mapper.Map<TwittedIssueEntity>(issue);
            //        batchOperation.Insert(twittedIssue);

            //    }
            //    if (batchOperation.Count() > 0)
            //        await table.ExecuteBatchAsync(batchOperation);
            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine(ex);
            //}
            await Store<TwittedIssue, TwittedIssueEntity>(issues);
        }

        public async Task Store(IList<TwittedPost> posts)
        {
            //if (posts == null || posts.Count == 0)
            //    return;

            //try
            //{
            //    var table = GetLinksTable();
            //    TableBatchOperation batchOperation = new TableBatchOperation();

            //    foreach (var post in posts)
            //    {
            //        var twittedIssue = AutoMapper.Mapper.Map<TwittedLinkEntity>(post);
            //        batchOperation.Insert(twittedIssue);
            //    }
            //    if (batchOperation.Count() > 0)
            //    {
            //        var result = await table.ExecuteBatchAsync(batchOperation);
            //    }
            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine(ex);
            //}
            await Store<TwittedPost, TwittedLinkEntity>(posts);
        }

        public async Task Store(IList<TwittedGitHubHostedDocument> documents)
        {
            //if (documents == null || documents.Count == 0)
            //    return;

            //try
            //{
            //    var table = GetDocumentsTable();
            //    TableBatchOperation batchOperation = new TableBatchOperation();

            //    foreach (var document in documents)
            //    {
            //        var twittedIssue = AutoMapper.Mapper.Map<TwittedGitHubHostedDocumentEntity>(document);
            //        batchOperation.Insert(twittedIssue);
            //    }
            //    if (batchOperation.Count() > 0)
            //    {
            //        var result = await table.ExecuteBatchAsync(batchOperation);
            //    }
            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine(ex);
            //}

            await Store<TwittedGitHubHostedDocument, TwittedGitHubHostedDocumentEntity>(documents);
        }

        private async Task Store<Source, Destination>(IList<Source> documents) where Destination: ITableEntity
        {
            if (documents == null || documents.Count == 0)
                return;

            try
            {
                var table = GetDocumentsTable();
                TableBatchOperation batchOperation = new TableBatchOperation();

                foreach (var document in documents)
                {
                    var twittedIssue = AutoMapper.Mapper.Map<Destination>(document);
                    batchOperation.InsertOrMerge(twittedIssue);
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
            //if (issues == null || issues.Count == 0)
            //    return;

            //try
            //{
            //    var table = GetIssuesTable();
            //    TableBatchOperation batchOperation = new TableBatchOperation();

            //    foreach (var issue in issues)
            //    {
            //        var twittedIssue = AutoMapper.Mapper.Map<IssueMergeEntity>(issue);
            //        batchOperation.Merge(twittedIssue);
            //    }
            //    if (batchOperation.Count() > 0)
            //    {
            //        var result = await table.ExecuteBatchAsync(batchOperation);
            //    }
            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine(ex);
            //}
            await Store<Issue, IssueMergeEntity>(issues);
        }

        #endregion

        //public async Task Store(Exception exception, Issue issue, string operation)
        //{
        //    if (exception == null )
        //        return;
        //    if (string.IsNullOrWhiteSpace(operation))
        //        return;

        //    try
        //    {
        //        var table = GetExceptionsTable();

        //        var storeException = new ExceptionEntity(operation, DateTime.Now.GetRowKey());
        //        storeException.TwitRowKey = issue.GetRowKey();
        //        storeException.TwitPartitionKey = issue.GetPartitionKey();
        //        storeException.CreatedAt = DateTime.Now;
        //        storeException.Exception = JsonConvert.SerializeObject(exception);
        //        storeException.Operation = operation;

        //        TableOperation insertOperation = TableOperation.Insert(storeException);
        //        var result = await table.ExecuteAsync(insertOperation);
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine(ex);
        //    }
        //}

        //public async Task Store(Exception exception, GitHubHostedDocument document, string operation)
        //{
        //    if (exception == null)
        //        return;
        //    if (string.IsNullOrWhiteSpace(operation))
        //        return;

        //    try
        //    {
        //        var table = GetExceptionsTable();

        //        var storeException = new ExceptionEntity(operation, DateTime.Now.GetRowKey());
        //        storeException.TwitRowKey = document.GetRowKey();
        //        storeException.TwitPartitionKey = document.GetPartitionKey();
        //        storeException.CreatedAt = DateTime.Now;
        //        storeException.Exception = JsonConvert.SerializeObject(exception);
        //        storeException.Operation = operation;

        //        TableOperation insertOperation = TableOperation.Insert(storeException);
        //        var result = await table.ExecuteAsync(insertOperation);
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine(ex);
        //    }
        //}

        //public async Task Store(Exception exception, FeedItem post, string operation)
        //{
        //    try
        //    {
        //        var table = GetExceptionsTable();

        //        var storeException = new ExceptionEntity(operation, DateTime.Now.GetRowKey());
        //        storeException.TwitRowKey = TableStorageUtilities.EncodeToKey(post.Id);
        //        storeException.TwitPartitionKey = TableStorageUtilities.EncodeToKey(post.Feed);
        //        storeException.CreatedAt = DateTime.Now;
        //        storeException.Exception = JsonConvert.SerializeObject(exception);
        //        storeException.Operation = operation;

        //        TableOperation insertOperation = TableOperation.Insert(storeException);
        //        var result = await table.ExecuteAsync(insertOperation);
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine(ex);
        //    }
        //}

        public async Task Store(Exception exception, Interfaces.ITableStorageKeyGet record, string operation)
        {
            try
            {
                var table = GetExceptionsTable();

                var storeException = new ExceptionEntity(operation, DateTime.Now.GetRowKey());
                storeException.TwitRowKey = record.GetRowKey();
                storeException.TwitPartitionKey = record.GetPartitionKey();
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

                return ExecuteQuery<TwittedIssueEntity, Issue>(table, dummy.GetPartitionKey(), rowKeys);

                ////string filter = "(PartitionKey eq '" + dummy.GetPartitionKey() + "') and (";
                ////string rowFilter = string.Empty;
                ////foreach (var chiave in rowKeys) {
                ////    if (!string.IsNullOrWhiteSpace(rowFilter))
                ////        rowFilter += " or ";

                ////    rowFilter += " (RowKey eq '" + chiave + "') ";
                ////}
                ////filter += rowFilter + ")";

                ////TableQuery<TwittedIssueEntity> query = new TableQuery<TwittedIssueEntity>().Where(filter);
                //TableQuery<TwittedIssueEntity> query = new TableQuery<TwittedIssueEntity>().Where(
                //    TableStorageUtilities.GetTableQuerySetString(dummy.GetPartitionKey(), rowKeys));


                //var results = new List<Issue>();
                
                //var alreadyStored = table.ExecuteQuery(query);
                //foreach (TwittedIssueEntity entity in alreadyStored)
                //{
                //    var issue = AutoMapper.Mapper.Map<Issue>(entity);

                //    results.Add(issue);
                //}
                //return results;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return new List<Models.Issue>();
            }
        }

        public IList<GitHubHostedDocument> GetBatchDocuments(string organization, string repository, IList<string> rowKeys)
        {
            try
            {
                var table = GetDocumentsTable();
                var dummy = new GitHubHostedDocument();
                dummy.Organization = organization;
                dummy.Repository = repository;

                //string filter = "(PartitionKey eq '" + partitionKey + "') and (";
                //string rowFilter = string.Empty;
                //foreach (var chiave in rowKeys)
                //{
                //    if (!string.IsNullOrWhiteSpace(rowFilter))
                //        rowFilter += " or ";

                //    rowFilter += " (RowKey eq '" + chiave + "') ";
                //}
                //filter += rowFilter + ")";

                //TableQuery<TwittedGitHubHostedDocumentEntity> query = new TableQuery<TwittedGitHubHostedDocumentEntity>().Where(filter);

                return ExecuteQuery<TwittedGitHubHostedDocumentEntity, GitHubHostedDocument>(table, dummy.GetPartitionKey(), rowKeys);

                //TableQuery<TwittedGitHubHostedDocumentEntity> query = new TableQuery<TwittedGitHubHostedDocumentEntity>().Where(
                //    TableStorageUtilities.GetTableQuerySetString(dummy.GetPartitionKey(), rowKeys));

                //var results = new List<GitHubHostedDocument>();

                //var alreadyStored = table.ExecuteQuery(query);
                //foreach (TwittedGitHubHostedDocumentEntity entity in alreadyStored)
                //{
                //    var issue = AutoMapper.Mapper.Map<GitHubHostedDocument>(entity);

                //    results.Add(issue);
                //}
                //return results;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return new List<Models.GitHubHostedDocument>();
            }
        }

        private IList<Destination> ExecuteQuery<StorageEntity, Destination>(CloudTable table, string partitionKey, IList<string>rowKeys) where StorageEntity : TableEntity, new ()
        {
            TableQuery<StorageEntity> query = new TableQuery<StorageEntity>().Where(
                TableStorageUtilities.GetTableQuerySetString(partitionKey, rowKeys));

            var results = new List<Destination>();

            var alreadyStored = table.ExecuteQuery(query);
            foreach (StorageEntity entity in alreadyStored)
            {
                var issue = AutoMapper.Mapper.Map<Destination>(entity);

                results.Add(issue);
            }
            return results;
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

                return ExecuteQuery<TwittedLinkEntity, FeedItem>(table, feed, realKeys);

                //TableQuery<TwittedLinkEntity> query = new TableQuery<TwittedLinkEntity>().Where(
                //    TableStorageUtilities.GetTableQuerySetString(feed, realKeys));
                //var results = new List<FeedItem>();

                //var alreadyStored = table.ExecuteQuery(query);
                //foreach (TwittedLinkEntity entity in alreadyStored)
                //{
                //    var issue = AutoMapper.Mapper.Map<FeedItem>(entity);
                //    results.Add(issue);
                //}
                //return results;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return new List<Models.FeedItem>();
            }
        }

        public IList<Issue> GetRecentIssues(string organization, string repository, DateTimeOffset since)
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

        public Task StoreSessionLog(string content) {
            CloudBlobContainer container = GetLogContainer();
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(DateTime.Now.ToLongDateString());
            return null;
        }

    }
}
