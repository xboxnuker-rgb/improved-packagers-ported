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
using System.Collections;
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

        public static bool HasCarriedUnpackProduct(Packager packager)
        {
            var behaviour = packager?.MoveItemBehaviour;
            var template = behaviour?.itemToRetrieveTemplate;
            return !(behaviour?.assignedRoute is null) &&
                TryGetUnpackSource(behaviour.assignedRoute, out _) &&
                !(template is null) &&
                behaviour.Npc?.Inventory?.GetIdenticalItemAmount(template) > 0;
        }

        public static bool HasCarriedProductForStation(Packager packager, PackagingStation station)
        {
            var behaviour = packager?.MoveItemBehaviour;
            var template = behaviour?.itemToRetrieveTemplate;
            if (station is null || behaviour is null || template is null ||
                !StationModeRegistry.TryGetMode(station, out var mode) ||
                mode != PackagingStation.EMode.Unpackage)
                return false;

            var sourceProduct = station.ProductSlot?.ItemInstance;
            return (sourceProduct is null || sourceProduct.ID == template.ID) &&
                behaviour.Npc?.Inventory?.GetIdenticalItemAmount(template) > 0 &&
                !(GetConfiguration(station)?.DestinationRoute is null);
        }

        public static bool ResumeCarriedDelivery(Packager packager, PackagingStation station)
        {
            if (!HasCarriedProductForStation(packager, station)) return false;

            var behaviour = packager.MoveItemBehaviour;
            var route = GetConfiguration(station).DestinationRoute;
            int carriedAmount = behaviour.Npc.Inventory.GetIdenticalItemAmount(behaviour.itemToRetrieveTemplate);

            bool sameRoute = TryGetUnpackSource(behaviour.assignedRoute, out var assignedStation) &&
                assignedStation.GUID == station.GUID;
            if (!sameRoute)
            {
#if Il2Cpp
                behaviour.Initialize(route, behaviour.itemToRetrieveTemplate, 0, false);
                behaviour.Enable_Networked();
#elif Mono
                behaviour.Initialize(route, behaviour.itemToRetrieveTemplate);
                behaviour.Enable_Networked(null);
#endif
            }

            behaviour.grabbedAmount = carriedAmount;
            behaviour.WalkToDestination();
            return true;
        }

        public static void LogRouteFailureOnce(PackagingStation station, string reason)
        {
            if (station is null || string.IsNullOrEmpty(reason)) return;
            string key = station.GUID + ":" + reason;
            if (LoggedRouteFailures.Add(key))
                MelonLogger.Warning($"Unpack delivery waiting: {reason}");
        }

        public static bool TryGetUnpackSource(TransitRoute route, out PackagingStation station)
        {
#if Il2Cpp
            station = (route?.Source as Il2CppObjectBase)?.TryCast<PackagingStation>();
#elif Mono
            station = route?.Source as PackagingStation;
#endif
            return !(station is null) &&
                StationModeRegistry.TryGetMode(station, out var mode) &&
                mode == PackagingStation.EMode.Unpackage;
        }

        public static bool ValidateProductRoute(
            MoveItemBehaviour behaviour,
            TransitRoute route,
            string expectedItemId,
            out string invalidReason)
        {
            invalidReason = string.Empty;

            if (!TryGetUnpackSource(route, out var station))
            {
                invalidReason = "Route source is not an unpack-mode Packaging Station.";
                return false;
            }

            var sourceProduct = station.ProductSlot?.ItemInstance;
            var product = sourceProduct ?? behaviour.itemToRetrieveTemplate;
            if (product is null)
            {
                invalidReason = "No unpacked or carried product is available.";
                return false;
            }

            int sourceQuantity = sourceProduct is null ? 0 : station.ProductSlot.Quantity;
            int carriedQuantity = behaviour.Npc?.Inventory?.GetIdenticalItemAmount(product) ?? 0;
            if (sourceQuantity <= 0 && carriedQuantity <= 0)
            {
                invalidReason = "Unpacked ProductSlot is empty and the Packager carries no matching product.";
                return false;
            }

            if (!string.IsNullOrEmpty(expectedItemId) && product.ID != expectedItemId)
            {
                invalidReason = "Unpacked ProductSlot contains a different item.";
                return false;
            }

            if (route.Destination is null ||
                route.Destination.GetInputCapacityForItem(product, behaviour.Npc, true) <= 0)
            {
                invalidReason = "Destination has no capacity for the unpacked product.";
                return false;
            }

            if (carriedQuantity <= 0 &&
                (behaviour.Npc?.Inventory is null ||
                 behaviour.Npc.Inventory.GetCapacityForItem(product) <= 0))
            {
                invalidReason = "Packager inventory has no capacity for the unpacked product.";
                return false;
            }

            return true;
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
                if (!(__result is null) &&
                    UnpackTransitRouting.ResumeCarriedDelivery(__instance, __result))
                {
                    __result = null;
                    return;
                }

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

                    if (UnpackTransitRouting.ResumeCarriedDelivery(__instance, station))
                    {
                        __result = null;
                        return;
                    }

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
                if (UnpackTransitRouting.ResumeCarriedDelivery(__instance, station))
                {
                    MelonLogger.Msg("Packager resumed delivery of carried unpacked product.");
                    return false;
                }

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

    [HarmonyPatch]
    static class MoveItemBehaviourIsTransitRouteValidStringReasonPatch
    {
        static System.Reflection.MethodBase TargetMethod() => AccessTools.Method(
            typeof(MoveItemBehaviour),
            nameof(MoveItemBehaviour.IsTransitRouteValid),
            new[] { typeof(TransitRoute), typeof(string), typeof(string).MakeByRefType() });

        static bool Prefix(
            MoveItemBehaviour __instance,
            TransitRoute route,
            string itemID,
            ref string invalidReason,
            ref bool __result)
        {
            if (!UnpackTransitRouting.TryGetUnpackSource(route, out _))
                return true;

            __result = UnpackTransitRouting.ValidateProductRoute(
                __instance,
                route,
                itemID,
                out invalidReason);
            return false;
        }
    }

    [HarmonyPatch]
    static class MoveItemBehaviourIsTransitRouteValidInstancePatch
    {
        static System.Reflection.MethodBase TargetMethod() => AccessTools.Method(
            typeof(MoveItemBehaviour),
            nameof(MoveItemBehaviour.IsTransitRouteValid),
            new[] { typeof(TransitRoute), typeof(ItemInstance), typeof(string).MakeByRefType() });

        static bool Prefix(
            MoveItemBehaviour __instance,
            TransitRoute route,
            ItemInstance templateItem,
            ref string invalidReason,
            ref bool __result)
        {
            if (!UnpackTransitRouting.TryGetUnpackSource(route, out _))
                return true;

            __result = UnpackTransitRouting.ValidateProductRoute(
                __instance,
                route,
                templateItem?.ID,
                out invalidReason);
            return false;
        }
    }

    [HarmonyPatch(typeof(MoveItemBehaviour), nameof(MoveItemBehaviour.IsTransitRouteValid), new[] {
        typeof(TransitRoute), typeof(string)
    })]
    static class MoveItemBehaviourIsTransitRouteValidStringPatch
    {
        static bool Prefix(
            MoveItemBehaviour __instance,
            TransitRoute route,
            string itemID,
            ref bool __result)
        {
            if (!UnpackTransitRouting.TryGetUnpackSource(route, out _))
                return true;

            __result = UnpackTransitRouting.ValidateProductRoute(
                __instance,
                route,
                itemID,
                out _);
            return false;
        }
    }

    [HarmonyPatch(typeof(MoveItemBehaviour), "TakeItem", new System.Type[] { })]
    static class MoveItemBehaviourTakeItemPatch
    {
        private static IEnumerator ResumeDestinationNextFrame(MoveItemBehaviour behaviour)
        {
            yield return null;
            if (!(behaviour is null))
                behaviour.WalkToDestination();
        }

        private static void QueueDestinationTransition(MoveItemBehaviour behaviour)
        {
            if (!(behaviour is null))
                MelonCoroutines.Start(ResumeDestinationNextFrame(behaviour));
        }

        static bool Prefix(MoveItemBehaviour __instance)
        {
            if (!UnpackTransitRouting.TryGetUnpackSource(__instance?.assignedRoute, out var station))
                return true;

            try
            {
                var product = station.ProductSlot?.ItemInstance;
                if (product is null || station.ProductSlot.Quantity <= 0)
                {
                    var carriedProduct = __instance.itemToRetrieveTemplate;
                    if (!(carriedProduct is null) &&
                        __instance.Npc?.Inventory?.GetIdenticalItemAmount(carriedProduct) > 0)
                        QueueDestinationTransition(__instance);

                    return false;
                }

                int amount = station.ProductSlot.Quantity;
                if (__instance.maxMoveAmount > 0)
                    amount = System.Math.Min(amount, __instance.maxMoveAmount);

                amount = System.Math.Min(
                    amount,
                    __instance.Npc.Inventory.GetCapacityForItem(product));
                amount = System.Math.Min(
                    amount,
                    __instance.assignedRoute.Destination.GetInputCapacityForItem(
                        product,
                        __instance.Npc,
                        true));

                if (amount <= 0) return false;

                var copy = product.GetCopy(amount);
                station.ProductSlot.ChangeQuantity(-amount, false);
                __instance.Npc.Inventory.InsertItem(copy, true);
                __instance.assignedRoute.Destination.ReserveInputSlotsForItem(
                    copy,
                    __instance.Npc.NetworkObject);
                __instance.grabbedAmount = amount;

                MelonLogger.Msg($"Packager collected {amount} unpacked item(s) for delivery.");
                if (__instance.grabbedAmount > 0)
                    QueueDestinationTransition(__instance);
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"Failed to collect unpacked product: {ex}");
            }

            return false;
        }
    }

    [HarmonyPatch(typeof(MoveItemBehaviour), nameof(MoveItemBehaviour.OnActiveTick))]
    static class MoveItemBehaviourOnActiveTickPatch
    {
        static bool Prefix(MoveItemBehaviour __instance)
        {
            var template = __instance?.itemToRetrieveTemplate;
            if (__instance is null ||
                !UnpackTransitRouting.TryGetUnpackSource(__instance.assignedRoute, out _) ||
                template is null ||
                __instance.Npc?.Inventory?.GetIdenticalItemAmount(template) <= 0 ||
                __instance.currentState != MoveItemBehaviour.EState.Grabbing)
                return true;

            __instance.WalkToDestination();
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
