using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Middleware
{
    public static class Logger
    {
        //private static readonly string BaseDirectory = ConfigurationManager.AppSettings["TCPDataLogPath"]
        //?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs"); // Fallback if not configured
        private static readonly string BaseDirectory = AppDomain.CurrentDomain.BaseDirectory;


        public static void LogMessage(string message, string filename)
        {
            try
            {
                // Create the month-based folder
                string TCPDataLoggerFolder = Path.Combine(BaseDirectory, "Logger");
                if (!Directory.Exists(TCPDataLoggerFolder))
                {
                    Directory.CreateDirectory(TCPDataLoggerFolder);
                }
                string monthFolder = Path.Combine(TCPDataLoggerFolder, DateTime.Now.ToString("yyyy-MM"));
                if (!Directory.Exists(monthFolder))
                {
                    Directory.CreateDirectory(monthFolder);
                }

                // Create the day-based folder within the month folder
                string dayFolder = Path.Combine(monthFolder, DateTime.Now.ToString("dd"));
                if (!Directory.Exists(dayFolder))
                {
                    Directory.CreateDirectory(dayFolder);
                }
                

                // Create the hour-based log file within the day folder
                string logFileName = filename + ".log";
                string logFilePath = Path.Combine(dayFolder, logFileName);

                // Write log data
                File.AppendAllText(logFilePath, $"{DateTime.Now:mm:ss.fff} {message}\n");


                // Run folder cleanup for outdated folders
                CleanupOldFolders(TimeSpan.FromDays(30)); // Adjust the retention period here
            }
            catch
            {

            }
        }

        private static void CleanupOldFolders(TimeSpan maxAge)
        {
            if (!Directory.Exists(BaseDirectory))
                return;

            var monthDirectories = Directory.GetDirectories(BaseDirectory);
            foreach (var monthDirectory in monthDirectories)
            {
                if (DateTime.TryParseExact(Path.GetFileName(monthDirectory), "yyyy-MM", null, System.Globalization.DateTimeStyles.None, out DateTime folderMonth))
                {
                    if (DateTime.Now - folderMonth > maxAge)
                    {
                        Directory.Delete(monthDirectory, true);
                    }
                }
            }
        }
    }
}
