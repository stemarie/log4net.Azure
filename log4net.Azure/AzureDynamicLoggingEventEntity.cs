using System;
using System.Collections;
using log4net.Core;

namespace log4net.Appender
{
	internal sealed class AzureDynamicLoggingEventEntity : ElasticTableEntity
	{
		public AzureDynamicLoggingEventEntity (LoggingEvent e, PartitionKeyTypeEnum partitionKeyType)
		{
			// NOTE - Remove fields that don't make sense in .NET Standard
			//this["Domain"] = e.Domain;
			//this["Identity"] = e.Identity;
			this["Level"] = e.Level.ToString();
			this["LoggerName"] = e.LoggerName;

			var exception = e.GetExceptionString();
			this["Message"] = string.IsNullOrWhiteSpace(exception) ? e.RenderedMessage : e.RenderedMessage + Environment.NewLine + exception;

			this["EventTimeStamp"] = e.TimeStampUtc;
			this["ThreadName"] = e.ThreadName;
			//this["UserName"] = e.UserName;
			//this["Location"] = e.LocationInformation.FullInfo;

			if (e.ExceptionObject != null) this["Exception"] = e.ExceptionObject.ToString();

			if (e.Properties != null && e.Properties.Count > 0) {
				foreach (DictionaryEntry entry in e.Properties) {
					var key = entry.Key.ToString()
							.Replace(":", "_")
							.Replace("@", "_")
							.Replace(".", "_");
					this[key] = entry.Value;
				}
			}

			Timestamp = e.TimeStampUtc;
			PartitionKey = e.MakePartitionKey(partitionKeyType);
			RowKey = e.MakeRowKey();
		}
	}
}