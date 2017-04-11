/*
 * Copyright 2017 by Starkku
 * This file is part of MapTool, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of
 * the License, or (at your option) any later version. For more
 * information, see COPYING.
 */

using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;

namespace MapTool.Utility
{
    public static class Logger
    {
        private static bool Initialized { get; set; }
        private static bool WriteFile { get; set; }
        private static bool EnableDebugConsole { get; set; }
        private static string Filename { get; set; }
        private static Stopwatch Timer = null;
        private static ConsoleColor DefaultColor;
        private static StreamWriter LogWriter = null;

        public static void Initialize(string filename, bool writefile, bool enabledebugconsole)
        {
            Filename = filename;
            WriteFile = writefile;
            EnableDebugConsole = enabledebugconsole;
            Initialized = true;
            Timer = new Stopwatch();
            Timer.Start();
            DefaultColor = Console.ForegroundColor;
            if (WriteFile)
            {
                File.Delete(Filename);
                LogWriter = File.CreateText(Filename);
                LogWriter.AutoFlush = true;
            }
        }

        private static string getSeconds()
        {
            return Timer.Elapsed.TotalSeconds.ToString("0.00000", CultureInfo.InvariantCulture);
        }

        private static string getDateTime()
        {
            string dateString = DateTime.Now.ToString("dd-MM-yyyy", CultureInfo.InvariantCulture);
            return dateString + " | " + Timer.Elapsed.ToString();
        }

        public static void Info (string log)
        {
            if (!Initialized) return;
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine(getSeconds() + " [Info] " + log);
            Console.ForegroundColor = DefaultColor;
            LogToFile("[Info]", log);
        }

        public static void Warn(string log)
        {
            if (!Initialized) return;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(getSeconds() + " [Warn] " + log);
            Console.ForegroundColor = DefaultColor;
            LogToFile("[Warn]", log);
        }

        public static void Error(string log)
        {
            if (!Initialized) return;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(getSeconds() + " [Error] " + log);
            Console.ForegroundColor = DefaultColor;
            LogToFile("[Error]", log);
        }

        public static void Debug(string log)
        {
            if (!Initialized) return;
            if (EnableDebugConsole)
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine(getSeconds() + " [Debug] " + log);
                Console.ForegroundColor = DefaultColor;
            }
            LogToFile("[Debug]", log);
        }

        public static void Trace(string log)
        {
            if (!Initialized) return;
            if (EnableDebugConsole)
            {
                Console.ForegroundColor = ConsoleColor.DarkCyan;
                Console.WriteLine(getSeconds() + " [Trace] " + log);
                Console.ForegroundColor = DefaultColor;
            }
            LogToFile("[Trace]", log);
        }

        private static void LogToFile(string label, string log)
        {
            if (!Initialized || !WriteFile || LogWriter == null) return;
            LogWriter.WriteLine(getDateTime() + " " + label + " " + log);
        }
    }
}
