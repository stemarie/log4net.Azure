using log4net.Appender.Extensions;
using log4net.Core;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using log4net.Util;

namespace log4net.Appender
{
    public class AsyncAzureTableAppender : AzureTableAppender
    {
        // track the tasks currently sending data so we can wait for them when we close down
        private readonly List<Task> _outstandingTasks = new List<Task>();

        // used to calculate the retry interval
        private readonly Random _rnd = new Random();

        // auto-flush timer
        private Timer _autoFlushTimer;

        public int BatchSize { get; set; } = 100;
        public int RetryCount { get; set; } = 5;
        public TimeSpan RetryWait { get; set; } = new TimeSpan(0, 0, 5);
        public TimeSpan FlushInterval { get; set; } = new TimeSpan(0, 1, 0);
        public int MaxMessageSize { get; set; } = 16000;

        protected override void SendBuffer(LoggingEvent[] events)
        {
            // build chunks of no more than 100 each of which share the same partition key
            var chunks = events.SelectMany(GetLogEntities).GroupBy(e => e.PartitionKey).SelectMany(i => i.Batch(100)).ToList();
            var tasks = chunks.Select(chunk => Task.Run(async () => await Send(chunk))).ToList();

            // remember the tasks
            lock (_outstandingTasks)
            {
                _outstandingTasks.AddRange(tasks);
            }

            // remove from the list when complete
            tasks.ForEach(t => t.ContinueWith(_ =>
            {
                lock (_outstandingTasks)
                {
                    _outstandingTasks.Remove(t);
                }
            }));
        }

        private async Task Send(IEnumerable<ITableEntity> chunk)
        {
            var batchOperation = new TableBatchOperation();
            foreach (var azureLoggingEvent in chunk)
            {
                batchOperation.Insert(azureLoggingEvent);
            }

            var attempt = 0;
            while (true)
            {
                try
                {
                    var sw = System.Diagnostics.Stopwatch.StartNew();
                    await Table.ExecuteBatchAsync(batchOperation);
                    LogLog.Debug(typeof(AsyncAzureTableAppender), string.Format("Sent batch of {0} in {1}", batchOperation.Count, sw.Elapsed));
                    return;
                }
                catch (Exception ex)
                {
                    attempt++;
                    if (attempt >= RetryCount)
                    {
                        LogLog.Error(typeof(AsyncAzureTableAppender), string.Format("Exception sending batch, aborting: {0}", ex.Message));
                        return;
                    }

                    LogLog.Warn(typeof(AsyncAzureTableAppender), string.Format("Exception sending batch, retrying: {0}", ex.Message));

                    // wait for a bit longer each time, and add a bit of randomness to make sure we're not retrying in lockstep
                    var wait = TimeSpan.FromSeconds(RetryWait.TotalSeconds * (attempt + GetExtraWaitModifier()));
                    await Task.Delay(wait);
                }
            }
        }

        private double GetExtraWaitModifier()
        {
            lock (_rnd)
            {
                return _rnd.NextDouble();
            }
        }

        public override void ActivateOptions()
        {
            base.ActivateOptions();

            _autoFlushTimer = new Timer(s =>
            {
                LogLog.Debug(typeof(AsyncAzureTableAppender), "Triggering flush");
                this.Flush(false);
            }, null, TimeSpan.FromSeconds(0), FlushInterval);
        }

        protected override void OnClose()
        {
            LogLog.Debug(typeof(AsyncAzureTableAppender), "Closing");
            if (null != _autoFlushTimer)
            {
                _autoFlushTimer.Dispose();
                _autoFlushTimer = null;
            }
            base.OnClose();

            // the close would have triggered a flush, which would have created messages in the queue.  Wait until they're all done.
            Task[] tasks;
            lock (_outstandingTasks)
            {
                tasks = _outstandingTasks.ToArray();
            }
            LogLog.Debug(typeof(AsyncAzureTableAppender), string.Format("Waiting on {0} outstanding logging calls", tasks.Length));
            Task.WaitAll(tasks);
            LogLog.Debug(typeof(AsyncAzureTableAppender), "Completing close");
        }

        private ITableEntity[] GetLogEntities(LoggingEvent @event)
        {
            var baseEntity = this.GetLogEntity(@event);

            var message = GetMessage(baseEntity) ?? "";
            if (message.Length <= MaxMessageSize)
                return new[] { baseEntity };

            var messageParts = message.Batch(MaxMessageSize).Select(i => new string(i.ToArray())).ToList();
            return messageParts.Select((m, ix) =>
            {
                // build the entity with updated message/Sequence number
                var entity = this.GetLogEntity(@event, m, ix);

                // setup the RowKey such that you could sort descending and reassemble
                entity.RowKey = string.Format("{0}.{1:d5}", baseEntity.RowKey, messageParts.Count - ix - 1);

                return entity;
            }).ToArray();
        }

        protected ITableEntity GetLogEntity(LoggingEvent @event, string message, int sequenceNumber)
        {
            if (Layout != null)
            {
                return new AzureLayoutLoggingEventEntity(@event, PartitionKeyType, Layout, message, sequenceNumber);
            }

            return PropAsColumn
                ? (ITableEntity)new AzureDynamicLoggingEventEntity(@event, PartitionKeyType, message, sequenceNumber)
                : new AzureLoggingEventEntity(@event, PartitionKeyType, message, sequenceNumber);
        }

        private string GetMessage(ITableEntity entity)
        {
            var eventEntity = entity as AzureDynamicLoggingEventEntity;
            if (eventEntity != null)
            {
                return (string)eventEntity["message"];
            }
            var layoutEvent = entity as AzureLayoutLoggingEventEntity;
            if (layoutEvent != null)
            {
                return layoutEvent.Message;
            }
            var loggingEvent = entity as AzureLoggingEventEntity;
            if (loggingEvent != null)
            {
                return loggingEvent.Message;
            }

            throw new NotSupportedException();
        }
    }
}