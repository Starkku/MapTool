/*
 * Copyright 2017 by Starkku
 * This file is part of MapTool, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of
 * the License, or (at your option) any later version. For more
 * information, see COPYING.
 */

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using NLog;
using NLog.Config;
using NLog.Targets;
using NDesk.Options;
using MapTool.Utility;

namespace MapTool
{

    class Program
    {
        private static OptionSet options;
        private static Settings settings = new Settings();
        private static NLog.Logger logger;

        static void Main(string[] args)
        {
            options = new OptionSet 
            {
            {"h|help", "Show help", v => settings.ShowHelp = true},
            {"i|infile=", "Input file.", v => settings.FileInput = v},
            {"o|outfile=", "Output file.", v => settings.FileOutput = v},
            {"p|profilefile=", "Conversion profile file.", v => settings.FileConfig = v},
            {"l|list=", "List theater data based on this theater config file.", v => settings.List = v},
            {"c|convert", "Convert map tiles/overlay according to a profile file.", v => settings.Convert = true},
            {"d|debug-logging", "If set, writes a (more detailed) log to a file called MapTool.log.", v => settings.DebugLogging = true}
            };
            options.Parse(args);
            initLogger(settings.DebugLogging);

            bool error = false;

            if (settings.ShowHelp)
            {
                ShowHelp();
                return;
            }
            if (string.IsNullOrEmpty(settings.FileInput) && string.IsNullOrEmpty(settings.List))
            {
                logger.Error("No valid input file specified.");
                ShowHelp();
                error = true;
            }
            else if (!string.IsNullOrEmpty(settings.FileInput) && !File.Exists(settings.FileInput))
            {
                logger.Error("Specified input file does not exist.");
                ShowHelp();
                error = true;
            }
            if (error) return;
            else logger.Info("Input file path OK.");

            if ((settings.FileOutput == null || !Directory.Exists(Path.GetDirectoryName(settings.FileOutput))) && string.IsNullOrEmpty(settings.List))
            {
                logger.Error("Specified output directory does not exist.");
                ShowHelp();
                return;
            }
            else if (!string.IsNullOrEmpty(settings.List))
            {
                if (string.IsNullOrEmpty(settings.FileOutput))
                {
                    logger.Warn("No output file available. Using input as output.");
                    settings.FileOutput = Path.ChangeExtension(settings.List, ".txt");
                }
            }
            else logger.Info("Output file path OK.");

            MapTool map = new MapTool(settings.FileInput, settings.FileOutput, settings.FileConfig, settings.List);
            if (map.Initialized) logger.Info("MapTool initialized.");
            else
            {
                logger.Error("MapTool could not be initialized. Aborting.");
                return;
            }

            if (!string.IsNullOrEmpty(settings.List) && File.Exists(settings.List))
            {
                map.ListTileSetData();
                return;
            }

            if (settings.Convert)
            {
                if (settings.FileConfig == null || settings.FileConfig == "")
                {
                    logger.Error("Configuration file required for tile data conversion does not exist.");
                    ShowHelp();
                    error = true;
                }
                else if (!File.Exists(settings.FileConfig))
                {
                    logger.Error("Specified configuration file does not exist.");
                    ShowHelp();
                    error = true;
                }
                if (error) return;
                map.ConvertTheaterData();
                map.ConvertTileData();
                map.ConvertOverlayData();
                map.ConvertObjectData();
                //map.ConvertSectionData();
            }
            if (map.Altered)
            {
                logger.Info("Saving the modified map as '" + settings.FileOutput + "'.");
                map.Save();
            }
        }

        private static void initLogger(bool debug = false)
        {
#if DEBUG
            debug = true;
#endif

            if (LogManager.Configuration == null)
            {
                ColoredConsoleTarget target = new ColoredConsoleTarget();
                target.Name = "console";
                target.Layout = "${processtime:format=ss.fff} [${level}] ${message}";
                target.RowHighlightingRules.Add(new ConsoleRowHighlightingRule()
                {
                    ForegroundColor = ConsoleOutputColor.Magenta,
                    Condition = "level = LogLevel.Fatal"
                });
                target.RowHighlightingRules.Add(new ConsoleRowHighlightingRule()
                {
                    ForegroundColor = ConsoleOutputColor.Red,
                    Condition = "level = LogLevel.Error"
                });
                target.RowHighlightingRules.Add(new ConsoleRowHighlightingRule()
                {
                    ForegroundColor = ConsoleOutputColor.Yellow,
                    Condition = "level = LogLevel.Warn"
                });
                target.RowHighlightingRules.Add(new ConsoleRowHighlightingRule()
                {
                    ForegroundColor = ConsoleOutputColor.Gray,
                    Condition = "level = LogLevel.Info"
                });
                target.RowHighlightingRules.Add(new ConsoleRowHighlightingRule()
                {
                    ForegroundColor = ConsoleOutputColor.DarkGray,
                    Condition = "level = LogLevel.Debug"
                });
                target.RowHighlightingRules.Add(new ConsoleRowHighlightingRule()
                {
                    ForegroundColor = ConsoleOutputColor.White,
                    Condition = "level = LogLevel.Trace"
                });
                LogManager.Configuration = new LoggingConfiguration();
                LogManager.Configuration.AddTarget("console", target);
#if DEBUG
                LogManager.Configuration.LoggingRules.Add(new LoggingRule("*", LogLevel.Debug, target));
#else
				LogManager.Configuration.LoggingRules.Add(new LoggingRule("*", LogLevel.Info, target));
#endif
                LogManager.ReconfigExistingLoggers();
            }

            if (debug == true)
            {
                LoggingRule console = LogManager.Configuration.LoggingRules[0];
                console.EnableLoggingForLevel(LogLevel.Debug);
                FileTarget file = new FileTarget();
                file.FileName = "MapTool.log";
                file.Name = "file";
                file.Layout = "${processtime:format=ss.fff} | ${logger} | ${pad:padding=7:inner=[${level}]} | ${message}";
                file.KeepFileOpen = false;
                file.DeleteOldFileOnStartup = true;
                file.Encoding = Encoding.UTF8;
                LogManager.Configuration.AddTarget("file", file);
                LogManager.Configuration.LoggingRules.Add(new LoggingRule("*", LogLevel.Trace, file));
                LogManager.ReconfigExistingLoggers();
            }

            logger = NLog.LogManager.GetCurrentClassLogger();
        }

        private static void ShowHelp()
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write("Usage: ");
            Console.WriteLine("");
            var sb = new System.Text.StringBuilder();
            var sw = new StringWriter(sb);
            options.WriteOptionDescriptions(sw);
            Console.WriteLine(sb.ToString());
        }

    }
}
