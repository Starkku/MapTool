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
using StarkkuUtils.Tools;

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
                { "h|help", "Show help", v => settings.ShowHelp = true},
                { "i|infile=", "Input file.", v => settings.FileInput = v},
                { "o|outfile=", "Output file.", v => settings.FileOutput = v},
                { "l|list", "List theater data based on input theater config file.", v => settings.List = true},
                { "p|profilefile=", "Conversion profile file. This also enables the conversion logic.", v => settings.FileConfig = v},
                { "d|debug-logging", "If set, writes a log to a file in program directory.", v => settings.DebugLogging = true}
            };
            try
            {
                options.Parse(args);
            }
            catch (Exception e)
            {
                ConsoleColor defcolor = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Encountered an error while parsing command-line parameters. Message: " + e.Message);
                Console.ForegroundColor = defcolor;
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
            if (String.IsNullOrEmpty(settings.FileConfig) && !settings.List)
            {
                Logger.Error("Not enough parameters. Must provide either -l or -p.");
                ShowHelp();
                return;
            }
            else if (settings.List)
            {
                Logger.Info("Mode set (-l): List Tile Data.");
            }
            else
            {
                Logger.Info("Mode set (-p): Apply Conversion Profile.");
            }
            if (String.IsNullOrEmpty(settings.FileInput))
            {
                Logger.Error("No valid input file specified.");
                ShowHelp();
                error = true;
            }
            else if (!String.IsNullOrEmpty(settings.FileInput) && !File.Exists(settings.FileInput))
            {
                Logger.Error("Specified input file does not exist.");
                ShowHelp();
                error = true;
            }
            if (error) return;
            else Logger.Info("Input file path OK.");
            if (settings.List)
            {
                if (String.IsNullOrEmpty(settings.FileOutput))
                {
                    Logger.Warn("No output file available. Using input as output.");
                    settings.FileOutput = Path.ChangeExtension(settings.FileInput, ".txt");
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

            if (settings.List)
            {
                map.ListTileSetData();
                return;
            }
            else
            {
                map.ConvertTheaterData();
                map.ConvertTileData();
                map.ConvertOverlayData();
                map.ConvertObjectData();
                map.ConvertSectionData();
            }

            if (map.Altered)
            {
                Logger.Info("Saving the modified map as '" + settings.FileOutput + "'.");
                map.Save();
            }
        }

        private static void initLogger(bool writefile = false)
        {
            string filename = AppDomain.CurrentDomain.BaseDirectory + Path.GetFileNameWithoutExtension(Assembly.GetEntryAssembly().Location) + ".log";
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
