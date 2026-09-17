# Improved Packagers

Improved Packagers lets Packagers follow each Packaging Station's pack or unpack mode and use routes to load vehicles parked in Loading Bays.

## Compatibility

| Schedule I branch | Backend | Status |
| --- | --- | --- |
| Main/default 0.4.6f13 | IL2CPP | Version 2.0.1 builds against the matching generated assemblies; live-game verification remains pending. |
| Alternate | Mono | Legacy source retained; no new compatibility claim is made by version 2.0.1. |

IL2CPP and Mono builds are not interchangeable.

## Installation

1. Close Schedule I.
2. Remove every older Improved Packagers DLL from `%ScheduleOne Install%/Mods/`, including `Main-ImprovedPackagers.dll`.
3. Copy `ImprovedPackagers.dll` into the `Mods` directory.
4. Start the game.

Only one Improved Packagers DLL should be installed at a time.

Configuration is stored in:

- Loading Bay preferences: `UserData/MelonPreferences.cfg`
- Packaging Station modes: `UserData/ImprovedPackagers.json`

Opening or toggling an existing Packaging Station records an explicit mode. If no saved mode exists, the worker can infer it when f13 reports exactly one work mode as ready.

## Features

### Unpackage

- Packagers obey the pack/unpack setting of assigned Packaging Stations.
- They can unpack baggies, jars, or bricks.
- An empty or matching product must be available in the input slot, as with manual unpacking.

### Load vehicles

- Packagers can use routes from storage and equipment to vehicles in Loading Bays.
- The IL2CPP build supports Load Only, Unload Only, and Dual direction per dock.
- Item filters, stack sizes, and normal route mechanics still apply.
- Two Packagers using Dual mode can move items in a loop; configure routes deliberately.

## Troubleshooting

- `Undefined target method` means the DLL was built for a different game version.
- A first-run missing-file error for `ImprovedPackagers.json` indicates an older build is still installed.
- The `PackagerConfiguration::.ctor` IL2CPP backend fallback is not patched by this mod. Reproduce it with an isolated mod set before attributing it to Improved Packagers.

Full build and contribution documentation is available in the maintained source repository: [xboxnuker-rgb/improved-packagers-ported](https://github.com/xboxnuker-rgb/improved-packagers-ported). The original project remains credited at [GuysWeForgotDre/Improved-Packagers](https://github.com/GuysWeForgotDre/Improved-Packagers).
