using System;
using System.IO;

// ReSharper disable AssignNullToNotNullAttribute

namespace Bot {
    public static class Logger {
        private static string logFile;

        private static void Initialize() {
            logFile = "Logs/" + DateTime.UtcNow.ToString("yyyy-MM-dd HH.mm.ss") + ".log";
            Directory.CreateDirectory(Path.GetDirectoryName(logFile));
        }
        

        private static void WriteLine(string type, string line, params object[] parameters) {
            if (logFile == null)
                Initialize();

            var msg = "[" + DateTime.UtcNow.ToString("HH:mm:ss") + " " + type + "] " + string.Format(line, parameters);

            var file = new StreamWriter(logFile, true);
            file.WriteLine(msg);
            file.Close();

            Console.WriteLine(msg, parameters);
        }        

        public static void Info(string line, params object[] parameters) {
            WriteLine("INFO", line, parameters);
        }
        
        public static void Warning(string line, params object[] parameters) {
            WriteLine("WARNING", line, parameters);
        }
        
        public static void Error(string line, params object[] parameters) {
            WriteLine("ERROR", line, parameters);
        }
    }
}