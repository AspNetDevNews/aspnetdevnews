using AspNetDevNews.Services.ATStorage;
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
    public class ATStorageService
    {
        private string accountName = ConfigurationManager.AppSettings["TwittedIssuesATAccountName"];
        private string accountKey = ConfigurationManager.AppSettings["TwittedIssuesATAccountKey"];

        private CloudTableClient GetClient() {
            StorageCredentials creds = new StorageCredentials(accountName, accountKey);
            CloudStorageAccount account = new CloudStorageAccount(creds, useHttps: true);

            CloudTableClient client = account.CreateCloudTableClient();
            return client;
        }

        private CloudTable GetTable() {
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
                var table = GetTable();
                TableBatchOperation batchOperation = new TableBatchOperation();

                foreach (var issue in issues) {
                    var twittedIssue = new TwittedIssueEntity(issue.ToPartitionKeyFormat(), issue.Number.ToString());
                    twittedIssue.Title = issue.Title;
                    twittedIssue.Url = issue.Url;
                    twittedIssue.Labels = string.Join(";", issue.Labels);
                    twittedIssue.CreatedAt = issue.CreatedAt;
                    twittedIssue.UpdatedAt = issue.UpdatedAt;
                    twittedIssue.TweetId = issue.StatusID;
                    //TableOperation insertOperation = TableOperation.Insert(twittedIssue);
                    //TableOperation insertOperation = TableOperation.InsertOrReplace(entity);
                    //await table.ExecuteAsync(insertOperation);
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

        public void Store(Exception exception, Models.Issue issue, string operation)
        {

            try
            {
                var table = GetExceptionsTable();

                var storeException = new ExceptionEntity(operation,
                    DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss-FFFFFFF"));
                storeException.TwitRowKey = issue.Organization + "+" + issue.Repository;
                storeException.TwitPartitionKey = issue.Number.ToString();
                storeException.CreatedAt = DateTime.Now;
                storeException.Exception = JsonConvert.SerializeObject(exception);
                storeException.Operation = operation;

                TableOperation insertOperation = TableOperation.Insert(storeException);
                table.Execute(insertOperation);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public void ReportExection(DateTime StartedAt, DateTime EndedAt, int TwittedIssues, int CheckedRepositories)
        {

            try
            {
                var table = GetExcecutionsTable();

                var Report = new ExecutionEntity("execution", DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss-FFFFFFF"));
                Report.EndedAt = EndedAt;
                Report.StartedAt = StartedAt;
                Report.TwittedIsseues = TwittedIssues;
                Report.CheckedRepositories = CheckedRepositories;

                TableOperation insertOperation = TableOperation.Insert(Report);
                table.Execute(insertOperation);
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
                var table = GetTable();

                foreach (var issue in issues) { 
                    TableOperation retrieveOperation = TableOperation.Retrieve<TwittedIssueEntity>(issue.ToPartitionKeyFormat(), issue.Number.ToString());

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
                var table = GetTable();

                TableOperation retrieveOperation = TableOperation.Retrieve<TwittedIssueEntity>(issue.ToPartitionKeyFormat(), issue.Number.ToString());

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

        public void QueryData(Models.TwittedIssue isses, DateTime dtCreate) {
            try
            {
                var table = GetTable();

                var baseballInventoryQuery = (from entry in table.CreateQuery<TwittedIssueEntity>()
                                              where entry.PartitionKey == "Baseball" && entry.CreatedAt > dtCreate
                                              select entry);

                var baseballInventory = baseballInventoryQuery.ToList();

                if (baseballInventory.Any())
                {
                    foreach (TwittedIssueEntity product in baseballInventory)
                    {
                        Console.WriteLine("Product: {0} as {1} items in stock", product.Title, product.RowKey);
                    }
                }
                else
                {
                    Console.WriteLine("No inventory was not found.  Better order more!");
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }


        }
    }
}
