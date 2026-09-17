#if   Il2Cpp
using Il2CppFishNet;
using Il2CppFishNet.Object;
using Il2CppInterop.Runtime.InteropTypes;
using Il2CppScheduleOne.Employees;
using Il2CppScheduleOne.Management;
using Il2CppScheduleOne.NPCs.Behaviour;
using Il2CppScheduleOne.ObjectScripts;
using Il2CppScheduleOne.Persistence;
using Il2CppScheduleOne.UI.Stations;
#elif Mono
using FishNet;
using FishNet.Object;
using ScheduleOne.Employees;
using ScheduleOne.Management;
using ScheduleOne.NPCs.Behaviour;
using ScheduleOne.ObjectScripts;
using ScheduleOne.Persistence;
using ScheduleOne.UI.Stations;
#endif
using HarmonyLib;
using MelonLoader;
using UnityEngine;
using UnityEngine.AI;

namespace ImprovedPackagers
{
    static class UnpackTransitRouting
    {
        public static PackagerConfiguration GetConfiguration(Packager packager)
        {
#if Il2Cpp
            return (packager?.Configuration as Il2CppObjectBase)?.TryCast<PackagerConfiguration>();
#elif Mono
            return packager?.Configuration as PackagerConfiguration;
#endif
        }

        public static PackagingStationConfiguration GetConfiguration(PackagingStation station)
        {
#if Il2Cpp
            return (station?.Configuration as Il2CppObjectBase)?.TryCast<PackagingStationConfiguration>();
#elif Mono
            return station?.Configuration as PackagingStationConfiguration;
#endif
        }

        public static bool HasValidProductRoute(Packager packager, PackagingStation station)
        {
            if (packager?.MoveItemBehaviour is null || station?.ProductSlot?.ItemInstance is null)
                return false;
            if (station.ProductSlot.Quantity <= 0)
                return false;

            var route = GetConfiguration(station)?.DestinationRoute;
            if (route is null || !route.AreEntitiesNonNull())
                return false;

            return packager.MoveItemBehaviour.IsTransitRouteValid(
                route,
                station.ProductSlot.ItemInstance,
                out _);
        }
    }

    /*
     * PackagingStationCanvas Patches
     * */
    [HarmonyPatch(typeof(PackagingStationCanvas), nameof(PackagingStationCanvas.Open), new[] { typeof(PackagingStation) })]
    static class PSCanvasOpenPatch
    {
        static void Postfix(PackagingStationCanvas __instance, PackagingStation station)
        {
            if (__instance is null || station is null) return;

            if (StationModeRegistry.TryGetMode(station, out var savedMode))
                __instance.SetMode(savedMode);

            StationModeRegistry.SetExplicit(station, __instance.CurrentMode);
        }
    }

    [HarmonyPatch(typeof(PackagingStationCanvas), nameof(PackagingStationCanvas.ToggleMode))]
    static class PSCanvasToggleModePatch
    {
        static void Postfix(PackagingStationCanvas __instance)
        {
            if (__instance is null || __instance.Station is null) return;
            StationModeRegistry.SetExplicit(__instance.Station, __instance.CurrentMode);
        }
    }

    /*
     * PackagingStationBehaviour Patches
     * */
    [HarmonyPatch(typeof(PackagingStationBehaviour), nameof(PackagingStationBehaviour.IsStationReady))]
    static class PSBehaviourIsStationReadyPatch
    {
        static bool Prefix(PackagingStationBehaviour __instance, PackagingStation station, ref bool __result)
        {
            if (station is null) { __result = false; return false; }

            var mode = PackagingStation.EMode.Package;
            StationModeRegistry.TryGetMode(station, out mode);

            if (station.GetState(mode) != PackagingStation.EState.CanBegin)
            {
                __result = false;
                return false;
            }
#if Il2Cpp
            IUsable usable = (station as Il2CppObjectBase)?.TryCast<IUsable>();
#elif Mono
            IUsable usable = station as IUsable;
#endif
            if (!(usable is null) && usable.IsInUse && station.NPCUserObject != __instance.Npc.NetworkObject)
            {
                __result = false;
                return false;
            }

            Vector3 target  = station.StandPoint.position;
            Vector3 backoff = -station.StandPoint.forward * 0.25f;
            Vector3 desired = target + backoff;
            Vector3 driveTo = NavMesh.SamplePosition(desired, out var hit, 0.6f, NavMesh.AllAreas) ? hit.position : desired;

            __result = __instance.Npc.Movement.CanGetTo(station.StandPoint.position);
            return false;
        }
    }

