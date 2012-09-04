using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;
using log4net.Core;

namespace log4net.Appender.Azure
{
    public class AzureBlobAppender : BufferingAppenderSkeleton
    {
        private CloudBlobClient _client;
        private CloudStorageAccount _account;
        private CloudBlobDirectory _blobDirectoryReference;
        private CloudBlobContainer _cloudBlobContainer;
        public string ConnectionString { get; set; }
        public string DirectoryName { get; set; }
        public string ContainerName { get; set; }

        public AzureBlobAppender()
            : base()
        {
            Initialize();
        }

        public AzureBlobAppender(string connectionString, string containerName, string directoryName)
            : base()
        {
            ConnectionString = connectionString;
            ContainerName = containerName;
            DirectoryName = directoryName;

            Initialize();
        }

        protected override void SendBuffer(LoggingEvent[] events)
        {
            Parallel.ForEach(events, ProcessEvent);
        }

        private void ProcessEvent(LoggingEvent loggingEvent)
        {
            var blob = _blobDirectoryReference.GetBlobReference(Filename(loggingEvent));
            blob.UploadText(RenderLoggingEvent(loggingEvent));
        }

        private static string Filename(LoggingEvent loggingEvent)
        {
            return string.Format("{0}.entry.log", loggingEvent.TimeStamp.ToString("yyyy_MM_dd_HH_mm_ss_ffff", DateTimeFormatInfo.InvariantInfo));
        }

        private void Initialize()
        {
            _account = CloudStorageAccount.Parse(ConnectionString);
            _client = _account.CreateCloudBlobClient();
            _cloudBlobContainer = _client.GetContainerReference(ContainerName.ToLower());
            _cloudBlobContainer.CreateIfNotExist();
            _blobDirectoryReference = _cloudBlobContainer.GetDirectoryReference(DirectoryName.ToLower());

        }
    }
}
