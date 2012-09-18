using System.Data.Services.Client;
using System.Threading.Tasks;
using log4net.Core;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;

namespace log4net.Appender.Azure
{
    public class AzureTableAppender : BufferingAppenderSkeleton
    {
        private CloudStorageAccount _account;
        private CloudTableClient _client;

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
            TableServiceContext context = _client.GetDataServiceContext();
            Parallel.ForEach(events, e => ProcessEvent(e, context));
            context.SaveChanges();
        }

        private void ProcessEvent(LoggingEvent loggingEvent, DataServiceContext context)
        {
            context.AddObject(TableName, new AzureLoggingEventEntity(loggingEvent));
        }

        private void Initialize()
        {
            _account = CloudStorageAccount.Parse(ConnectionString);
            _client = _account.CreateCloudTableClient();
            _client.CreateTableIfNotExist(TableName);
        }
    }
}