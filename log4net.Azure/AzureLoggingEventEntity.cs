using System;
using System.Collections;
using System.Globalization;
using System.Text;
using Microsoft.WindowsAzure.Storage.Table;
using log4net.Core;

namespace log4net.Appender.Azure
{
    internal sealed class AzureLoggingEventEntity : TableEntity
    {
        public AzureLoggingEventEntity(LoggingEvent e)
        {
            Domain = e.Domain;
            Identity = e.Identity;
            Level = e.Level;
            LoggerName = e.LoggerName;
            var sb = new StringBuilder(e.Properties.Count);
            foreach (DictionaryEntry entry in e.Properties)
            {
                sb.AppendFormat("{0}:{1}", entry.Key, entry.Value);
                sb.AppendLine();
            }
            Properties = sb.ToString();
            Message = e.RenderedMessage;
            ThreadName = e.ThreadName;
            TimeStamp = e.TimeStamp;
            UserName = e.UserName;

            PartitionKey = e.LoggerName;
            RowKey = MakeRowKey(e);
        }

        private static string MakeRowKey(LoggingEvent loggingEvent)
        {
            return string.Format("{0}.{1}",
                                 loggingEvent.TimeStamp.ToString("yyyy_MM_dd_HH_mm_ss_fffffff",
                                                                 DateTimeFormatInfo.InvariantInfo),
                                 Guid.NewGuid().ToString().ToLower());
        }

        public string UserName { get; set; }

        public DateTime TimeStamp { get; set; }

        public string ThreadName { get; set; }

        public string Message { get; set; }

        public string Properties { get; set; }

        public string LoggerName { get; set; }

        public Level Level { get; set; }

        public string Identity { get; set; }

        public string Domain { get; set; }
    }
}