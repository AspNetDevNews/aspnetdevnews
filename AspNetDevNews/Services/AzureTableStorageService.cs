using AspNetDevNews.Services.ATStorage;
using AspNetDevNews.Services.Interfaces;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AspNetDevNews.Services
{
    public class AzureTableStorageService: IStorageService
    {
        private ISettingsService Settings { get; set; }

        public AzureTableStorageService() {
            this.Settings = new SettingsService();
        }

        public AzureTableStorageService(ISettingsService settings) {
            if (settings == null)
                throw new ArgumentNullException("settings cannot be null");
            this.Settings = settings;
        }

        //private string accountName = ConfigurationManager.AppSettings["TwittedIssuesATAccountName"];
        //private string accountKey = ConfigurationManager.AppSettings["TwittedIssuesATAccountKey"];

        private CloudTableClient GetClient() {
            StorageCredentials creds = new StorageCredentials(this.Settings.TwittedIssuesATAccountName, 
                this.Settings.TwittedIssuesATAccountKey);
            CloudStorageAccount account = new CloudStorageAccount(creds, useHttps: true);

            CloudTableClient client = account.CreateCloudTableClient();
            return client;
        }

        public enum Tables { Issues, Exception, Executions }

        private CloudTable GetIssuesTable() {
            CloudTableClient client = GetClient();

            CloudTable table = client.GetTableReference("twittedIssues");
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

        public async Task Store(List<Models.TwittedIssue> issues) {

            try
            {
                var table = GetIssuesTable();
                TableBatchOperation batchOperation = new TableBatchOperation();

                foreach (var issue in issues) {
                    var twittedIssue = new TwittedIssueEntity(issue.GetPartitionKey(), issue.GetRowKey());
                    twittedIssue.Title = issue.Title;
                    twittedIssue.Url = issue.Url;
                    twittedIssue.Labels = string.Join(";", issue.Labels);
                    twittedIssue.CreatedAt = issue.CreatedAt;
                    twittedIssue.UpdatedAt = issue.UpdatedAt;
                    twittedIssue.TweetId = issue.StatusID;
                    twittedIssue.Body = issue.Body;
                    twittedIssue.TwittedAt = DateTime.Now;
                    //TableOperation insertOperation = TableOperation.Insert(twittedIssue);
                    //TableOperation insertOperation = TableOperation.InsertOrReplace(entity);
                    //await table.ExecuteAsync(insertOperation);
                    batchOperation.Insert(twittedIssue);

                }
                if (batchOperation.Count() > 0) { 
                    var result = await table.ExecuteBatchAsync(batchOperation);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public async Task Store(Exception exception, Models.Issue issue, string operation)
        {

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

        public async Task ReportExection(DateTime StartedAt, DateTime EndedAt, int TwittedIssues, int CheckedRepositories)
        {

            try
            {
                var table = GetExcecutionsTable();

                var Report = new ExecutionEntity("execution", DateTime.Now.GetRowKey());
                Report.EndedAt = EndedAt;
                Report.StartedAt = StartedAt;
                Report.TwittedIsseues = TwittedIssues;
                Report.CheckedRepositories = CheckedRepositories;

                TableOperation insertOperation = TableOperation.Insert(Report);
                var result = await table.ExecuteAsync(insertOperation);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }


        public async Task<List<Models.Issue>> RemoveExisting(List<Models.Issue> issues) {
            List<Models.Issue> result = new List<Models.Issue>();

            try
            {
                var table = GetIssuesTable();

                foreach (var issue in issues) { 
                    TableOperation retrieveOperation = TableOperation.Retrieve<TwittedIssueEntity>(issue.GetPartitionKey(), issue.GetRowKey());

                    TableResult query = await table.ExecuteAsync(retrieveOperation);
                    if (query.Result == null)
                        result.Add(issue);
                }
                return result;
            }
            catch (Exception ex)
            {
                return result;
            }

        }

        public async Task<bool> Exists(Models.TwittedIssue issue) {
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

        //public void QueryData(Models.TwittedIssue isses, DateTime dtCreate) {
        //    try
        //    {
        //        var table = GetIssuesTable();

        //        var baseballInventoryQuery = (from entry in table.CreateQuery<TwittedIssueEntity>()
        //                                      where entry.PartitionKey == "Baseball" && entry.CreatedAt > dtCreate
        //                                      select entry);

        //        var baseballInventory = baseballInventoryQuery.ToList();

        //        if (baseballInventory.Any())
        //        {
        //            foreach (TwittedIssueEntity product in baseballInventory)
        //            {
        //                Console.WriteLine("Product: {0} as {1} items in stock", product.Title, product.RowKey);
        //            }
        //        }
        //        else
        //        {
        //            Console.WriteLine("No inventory was not found.  Better order more!");
        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine(ex);
        //    }


        //}
    }
}
