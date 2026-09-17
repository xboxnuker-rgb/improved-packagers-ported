#if   Il2Cpp
using Il2CppScheduleOne.ObjectScripts;
#elif Mono
using ScheduleOne.ObjectScripts;
#endif
using MelonLoader;
using System;
using System.Collections.Generic;
using System.IO;

public static class StationModeRegistry
{
    private static readonly Dictionary<string, int> _explicit = new Dictionary<string, int>();
    private static readonly Dictionary<string, int> _sticky   = new Dictionary<string, int>();
    private static readonly HashSet<string> _loggedInferences = new HashSet<string>();

    private static readonly string FilePath = Path.Combine("UserData", "ImprovedPackagers.json");

    public static void Load()
    {
        if (!File.Exists(FilePath)) return;

        _explicit.Clear();
        _sticky.Clear();

        try
        {
            foreach (string line in File.ReadAllLines(FilePath))
            {
                string trimmed = line.Trim();
                if (trimmed.Length == 0 || trimmed.StartsWith("#")) continue;
                string[] split = trimmed.Split(':');
                if (split.Length != 2) continue;
                string guid = split[0];
                if (int.TryParse(split[1], out var mode))
                    _explicit[guid] = mode;
            }
        }
        catch (Exception ex) { MelonLogger.Error($"Load data error {ex.Message}"); }
    }

    public static void Save()
    {
        try
        {
            using var w = new StreamWriter(FilePath, false);
            w.WriteLine("# Improve Packagers: Unpacking Stations");
            foreach (var kv in _explicit)
                w.WriteLine($"{kv.Key}:{kv.Value}");
        }
        catch (Exception ex) { MelonLogger.Error($"Save data error {ex.Message}"); }
    }

    public static void SetExplicit(PackagingStation station, PackagingStation.EMode mode)
    {
        if (station is null) return;

        string guid = station.GUID.ToString();
        bool changed = !_explicit.TryGetValue(guid, out var previous) || previous != (int)mode;
        _explicit[guid] = (int)mode;
        _sticky.Remove(guid);
        ApplyTransitOutput(station, mode);
        Save();

        if (changed)
            MelonLogger.Msg($"Packaging Station mode set to {mode}.");
    }

    public static bool TryGetMode(PackagingStation station, out PackagingStation.EMode mode)
    {
        mode = PackagingStation.EMode.Package;
        if (!(station is null) && _explicit.TryGetValue(station.GUID.ToString(), out var m)) 
        { 
            mode = (PackagingStation.EMode)m; 
            return true;
        }

        if (!(station is null) && _sticky  .TryGetValue(station.GUID.ToString(), out m)) 
        {
            mode = (PackagingStation.EMode)m;
            return true;
        }
        return false;
    }

    public static void SetExplicitIfExists(PackagingStation station)
    {
        if (station is null) return;
        if (_explicit.TryGetValue(station.GUID.ToString(), out var m))
        {
            _sticky[station.GUID.ToString()] = m;
            ApplyTransitOutput(station, (PackagingStation.EMode)m);
        }
    }

    public static void SetStickyIfNone(PackagingStation station, PackagingStation.EMode mode)
    {
        if (station is null) return;
        string guid = station.GUID.ToString();
        if (_explicit.ContainsKey(guid)) return;
        if (_sticky.TryGetValue(guid, out var existing))
            mode = (PackagingStation.EMode)existing;
        else
            _sticky[guid] = (int)mode;
        ApplyTransitOutput(station, mode);
    }

    public static PackagingStation.EMode ResolveForWork(PackagingStation station)
    {
        if (TryGetMode(station, out var mode))
            return mode;

        bool canPackage = station.GetState(PackagingStation.EMode.Package) ==
            PackagingStation.EState.CanBegin;
        bool canUnpackage = station.GetState(PackagingStation.EMode.Unpackage) ==
            PackagingStation.EState.CanBegin;

        // Do not persist a guess while the station is idle or ambiguous. As soon
        // as exactly one f13 mode is ready, use that state to break the first-run
        // chicken-and-egg problem (the NPC cannot claim the station until a mode
        // is known, and SetNPCUser cannot run until the station is claimed).
        if (canUnpackage && !canPackage)
            mode = PackagingStation.EMode.Unpackage;
        else if (canPackage && !canUnpackage)
            mode = PackagingStation.EMode.Package;
        else
            return PackagingStation.EMode.Package;

        SetStickyIfNone(station, mode);
        string key = station.GUID + ":" + mode;
        if (_loggedInferences.Add(key))
            MelonLogger.Msg($"Packaging Station mode inferred as {mode} from its ready state.");
        return mode;
    }

    public static void Remove(PackagingStation station)
    {
        if (station is null) return;
        _explicit.Remove(station.GUID.ToString());
        _sticky.Remove(station.GUID.ToString());
        Save();
    }

    // PR #2 by ecrgr identified that loose product created by Unpack lives in
    // ProductSlot, while vanilla worker transit reads the station's OutputSlots.
    // Point the existing transit route at the slot for the selected mode rather
    // than replacing the game's move-item behaviour with version-specific code.
    public static void ApplyTransitOutput(PackagingStation station, PackagingStation.EMode mode)
    {
        if (station is null || station.OutputSlots is null) return;

        var transitOutput = mode == PackagingStation.EMode.Unpackage
            ? station.ProductSlot
            : station.OutputSlot;

        if (transitOutput is null) return;

        station.OutputSlots.Clear();
        station.OutputSlots.Add(transitOutput);
    }

    public static void ClearAll()
    {
        _explicit.Clear();
        _sticky  .Clear();
        _loggedInferences.Clear();
    }
}
