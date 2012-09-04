using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using log4net.Appender.Azure;
using log4net.Core;

namespace log4net.Azure.Tests
{
    [TestClass]
    public class UnitTestAzureTableAppender
    {
        private AzureTableAppender _appender;

        [TestInitialize]
        public void Initialize()
        {
            _appender = new AzureTableAppender("UseDevelopmentStorage=true", "testLoggingTable");
        }

        [TestMethod]
        public void Test_Appender()
        {
            _appender.DoAppend(
                new LoggingEvent(
                    new LoggingEventData
                        {
                            Domain = "testDomain",
                            Identity = "testIdentity",
                            Level = Level.Critical,
                            LoggerName = "testLoggerName",
                            Message = "testMessage",
                            ThreadName = "testThreadName",
                            TimeStamp = DateTime.UtcNow,
                            UserName = "testUsername"
                        }
                    ));
        }
    }
}
