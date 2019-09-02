using System;

namespace MemeTV.BusinessLogic
{
    public class ConsoleLogger : ILogger
    {
        private readonly object mutex = new object();

        public void Debug(string msg)
        {
            Write(msg, LogSeverity.Debug);
        }

        public void Error(string msg)
        {
            Write(msg, LogSeverity.Error);
        }

        public void Message(string msg)
        {
            Write(msg, LogSeverity.Message);
        }

        public void Warning(string msg)
        {
            Write(msg, LogSeverity.Warning);
        }

        private void Write(string msg, LogSeverity severity)
        {
            lock (mutex)
            {
                var color = ConsoleColor.Gray;
                switch (severity)
                {
                    case LogSeverity.Debug:
                        color = ConsoleColor.Cyan;
                        break;
                    case LogSeverity.Message:
                        color = ConsoleColor.White;
                        break;
                    case LogSeverity.Warning:
                        color = ConsoleColor.Yellow;
                        break;
                    case LogSeverity.Error:
                        color = ConsoleColor.Red;
                        break;
                }

                var oldForeground = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.Write("[");
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write($"{DateTime.Now.ToShortTimeString()}");
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.Write("] ");
                Console.ForegroundColor = color;
                Console.WriteLine("(" + severity + "): " + msg);
                Console.ForegroundColor = oldForeground;
            }
        }
    }
}