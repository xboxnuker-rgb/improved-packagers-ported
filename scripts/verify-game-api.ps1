[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string] $MelonLoaderRoot,

    [string] $ExpectedAssemblySha256 = "0D2EB364F3E84120AF7CCC9FA6BAFD597D42D495EBACC3A260CB4CA0CF0513DA"
)

$ErrorActionPreference = "Stop"

$assemblyPath = Join-Path $MelonLoaderRoot "Il2CppAssemblies\Assembly-CSharp.dll"
$cecilPath = Join-Path $MelonLoaderRoot "net6\Mono.Cecil.dll"

if (-not (Test-Path -LiteralPath $assemblyPath -PathType Leaf)) {
    throw "Missing game assembly: $assemblyPath"
}

if (-not (Test-Path -LiteralPath $cecilPath -PathType Leaf)) {
    throw "Missing Mono.Cecil reference: $cecilPath"
}

$actualHash = (Get-FileHash -LiteralPath $assemblyPath -Algorithm SHA256).Hash
if ($ExpectedAssemblySha256 -and $actualHash -ne $ExpectedAssemblySha256) {
    throw "Assembly-CSharp.dll hash mismatch. Expected $ExpectedAssemblySha256, found $actualHash."
}

[void] [System.Reflection.Assembly]::LoadFrom($cecilPath)
$assembly = [Mono.Cecil.AssemblyDefinition]::ReadAssembly($assemblyPath)

