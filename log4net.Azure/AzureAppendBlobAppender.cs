using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net.Appender.Extensions;
using log4net.Appender.Language;
using log4net.Core;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

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

		public string ConnectionString
		{
			get {
				if (!string.IsNullOrWhiteSpace(ConnectionStringName)) {
					return Util.GetConnectionString(ConnectionStringName);
				}
				if (String.IsNullOrEmpty(_connectionString))
					throw new ApplicationException(Resources.AzureConnectionStringNotSpecified);
				return _connectionString;
			}
			set {
				_connectionString = value;
			}
		}

		private string _containerName;

		public string ContainerName
		{
			get {
				if (String.IsNullOrEmpty(_containerName))
					throw new ApplicationException(Resources.ContainerNameNotSpecified);
				return _containerName;
			}
			set {
				_containerName = value;
			}
		}

		private string _directoryName;

		public string DirectoryName
		{
			get {
				if (String.IsNullOrEmpty(_directoryName))
					throw new ApplicationException(Resources.DirectoryNameNotSpecified);
				return _directoryName;
			}
			set {
				_directoryName = value;
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
		protected override void SendBuffer (LoggingEvent[] events)
		{
			SendBufferAsync(events).Wait(Util.TIMEOUT);
		}

		private async Task SendBufferAsync (LoggingEvent[] events)
		{
			CloudAppendBlob appendBlob = _cloudBlobContainer.GetAppendBlobReference(Filename(_directoryName));
			if (!await appendBlob.ExistsAsync().ConfigureAwait(false))
				await appendBlob.CreateOrReplaceAsync().ConfigureAwait(false);
			else
				_lineFeed = Environment.NewLine;

			await Task.WhenAll(events.Select(ProcessEvent));
		}

		private async Task ProcessEvent (LoggingEvent loggingEvent)
		{
			CloudAppendBlob appendBlob = _cloudBlobContainer.GetAppendBlobReference(Filename(_directoryName));
			var xml = _lineFeed + loggingEvent.GetXmlString(Layout);
			using (MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(xml))) {
				await appendBlob.AppendBlockAsync(ms);
			}
		}

		private static string Filename (string directoryName)
		{
			return string.Format("{0}/{1}.entry.log.xml",
													 directoryName,
													 DateTime.Today.ToString("yyyy_MM_dd",
																													 DateTimeFormatInfo.InvariantInfo));
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
		public override void ActivateOptions ()
		{
			base.ActivateOptions();

			_account = CloudStorageAccount.Parse(ConnectionString);
			_client = _account.CreateCloudBlobClient();
			_cloudBlobContainer = _client.GetContainerReference(ContainerName.ToLower());
			_cloudBlobContainer.CreateIfNotExistsAsync().Wait(Util.TIMEOUT);
		}
	}
}
