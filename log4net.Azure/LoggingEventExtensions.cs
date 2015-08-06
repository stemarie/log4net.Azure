using System;
using log4net.Core;

namespace log4net.Appender
{
    internal static class LoggingEventExtensions
    {
        internal static string MakeRowKey(this LoggingEvent loggingEvent)
        {
            return string.Format(
                "{0:D19}.{1}",
                 DateTime.MaxValue.Ticks - loggingEvent.TimeStamp.Ticks,
                Guid.NewGuid().ToString().ToLower());
        }

        internal static string MakePartitionKey(this LoggingEvent loggingEvent, PartitionKeyTypeEnum partitionKeyType)
        {
            switch (partitionKeyType)
            {
                case PartitionKeyTypeEnum.LoggerName:
                    return loggingEvent.LoggerName;
                case PartitionKeyTypeEnum.DateReverse:
                    // substract from DateMaxValue the Tick Count of the current hour
                    // so a Table Storage Parttition spans an hour
                    return string.Format("{0:D19}",
                        (DateTime.MaxValue.Ticks -
                         loggingEvent.TimeStamp.Date.AddHours(loggingEvent.TimeStamp.Hour).Ticks + 1));
                default:
                    throw new ArgumentOutOfRangeException(nameof(partitionKeyType), partitionKeyType, null);
            }
        }
    }
}