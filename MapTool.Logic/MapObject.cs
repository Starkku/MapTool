using Starkku.Utilities;
using System;
using System.Collections.Generic;

namespace MapTool.Logic
{
    /// <summary>
    /// Base map object class.
    /// </summary>
    public abstract class MapObject
    {
        /// <summary>
        /// Whether or not object is a properly initialized map object.
        /// </summary>
        public bool Initialized { get; set; }

        /// <summary>
        /// Object ID.
        /// </summary>
        public string ID { get; set; }

        /// <summary>
        /// Object's X coordinate.
        /// </summary>
        public int X { get; set; }

        /// <summary>
        /// Object's Y coordinate.
        /// </summary>
        public int Y { get; set; }

        /// <summary>
        /// Creates new instance of map object.
        /// </summary>
        /// <param name="id">Object ID.</param>
        /// <param name="x">Object's X coordinate.</param>
        /// <param name="y">Object's Y coordinate.</param>
        public MapObject(string id, int x, int y)
        {
            ID = id;
            X = x;
            Y = y;
        }

        /// <summary>
        /// Creates new instance of map object based on INI code.
        /// </summary>
        /// <param name="iniCode">INI code.</param>
#pragma warning disable IDE0060 // Remove unused parameter
        public MapObject(string iniCode)
#pragma warning restore IDE0060 // Remove unused parameter
        {
        }

        /// <summary>
        /// Gets INI code for this map object.
        /// </summary>
        /// <returns>INI code for this map object.</returns>
        public abstract string GetINICode();
    }

    /// <summary>
    /// Base ownable map object class.
    /// </summary>
    public abstract class MapOwnableObject : MapObject
    {
        /// <summary>
        /// House ID for object's owner.
        /// </summary>
        public string Owner { get; set; }

        /// <summary>
        /// Object's current health (measured in 1/256ths of its actual total health).
        /// </summary>
        public int Health { get; set; }

        /// <summary>
        /// Object's facing direction.
        /// </summary>
        public int Facing { get; set; }

        /// <summary>
        /// ID of a tag attached to the object.
        /// </summary>
        public string Tag { get; set; }

        /// <summary>
        /// Creates a new instance of ownable map object.
        /// </summary>
        /// <param name="id">Object ID.</param>
        /// <param name="x">Object's X coordinate.</param>
        /// <param name="y">Object's Y coordinate.</param>
        /// <param name="owner">Object owner.</param>
        /// <param name="health">Object's current health.</param>
        /// <param name="facing">Object facing direction.</param>
        /// <param name="tag">ID of a tag attached to the map object.</param>
        public MapOwnableObject(string id, int x, int y, string owner, int health, int facing, string tag) : base(id, x, y)
        {
            Owner = owner;
            Health = health;
            Facing = facing;
            Tag = tag;
        }

        /// <summary>
        /// Creates new instance of ownable map object based on INI code.
        /// </summary>
        /// <param name="iniCode">INI code.</param>
        public MapOwnableObject(string iniCode) : base(iniCode)
        {
        }
    }

    /// <summary>
    /// Base map unit object class.
    /// </summary>
    public abstract class MapUnitObject : MapOwnableObject
    {
        /// <summary>
        /// Unit's group number.
        /// </summary>
        public int Group { get; set; }

        /// <summary>
        /// Unit's mission.
        /// </summary>
        public Mission Mission { get; set; }

        /// <summary>
        /// Unit's current experience.
        /// </summary>
        public int Veterancy { get; set; }

        /// <summary>
        /// Whether or not unit is recruitable if it belongs to team with AutoCreate=yes.
        /// </summary>
        public bool AutocreateYesRecruitable { get; set; }

        /// <summary>
        /// Whether or not unit is recruitable if it belongs to team with AutoCreate=no.
        /// </summary>
        public bool AutocreateNoRecruitable { get; set; }

