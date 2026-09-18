#if   Il2Cpp
using Il2CppFishNet;
using Il2CppFishNet.Object;
using Il2CppInterop.Runtime.InteropTypes;
using Il2CppScheduleOne.Employees;
using Il2CppScheduleOne.ItemFramework;
using Il2CppScheduleOne.Management;
using Il2CppScheduleOne.NPCs.Behaviour;
using Il2CppScheduleOne.ObjectScripts;
using Il2CppScheduleOne.Persistence;
using Il2CppScheduleOne.UI.Stations;
#elif Mono
using FishNet;
using FishNet.Object;
using ScheduleOne.Employees;
using ScheduleOne.ItemFramework;
using ScheduleOne.Management;
using ScheduleOne.NPCs.Behaviour;
using ScheduleOne.ObjectScripts;
using ScheduleOne.Persistence;
using ScheduleOne.UI.Stations;
#endif
using HarmonyLib;
using MelonLoader;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace ImprovedPackagers
{
    static class UnpackTransitRouting
    {
        private static readonly HashSet<string> LoggedRouteFailures = new HashSet<string>();

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
            => HasValidProductRoute(packager, station, out _);

        public static bool HasValidProductRoute(
            Packager packager,
            PackagingStation station,
            out string invalidReason)
        {
            invalidReason = string.Empty;
            if (packager?.MoveItemBehaviour is null || station?.ProductSlot?.ItemInstance is null)
            {
                invalidReason = "No loose product is available yet.";
                return false;
            }
            if (station.ProductSlot.Quantity <= 0)
            {
                invalidReason = "The loose-product quantity is zero.";
                return false;
            }

            var route = GetConfiguration(station)?.DestinationRoute;
            if (route is null || !route.AreEntitiesNonNull())
            {
                invalidReason = "The selected storage did not produce a complete destination route.";
                return false;
            }

            return packager.MoveItemBehaviour.IsTransitRouteValid(
                route,
                station.ProductSlot.ItemInstance,
                out invalidReason);
        }

        public static void LogRouteFailureOnce(PackagingStation station, string reason)
        {
            if (station is null || string.IsNullOrEmpty(reason)) return;
            string key = station.GUID + ":" + reason;
            if (LoggedRouteFailures.Add(key))
                MelonLogger.Warning($"Unpack delivery waiting: {reason}");
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

            var mode = StationModeRegistry.ResolveForWork(station);
            var packageState = station.GetState(PackagingStation.EMode.Package);
            var unpackageState = station.GetState(PackagingStation.EMode.Unpackage);
            var selectedState = mode == PackagingStation.EMode.Unpackage ? unpackageState : packageState;

            if (selectedState != PackagingStation.EState.CanBegin)
            {
                RuntimeTrace.StationReady(__instance, station, mode, packageState, unpackageState, selectedState, "state-not-ready");
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
                RuntimeTrace.StationReady(__instance, station, mode, packageState, unpackageState, selectedState, "owned-by-other-user");
                __result = false;
                return false;
            }

            Vector3 target  = station.StandPoint.position;
            Vector3 backoff = -station.StandPoint.forward * 0.25f;
            Vector3 desired = target + backoff;
            Vector3 driveTo = NavMesh.SamplePosition(desired, out var hit, 0.6f, NavMesh.AllAreas) ? hit.position : desired;

            __result = __instance.Npc.Movement.CanGetTo(station.StandPoint.position);
            RuntimeTrace.StationReady(
                __instance,
                station,
                mode,
                packageState,
                unpackageState,
                selectedState,
                __result ? "ready" : "navigation-unreachable");
            return false;
        }
    }

    [HarmonyPatch(typeof(PackagingStationBehaviour), nameof(PackagingStationBehaviour.OnActiveTick))]
    static class PSBehaviourOnActiveTickTracePatch
    {
        static void Prefix(PackagingStationBehaviour __instance) => RuntimeTrace.BehaviourTick(__instance);
    }

    [HarmonyPatch(typeof(PackagingStationBehaviour), nameof(PackagingStationBehaviour.StartPackaging))]
    static class PSBehaviourStartPackagingTracePatch
    {
        static void Prefix(PackagingStationBehaviour __instance) =>
            RuntimeTrace.Hook("PackagingBehaviour.StartPackaging", __instance?.Station,
                $"packagingInProgress={__instance?.PackagingInProgress}");
    }

    [HarmonyPatch(typeof(PackagingStationBehaviour), nameof(PackagingStationBehaviour.BeginPackaging))]
    static class PSBehaviourBeginPackagingTracePatch
    {
        static void Prefix(PackagingStationBehaviour __instance) =>
            RuntimeTrace.Hook("PackagingBehaviour.BeginPackaging", __instance?.Station,
                $"packagingInProgress={__instance?.PackagingInProgress}");
    }

    /*
     * PackagingStation Patches
     * */
    [HarmonyPatch(typeof(PackagingStation), nameof(PackagingStation.Awake))]
    static class PackaginStationAwakePatch
    {
        static void Postfix(PackagingStation __instance) => StationModeRegistry.SetExplicitIfExists(__instance);
    }

    [HarmonyPatch(typeof(PackagingStation), nameof(PackagingStation.IsAcceptingItems), MethodType.Getter)]
    static class PackagingStationIsAcceptingItemsPatch
    {
        static void Postfix(PackagingStation __instance, ref bool __result)
        {
            if (__instance is null ||
                !StationModeRegistry.TryGetMode(__instance, out var mode) ||
                mode != PackagingStation.EMode.Unpackage)
                return;

            // Item-specific capacity and compatibility are enforced through the
            // unpack-mode InputSlots route. This coarse property must stay true
            // for partially filled PackagingSlots as well as empty ones.
            if (!(__instance.PackagingSlot is null))
                __result = true;
        }
    }

    [HarmonyPatch(typeof(PackagingStation), nameof(PackagingStation.InputSlots), MethodType.Getter)]
    static class PackagingStationInputSlotsPatch
    {
        static void Postfix(
            PackagingStation __instance,
#if Il2Cpp
            ref Il2CppSystem.Collections.Generic.List<ItemSlot> __result)
#elif Mono
            ref List<ItemSlot> __result)
#endif
        {
            if (__instance is null || __result is null ||
                !StationModeRegistry.TryGetMode(__instance, out var mode) ||
                mode != PackagingStation.EMode.Unpackage)
                return;

            var packagingSlot = __instance.PackagingSlot;
            if (packagingSlot is null) return;
            foreach (var slot in __result)
                if (slot == packagingSlot) return;
            __result.Add(packagingSlot);
        }
    }

    [HarmonyPatch(typeof(PackagingStation), nameof(PackagingStation.PackSingleInstance))]
    static class PackagingStationPackSingleInstancePatch
    {
        static bool Prefix(PackagingStation __instance)
        {
            if (__instance is null || __instance.NPCUserObject is null) return true;

            var mode = StationModeRegistry.ResolveForWork(__instance);
            RuntimeTrace.Hook("PackagingStation.PackSingleInstance", __instance, $"mode={mode}");

            if (InstanceFinder.IsServer && mode == PackagingStation.EMode.Unpackage)
            {
                __instance.Unpack();
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(PackagingStation), nameof(PackagingStation.Unpack))]
    static class PackagingStationUnpackTracePatch
    {
        static void Prefix(PackagingStation __instance) => RuntimeTrace.Hook("PackagingStation.Unpack", __instance);
    }

    [HarmonyPatch(typeof(Packager), "GetStationToAttend", new System.Type[] { })]
    static class PackagerGetStationToAttendTracePatch
    {
        static void Postfix(Packager __instance, PackagingStation __result) =>
            RuntimeTrace.PackagerSelection("Packager.GetStationToAttend", __instance, __result);
    }

    [HarmonyPatch(typeof(Packager), "StartPackaging", new[] { typeof(PackagingStation) })]
    static class PackagerStartPackagingTracePatch
    {
        static void Prefix(Packager __instance, PackagingStation station)
        {
            RuntimeTrace.PackagerSelection("Packager.StartPackaging", __instance, station);
            RuntimeTrace.Hook("Packager.StartPackaging", station);
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

                    StationModeRegistry.ApplyTransitOutput(__result, selectedMode);
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

                    StationModeRegistry.ApplyTransitOutput(station, mode);
                    if (UnpackTransitRouting.HasValidProductRoute(__instance, station, out var reason))
                    {
                        __result = station;
                        return;
                    }

                    if (station.ProductSlot?.ItemInstance != null && station.ProductSlot.Quantity > 0)
                        UnpackTransitRouting.LogRouteFailureOnce(station, reason);
                }
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"Failed to select unpacked product for delivery: {ex}");
                __result = null;
            }
        }
    }

    [HarmonyPatch(typeof(Packager), "GetStationMoveItems", new System.Type[] { })]
    static class PackagerGetStationMoveItemsTracePatch
    {
        [HarmonyPostfix]
        [HarmonyPriority(Priority.Last)]
        static void Postfix(Packager __instance, PackagingStation __result) =>
            RuntimeTrace.PackagerSelection("Packager.GetStationMoveItems", __instance, __result);
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
                StationModeRegistry.ApplyTransitOutput(station, mode);
                if (!UnpackTransitRouting.HasValidProductRoute(__instance, station, out var invalidReason))
                {
                    RuntimeTrace.Hook("Packager.StartMoveItem rejected", station, $"reason={invalidReason}");
                    return false;
                }

                var route = UnpackTransitRouting.GetConfiguration(station).DestinationRoute;
                var product = station.ProductSlot.ItemInstance;

#if Il2Cpp
                __instance.MoveItemBehaviour.Initialize(route, product, -1, false);
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
            RuntimeTrace.StationUser(__instance, npcObject);
            if (__instance is null || npcObject is null) return;
            StationModeRegistry.ResolveForWork(__instance);
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
