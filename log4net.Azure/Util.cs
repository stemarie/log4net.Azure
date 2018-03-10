using System;
using System.Configuration;
using log4net.Appender.Language;

namespace log4net.Appender
{
	public static class Util
	{
		public const int TIMEOUT = 15000;

		/// <summary>
		/// Attempt to retrieve the connection string using ConfigurationManager 
		/// with CloudConfigurationManager as fallback
		/// </summary>
		/// <param name="connectionStringName">The name of the connection string to retrieve</param>
		/// <returns></returns>
		public static string GetConnectionString (string connectionStringName)
		{
			// Attempt to retrieve the connection string using the regular ConfigurationManager first
			var config = ConfigurationManager.ConnectionStrings[connectionStringName];
			if (config != null) {
				return config.ConnectionString;
			}

			// Fallback to CloudConfigurationManager in case we're running as a worker/web role
			// TODO - No CloudConfigurationManager for .NET Standard
			/*
			var azConfig = CloudConfigurationManager.GetSetting(connectionStringName);
			if (!string.IsNullOrWhiteSpace(azConfig)) {
				return azConfig;
			}
			*/

			// Connection string not found, throw exception to notify the user
			throw new ApplicationException(Resources.AzureConnectionStringNotSpecified);
		}
	}
}
