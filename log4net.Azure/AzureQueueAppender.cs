using log4net.Appender.Language;
using log4net.Core;
using log4net.Layout;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace log4net.Appender
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AzureQueueAppender" /> class.
    /// </summary>
    /// <remarks>
    /// The instance of the <see cref="AzureQueueAppender" /> class is set up to write 
    /// to an azure storage queue
    /// </remarks>
    public class AzureQueueAppender : BufferingAppenderSkeleton
    {
        private CloudStorageAccount _account;
        private CloudQueueClient _queueClient;
        private CloudQueue _queue;
        public string ConnectionStringName { get; set; }
        private string _connectionString;

        public AzureQueueAppender()
        {
            PatternLayout layout = new PatternLayout();
            layout.ConversionPattern = PatternLayout.DetailConversionPattern;
            layout.ActivateOptions();
            Layout = layout;
        }

        public string ConnectionString
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(ConnectionStringName))
                {
                    var config = ConfigurationManager.ConnectionStrings[ConnectionStringName];
                    if (config != null)
                        return config.ConnectionString;
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

        private string _queueName;
        public string QueueName
        {
            get
            {
                if (String.IsNullOrEmpty(_queueName))
                    throw new ApplicationException(Resources.QueueNameNotSpecified);
                return _queueName;
            }
            set
            {
                _queueName = value;
            }
        }

        /// <summary>
        /// Sends the events.
        /// </summary>
        /// <param name="events">The events that need to be send.</param>
        /// <remarks>
        /// <para>
        /// The subclass must override this method to process the buffered events.
        /// </para>
        /// </remarks>
        protected override void SendBuffer(LoggingEvent[] events)
        {
            Parallel.ForEach(events, ProcessEvent);
        }

        private void ProcessEvent(LoggingEvent loggingEvent)
        {
            // Create a message and add it to the queue.
            CloudQueueMessage message = new CloudQueueMessage(RenderLoggingEvent(loggingEvent));
            _queue.AddMessage(message);
        }
        
        /// <summary>
        /// This appender requires a <see cref="Layout"/> to be set.
        /// </summary>
        /// <value><c>true</c></value>
        /// <remarks>
        /// <para>
        /// This appender requires a <see cref="Layout"/> to be set.
        /// </para>
        /// </remarks>
        override protected bool RequiresLayout
        {
            get { return true; }
        }
        /// <summary>
        /// Initialize the appender based on the options set
        /// </summary>
        /// <remarks>
        /// <para>
        /// This is part of the <see cref="T:log4net.Core.IOptionHandler"/> delayed object
        ///             activation scheme. The <see cref="M:log4net.Appender.BufferingAppenderSkeleton.ActivateOptions"/> method must 
        ///             be called on this object after the configuration properties have
        ///             been set. Until <see cref="M:log4net.Appender.BufferingAppenderSkeleton.ActivateOptions"/> is called this
        ///             object is in an undefined state and must not be used. 
        /// </para>
        /// <para>
        /// If any of the configuration properties are modified then 
        ///             <see cref="M:log4net.Appender.BufferingAppenderSkeleton.ActivateOptions"/> must be called again.
        /// </para>
        /// </remarks>
        public override void ActivateOptions()
        {
            base.ActivateOptions();

            _account = CloudStorageAccount.Parse(ConnectionString);
            _queueClient = _account.CreateCloudQueueClient();
            _queue = _queueClient.GetQueueReference(QueueName.ToLower());
            _queue.CreateIfNotExists();
        }
    }
}
