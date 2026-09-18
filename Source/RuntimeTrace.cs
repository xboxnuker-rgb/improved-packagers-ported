#if Il2Cpp
using Il2CppFishNet.Object;
using Il2CppScheduleOne.Employees;
using Il2CppScheduleOne.ItemFramework;
using Il2CppScheduleOne.NPCs.Behaviour;
using Il2CppScheduleOne.ObjectScripts;
#elif Mono
using FishNet.Object;
using ScheduleOne.Employees;
using ScheduleOne.ItemFramework;
using ScheduleOne.NPCs.Behaviour;
using ScheduleOne.ObjectScripts;
#endif
using MelonLoader;
using System;
using System.Collections.Generic;

namespace ImprovedPackagers
{
    internal static class RuntimeTrace
    {
        private static readonly Dictionary<string, string> LastValues = new Dictionary<string, string>();

        public static void Changed(string key, string message)
        {
            if (LastValues.TryGetValue(key, out var previous) && previous == message)
                return;

            LastValues[key] = message;
            MelonLogger.Msg($"[Trace] {message}");
        }

        public static void Hook(string hook, PackagingStation station, string detail = null)
        {
            try
            {
                string key = StationKey(station);
                Changed(
                    $"hook:{hook}:{key}",
                    $"{hook}: station={key}{(string.IsNullOrEmpty(detail) ? string.Empty : ", " + detail)}, {Slots(station)}");
            }
            catch (Exception ex)
            {
                Changed($"trace-error:{hook}", $"trace error in {hook}: {ex.GetType().Name}: {ex.Message}");
            }
        }

        public static void StationReady(
            PackagingStationBehaviour behaviour,
            PackagingStation station,
            PackagingStation.EMode mode,
            PackagingStation.EState packageState,
            PackagingStation.EState unpackageState,
            PackagingStation.EState selectedState,
            string gate)
        {
            try
            {
                string key = StationKey(station);
                bool inUse = false;
                bool ownerIsThisNpc = false;
#if Il2Cpp
                var usable = (station as Il2CppInterop.Runtime.InteropTypes.Il2CppObjectBase)?.TryCast<Il2CppScheduleOne.Management.IUsable>();
#elif Mono
                var usable = station as ScheduleOne.Management.IUsable;
#endif
                if (!(usable is null))
                {
                    inUse = usable.IsInUse;
                    ownerIsThisNpc = !(behaviour?.Npc is null) && station.NPCUserObject == behaviour.Npc.NetworkObject;
                }

                Changed(
                    $"station-ready:{key}",
                    $"station-ready: station={key}, gate={gate}, mode={mode}, selected={selectedState}, package={packageState}, unpackage={unpackageState}, inUse={inUse}, ownerIsThisNpc={ownerIsThisNpc}, {Slots(station)}");
            }
            catch (Exception ex)
            {
                Changed("trace-error:station-ready", $"trace error in station-ready: {ex.GetType().Name}: {ex.Message}");
            }
        }

        public static void BehaviourTick(PackagingStationBehaviour behaviour)
        {
            try
            {
                var station = behaviour?.Station;
                string key = StationKey(station);
                bool moving = !(behaviour?.Npc?.Movement is null) && behaviour.Npc.Movement.IsMoving;
                bool atStation = !(behaviour is null) && behaviour.IsAtStation();
                bool ownsStation = !(behaviour?.Npc is null) && !(station is null) &&
                    station.NPCUserObject == behaviour.Npc.NetworkObject;

                Changed(
                    $"behaviour-tick:{key}",
                    $"station-tick: station={key}, packagingInProgress={behaviour?.PackagingInProgress}, moving={moving}, atStation={atStation}, ownsStation={ownsStation}, {Slots(station)}");
            }
            catch (Exception ex)
            {
                Changed("trace-error:behaviour-tick", $"trace error in station-tick: {ex.GetType().Name}: {ex.Message}");
            }
        }

        public static void PackagerSelection(string hook, Packager packager, PackagingStation station)
        {
            try
            {
                string stationKey = StationKey(station);
                Changed(
                    $"selection:{hook}:{PackagerKey(packager)}",
                    $"{hook}: selected={stationKey}{(station is null ? string.Empty : ", " + Slots(station))}");
            }
            catch (Exception ex)
            {
                Changed($"trace-error:{hook}", $"trace error in {hook}: {ex.GetType().Name}: {ex.Message}");
            }
        }

        public static void StationUser(PackagingStation station, NetworkObject npcObject)
        {
            try
            {
                string key = StationKey(station);
                Changed(
                    $"station-user:{key}",
                    $"station-user: station={key}, assigned={!(npcObject is null)}, {Slots(station)}");
            }
            catch (Exception ex)
            {
                Changed("trace-error:station-user", $"trace error in station-user: {ex.GetType().Name}: {ex.Message}");
            }
        }

        public static string Slots(PackagingStation station)
        {
            if (station is null)
                return "slots[station=null]";

            return $"slots[packaging={Slot(station.PackagingSlot)}, product={Slot(station.ProductSlot)}, output={Slot(station.OutputSlot)}, transitInputs={Count(station.InputSlots)}, transitOutputs={Count(station.OutputSlots)}]";
        }

        private static string Slot(ItemSlot slot)
        {
            if (slot is null)
                return "null";

            return $"qty:{slot.Quantity}/item:{(!(slot.ItemInstance is null) ? "yes" : "no")}/locked:{slot.IsLocked}";
        }

#if Il2Cpp
        private static int Count(Il2CppSystem.Collections.Generic.List<ItemSlot> slots) => slots is null ? -1 : slots.Count;
#elif Mono
        private static int Count(List<ItemSlot> slots) => slots is null ? -1 : slots.Count;
#endif

        private static string StationKey(PackagingStation station) => station is null ? "none" : station.GUID.ToString();
        private static string PackagerKey(Packager packager) => packager is null ? "none" : packager.GUID.ToString();
    }
}
