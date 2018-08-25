using System;
using System.IO;
// ReSharper disable AssignNullToNotNullAttribute

namespace Bot {
    public static class Logger {
        private static string logFile = null;
        
        private static void Initialize() {
            logFile = "Logs/" + DateTime.UtcNow.ToString("yyyy-MM-dd HH.mm.ss") + ".log";
            Directory.CreateDirectory(Path.GetDirectoryName(logFile));            
        }
        
        public static void Info(string line, params object[] parameters) {
            if (logFile == null) 
                Initialize();
                    
            var msg = "[" + DateTime.UtcNow.ToString("HH:mm:ss") + " INFO] " + string.Format(line, parameters);

            var file = new System.IO.StreamWriter(logFile, true);
            file.WriteLine(msg);
            file.Close();

            Console.WriteLine(msg, parameters);
        }
    }
}