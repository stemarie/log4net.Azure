#log4net.Azure

Transfer all your logs to the [Azure Table or Blob Storage](http://azure.microsoft.com/de-de/services/storage/) via Appender for [log4Net](https://logging.apache.org/log4net/)

## Install
Add To project via NuGet:  
1. Right click on a project and click 'Manage NuGet Packages'.  
2. Search for 'log4net.Appender.Azure' and click 'Install'.  

## Configuration
### Table Storage 
Every log entry ist stored in a separate row.

	<appender name="AzureTableAppender" type="log4net.Appender.AzureTableAppender, log4net.Appender.Azure">
	   <param name="TableName" value="testLoggingTable"/>
	   <!-- You can either specify a connection string or use the ConnectionStringName property instead -->
	   <param name="ConnectionString" value="UseDevelopmentStorage=true"/>
	   <!--<param name="ConnectionStringName" value="GlobalConfigurationString" />-->
	   <!-- You can specify this to make each LogProperty as separate Column in TableStorage, 
		Default: all Custom Properties were logged into one single field -->
	   <param name="PropAsColumn" value="true" />
	   <!-- You can specify this to make each LogProperty as separate Column in TableStorage, 
		Default: all Custom Properties were logged into one single field -->
	   <param name="PropAsColumn" value="true" />
	   <param name="PartitionKeyType" value="LoggerName" />
	 </appender>
	
* <b>TableName:</b>  
  Name of the table in Table Storage
* <b>ConnectionString:</b>  
  the full Azure Storage connection string
* <b>ConnectionStringName:</b>  
  Name of a connection string specified under connectionString
* <b>PropAsColumn(optional):</b>  
  Default: all properties were written in a single field(default).  
  If you specifiy this with the value true then each custom log4net property is logged as separate column/field in the table.  
  Remember that Table storage has a Limit of 255 Properties ([see here](https://azure.microsoft.com/en-us/documentation/articles/storage-table-design-guide/#about-the-azure-table-service)).
* <b>PartitionKeyType(optional):</b>  
  Default "LoggerName": (each logger gets his own partition in Table Storage)  
  "DateReverse": order by Date Reverse to see the latest items first ([How to order elements by date reverse](http://gauravmantri.com/2012/02/17/effective-way-of-fetching-diagnostics-data-from-windows-azure-diagnostics-table-hint-use-partitionkey/))

	
### BlobStorage
Every log Entry is stored as separate XML file.

    <appender name="AzureBlobAppender" type="log4net.Appender.AzureBlobAppender, log4net.Appender.Azure">
      <param name="ContainerName" value="testloggingblob"/>
      <param name="DirectoryName" value="logs"/>
      <!-- You can either specify a connection string or use the ConnectionStringName property instead -->
      <param name="ConnectionString" value="UseDevelopmentStorage=true"/>
      <!--<param name="ConnectionStringName" value="GlobalConfigurationString" />-->
    </appender>
	
* <b>ContainerName:</b>  
  Name of the container in Blob Storage	
* <b>DirectoryName:</b>  
  Name of the folder in the specified container
* <b>ConnectionString:</b>  
  the full Azure Storage connection string
* <b>ConnectionStringName:</b>  
  Name of a connection string specified under connectionString

## View Logs

You can take a look on this [Site](http://storagetools.azurewebsites.net/) to use one of this tools based on your selected appender.