    /*
     * PackagingStation Patches
     * */
    [HarmonyPatch(typeof(PackagingStation), nameof(PackagingStation.Awake))]
    static class PackaginStationAwakePatch
    {
        static void Postfix(PackagingStation __instance) => StationModeRegistry.SetExplicitIfExists(__instance);
    }

    [HarmonyPatch(typeof(PackagingStation), nameof(PackagingStation.PackSingleInstance))]
    static class PackagingStationPackSingleInstancePatch
    {
        static bool Prefix(PackagingStation __instance)
        {
            if (__instance is null || __instance.NPCUserObject is null) return true;

            var mode = PackagingStation.EMode.Package;
            if (StationModeRegistry.TryGetMode(__instance, out var chosen))
                mode = chosen;

            if (InstanceFinder.IsServer && mode == PackagingStation.EMode.Unpackage)
            {
                __instance.Unpack();
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(Packager), "GetStationMoveItems", new System.Type[] { })]
    static class PackagerGetStationMoveItemsPatch
    {
        static void Postfix(Packager __instance, ref PackagingStation __result)
        {
            try
            {
                if (!(__result is null))
                {
                    if (!StationModeRegistry.TryGetMode(__result, out var selectedMode) ||
                        selectedMode != PackagingStation.EMode.Unpackage)
                        return;

                    if (UnpackTransitRouting.HasValidProductRoute(__instance, __result))
                        return;

                    // Never let the vanilla path carry the packaged item away
                    // from an unpack-mode station's physical OutputSlot.
                    __result = null;
                }

                var configuration = UnpackTransitRouting.GetConfiguration(__instance);
                if (configuration?.AssignedStations is null) return;

                foreach (var station in configuration.AssignedStations)
                {
                    if (station is null ||
                        !StationModeRegistry.TryGetMode(station, out var mode) ||
                        mode != PackagingStation.EMode.Unpackage)
                        continue;

                    if (UnpackTransitRouting.HasValidProductRoute(__instance, station))
                    {
                        __result = station;
                        return;
                    }
                }
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"Failed to select unpacked product for delivery: {ex}");
                __result = null;
            }
        }
    }

    [HarmonyPatch(typeof(Packager), "StartMoveItem", new[] { typeof(PackagingStation) })]
    static class PackagerStartMoveItemPatch
    {
        static bool Prefix(Packager __instance, PackagingStation station)
        {
            if (station is null ||
                !StationModeRegistry.TryGetMode(station, out var mode) ||
                mode != PackagingStation.EMode.Unpackage)
                return true;

            try
            {
                if (!UnpackTransitRouting.HasValidProductRoute(__instance, station))
                    return false;

                var route = UnpackTransitRouting.GetConfiguration(station).DestinationRoute;
                var product = station.ProductSlot.ItemInstance;

#if Il2Cpp
                __instance.MoveItemBehaviour.Initialize(route, product, 0, false);
                __instance.MoveItemBehaviour.Enable_Networked();
#elif Mono
                __instance.MoveItemBehaviour.Initialize(route, product);
                __instance.MoveItemBehaviour.Enable_Networked(null);
#endif
                MelonLogger.Msg("Packager started delivery of unpacked product.");
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"Failed to start unpacked product delivery: {ex}");
            }

            return false;
        }
    }

    [HarmonyPatch(typeof(PackagingStation), nameof(PackagingStation.SetNPCUser))]
    static class PackagingStationSetNPCUserPatch
    {
        static void Postfix(PackagingStation __instance, NetworkObject npcObject)
        {
            if (__instance is null || npcObject is null) return;
            if (!StationModeRegistry.TryGetMode(__instance, out _))
            {
                bool canPack   = __instance.GetState(PackagingStation.EMode.Package)   == PackagingStation.EState.CanBegin;
                bool canUnpack = __instance.GetState(PackagingStation.EMode.Unpackage) == PackagingStation.EState.CanBegin;

                var chosen = canUnpack && !canPack ? PackagingStation.EMode.Unpackage : PackagingStation.EMode.Package;
                StationModeRegistry.SetStickyIfNone(__instance, chosen);
            }
        }
    }

    [HarmonyPatch(typeof(PackagingStation), nameof(PackagingStation.Destroy))]
    static class PackagingStationDestroyPatch
    {
        static void Postfix(PackagingStation __instance) => StationModeRegistry.Remove(__instance);
    }

    /*
     * SaveManager Patches
     * */
    [HarmonyPatch(typeof(SaveManager), nameof(SaveManager.Save), new[] { typeof(string) })]
    public class SaveManagerSavePatch
    {
        static void Prefix() => StationModeRegistry.Save();
    }
}
