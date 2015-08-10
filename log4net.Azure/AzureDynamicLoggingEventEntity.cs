using System;
using System.Collections;
using log4net.Core;

namespace log4net.Appender
{
    internal sealed class AzureDynamicLoggingEventEntity : ElasticTableEntity
    {
        public AzureDynamicLoggingEventEntity(LoggingEvent e, PartitionKeyTypeEnum partitionKeyType)
        {
            this["Domain"] = e.Domain;
            this["Identity"] = e.Identity;
            this["Level"] = e.Level.ToString();
            this["LoggerName"] = e.LoggerName;
            this["Message"] = e.RenderedMessage + Environment.NewLine + e.GetExceptionString();
            this["EventTimeStamp"] = e.TimeStamp;
            this["ThreadName"] = e.ThreadName;
            this["UserName"] = e.UserName;
            this["Location"] = e.LocationInformation.FullInfo;

            if (e.ExceptionObject != null)
            {
                this["Exception"] = e.ExceptionObject.ToString();
            }
            
            foreach (DictionaryEntry entry in e.Properties)
            {
                var key = entry.Key.ToString()
                    .Replace(":", "_")
                    .Replace("@", "_")
                    .Replace(".", "_");
                this[key] = entry.Value;
            }

            Timestamp = e.TimeStamp;
            PartitionKey = e.MakePartitionKey(partitionKeyType);
            RowKey = e.MakeRowKey();
        }
    }
}