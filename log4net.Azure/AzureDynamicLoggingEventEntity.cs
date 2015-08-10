using System;
using System.Collections;
using System.Globalization;
using log4net.Core;

namespace log4net.Appender
{
    internal sealed class AzureDynamicLoggingEventEntity : ElasticTableEntity
    {
        public AzureDynamicLoggingEventEntity(LoggingEvent e)
        {
            this["Domain"] = e.Domain;
            this["Identity"] = e.Identity;
            this["Level"] = e.Level.ToString();
            this["LoggerName"] = e.LoggerName;
            this["Message"] = e.RenderedMessage + Environment.NewLine + e.GetExceptionString();
            this["ThreadName"] = e.ThreadName;
            this["UserName"] = e.UserName;
            this["Location"] = e.LocationInformation.FullInfo;

            if (e.ExceptionObject != null)
            {
                this["Exception"] = e.ExceptionObject.ToString();
            }

            Timestamp = e.TimeStamp;
            PartitionKey = e.LoggerName;
            RowKey = MakeRowKey(e);
            
            foreach (DictionaryEntry entry in e.Properties)
            {
                var key = entry.Key.ToString()
                    .Replace(":", "_")
                    .Replace("@", "_")
                    .Replace(".", "_");
                this[key] = entry.Value;
            }
        }

        private static string MakeRowKey(LoggingEvent loggingEvent)
        {
            return string.Format("{0}.{1}",
                loggingEvent.TimeStamp.ToString("yyyy_MM_dd_HH_mm_ss_fffffff",
                    DateTimeFormatInfo.InvariantInfo),
                Guid.NewGuid().ToString().ToLower());
        }
    }
}