using System;
using log4net.Appender;
using log4net.Core;
using log4net.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace log4net.Azure.Tests
{
    [TestClass]
    public class UnitTestAzureDynamicTableAppender
    {
        private AzureTableAppender _appender;

        [TestInitialize]
        public void Initialize()
        {
            _appender = new AzureTableAppender()
            {
                ConnectionString = "UseDevelopmentStorage=true",
                TableName = "testDynamicLoggingTable",
                PropAsColumn = true
            };
            _appender.ActivateOptions();
        }

        [TestMethod]
        public void Test_Table_Appender()
        {
            var @event = MakeEvent();

            _appender.DoAppend(@event);
        }

        [TestMethod]
        public void Test_Table_Appender_Multiple_5()
        {
            _appender.DoAppend(MakeEvents(5));
        }

        [TestMethod]
        public void Test_Table_Appender_Multiple_10()
        {
            _appender.DoAppend(MakeEvents(10));
        }

        [TestMethod]
        public void Test_Table_Appender_Multiple_100()
        {
            _appender.DoAppend(MakeEvents(100));
        }

        private static LoggingEvent[] MakeEvents(int number)
        {
            var result = new LoggingEvent[number];
            for (int i = 0; i < number; i++)
            {
                result[i] = MakeEvent();
            }
            return result;
        }

        private static LoggingEvent MakeEvent()
        {
            var loggingEventData = new LoggingEventData
            {
                Domain = "testDomain",
                Identity = "testIdentity",
                Level = Level.Critical,
                LoggerName = "testLoggerName",
                Message = "testMessage",
                ThreadName = "testThreadName",
                TimeStamp = DateTime.UtcNow,
                UserName = "testUsername",
                Properties = new PropertiesDictionary()
            };

            loggingEventData.Properties["test1"] = DateTime.UtcNow;
            loggingEventData.Properties["Url"] = "http://google.de";
            loggingEventData.Properties["requestId"] = Guid.NewGuid();


            return new LoggingEvent(
                loggingEventData
                );
        }
    }
}