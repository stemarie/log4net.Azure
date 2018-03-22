using System;
using System.Collections;
using System.Text;
using log4net.Core;
using Microsoft.WindowsAzure.Storage.Table;

namespace log4net.Appender
{
	internal sealed class AzureLoggingEventEntity : TableEntity
	{
		public AzureLoggingEventEntity (LoggingEvent e, PartitionKeyTypeEnum partitionKeyType)
		{
			// NOTE - Remove fields that don't make sense in .NET Standard
			//Domain = e.Domain;
			//Identity = e.Identity;
			Level = e.Level.ToString();

			if (e.Properties != null && e.Properties.Count > 0) {
				var sb = new StringBuilder();
				foreach (DictionaryEntry entry in e.Properties) {
					sb.AppendFormat("{0}:{1}", entry.Key, entry.Value);
					sb.AppendLine();
				}
				Properties = sb.ToString();
			}

			var exception = e.GetExceptionString();
			Message = string.IsNullOrWhiteSpace(exception) ? e.RenderedMessage : e.RenderedMessage + Environment.NewLine + exception;
			ThreadName = e.ThreadName;
			EventTimeStamp = e.TimeStampUtc;
			//UserName = e.UserName;
			//Location = e.LocationInformation.FullInfo;
			//ClassName = e.LocationInformation.ClassName;
			//FileName = e.LocationInformation.FileName;
			//LineNumber = e.LocationInformation.LineNumber;
			//MethodName = e.LocationInformation.MethodName;
			// TODO - No stack frames for .NET Standard?
			//StackFrames = e.LocationInformation.StackFrames;

			if (e.ExceptionObject != null) Exception = e.ExceptionObject.ToString();

			PartitionKey = e.MakePartitionKey(partitionKeyType);
			RowKey = e.MakeRowKey();
		}

		//public string UserName { get; set; }

		public DateTime EventTimeStamp { get; set; }

		public string ThreadName { get; set; }

		public string Message { get; set; }

		public string Properties { get; set; }

		public string Level { get; set; }

		//public string Identity { get; set; }

		//public string Domain { get; set; }

		//public string Location { get; set; }

		public string Exception { get; set; }

		//public string ClassName { get; set; }

		//public string FileName { get; set; }

		//public string LineNumber { get; set; }

		//public string MethodName { get; set; }

		//public StackFrameItem[] StackFrames { get; set; }
	}
}
