using System;
using System.Linq;
using log4net.Appender.Language;
using log4net.Core;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace log4net.Appender
{
    public class AzureTableAppender : BufferingAppenderSkeleton
    {
        private CloudStorageAccount _account;
        private CloudTableClient _client;
        private CloudTable _table;

        private string _connectionString;

        public string ConnectionString
        {
            get
            {
                if (String.IsNullOrEmpty(_connectionString))
                    throw new ApplicationException(Resources.AzureConnectionStringNotSpecified);
                return _connectionString;
            }
            set
            {
                _connectionString = value;
            }
        }

        private string _tableName;

        public string TableName
        {
            get
            {
                if (String.IsNullOrEmpty(_tableName))
                    throw new ApplicationException(Resources.TableNameNotSpecified);
                return _tableName;
            }
            set
            {
                _tableName = value;
            }
        }

        protected override void SendBuffer(LoggingEvent[] events)
        {
            

            //Batched ops require single partition key, group
            //by loggername to obey requirment.
            var grouped = events.GroupBy(evt => evt.LoggerName);

            foreach (var group in grouped)
            {
                var batchOperation = new TableBatchOperation();
                foreach (var azureLoggingEvent in group.Select(@event => new AzureLoggingEventEntity(@event)))
                {
                    batchOperation.Insert(azureLoggingEvent);
                }
                _table.ExecuteBatch(batchOperation);
            }
            
            
        }

        public override void ActivateOptions()
        {
            base.ActivateOptions();

            _account = CloudStorageAccount.Parse(ConnectionString);
            _client = _account.CreateCloudTableClient();
            _table = _client.GetTableReference(TableName);
            _table.CreateIfNotExists();
        }
    }
}