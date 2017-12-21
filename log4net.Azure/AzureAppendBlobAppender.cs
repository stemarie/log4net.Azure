using System;
using System.IO;
using System.Text;
using System.Configuration;
using System.Globalization;
using System.Threading.Tasks;
using log4net.Appender.Extensions;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using log4net.Appender.Language;
using log4net.Core;
using Microsoft.Azure;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using log4net.Util;

namespace log4net.Appender
{
    public class AzureAppendBlobAppender : BufferingAppenderSkeleton
    {
        private CloudStorageAccount _account;
        private CloudBlobClient _client;
        private CloudBlobContainer _cloudBlobContainer;

        public string ConnectionStringName { get; set; }
        private string _connectionString;
        private string _lineFeed = "";

        /// <summary>
        /// The fully qualified type of the RollingFileAppender class.
        /// </summary>
        /// <remarks>
        /// Used by the internal logger to record the Type of the
        /// log message.
        /// </remarks>
        private readonly static Type declaringType = typeof(AzureAppendBlobAppender);

        override protected void Append(LoggingEvent loggingEvent)
        {           

            string message = loggingEvent.GetXmlString(Layout, IncludeBasicLogging);

            //Making copy of loggingEvent with new Message with Layout details
            LoggingEventData loggingEventData = loggingEvent.GetLoggingEventData();
            loggingEventData.Message = message;

            LoggingEvent newLog = new LoggingEvent(loggingEventData);            

            base.Append(newLog);

        }