        /// <summary>
        /// Creates a new instance of map unit object.
        /// </summary>
        /// <param name="id">Unit ID.</param>
        /// <param name="x">Unit's X coordinate.</param>
        /// <param name="y">Unit's Y coordinate.</param>
        /// <param name="owner">Unit owner.</param>
        /// <param name="health">Unit's current health.</param>
        /// <param name="facing">Unit facing direction.</param>
        /// <param name="tag">ID of a tag attached to the unit.</param>
        /// <param name="group">Unit's group number.</param>
        /// <param name="mission">Unit's mission.</param>
        /// <param name="veterancy">Unit's current experience.</param>
        /// <param name="autocreateNoRecruitable">Whether or not unit is recruitable if it belongs to team with AutoCreate=no.</param>
        /// <param name="autocreateYesRecruitable">Whether or not unit is recruitable if it belongs to team with AutoCreate=yes.</param>
        public MapUnitObject(string id, int x, int y, string owner, int health, int facing, string tag,
            int group, Mission mission, int veterancy, bool autocreateNoRecruitable, bool autocreateYesRecruitable) : base(id, x, y, owner, health, facing, tag)
        {
            Group = group;
            Mission = mission;
            Veterancy = veterancy;
            AutocreateYesRecruitable = autocreateYesRecruitable;
            AutocreateNoRecruitable = autocreateNoRecruitable;
            Initialized = true;
        }

        /// <summary>
        /// Creates new instance of map unit object based on INI code.
        /// </summary>
        /// <param name="iniCode">INI code.</param>
        public MapUnitObject(string value) : base(value)
        {
        }
    }

    /// <summary>
    /// Map aircraft object class.
    /// </summary>
    public class MapAircraftObject : MapUnitObject
    {

        /// <summary>
        /// Creates a new instance of map aircraft object.
        /// </summary>
        /// <param name="id">Aircraft ID.</param>
        /// <param name="x">Aircraft's X coordinate.</param>
        /// <param name="y">Aircraft's Y coordinate.</param>
        /// <param name="owner">Aircraft owner.</param>
        /// <param name="health">Aircraft's current health.</param>
        /// <param name="facing">Aircraft facing direction.</param>
        /// <param name="tag">ID of a tag attached to the aircraft.</param>
        /// <param name="group">Aircraft's group number.</param>
        /// <param name="mission">Aircraft's mission.</param>
        /// <param name="veterancy">Aircraft's current experience.</param>
        /// <param name="autocreateNoRecruitable">Whether or not aircraft is recruitable if it belongs to team with AutoCreate=no.</param>
        /// <param name="autocreateYesRecruitable">Whether or not aircraft is recruitable if it belongs to team with AutoCreate=yes.</param>
        public MapAircraftObject(string id, int x, int y, string owner, int health, int facing, string tag,
            int group, Mission mission, int veterancy, bool autocreateNoRecruitable, bool autocreateYesRecruitable)
            : base(id, x, y, owner, health, facing, tag, group, mission, veterancy, autocreateNoRecruitable, autocreateYesRecruitable)
        {
            Initialized = true;
        }

        /// <summary>
        /// Creates new instance of map aircraft object based on INI code.
        /// </summary>
        /// <param name="iniCode">INI code.</param>
        public MapAircraftObject(string value) : base(value)
        {
            string[] values = value.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);

            if (values.Length < 12)
                return;

            Owner = values[0];
            ID = values[1];
            Health = Math.Min(Math.Max(Conversion.GetIntFromString(values[2], 256), 0), 256);
            X = Conversion.GetIntFromString(values[3], -1);
            Y = Conversion.GetIntFromString(values[4], -1);
            Facing = Conversion.GetIntFromString(values[5], 0);

            if (Enum.TryParse(values[6].Replace(" ", "_"), out Mission mission))
                Mission = mission;

            Tag = values[7];
            Veterancy = Conversion.GetIntFromString(values[8], 0);
            Group = Conversion.GetIntFromString(values[9], -1);
            AutocreateNoRecruitable = Conversion.GetIntFromString(values[10], 1) != 0;
            AutocreateYesRecruitable = Conversion.GetIntFromString(values[11], 0) != 0;

            Initialized = true;
        }

