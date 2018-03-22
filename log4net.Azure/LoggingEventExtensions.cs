using System;
using log4net.Core;

namespace log4net.Appender
{
	internal static class LoggingEventExtensions
	{
		internal static string MakeRowKey (this LoggingEvent loggingEvent)
		{
			return string.Format(
					"{0:D19}.{1}",
					 DateTime.MaxValue.Ticks - loggingEvent.TimeStampUtc.Ticks,
					Guid.NewGuid().ToString().ToLower());
		}

		internal static string MakePartitionKey (this LoggingEvent loggingEvent, PartitionKeyTypeEnum partitionKeyType)
		{
			switch (partitionKeyType) {
				case PartitionKeyTypeEnum.LoggerName:
					return loggingEvent.LoggerName;
				case PartitionKeyTypeEnum.DateReverse:
					// substract from DateMaxValue the Tick Count of the current hour
					// so a Table Storage Partition spans an hour
					return string.Format("{0:D19}",
							(DateTime.MaxValue.Ticks -
							 loggingEvent.TimeStampUtc.Date.AddHours(loggingEvent.TimeStampUtc.Hour).Ticks + 1));
				default:
					// ReSharper disable once NotResolvedInText
					throw new ArgumentOutOfRangeException("PartitionKeyType", partitionKeyType, null);
			}
		}
	}
}