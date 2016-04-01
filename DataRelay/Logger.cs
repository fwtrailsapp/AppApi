using System;
using System.Diagnostics;
using System.IO;
using log4net;

namespace DataRelay
{
    public enum LogLevel
    {
        Debug = 0,
        Info = 1,
        Warn = 2,
        Error = 3,
        Fatal = 4
    }

    public class Logger
    {
        public Logger(string configFilePath)
        {
            if (!File.Exists(configFilePath))
            {
                Trace.WriteLine(string.Format("Cannot find logging configuration file: {0}. Looking in folder {1}", configFilePath, Environment.CurrentDirectory));
            }
            else
            {
                log4net.Config.XmlConfigurator.ConfigureAndWatch(new FileInfo(configFilePath));
            }
        }

        public void WriteTraceLine(object sender, string text)
        {
            StackTrace trace = new StackTrace();

            String output = String.Format("{0} {1} {2}", sender.GetType().Name, trace.GetFrame(1).GetMethod().Name, text);

            Trace.WriteLine(output);
        }

        public void WriteLog(Type type, Exception ex)
        {
            WriteLog(type.FullName, LogLevel.Error, ex.ToString());
        }

        public void WriteLog(string logMessage, LogLevel level)
        {
            //write to root logger
            WriteLog(string.Empty, level, logMessage);
        }

        public void WriteLog(Type type, string logMessage, LogLevel level)
        {
            WriteLog(type.FullName, level, logMessage);
        }

        public void WriteDebugLog(string logMessage, params object[] p)
        {
            WriteLog(typeof(Logger).FullName, LogLevel.Debug, logMessage, p);
        }

        public void WriteDebugLog(Type type, string logMessage, params object[] p)
        {
            WriteLog(type.FullName, LogLevel.Debug, logMessage, p);
        }

        public void WriteErrorLog(Type type, string logMessage, params object[] p)
        {
            WriteLog(type.FullName, LogLevel.Error, logMessage, p);
        }

        public void WriteErrorLog(Type type, Exception ex)
        {
            WriteLog(type.FullName, LogLevel.Error, ex.ToString());
        }

        public void WriteErrorLog(Type type, Exception ex, string logMessage, params object[] p)
        {
            WriteLog(type.FullName, LogLevel.Error, logMessage, p);
            WriteLog(type.FullName, LogLevel.Error, ex.ToString());
        }

        public void WriteLog(string logger, string logMessage, LogLevel level)
        {
            ILog log = LogManager.GetLogger(logger);
            if (level == LogLevel.Debug)
            {
                log.Debug(logMessage);
            }
            else if (level == LogLevel.Error)
            {
                log.Error(logMessage);
            }
            else if (level == LogLevel.Fatal)
            {
                log.Fatal(logMessage);
            }
            else if (level == LogLevel.Info)
            {
                log.Info(logMessage);
            }
            else if (level == LogLevel.Warn)
            {
                log.Warn(logMessage);
            }
        }

        public void WriteLog(string logger, LogLevel level, string logMessage, params object[] p)
        {
            ILog log = LogManager.GetLogger(logger);
            if (level == LogLevel.Debug)
            {
                log.DebugFormat(logMessage, p);
            }
            else if (level == LogLevel.Error)
            {
                log.ErrorFormat(logMessage, p);
            }
            else if (level == LogLevel.Fatal)
            {
                log.FatalFormat(logMessage, p);
            }
            else if (level == LogLevel.Info)
            {
                log.InfoFormat(logMessage, p);
            }
            else if (level == LogLevel.Warn)
            {
                log.WarnFormat(logMessage, p);
            }
        }
    }
}