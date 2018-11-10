using System;
using System.Data.Entity.Design.PluralizationServices;
using System.IO;
using System.Reflection;
using System.Globalization;
using SysAudit.Properties;
using log4net;

namespace SysAudit
{
    public static class Global
    {
        public static string appName = Path.GetFileNameWithoutExtension(Assembly.GetEntryAssembly().Location);

        #region Logging

        public static void LogEvent(string message, LogEvents logEvent = LogEvents.Error, bool logToLog4Net = true, bool logToConsole = true)
        {
            message = string.Format("{0} | {1}@{2}", message, Environment.UserName, Environment.MachineName);

            if (Settings.Default.LogToLog4Net && logToLog4Net)
            {
                ILog Logger = LogManager.GetLogger("EventLogAppender");

                try
                {
                    switch (logEvent)
                    {
                        case LogEvents.Debug:
                            Logger.DebugFormat(message); break;
                        case LogEvents.Error:
                            Logger.ErrorFormat(message); break;
                        case LogEvents.Fatal:
                            Logger.FatalFormat(message); break;
                        case LogEvents.Info:
                            Logger.InfoFormat(message); break;
                        case LogEvents.Warn:
                            Logger.WarnFormat(message); break;
                    }
                }
                catch (Exception ex)
                {
                    ExitApp(1, string.Format("EXCEPTION in {0}: {1}", MethodBase.GetCurrentMethod(), ex.Message), errorWritingToLog:true);
                }

                if (Settings.Default.LogToConsole && logToConsole) Console.WriteLine(message);
            }
        }

        public enum LogEvents
        {
            Debug,
            Error,
            Fatal,
            Info,
            Warn
        }

        #endregion

        public static string Pluralize(string @string)
        {
            CultureInfo cultureInfo = new CultureInfo("en-us");
            PluralizationService pluralizationService = PluralizationService.CreateService(cultureInfo);
            if (pluralizationService.IsSingular(@string))
                return pluralizationService.Pluralize(@string);

            return null;
        }

        public static void ExitApp(int exitCode, string message = "", LogEvents logEvent = LogEvents.Error, bool errorWritingToLog = false)
        {
            if (exitCode == 0) message = "completed successfully";

            message = string.Format("[{0}] {1} - exit code: {2}", appName, message, exitCode);
            if (errorWritingToLog)
                Console.WriteLine(message);
            else
                LogEvent(message, logEvent);

            Environment.Exit(exitCode);
        }
    }
}