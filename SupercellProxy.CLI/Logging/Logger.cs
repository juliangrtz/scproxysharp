using System.IO;

namespace SupercellProxy.Logging
{
    public class Logger
    {
        /// <summary>
        /// Centers a string.
        /// </summary>
        public static void CenterString(string str)
        {
            SetCursorPosition((WindowWidth - str.Length) / 2, CursorTop);
            WriteLine(str);
            SetCursorPosition(CursorLeft, CursorTop);
        }

        /// <summary>
        /// Logs a passed message to the console screen.
        /// </summary>
        public static void Log(string message, LogType logtype = LogType.INFO, bool saveToFile = true)
        {
            // Reset line with \r
            Write("\r");

            // Format: [TYPE] MESSAGE
            Write("[");
            Write(logtype);
            Write("] ");
            ResetColor();
            WriteLine(message);

            // Save message to file
            if (saveToFile)
                SaveToFile(message, logtype);
        }

        public static void LogConsole(string message)
            => Log(message, LogType.CONSOLE);

        public static void LogWarning(string message)
            => Log(message, LogType.WARNING);

        public static void LogException(string message)
            => Log(message, LogType.EXCEPTION);

        public static void LogPacket(string message)
            => Log(message, LogType.PACKET);

        /// <summary>
        /// Saves a string including the logtype to a file.
        /// The logs are saved day-to-day.
        /// </summary>
        private static void SaveToFile(string text, LogType logtype)
        {
            var path = "Logs\\" + DateTime.UtcNow.ToLocalTime().ToString("dd-MM-yyyy") + ".log";
            using (FileStream fs = new FileStream(path, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.WriteLine("[" + DateTime.UtcNow.ToLocalTime().ToString("hh-mm-ss") + "-" + logtype + "]" + text);
                    sw.Close();
                }
            }
        }
    }
}