using System;
using Microsoft.WindowsAzure.StorageClient;
using log4net.Core;
using log4net.Util;

namespace log4net.Appender.Azure
{
    internal sealed class AzureLoggingEventEntity : TableServiceEntity
    {
        public AzureLoggingEventEntity(LoggingEvent e)
        {
            Domain = e.Domain;
            Fix = e.Fix;
            Identity = e.Identity;
            Level = e.Level;
            LocationInformation = e.LocationInformation;
            LoggerName = e.LoggerName;
            Properties = e.Properties;
            Message = e.RenderedMessage;
            ThreadName = e.ThreadName;
            TimeStamp = e.TimeStamp;
            UserName = e.UserName;

            PartitionKey = e.LoggerName;
            RowKey = e.TimeStamp.ToString("yyyy_MM_dd_HH_mm_ss_fffffff");
        }
        
        public string UserName { get; set; }

        public DateTime TimeStamp { get; set; }

        public string ThreadName { get; set; }

        public string Message { get; set; }

        public PropertiesDictionary Properties { get; set; }

        public string LoggerName { get; set; }

        public LocationInfo LocationInformation { get; set; }

        public Level Level { get; set; }

        public string Identity { get; set; }

        public FixFlags Fix { get; set; }

        public string Domain { get; set; }
    }
}