using System;
using System.Globalization;
using System.Xml.Linq;
using log4net.Core;

namespace log4net.Appender.Extensions
{
    internal static class LoggingEventExtensions
    {
        internal static string GetXmlString(this LoggingEvent loggingEvent)
        {
            return new XElement(
                "LogEntry",
                new XElement("UserName", loggingEvent.UserName),
                new XElement("TimeStamp",
                             loggingEvent.TimeStamp.ToString(CultureInfo.InvariantCulture)),
                new XElement("ThreadName", loggingEvent.ThreadName),
                new XElement("LoggerName", loggingEvent.LoggerName),
                new XElement("Level", loggingEvent.Level),
                new XElement("Identity", loggingEvent.Identity),
                new XElement("Domain", loggingEvent.Domain),
                new XElement("CreatedOn", DateTime.UtcNow.ToString(CultureInfo.InvariantCulture)),
                new XElement("RenderedMessage", loggingEvent.RenderedMessage)
                ).ToString();
        }
    }
}
