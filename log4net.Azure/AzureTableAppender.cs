using System.Linq;
using log4net.Core;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace log4net.Appender.Azure
{
    public class AzureTableAppender : BufferingAppenderSkeleton
    {
        private CloudStorageAccount _account;
        private CloudTableClient _client;
        private CloudTable _table;

        public AzureTableAppender()
        {
            Initialize();
        }

        public AzureTableAppender(string connectionString, string tableName)
        {
            ConnectionString = connectionString;
            TableName = tableName;

            Initialize();
        }

        public string ConnectionString { get; set; }
        public string TableName { get; set; }

        protected override void SendBuffer(LoggingEvent[] events)
        {
            var batchOperation = new TableBatchOperation();
            foreach (var azureLoggingEvent in events.Select(@event => new AzureLoggingEventEntity(@event)))
            {
                batchOperation.Insert(azureLoggingEvent);
            }
            _table.ExecuteBatch(batchOperation);
        }



        private void Initialize()
        {
            _account = CloudStorageAccount.Parse(ConnectionString);
            _client = _account.CreateCloudTableClient();
            _table = _client.GetTableReference(TableName);
            _table.CreateIfNotExists();
        }
    }
}