using System;
using System.Globalization;
using log4net.Core;
using System.Xml.Linq;

namespace log4net.Appender.Azure
{
    internal class Utility
    {
        internal static string GetXmlString(LoggingEvent loggingEvent)
        {
            return new XElement(
                "LogEntry",
                new XElement("UserName", loggingEvent.UserName),
                new XElement("TimeStamp",
                             loggingEvent.TimeStamp.ToString(CultureInfo.InvariantCulture)),
                new XElement("ThreadName", loggingEvent.ThreadName),
                new XElement("LoggerName", loggingEvent.LoggerName),
                new XElement("Level", loggingEvent.Level.ToString()),
                new XElement("Identity", loggingEvent.Identity),
                new XElement("Domain", loggingEvent.Domain),
                new XElement("CreatedOn", DateTime.UtcNow.ToString(CultureInfo.InvariantCulture)),
                new XElement("RenderedMessage", loggingEvent.RenderedMessage)
                ).ToString();
        }
    }
}
