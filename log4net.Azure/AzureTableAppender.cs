using log4net.Appender.Extensions;
using log4net.Appender.Language;
using log4net.Core;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Configuration;
using System.Linq;
using Microsoft.Azure;

namespace log4net.Appender
{
    public class AzureTableAppender : BufferingAppenderSkeleton
    {
        private CloudStorageAccount _account;
        private CloudTableClient _client;
        private CloudTable _table;

        public string ConnectionStringName { get; set; }

        private string _connectionString;

        public string ConnectionString
        {
            get
            {
                if (!String.IsNullOrWhiteSpace(ConnectionStringName))
                {
                    var config = ConfigurationManager.ConnectionStrings[ConnectionStringName];
                    if (config != null)
                        return config.ConnectionString;
                    var azConfig = CloudConfigurationManager.GetSetting(ConnectionStringName);
                    if (!string.IsNullOrWhiteSpace(azConfig)) return azConfig;
                    throw new ApplicationException(Resources.AzureConnectionStringNotSpecified);
                }
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
        private bool _propAsColumn;

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

        public bool PropAsColumn
        {
            get { return _propAsColumn; }
            set { _propAsColumn = value; }
        }

        private PartitionKeyTypeEnum _partitionKeyType = PartitionKeyTypeEnum.LoggerName;
        public PartitionKeyTypeEnum PartitionKeyType
        {
            get { return _partitionKeyType; }
            set { _partitionKeyType = value; }
        }

        protected override void SendBuffer(LoggingEvent[] events)
        {
            var grouped = events.GroupBy(evt => evt.LoggerName);

            foreach (var group in grouped)
            {
                foreach (var batch in group.Batch(100))
                {
                    var batchOperation = new TableBatchOperation();
                    foreach (var azureLoggingEvent in batch.Select(GetLogEntity))
                    {
                        batchOperation.Insert(azureLoggingEvent);
                    }
                    _table.ExecuteBatch(batchOperation);
                }
            }
        }

        private ITableEntity GetLogEntity(LoggingEvent @event)
        {
            return PropAsColumn
                ? (ITableEntity)new AzureDynamicLoggingEventEntity(@event, PartitionKeyType)
                : new AzureLoggingEventEntity(@event, PartitionKeyType);
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