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
        public string Error { get; set; }

        public AzureTableStorageService(ISettingsService settings) {
            if (settings == null)
                throw new ArgumentNullException( nameof(settings), "cannot be null");

            this.Settings = settings;
        }

        #region retrieve Azure Resources
        private CloudTableClient GetTableClient() {
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

            CloudBlobContainer container = client.GetContainerReference("sessionlog");

            container.CreateIfNotExists();
            return container;
        }


        private CloudTable GetIssuesTable()
        {
            CloudTableClient client = GetTableClient();

            CloudTable table = client.GetTableReference("twittedIssues");

            table.CreateIfNotExists();
            return table;
        }

        private CloudTable GetLinksTable()
        {
            CloudTableClient client = GetTableClient();

            CloudTable table = client.GetTableReference("twittedLinks");

            table.CreateIfNotExists();
            return table;
        }

        private CloudTable GetExceptionsTable()
        {
            CloudTableClient client = GetTableClient();

            CloudTable table = client.GetTableReference("exceptions");
            table.CreateIfNotExists();
            return table;
        }

        private CloudTable GetExcecutionsTable()
        {
            CloudTableClient client = GetTableClient();

            CloudTable table = client.GetTableReference("executions");
            table.CreateIfNotExists();
            return table;
        }

        private CloudTable GetDocumentsTable()
        {
            CloudTableClient client = GetTableClient();

            CloudTable table = client.GetTableReference("twittedGitHubDocuments");
            table.CreateIfNotExists();
            return table;
        }

        #endregion

        #region insert and update entities
        public async Task Store(IList<TwittedIssue> issues)
        {
            await Store<TwittedIssue, IssueEntity>(issues, GetIssuesTable());
        }

        public async Task Store(IList<TwittedPost> posts)
        {
            await Store<TwittedPost, LinkEntity>(posts, GetLinksTable());
        }

        public async Task Store(IList<TwittedGitHubHostedDocument> documents)
        {
            await Store<TwittedGitHubHostedDocument, GitHubHostedDocumentEntity>(documents, GetDocumentsTable());
        }

        public enum OperationType { Insert, Replace, Merge};

        protected virtual async Task Store<Source, Destination>(IList<Source> documents, CloudTable destinationTable, OperationType operationType = OperationType.Insert) 
            where Destination: ITableEntity
        {
            if (documents == null || documents.Count == 0)
                return;
            if (destinationTable == null )
                throw new ArgumentNullException( nameof(destinationTable), "cannot be null");

            try
            {
                TableBatchOperation batchOperation = new TableBatchOperation();

                foreach (var document in documents)
                {
                    var twittedIssue = AutoMapper.Mapper.Map<Destination>(document);
                    switch (operationType) {
                        case OperationType.Insert:
                            batchOperation.Insert(twittedIssue);
                            //TableOperation insertOperation = TableOperation.Insert(twittedIssue);
                            //await destinationTable.ExecuteAsync(insertOperation);
                            break;
                        case OperationType.Replace:
                            batchOperation.Replace(twittedIssue);
                            //TableOperation replaceOperation = TableOperation.Replace(twittedIssue);
                            //await destinationTable.ExecuteAsync(replaceOperation);
                            break;
                        case OperationType.Merge:
                            batchOperation.Merge(twittedIssue);
                            //TableOperation mergeOperation = TableOperation.Merge(twittedIssue);
                            //await destinationTable.ExecuteAsync(mergeOperation);
                            break;
                    }
                }
                var result = await destinationTable.ExecuteBatchAsync(batchOperation);
            }
            catch (Exception exc)
            {
                Console.Error.WriteLine(exc.Message);
                Error = exc.Message;
            }
        }

        public async Task Merge(IList<Issue> issues)
        {
            await Store<Issue, IssueMergeEntity>(issues, GetIssuesTable(), OperationType.Merge);
        }

        #endregion

        public async Task Store(Exception exception, Interfaces.ITableStorageKeyGet record, string operation)
        {
            if (exception == null)
                return;
            if (record == null)
                return;
            if (string.IsNullOrWhiteSpace(operation))
                return;

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
            catch (Exception exc)
            {
                Console.Error.WriteLine(exc.Message);
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
            catch (Exception exc)
            {
                Console.Error.WriteLine(exc.Message);
            }
        }

        public async Task<bool> Exists(TwittedIssue issue) {
            try
            {
                var table = GetIssuesTable();

                TableOperation retrieveOperation = TableOperation.Retrieve<IssueEntity>(issue.GetPartitionKey(), issue.GetRowKey());

                TableResult query = await table.ExecuteAsync(retrieveOperation);

                if (query.Result != null)
                {
                    Console.WriteLine("Product: {0}", ((IssueEntity)query.Result).Title);
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
            if (string.IsNullOrWhiteSpace(organization))
                return new List<Issue>();
            if (string.IsNullOrWhiteSpace(repository))
                return new List<Issue>();
            if (rowKeys == null || rowKeys.Count == 0)
                return new List<Issue>();
            try
            {
                var table = GetIssuesTable();
                var dummy = new Issue();
                dummy.Organization = organization;
                dummy.Repository = repository;

                return ExecuteQuery<IssueEntity, Issue>(table, dummy.GetPartitionKey(), rowKeys);

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
            catch (Exception exc)
            {
                Console.Error.WriteLine(exc.Message);
                return new List<Models.Issue>();
            }
        }

        public IList<GitHubHostedDocument> GetBatchDocuments(string organization, string repository, IList<string> rowKeys)
        {
            if (string.IsNullOrWhiteSpace(organization))
                return new List<GitHubHostedDocument>();
            if (string.IsNullOrWhiteSpace(repository))
                return new List<GitHubHostedDocument>();
            if (rowKeys == null || rowKeys.Count == 0)
                return new List<GitHubHostedDocument>();

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

                return ExecuteQuery<GitHubHostedDocumentEntity, GitHubHostedDocument>(table, dummy.GetPartitionKey(), rowKeys);

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
            catch (Exception exc)
            {
                Console.Error.WriteLine(exc.Message);
                return new List<Models.GitHubHostedDocument>();
            }
        }

        protected virtual IList<Destination> ExecuteQuery<StorageEntity, Destination>(CloudTable table, string partitionKey, IList<string>rowKeys) 
            where StorageEntity : TableEntity, new ()
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table), "cannot be null");
            if (string.IsNullOrWhiteSpace(partitionKey))
                throw new ArgumentNullException(nameof(partitionKey), "cannot be null");
            if (rowKeys == null || rowKeys.Count == 0)
                throw new ArgumentNullException(nameof(rowKeys), "cannot be null");

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
            if (string.IsNullOrWhiteSpace(feed))
                return new List<FeedItem>();
            if (rowKeys == null || rowKeys.Count == 0)
                return new List<FeedItem>();

            try
            {
                var table = GetLinksTable();

                List<string> realKeys = new List<string>();
                for (int i = 0; i < rowKeys.Count; i++)
                    realKeys.Add(TableStorageUtilities.EncodeToKey(rowKeys[i]));
                feed = TableStorageUtilities.EncodeToKey(feed);

                return ExecuteQuery<LinkEntity, FeedItem>(table, feed, realKeys);

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
            catch (Exception exc)
            {
                Console.Error.WriteLine(exc.Message);
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

                var recentIssuesQuery = (from entry in table.CreateQuery<IssueEntity>()
                    where entry.PartitionKey == partitionKey && entry.CreatedAt > since.DateTime
                                              select entry);
                // using async method raises an exception
                var recentIssues = recentIssuesQuery.ToList();

                var results = new List<Issue>();

                if (recentIssues.Any())
                {
                    foreach (IssueEntity entity in recentIssues)
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

        public void StoreSessionLog(string content) {
            CloudBlobContainer container = GetLogContainer();
            string blobName = DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString();
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(blobName);
            blockBlob.UploadText(content);
        }

    }
}
