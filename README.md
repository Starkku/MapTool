# MapTool

This program exists to apply conversion profiles, 'scripts' of sorts to map files from Command & Conquer: Tiberian Sun and Command & Conquer: Red Alert 2 and their respective expansion packs that offer ability to alter map's theater, tile, overlay and other object data, essentially allowing user to perform operations such as cross-theater, or even cross-game map conversions.

Project Page:

* http://github.com/Starkku/MapTool

Downloads: 

* https://github.com/Starkku/MapTool/releases

## Installation

As of current, MapTool has been designed to run on Windows operating systems, with Microsoft .NET Framework 4.0 as a requirement. Installation is simple, just place all of the required program files in a directory of your choice, and run it from there. 

## Usage

Using the graphical user interface (MapTool_UI.exe) should be fairly straightforward. If the program was installed correctly, available conversion profiles (conversion profiles are loaded from subdirectory called 'Profiles' in the program directory) should be displayed in a list for user to choose from, with a description for each of the profiles displayed next to the list if available. Maps can be added to the list of files to process by using Browse button or drag & drop on the file list itself.

Instructions for the command line tool (MapTool.exe) can be found by running the program with argument -h (f.ex `MapTool.exe -h`).
MapTool_UI.exe can also be ran with the argument -log, which enables writing of full debug log file (which can get fairly large), and is passed to MapTool.exe.

Documentation on the contents of conversion profile INI files and how to write them can be found [here](https://github.com/starkku/maptool/blob/master/Conversion-Profile-Documentation.md).

## Acknowledgements

MapTool uses code from the following open-source projects to make it's functionality possible.

* CnCMaps Renderer by Frank Razenberg - http://github.com/zzattack/ccmaps-net
* OpenRA by [OpenRA Authors](https://raw.github.com/OpenRA/OpenRA/master/AUTHORS) - http://github.com/OpenRA/OpenRA
* FinalSun Tunnel Fixer by Rampastring - https://ppmforums.com/viewtopic.php?t=42008

Additionally thanks to [E1 Elite](https://ppmforums.com/profile.php?mode=viewprofile&u=7356) for writing several conversion profile files to use with Command & Conquer: Tiberian Sun and implementing IsoMapPack5 optimization & ice growth fix features.

## License

See [COPYING](https://github.com/Starkku/MapTool/blob/master/COPYING).
