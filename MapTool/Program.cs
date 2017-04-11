/*
 * Copyright 2017 by Starkku
 * This file is part of MapTool, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of
 * the License, or (at your option) any later version. For more
 * information, see COPYING.
 */

using System;
using System.IO;
using NDesk.Options;
using MapTool.Utility;
using System.Reflection;

namespace MapTool
{

    class Program
    {
        private static OptionSet options;
        private static Settings settings = new Settings();

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
            {"d|debug-logging", "If set, writes a log to a file in program directory.", v => settings.DebugLogging = true}
            };
            try
            {
                options.Parse(args);
            }
            catch (Exception e)
            {
                Console.WriteLine("Encountered an error while parsing command-line parameters. Message: " + e.Message);
                ShowHelp();
                return;
            }
            initLogger(settings.DebugLogging);

            bool error = false;

            if (settings.ShowHelp)
            {
                ShowHelp();
                return;
            }
            if (string.IsNullOrEmpty(settings.FileInput) && string.IsNullOrEmpty(settings.List))
            {
                Logger.Error("No valid input file specified.");
                ShowHelp();
                error = true;
            }
            else if (!string.IsNullOrEmpty(settings.FileInput) && !File.Exists(settings.FileInput))
            {
                Logger.Error("Specified input file does not exist.");
                ShowHelp();
                error = true;
            }
            if (error) return;
            else Logger.Info("Input file path OK.");

            if ((settings.FileOutput == null || !Directory.Exists(Path.GetDirectoryName(settings.FileOutput))) && string.IsNullOrEmpty(settings.List))
            {
                Logger.Error("Specified output directory does not exist.");
                ShowHelp();
                return;
            }
            else if (!string.IsNullOrEmpty(settings.List))
            {
                if (string.IsNullOrEmpty(settings.FileOutput))
                {
                    Logger.Warn("No output file available. Using input as output.");
                    settings.FileOutput = Path.ChangeExtension(settings.List, ".txt");
                }
            }
            else Logger.Info("Output file path OK.");

            MapTool map = new MapTool(settings.FileInput, settings.FileOutput, settings.FileConfig, settings.List);
            if (map.Initialized) Logger.Info("MapTool initialized.");
            else
            {
                Logger.Error("MapTool could not be initialized. Aborting.");
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
                    Logger.Error("Configuration file required for tile data conversion does not exist.");
                    ShowHelp();
                    error = true;
                }
                else if (!File.Exists(settings.FileConfig))
                {
                    Logger.Error("Specified configuration file does not exist.");
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
                Logger.Info("Saving the modified map as '" + settings.FileOutput + "'.");
                map.Save();
            }
        }

        private static void initLogger(bool writefile = false)
        {
            string filename = AppDomain.CurrentDomain.BaseDirectory + Path.GetFileNameWithoutExtension(Assembly.GetEntryAssembly().Location) +".log";
            bool enabledebug = false; 
#if DEBUG
            enabledebug = true;
#endif
            Logger.Initialize(filename, writefile, enabledebug);
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
