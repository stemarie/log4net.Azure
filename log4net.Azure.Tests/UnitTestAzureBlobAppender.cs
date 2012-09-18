using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using log4net.Appender.Azure;
using log4net.Core;

namespace log4net.Azure.Tests
{
    [TestClass]
    public class UnitTestAzureBlobAppender
    {
        private AzureBlobAppender _appender;

        [TestInitialize]
        public void Initialize()
        {
            _appender = new AzureBlobAppender("UseDevelopmentStorage=true", "testLoggingBlob", "testLogging");
        }

        [TestMethod]
        public void Test_Appender()
        {
            var @event = MakeEvent();

            _appender.DoAppend(@event);
        }

        [TestMethod]
        public void Test_Appender_Multiple()
        {
            _appender.DoAppend(MakeEvents(5));
        }

        private static LoggingEvent[] MakeEvents(int number)
        {
            LoggingEvent[] result = new LoggingEvent[number];
            for (int i = 0; i < number; i++)
            {
                result[i] = MakeEvent();
            }
            return result;
        }

        private static LoggingEvent MakeEvent()
        {
            return new LoggingEvent(
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
                );
        }
    }
}