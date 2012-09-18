using System.Globalization;
using System.Threading.Tasks;
using log4net.Core;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;

namespace log4net.Appender.Azure
{
    public class AzureBlobAppender : BufferingAppenderSkeleton
    {
        private CloudStorageAccount _account;
        private CloudBlobDirectory _blobDirectoryReference;
        private CloudBlobClient _client;
        private CloudBlobContainer _cloudBlobContainer;

        public AzureBlobAppender()
        {
            Initialize();
        }

        public AzureBlobAppender(string connectionString, string containerName, string directoryName)
        {
            ConnectionString = connectionString;
            ContainerName = containerName;
            DirectoryName = directoryName;

            Initialize();
        }

        public string ConnectionString { get; set; }
        public string DirectoryName { get; set; }
        public string ContainerName { get; set; }

        protected override void SendBuffer(LoggingEvent[] events)
        {
            Parallel.ForEach(events, ProcessEvent);
        }

        private void ProcessEvent(LoggingEvent loggingEvent)
        {
            CloudBlob blob = _blobDirectoryReference.GetBlobReference(Filename(loggingEvent));
            blob.UploadText(RenderLoggingEvent(loggingEvent));
        }

        private static string Filename(LoggingEvent loggingEvent)
        {
            return string.Format("{0}.entry.log",
                                 loggingEvent.TimeStamp.ToString("yyyy_MM_dd_HH_mm_ss_fffffff",
                                                                 DateTimeFormatInfo.InvariantInfo));
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