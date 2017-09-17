# Conversion Profile Documentation

Conversion profiles used by MapTool are INI files (text-based configuration files) which determine what sort of changes to tile & object data the tool should perform.

Users unfamiliar with structure of INI files should use one of the pre-made profile INI files as a reference.

## Available Sections


### ProfileData

#### Name
Name displayed in the GUI for this profile.

#### Description
Description displayed in the GUI for this profile.

#### IncludeFiles
A comma-separated list of filenames including file extensions to read from *same directory as the current conversion profile*. Contents of these files will be merged with the current one. This only works on one level, so trying to include files from already included files will fail.

#### ApplyMapOptimization
If set to yes/true, will ensure that on the saved map, section with name 'Basic' will be the first section and section with name 'Digest' will be the last. This potentially allows for game to find these particular sections marginally faster.

#### ApplyMapCompress
If set to yes/true, no unnecessary white space is put on the saved map. This allows for map size to be marginally smaller.

### TheaterRules

#### ApplicableTheaters
A comma-separated list of theater ID's which must match with one declared in a map for the tool to process it. Defaults to <pre>ApplicableTheaters=TEMPERATE,SNOW,URBAN,DESERT,NEWURBAN,LUNAR</pre> if a list is omitted.

### NewTheater
A single theater ID which is assigned on any processed maps.


### TileRules

A list of tile index conversion rules, each on it's own line with | as separator between source and destination value, as well as optional height override and sub-tile index override values.

To assist in figuring out the numbers to use, MapTool can be run with command line parameter *-l* with a game theater configuration INI (such as temperat(md).ini) as input file to extract a listing of tiles and their tile indices to a plaintext output file.

**Example #1:**
<pre>
[TileRules]
0-15|25-40
</pre>

Tiles 0-15 will get converted to tiles 25-40, respectively, respecting the range declarations.

**Example #2:**
<pre>
[TileRules]
0-15|25
</pre>

This example should produce results identical with the first one.

**Example #3:**
<pre>
[TileRules]
0-15|25-25
</pre>

Using a range declaration with identical start and end points as destination forces all matching source tiles to be converted to that specific tile.

**Example #4:**
<pre>
[TileRules]
0-15|25-40|1
</pre>

Adding a third value overrides the height of all of the applicable tiles with specific value. Only values from 0 to 14 are respected, with values lower than 0 interpreted as 0, and values higher than 14 interpreted as 14.

**Example #5:**
<pre>
[TileRules]
0-15|25-40|*|0
</pre>

Fourth value serves as an override to tile's sub-tile index, serving to determine which particular piece of that tile is used for a map cell. It might be necessary to set the override to 0 if you are converting from tiles with more than one sub-tile to a tile with just one.

Also worth noting is that if you declare sub-tile index override, you must also declare height override before it. Substituting the value with * retains the original height values in processed maps.


### OverlayRules

A list of overlay ID conversion rules, each on it's own line with a | as a separator between source and destination value.

**Example:**
<pre>
[OverlayRules]
0|5
</pre>

All overlays with ID 0 are converted to overlays with ID 5.

Values from 0 to 254 are available for regular use. Using 255 as destination ID will remove overlays. Using 255 as source ID is not valid and results in the conversion rule being ignored.


### ObjectRules

A list of object ID conversion rules, each on it's own line with a | as a separator between source and destination value.

**Example #1:**
<pre>
[ObjectRules]
GACNST|YACNST
</pre>

Will convert any objects, be it Infantry, Building, Aircraft, Vehicle or Terrain with ID GACNST on the processed maps to an object of same type with ID YACNST.

**Example #2**
<pre>
[ObjectRules]
GACNST
</pre>

Will remove any objects, be it Infantry, Building, Aircraft, Vehicle or Terrain with ID GACNST on the processed maps.

### SectionRules

A list of section name, keys and values conversion rules, each on it's own line with | as a separator between section name, key and value information.

**Example #1:**
<pre>
[SectionRules]
Basic|Official|no
</pre>

Sets the value for key 'Official' under section 'Basic' to 'no'.

**Example #2:**
<pre>
[SectionRules]
Basic|Official=
</pre>

Removes key 'Official' under section 'Basic'.

**Example #3:**
<pre>
[SectionRules]
Basic=
</pre>

Removes section 'Basic' altogether.

**Example #4:**
<pre>
[SectionRules]
Basic=NotSoBasic|Official=Unofficial|Yes
</pre>

Changes name of section 'Basic' to 'NotSoBasic', name of key 'Official' under said section to 'Unofficial' and it's value to 'Yes'.

**Example #5:**
<pre>
[SectionRules]
Basic|Official|$GETVAL(SpecialFlags,DestroyableBridges)
</pre>

Sets the value of key 'Official' under section 'Basic' to the value of key 'DestroyableBridges' under section 'SpecialFlags'.