using System;
using System.Globalization;
using System.Xml.Linq;
using log4net.Core;
using log4net.Layout;
using System.IO;

namespace log4net.Appender.Extensions
{
    internal static class LoggingEventExtensions
    {
        internal static string GetXmlString(this LoggingEvent loggingEvent, ILayout layout = null)
        {
            string message = loggingEvent.RenderedMessage + Environment.NewLine + loggingEvent.GetExceptionString();
            if (layout != null)
            {
                using (var w = new StringWriter())
                {
                    layout.Format(w, loggingEvent);
                    message = w.ToString();
                }
            }

            var logXml = new XElement(
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
                new XElement("RenderedMessage", message),
                new XElement("Location", loggingEvent.LocationInformation.FullInfo)
                );

            if (loggingEvent.Properties != null && loggingEvent.Properties.Count > 0)
            {
                var props = loggingEvent.Properties;
                if (props.Contains("AddPropertiesToXml"))
                {
                    foreach (var k in props.GetKeys())
                    {
                        var key = k.Replace(":", "_")
                                   .Replace("@", "_")
                                   .Replace(".", "_");
                        logXml.Add(new XElement(key, props[k].ToString()));
                    }
                }
            }

            if (loggingEvent.ExceptionObject != null)
            {
                logXml.Add(new XElement("Exception", loggingEvent.ExceptionObject.ToString()));
            }

            return logXml.ToString();
        }
    }
}