        /// <summary>
        /// Gets INI code for map aircraft object.
        /// </summary>
        /// <returns>INI code for map object.</returns>
        public override string GetINICode()
        {
            List<string> values = new List<string>
            {
                Owner,
                ID,
                Health.ToString(),
                X.ToString(),
                Y.ToString(),
                Facing.ToString(),
                Mission.ToString().Replace("_", " "),
                Tag,
                Veterancy.ToString(),
                Group.ToString(),
                AutocreateNoRecruitable ? "1" : "0",
                AutocreateYesRecruitable ? "1" : "0"
            };

            return string.Join(",", values);
        }
    }


    /// <summary>
    /// Map building object class.
    /// </summary>
    public class MapBuildingObject : MapOwnableObject
    {
        /// <summary>
        /// Whether or not building is sellable by AI.
        /// </summary>
        public bool AISellable { get; set; }

        /// <summary>
        /// Whether or not building can be rebuilt by AI.
        /// </summary>
        public bool AIRebuildable { get; set; }

        /// <summary>
        /// Whether or not building can be repaired by AI.
        /// </summary>
        public bool AIRepairable { get; set; }

        /// <summary>
        /// Whether or not building is powered up.
        /// </summary>
        public bool Powered { get; set; }

        /// <summary>
        /// Spotlight mode for building.
        /// 0 = No spotlight.
        /// 1 = Reciprocating spotlight.
        /// 2 = Circular spotlight.
        /// Any other values will be treated as 0.
        /// </summary>
        public int SpotlightMode { get; set; }

        /// <summary>
        /// Number of upgrades on the building.
        /// </summary>
        public int UpgradeCount { get; set; }

        /// <summary>
        /// Whether or not building's full name is displayed in tooltip for all players.
        /// </summary>
        public bool Nominal { get; set; }

        /// <summary>
        /// List of upgrades on the building.
        /// </summary>
        public string[] Upgrades { get; private set; } = new string[3] { "None", "None", "None" };

        /// <summary>
        /// Creates a new instance of map building object.
        /// </summary>
        /// <param name="id">Building ID.</param>
        /// <param name="x">Building's X coordinate.</param>
        /// <param name="y">Building's Y coordinate.</param>
        /// <param name="owner">Building's owner.</param>
        /// <param name="health">Building's current health.</param>
        /// <param name="facing">Building's facing direction.</param>
        /// <param name="tag">ID of a tag attached to the building.</param>
        /// <param name="aiSellable">Whether or not building is sellable by AI.</param>
        /// <param name="aiRebuildable">Whether or not building can be rebuilt by AI.</param>
        /// <param name="aiRepairable">Whether or not building can be repaired by AI.</param>
        /// <param name="powered">Whether or not building is powered up.</param>
        /// <param name="spotlightMode">Spotlight mode for building.</param>
        /// <param name="upgrades">List of upgrades on the building.</param>
        /// <param name="nominal">Whether or not building's full name is displayed in tooltip for all players.</param>
        public MapBuildingObject(string id, int x, int y, string owner, int health, int facing, string tag,
            bool aiSellable, bool aiRebuildable, bool aiRepairable, bool powered, int spotlightMode, IList<string> upgrades, bool nominal)
            : base(id, x, y, owner, health, facing, tag)
        {
            AISellable = aiSellable;
            AIRebuildable = aiRebuildable;
            AIRepairable = aiRepairable;
            Powered = powered;
            Nominal = nominal;
            SpotlightMode = spotlightMode;
            UpgradeCount = 0;

            foreach (string upgrade in upgrades)
            {
                Upgrades[UpgradeCount++] = upgrade;

                if (UpgradeCount == Upgrades.Length)
                    break;
            }

            Initialized = true;
        }

        /// <summary>
        /// Creates new instance of map building object based on INI code.
        /// </summary>
        /// <param name="iniCode">INI code.</param>
        public MapBuildingObject(string value) : base(value)
        {
            string[] values = value.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);

            if (values.Length < 17)
                return;

            Owner = values[0];
            ID = values[1];
            Health = Math.Min(Math.Max(Conversion.GetIntFromString(values[2], 256), 0), 256);
            X = Conversion.GetIntFromString(values[3], -1);
            Y = Conversion.GetIntFromString(values[4], -1);
            Facing = Conversion.GetIntFromString(values[5], 0);
            Tag = values[6];
            AISellable = Conversion.GetIntFromString(values[7], 1) != 0;
            AIRebuildable = Conversion.GetIntFromString(values[8], 0) != 0;
            Powered = Conversion.GetIntFromString(values[9], 1) != 0;
            UpgradeCount = Conversion.GetIntFromString(values[10], 0);
            SpotlightMode = Conversion.GetIntFromString(values[11], 0);
            Upgrades[0] = values[12];
            Upgrades[1] = values[13];
            Upgrades[2] = values[14];
            AIRepairable = Conversion.GetIntFromString(values[15], 0) != 0;
            Nominal = Conversion.GetIntFromString(values[16], 0) != 0;

            Initialized = true;
        }

        /// <summary>
        /// Gets INI code for map building object.
        /// </summary>
        /// <returns>INI code for map object.</returns>
        public override string GetINICode()
        {
            List<string> values = new List<string>
            {
                Owner,
                ID,
                Health.ToString(),
                X.ToString(),
                Y.ToString(),
                Facing.ToString(),
                Tag,
                AISellable ? "1" : "0",
                AIRebuildable ? "1" : "0",
                Powered ? "1" : "0",
                UpgradeCount.ToString(),
                SpotlightMode.ToString(),
                Upgrades[0],
                Upgrades[1],
                Upgrades[2],
                AIRepairable ? "1" : "0",
                Nominal ? "1" : "0"
            };

            return string.Join(",", values);
        }

        /// <summary>
        /// Set upgrades & upgrade count for building object.
        /// Only up to three upgrade IDs from the given list will be applied.
        /// </summary>
        /// <param name="upgrades">List of upgrade IDs.</param>
        public void SetUpgrades(IList<string> upgrades)
        {
            if (upgrades == null || upgrades.Count < 1)
                return;

            UpgradeCount = 0;
            for (int i = 0; i < Upgrades.Length; i++)
            {
                if (i >= upgrades.Count)
                    Upgrades[i] = "None";
                else
                {
                    Upgrades[i] = upgrades[i];
                    UpgradeCount++;
                }
            }
        }
    }

    /// <summary>
    /// Map infantry object class.
    /// </summary>
    public class MapInfantryObject : MapUnitObject
    {
        /// <summary>
        /// Infantry's sub-cell position.
        /// </summary>
        public byte SubCell { get; set; }

        /// <summary>
        /// Whether or not infantry is spawned above ground (such as on a bridge).
        /// </summary>
        public bool OnBridge { get; set; }

        /// <summary>
        /// Creates a new instance of map building object.
        /// </summary>
        /// <param name="id">Infantry ID.</param>
        /// <param name="x">Infantry's X coordinate.</param>
        /// <param name="y">Infantry's Y coordinate.</param>
        /// <param name="owner">Infantry's owner.</param>
        /// <param name="health">Infantry's current health.</param>
        /// <param name="facing">Infantry's facing direction.</param>
        /// <param name="tag">ID of a tag attached to the infantry.</param>
        /// <param name="group">infantry's group number.</param>
        /// <param name="mission">infantry's mission.</param>
        /// <param name="veterancy">infantry's current experience.</param>
        /// <param name="autocreateNoRecruitable">Whether or not infantry is recruitable if it belongs to team with AutoCreate=no.</param>
        /// <param name="autocreateYesRecruitable">Whether or not infantry is recruitable if it belongs to team with AutoCreate=yes.</param>
        /// <param name="subCell">Infantry's sub-cell position.</param>
        /// <param name="onBridge">Whether or not infantry is spawned above ground (such as on a bridge).</param>
        public MapInfantryObject(string id, int x, int y, string owner, int health, int facing, string tag,
            int group, Mission mission, int veterancy, bool autocreateNoRecruitable, bool autocreateYesRecruitable, byte subCell, bool onBridge)
            : base(id, x, y, owner, health, facing, tag, group, mission, veterancy, autocreateNoRecruitable, autocreateYesRecruitable)
        {
            SubCell = subCell;
            OnBridge = onBridge;
            Initialized = true;
        }

        /// <summary>
        /// Creates new instance of map infantry object based on INI code.
        /// </summary>
        /// <param name="iniCode">INI code.</param>
        public MapInfantryObject(string value) : base(value)
        {
            string[] values = value.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);

            if (values.Length < 14)
                return;

            Owner = values[0];
            ID = values[1];
            Health = Math.Min(Math.Max(Conversion.GetIntFromString(values[2], 256), 0), 256);
            X = Conversion.GetIntFromString(values[3], -1);
            Y = Conversion.GetIntFromString(values[4], -1);
            SubCell = (byte)(Math.Min(Math.Max(Conversion.GetIntFromString(values[5], 1), 0), 3));

            if (Enum.TryParse(values[6].Replace(" ", "_"), out Mission mission))
                Mission = mission;

            Facing = Conversion.GetIntFromString(values[7], 0);
            Tag = values[8];
            Veterancy = Conversion.GetIntFromString(values[9], 0);
            Group = Conversion.GetIntFromString(values[10], -1);
            OnBridge = Conversion.GetIntFromString(values[11], 0) != 0;
            AutocreateNoRecruitable = Conversion.GetIntFromString(values[12], 1) != 0;
            AutocreateYesRecruitable = Conversion.GetIntFromString(values[13], 0) != 0;

            Initialized = true;
        }

        /// <summary>
        /// Gets INI code for map infantry object.
        /// </summary>
        /// <returns>INI code for map infantry object.</returns>
        public override string GetINICode()
        {
            List<string> values = new List<string>
            {
                Owner,
                ID,
                Health.ToString(),
                X.ToString(),
                Y.ToString(),
                SubCell.ToString(),
                Mission.ToString().Replace("_", " "),
                Facing.ToString(),
                Tag,
                Veterancy.ToString(),
                Group.ToString(),
                OnBridge ? "1" : "0",
                AutocreateNoRecruitable ? "1" : "0",
                AutocreateYesRecruitable ? "1" : "0"
            };

            return string.Join(",", values);
        }
    }


    /// <summary>
    /// Map vehicle object class.
    /// </summary>
    public class MapVehicleObject : MapUnitObject
    {
        /// <summary>
        /// Index of a VehicleType to follow this vehicle.
        /// </summary>
        public int FollowsIndex { get; set; }

        /// <summary>
        /// Whether or not vehicle is spawned above ground (such as on a bridge).
        /// </summary>
        public bool OnBridge { get; set; }

        /// <summary>
        /// Creates a new instance of map vehicle object.
        /// </summary>
        /// <param name="id">Vehicle ID.</param>
        /// <param name="x">Vehicle's X coordinate.</param>
        /// <param name="y">Vehicle's Y coordinate.</param>
        /// <param name="owner">Vehicle's owner.</param>
        /// <param name="health">Vehicle's current health.</param>
        /// <param name="facing">Vehicle's facing direction.</param>
        /// <param name="tag">ID of a tag attached to the vehicle.</param>
        /// <param name="group">Vehicle's group number.</param>
        /// <param name="mission">Vehicle's mission.</param>
        /// <param name="veterancy">Vehicle's current experience.</param>
        /// <param name="autocreateNoRecruitable">Whether or not vehicle is recruitable if it belongs to team with AutoCreate=no.</param>
        /// <param name="autocreateYesRecruitable">Whether or not vehicle is recruitable if it belongs to team with AutoCreate=yes.</param>
        /// <param name="followsIndex">Index of a VehicleType to follow this vehicle.</param>
        /// <param name="onBridge">Whether or not vehicle is spawned above ground (such as on a bridge).</param>
        public MapVehicleObject(string id, int x, int y, string owner, int health, int facing, string tag,
            int group, Mission mission, int veterancy, bool autocreateYesRecruitable, bool autocreateNoRecruitable, int followsIndex, bool onBridge)
            : base(id, x, y, owner, health, facing, tag, group, mission, veterancy, autocreateYesRecruitable, autocreateNoRecruitable)
        {
            FollowsIndex = followsIndex;
            OnBridge = onBridge;
            Initialized = true;
        }

        /// <summary>
        /// Creates new instance of map vehicle object based on INI code.
        /// </summary>
        /// <param name="iniCode">INI code.</param>
        public MapVehicleObject(string value) : base(value)
        {
            string[] values = value.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);

            if (values.Length < 14)
                return;

            Owner = values[0];
            ID = values[1];
            Health = Math.Min(Math.Max(Conversion.GetIntFromString(values[2], 256), 0), 256);
            X = Conversion.GetIntFromString(values[3], -1);
            Y = Conversion.GetIntFromString(values[4], -1);
            Facing = Conversion.GetIntFromString(values[5], 0);

            if (Enum.TryParse(values[6].Replace(" ", "_"), out Mission mission))
                Mission = mission;

            Tag = values[7];
            Veterancy = Conversion.GetIntFromString(values[8], 0);
            Group = Conversion.GetIntFromString(values[9], -1);
            OnBridge = Conversion.GetIntFromString(values[10], 0) != 0;
            FollowsIndex = Conversion.GetIntFromString(values[11], -1);
            AutocreateNoRecruitable = Conversion.GetIntFromString(values[12], 1) != 0;
            AutocreateYesRecruitable = Conversion.GetIntFromString(values[13], 0) != 0;

            Initialized = true;
        }

        /// <summary>
        /// Gets INI code for map vehicle object.
        /// </summary>
        /// <returns>INI code for map vehicle object.</returns>
        public override string GetINICode()
        {
            List<string> values = new List<string>
            {
                Owner,
                ID,
                Health.ToString(),
                X.ToString(),
                Y.ToString(),
                Facing.ToString(),
                Mission.ToString().Replace("_", " "),
                Tag,
                Veterancy.ToString(),
                Group.ToString(),
                OnBridge ? "1" : "0",
                FollowsIndex.ToString(),
                AutocreateNoRecruitable ? "1" : "0",
                AutocreateYesRecruitable ? "1" : "0"
            };

            return string.Join(",", values);
        }
    }

    /// <summary>
    /// Map terrain object class.
    /// </summary>
    public class MapTerrainObject : MapObject
    {
        /// <summary>
        /// Creates a new instance of map terrain object.
        /// </summary>
        /// <param name="id">Terrain object ID.</param>
        /// <param name="x">Terrain object's X coordinate.</param>
        /// <param name="y">Terrain object's Y coordinate.</param>
        public MapTerrainObject(string id, int x, int y) : base(id, x, y)
        {
            Initialized = true;
        }

        /// <summary>
        /// Creates new instance of map terrain object based on INI code.
        /// </summary>
        /// <param name="iniCode">INI code.</param>
        public MapTerrainObject(string key, string value) : base(value)
        {
            ID = value;
            int coord = Conversion.GetIntFromString(key, -1);
            if (coord >= 0)
            {
                X = coord % 1000;
                Y = (coord - X) / 1000;
                Initialized = true;
            }
        }

        /// <summary>
        /// Gets INI code for map terrain object.
        /// </summary>
        /// <returns>INI code for map terrain object.</returns>
        public override string GetINICode() => ID;
    }

    /// <summary>
    /// Represents missions units placed on maps can have.
    /// </summary>
    public enum Mission
    {
        NotDefined,
        Sleep,
        Harmless,
        Sticky,
        Attack,
        Move,
        Patrol,
        QMove,
        Retreat,
        Guard,
        Enter,
        Eaten,
        Capture,
        Harvest,
        Area_Guard,
        Return,
        Stop,
        Ambush,
        Hunt,
        Unload,
        Sabotage,
        Construction,
        Selling,
        Repair,
        Rescue,
        Missile,
        Open
    }
}
