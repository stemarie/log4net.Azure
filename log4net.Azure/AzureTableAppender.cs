using System.Data.Services.Client;
using System.Threading.Tasks;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;
using log4net.Core;

namespace log4net.Appender.Azure
{
    public class AzureTableAppender : BufferingAppenderSkeleton
    {
        private CloudTableClient _client;
        private CloudStorageAccount _account;
        public string ConnectionString { get; set; }
        public string TableName { get; set; }

        public AzureTableAppender()
            : base()
        {
            Initialize();
        }

        public AzureTableAppender(string connectionString, string tableName)
            : base()
        {
            ConnectionString = connectionString;
            TableName = tableName;

            Initialize();
        }

        protected override void SendBuffer(LoggingEvent[] events)
        {
            TableServiceContext context = _client.GetDataServiceContext();
            Parallel.ForEach(events, e=> ProcessEvent(e,context));
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