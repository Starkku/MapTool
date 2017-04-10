# MapTool

This program exists to apply conversion profiles, 'scripts' of sorts to map files from Command & Conquer: Tiberian Sun and Command & Conquer: Red Alert 2 and their respective expansion packs that offer ability to alter map's theater, tile, overlay and other object data, essentially allowing user to perform operations such as cross-theater, or even cross-game map conversions.

Project Page:

* http://github.com/starkku/maptool

Download:

## Installation

As of current, MapTool only runs on Windows operating systems, with Microsoft .NET Framework 4.0 as a requirement. Installation is simple, just place all of the required program files in a directory of your choice, and run it from there. 

## Usage

Using the graphical user interface (MapTool_UI.exe) should be fairly straightforward. If the program was installed correctly, available conversion profiles (conversion profiles are loaded from subdirectory called 'Profiles' in the program directory) should be displayed in a list for user to choose from, with a description for each of the profiles displayed next to the list if available. Maps can be added to the list of files to process by using Browse button or drag & drop on the file list itself.

Instructions for the command line tool (MapTool.exe) can be found by running the program with argument -h (f.ex `MapTool.exe -h`).

Documentation on the contents of conversion profile INI files and how to write them can be found [here](https://github.com/starkku/maptool/blob/master/Conversion-Profile-Documentation.md).

## Acknowledgements

Special thanks to the following open source projects and people who have contributed to them - as their work has been pivotal in making the implementation of MapTool possible.

* CnCMaps Renderer by Frank Razenberg - http://github.com/zzattack/ccmaps-net
* OpenRA by [OpenRA Authors](https://raw.github.com/OpenRA/OpenRA/master/AUTHORS) - http://github.com/OpenRA/OpenRA

## License

The MIT License (MIT)

Copyright (c) 2017 Starkku

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.

Above license __does not apply__ to following portions of this software which are licensed under different, conflicting licenses.

* NLog, which is released under a [3-clause BSD License](https://github.com/NLog/NLog/blob/master/LICENSE.txt).
 * NLog.dll, NLog.config
* Code from CnCMaps Renderer project, which is released under [GPLv3](https://github.com/zzattack/ccmaps-net/blob/master/COPYING).
 * https://github.com/starkku/maptool/blob/master/MapTool/Utility/Format5.cs
 * https://github.com/starkku/maptool/blob/master/MapTool/Utility/Format80.cs
 * https://github.com/starkku/maptool/blob/master/MapTool/Utility/MemoryFile.cs
 * https://github.com/starkku/maptool/blob/master/MapTool/Utility/MiniLZO.cs
 * https://github.com/starkku/maptool/blob/master/MapTool/Utility/VirtualFile.cs
* Code from OpenRA project, which is released under [GPLv3](https://raw.github.com/OpenRA/OpenRA/master/COPYING).
 * https://github.com/starkku/maptool/blob/master/MapTool/Utility/Format80.cs
* MiniLZO by Markus F.X.J. Oberhumer, released under [GPL v2+](http://www.oberhumer.com/opensource/gpl.html).
 * https://github.com/starkku/maptool/blob/master/MapTool/Utility/MiniLZO.cs

In addition, code from following projects with similar, compatible licenses is used.

* NDesk Options
 * https://github.com/starkku/maptool/blob/master/MapTool/Utility/Options.cs
* Nini
 * http://nini.sourceforge.net/license.php