        public string ConnectionString
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(ConnectionStringName))
                {
                    return Util.GetConnectionString(ConnectionStringName);
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
        
        private string _containerName;

        public string ContainerName
        {
            get
            {
                if (String.IsNullOrEmpty(_containerName))
                    throw new ApplicationException(Resources.ContainerNameNotSpecified);
                return _containerName;
            }
            set
            {
                _containerName = value;
            }
        }

        private string _directoryName;

        public string DirectoryName
        {
            get
            {
                if (String.IsNullOrEmpty(_directoryName))
                    throw new ApplicationException(Resources.DirectoryNameNotSpecified);
                return _directoryName;
            }
            set
            {
                _directoryName = value;
            }
        }

        private string _fileName;
        
        /// <summary>
        /// Configured file name
        /// </summary>
        public string FileName { get; set; }


        /// <summary>
        /// Provides custom file name
        /// if FileName is not provided ==> file name will be yyyy_MM_dd.entry.log.xml 
        /// isDateRollOverEnabled=true ==> will provide yyyyMMdd.FileName
        /// </summary>
        private string CurrentFileName
        {
            get 
            {
                //bool isDateRollOverEnabled = Convert.ToBoolean(RollOverDailyEnabled);
                string today = DateTime.Today.ToString("yyyyMMdd", DateTimeFormatInfo.InvariantInfo);

                if (String.IsNullOrEmpty(FileName))
                {
                    _fileName = string.Format("{0}.entry.log.xml",
                                        DateTime.Today.ToString("yyyy_MM_dd", DateTimeFormatInfo.InvariantInfo));
                }
                //Check : FileName should not contain "today" - required so that we do not append "today" everytime
                else if (RollOverDailyEnabled && (_fileName == null || !_fileName.Contains(today)))
                {
                    string lastCreatedFile = GetLastFileCreatedToday();
                    _fileName = lastCreatedFile;
                    if (_fileName == null)
                    {
                        _fileName = string.Format("{0}.{1}_{2}", DateTime.Today.ToString("yyyyMMdd", DateTimeFormatInfo.InvariantInfo), FileName, 1); 
                    }                    
                }
                return _fileName;
            }
            set
            {
                _fileName = value;

            }
        }


        public bool IncludeBasicLogging { get; set; }


        /// <summary>
        /// Specifies if Logging needs to be rolled over daily
        /// Value : Boolean
        /// </summary>
        public bool RollOverDailyEnabled { get; set; }

        /// <summary>
        /// Specifies the number of log event which should be sent in one append block operation
        /// value : INT
        /// </summary>
        public string LogEventsCountInBlock { get; set; }


        /// <summary>
        /// Sends the events.
        /// </summary>
        /// <param name="events">The events that need to be send.</param>
        /// <remarks>
        /// <para>
        /// The subclass must override this method to process the buffered events.
        /// </para>
        /// </remarks>
        ///         
        protected override void SendBuffer(LoggingEvent[] events)
        {            
            CloudAppendBlob appendBlob = _cloudBlobContainer.GetAppendBlobReference(BlobName(DirectoryName, CurrentFileName));
            LogLog.Debug(declaringType, "Preparing the blob for logging");

            if (!appendBlob.Exists()) 
            {
                LogLog.Debug(declaringType, "Creating Append Blob :::" + DirectoryName + "/" + CurrentFileName);
                appendBlob.CreateOrReplace();
            }
            else
            {
                LogLog.Debug(declaringType, "Using existing Append Blob :::" + DirectoryName + "/" + CurrentFileName);
               int blockCount = getBlockCount(appendBlob);

                //Azure append blob can take in only 50,000 blocks. Hence, this check
               if (blockCount > 49800)
                {
                                                          
                    int separatorIndex = CurrentFileName.LastIndexOf('_');

                    int lastFileIndex = 0;
                    Int32.TryParse(CurrentFileName.Substring(separatorIndex + 1), out lastFileIndex);

                   int nextFileIndex = lastFileIndex + 1;
                   string tempFileName = CurrentFileName.Remove(separatorIndex + 1) + nextFileIndex;

                    appendBlob = _cloudBlobContainer.GetAppendBlobReference(BlobName(DirectoryName, tempFileName));
                    LogLog.Debug(declaringType, "Trying to create blob file" + DirectoryName + "/" + tempFileName);
                    if (!appendBlob.Exists())
                    {
                        appendBlob.CreateOrReplace();
                        CurrentFileName = tempFileName;                     
                    }

                }
            }
            FlushLogs(events, appendBlob);
        }


        private void FlushLogs(LoggingEvent[] events, CloudAppendBlob appendBlob)
        {
            LogLog.Debug(declaringType, "Flusing events into the blob");
            var memoryStream = new MemoryStream();

            try
            {
                foreach (var loggingEvent in events)
                {
                    var xml = loggingEvent.RenderedMessage;

                    memoryStream.Write(Encoding.UTF8.GetBytes(xml), 0, xml.Length);

                    //This check is added because - an append operation to Azure blob can take only data of size 4MB
                    if (memoryStream.Length > 3950000)
                    {
                        memoryStream.Position = 0;
                        appendBlob.AppendBlock(memoryStream);
                        memoryStream.SetLength(0);
                    }
                }
                if (memoryStream.Length > 0)
                {
                    memoryStream.Position = 0;
                    appendBlob.AppendBlock(memoryStream);
                    memoryStream.SetLength(0);
                }

            }
            catch (Exception exception)
            {
                string log = string.Format("ERROR - Error occured while appending Blob on {0} size ={1} bytes: msg={2}, stacktrace={3}",
                    DateTime.Today, memoryStream.Length, exception.Message, exception.StackTrace);
                appendBlob.AppendText(log);
            }
            finally
            {
                LogLog.Debug(declaringType, "*Completed* flushing events into the blob");
                memoryStream.SetLength(0);
            }
        }

        /// <summary>
        /// Get the last file index created today
        /// This is needed when server restart happens
        /// </summary>
        /// <returns></returns>
        private string GetLastFileCreatedToday()
        {
            var cloudDir = _cloudBlobContainer.GetDirectoryReference(_directoryName);
            var blobList = cloudDir.ListBlobs(useFlatBlobListing: true);

            string file = null;
            int maxIndex = 0;

            string today = DateTime.Today.ToString("yyyyMMdd", DateTimeFormatInfo.InvariantInfo);

            foreach (var blob in blobList)
            {

                var fileName = blob.Uri.Segments[blob.Uri.Segments.Length - 1];

                if (fileName.Contains(today))
                {
                    if (file == null)
                    {
                        file = fileName;
                    }

                    int index = fileName.LastIndexOf('_');
                    if (index > 0)
                    {
                        int lastIndex = 0;
                        Int32.TryParse(fileName.Substring(index+1), out lastIndex);
                        if (lastIndex > maxIndex)
                        {
                            maxIndex = lastIndex;
                            file = fileName;
                        }                        
                    }
                }

            }
            return file;
        }


        private int getBlockCount(CloudAppendBlob appendBlob)
        {
            if (appendBlob == null)
            {
                LogLog.Error(declaringType, "appendBlob does not exist. Check the connection string. Returning BlockCount=0");
                return 0; // This should not occur
            }
            appendBlob.FetchAttributes();
            int? count = appendBlob.Properties.AppendBlobCommittedBlockCount;

            LogLog.Debug(declaringType, string.Format("appendBlob:{0}, has block count={1}", appendBlob.Name, count));
            return count == null ? 0 : count.Value;             
        }


       /* private void ProcessEvent(LoggingEvent loggingEvent)
        {
            CloudAppendBlob appendBlob = _cloudBlobContainer.GetAppendBlobReference(BlobName(DirectoryName, FileName));
            var xml = _lineFeed + loggingEvent.GetXmlString(Layout, IncludeBasicLogging);
            //appendBlob.
                 
            using (MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(xml)))
            {
                appendBlob.AppendBlock(ms);
            }
        }*/

        private static string BlobName(string directoryName, string fileName)
        {
            return string.Format("{0}/{1}", directoryName, fileName);
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
            BufferSize = Convert.ToInt32(LogEventsCountInBlock);

            base.ActivateOptions();

            _account = CloudStorageAccount.Parse(ConnectionString);
            _client = _account.CreateCloudBlobClient();
            _cloudBlobContainer = _client.GetContainerReference(ContainerName.ToLower());
            _cloudBlobContainer.CreateIfNotExists();
            LogLog.Debug(declaringType, "Log4netAppender - Using Blob container created or exists=" + ContainerName.ToLower());
        }
    }
}