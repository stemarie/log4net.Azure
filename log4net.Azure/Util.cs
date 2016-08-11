using log4net.Appender.Language;
using Microsoft.Azure;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace log4net.Appender
{
    public static class Util
    {
        /// <summary>
        /// Attempt to retrieve the connection string using ConfigurationManager 
        /// with CloudConfigurationManager as fallback
        /// </summary>
        /// <param name="connectionStringName">The name of the connection string to retrieve</param>
        /// <returns></returns>
        public static string GetConnectionString(string connectionStringName)
        {
            // Attempt to retrieve the connection string using the regular ConfigurationManager first
            var config = ConfigurationManager.ConnectionStrings[connectionStringName];
            if (config != null)
            {
                return config.ConnectionString;
            }

            // Fallback to CloudConfigurationManager in case we're running as a worker/web role
            var azConfig = CloudConfigurationManager.GetSetting(connectionStringName);
            if (!string.IsNullOrWhiteSpace(azConfig))
            {
                return azConfig;
            }

            // Connection string not found, throw exception to notify the user
            throw new ApplicationException(Resources.AzureConnectionStringNotSpecified);
        }
    }
}