try {
    function Get-RequiredType {
        param([string] $FullName)

        $type = $assembly.MainModule.Types | Where-Object FullName -eq $FullName | Select-Object -First 1
        if (-not $type) {
            throw "Missing type: $FullName"
        }

        return $type
    }

    function Assert-Method {
        param(
            [string] $TypeName,
            [string] $MethodName,
            [string[]] $ParameterTypes = @()
        )

        $type = Get-RequiredType $TypeName
        $matches = @($type.Methods | Where-Object {
            if ($_.Name -ne $MethodName -or $_.Parameters.Count -ne $ParameterTypes.Count) {
                return $false
            }

            for ($index = 0; $index -lt $ParameterTypes.Count; $index++) {
                if ($_.Parameters[$index].ParameterType.FullName -ne $ParameterTypes[$index]) {
                    return $false
                }
            }

            return $true
        })

        if ($matches.Count -ne 1) {
            $signature = "$TypeName::$MethodName($($ParameterTypes -join ', '))"
            throw "Expected exactly one method for $signature; found $($matches.Count)."
        }

        Write-Output "PASS method $TypeName::$MethodName($($ParameterTypes -join ', '))"
    }

    function Assert-Property {
        param(
            [string] $TypeName,
            [string] $PropertyName,
            [string] $PropertyType
        )

        $type = Get-RequiredType $TypeName
        $matches = @($type.Properties | Where-Object {
            $_.Name -eq $PropertyName -and $_.PropertyType.FullName -eq $PropertyType
        })

        if ($matches.Count -ne 1) {
            throw "Expected property $TypeName::$PropertyName of type $PropertyType; found $($matches.Count)."
        }

        Write-Output "PASS property $TypeName::$PropertyName"
    }

    Assert-Method "Il2CppScheduleOne.UI.Stations.PackagingStationCanvas" "Open" @(
        "Il2CppScheduleOne.ObjectScripts.PackagingStation"
    )
    Assert-Method "Il2CppScheduleOne.UI.Stations.PackagingStationCanvas" "ToggleMode"
    Assert-Method "Il2CppScheduleOne.UI.Stations.PackagingStationCanvas" "SetMode" @(
        "Il2CppScheduleOne.ObjectScripts.PackagingStation/EMode"
    )
    Assert-Property "Il2CppScheduleOne.UI.Stations.PackagingStationCanvas" "Station" "Il2CppScheduleOne.ObjectScripts.PackagingStation"
    Assert-Property "Il2CppScheduleOne.UI.Stations.PackagingStationCanvas" "CurrentMode" "Il2CppScheduleOne.ObjectScripts.PackagingStation/EMode"

    Assert-Method "Il2CppScheduleOne.NPCs.Behaviour.PackagingStationBehaviour" "IsStationReady" @(
        "Il2CppScheduleOne.ObjectScripts.PackagingStation"
    )
    Assert-Method "Il2CppScheduleOne.Employees.Packager" "GetStationMoveItems"
    Assert-Method "Il2CppScheduleOne.Employees.Packager" "StartMoveItem" @(
        "Il2CppScheduleOne.ObjectScripts.PackagingStation"
    )
    Assert-Property "Il2CppScheduleOne.Employees.Packager" "Configuration" "Il2CppScheduleOne.Management.EntityConfiguration"
    Assert-Property "Il2CppScheduleOne.Employees.Employee" "MoveItemBehaviour" "Il2CppScheduleOne.NPCs.Behaviour.MoveItemBehaviour"
    Assert-Property "Il2CppScheduleOne.Management.PackagerConfiguration" "AssignedStations" 'Il2CppSystem.Collections.Generic.List`1<Il2CppScheduleOne.ObjectScripts.PackagingStation>'
    Assert-Method "Il2CppScheduleOne.ObjectScripts.PackagingStation" "Awake"
    Assert-Method "Il2CppScheduleOne.ObjectScripts.PackagingStation" "PackSingleInstance"
    Assert-Method "Il2CppScheduleOne.ObjectScripts.PackagingStation" "Unpack"
    Assert-Method "Il2CppScheduleOne.ObjectScripts.PackagingStation" "GetState" @(
        "Il2CppScheduleOne.ObjectScripts.PackagingStation/EMode"
    )
    Assert-Property "Il2CppScheduleOne.ObjectScripts.PackagingStation" "ProductSlot" "Il2CppScheduleOne.ItemFramework.ItemSlot"
    Assert-Property "Il2CppScheduleOne.ObjectScripts.PackagingStation" "OutputSlot" "Il2CppScheduleOne.ItemFramework.ItemSlot"
    Assert-Property "Il2CppScheduleOne.ObjectScripts.PackagingStation" "OutputSlots" 'Il2CppSystem.Collections.Generic.List`1<Il2CppScheduleOne.ItemFramework.ItemSlot>'
    Assert-Property "Il2CppScheduleOne.ObjectScripts.PackagingStation" "Configuration" "Il2CppScheduleOne.Management.EntityConfiguration"
    Assert-Property "Il2CppScheduleOne.Management.PackagingStationConfiguration" "DestinationRoute" "Il2CppScheduleOne.Management.TransitRoute"
    Assert-Method "Il2CppScheduleOne.Management.TransitRoute" "AreEntitiesNonNull"
    Assert-Method "Il2CppScheduleOne.NPCs.Behaviour.MoveItemBehaviour" "IsTransitRouteValid" @(
        "Il2CppScheduleOne.Management.TransitRoute",
        "Il2CppScheduleOne.ItemFramework.ItemInstance",
        "System.String&"
    )
    Assert-Method "Il2CppScheduleOne.NPCs.Behaviour.MoveItemBehaviour" "IsTransitRouteValid" @(
        "Il2CppScheduleOne.Management.TransitRoute",
        "System.String",
        "System.String&"
    )
    Assert-Method "Il2CppScheduleOne.NPCs.Behaviour.MoveItemBehaviour" "IsTransitRouteValid" @(
        "Il2CppScheduleOne.Management.TransitRoute",
        "System.String"
    )
    Assert-Method "Il2CppScheduleOne.NPCs.Behaviour.MoveItemBehaviour" "TakeItem"
    Assert-Method "Il2CppScheduleOne.NPCs.Behaviour.MoveItemBehaviour" "OnActiveTick"
    Assert-Method "Il2CppScheduleOne.NPCs.Behaviour.MoveItemBehaviour" "PlaceItem"
    Assert-Method "Il2CppScheduleOne.NPCs.Behaviour.MoveItemBehaviour" "WalkToDestination"
    Assert-Method "Il2CppScheduleOne.NPCs.Behaviour.MoveItemBehaviour" "Initialize" @(
        "Il2CppScheduleOne.Management.TransitRoute",
        "Il2CppScheduleOne.ItemFramework.ItemInstance",
        "System.Int32",
        "System.Boolean"
    )
    Assert-Method "Il2CppScheduleOne.NPCs.Behaviour.Behaviour" "Enable_Networked"
    Assert-Method "Il2CppScheduleOne.Management.ITransitEntity" "GetInputCapacityForItem" @(
        "Il2CppScheduleOne.ItemFramework.ItemInstance",
        "Il2CppScheduleOne.NPCs.NPC",
        "System.Boolean"
    )
    Assert-Method "Il2CppScheduleOne.Management.ITransitEntity" "ReserveInputSlotsForItem" @(
        "Il2CppScheduleOne.ItemFramework.ItemInstance",
        "Il2CppFishNet.Object.NetworkObject"
    )
    Assert-Method "Il2CppScheduleOne.Management.ITransitEntity" "InsertItemIntoInput" @(
        "Il2CppScheduleOne.ItemFramework.ItemInstance",
        "Il2CppScheduleOne.NPCs.NPC"
    )
    Assert-Method "Il2CppScheduleOne.Management.ITransitEntity" "RemoveSlotLocks" @(
        "Il2CppFishNet.Object.NetworkObject"
    )
    Assert-Method "Il2CppScheduleOne.NPCs.NPCInventory" "GetCapacityForItem" @(
        "Il2CppScheduleOne.ItemFramework.ItemInstance"
    )
    Assert-Method "Il2CppScheduleOne.NPCs.NPCInventory" "GetIdenticalItemAmount" @(
        "Il2CppScheduleOne.ItemFramework.ItemInstance"
    )
    Assert-Method "Il2CppScheduleOne.NPCs.NPCInventory" "InsertItem" @(
        "Il2CppScheduleOne.ItemFramework.ItemInstance",
        "System.Boolean"
    )
    Assert-Method "Il2CppScheduleOne.ItemFramework.ItemInstance" "GetCopy" @("System.Int32")
    Assert-Method "Il2CppScheduleOne.ItemFramework.ItemSlot" "ChangeQuantity" @(
        "System.Int32",
        "System.Boolean"
    )
    Assert-Method "Il2CppScheduleOne.ObjectScripts.PackagingStation" "SetNPCUser" @(
        "Il2CppFishNet.Object.NetworkObject"
    )
    Assert-Method "Il2CppScheduleOne.ObjectScripts.PackagingStation" "Destroy"
    Assert-Method "Il2CppScheduleOne.Persistence.SaveManager" "Save" @("System.String")
    Assert-Method "Il2CppScheduleOne.Delivery.LoadingDock" "SetOccupant" @(
        "Il2CppScheduleOne.Vehicles.LandVehicle"
    )
    Assert-Method "Il2CppScheduleOne.Property.Property" "Start"

    Assert-Method "Il2CppScheduleOne.Management.PackagerConfiguration" ".ctor" @(
        "Il2CppScheduleOne.Management.ConfigurationReplicator",
        "Il2CppScheduleOne.Management.IConfigurable",
        "Il2CppScheduleOne.Employees.Packager"
    )

    Write-Output "PASS Assembly-CSharp.dll SHA256 $actualHash"
    Write-Output "Schedule I 0.4.6f13 IL2CPP API verification passed."
}
finally {
    $assembly.Dispose()
}
