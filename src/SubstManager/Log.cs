using System;
using System.Collections.Generic;
using System.Text;

namespace SubstManager
{
    public static class Log
    {
        private static int indent_ = 0;

        public static void PushIndent()
        {
            indent_ += 4;
        }

        public static void PopIndent()
        {
            indent_ -= 4;
        }

        public static bool IsVerbose { get; set; } = false;

        public static void Info(string message)
        {
            var lines = message.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

            foreach(var line in lines)
            {
                Console.Write(new string(' ', indent_));
                Console.WriteLine(line);
            }
        }

        public static void Error(string error)
        {
            Console.Error.WriteLine(error);
        }

        public static void Verbose(string message)
        {
            if(IsVerbose)
            {
                Info(message);
            }
        }
    }
}
