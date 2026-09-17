# Current Handover

## Schedule I 0.4.6f13 IL2CPP compatibility port

- Status: `AWAITING_RUNTIME_VERIFICATION`
- Branch: `fix/schedule-i-0.4.6f13-compat`
- Base: upstream `master` at `4519a1f48e3f461aa78681bb5a48bd683e2b6962`
- Compatibility implementation: `4e9cb2f9decedc9a3299cec96f23974f2ff6f9ec`
- Requested pull request account: `xboxnuker-rgb`

### Scope

Port Improved Packagers 2.0.0 to Schedule I 0.4.6f13 IL2CPP, produce a locally installable 2.0.1 DLL package, and submit a draft upstream pull request. Mono source remains present, but this work makes no new Mono compatibility claim.

### Reference evidence

- Supplied archive: out-of-tree `MelonLoader.zip`
- Archive SHA-256: `A2B9B715ECD56747E59C9EF8FF949A4898BD10ED0D3A62321D7ECE701B7EBB67`
- `Assembly-CSharp.dll` SHA-256: `0D2EB364F3E84120AF7CCC9FA6BAFD597D42D495EBACC3A260CB4CA0CF0513DA`
- Schedule I: `0.4.6f13`
- Unity: `2022.3.62f2`
- MelonLoader: `0.7.0 Open-Beta`, net6
- .NET SDK used for the build: `8.0.425`
- No reference binaries or logs belong in Git.

### Confirmed API drift and fixes

- Replaced the removed `PackagingStationCanvas.SetIsOpen(PackagingStation, bool)` patch with `Open(PackagingStation)`.
- Replaced `PackagingStationCanvas.PackagingStation` with the renamed `Station` property.
- Replaced the removed `PackagingStation.DestroyItem()` patch target with `Destroy()`.
- Fixed `StationModeRegistry.Load()` so a missing first-run file returns without attempting to read it.
- `PackagerConfiguration(ConfigurationReplicator, IConfigurable, Packager)` still exists. Improved Packagers does not patch it, so the logged backend fallback must be attributed through an isolated runtime test rather than a speculative change.

### Static and build verification

The following commands completed successfully against the supplied 0.4.6f13 references:

```powershell
$melonLoaderRoot = "<out-of-tree-path-to-MelonLoader>"
$dotnet = "<path-to-dotnet-8.0.425>"
.\scripts\verify-game-api.ps1 -MelonLoaderRoot $melonLoaderRoot
.\scripts\build-il2cpp.ps1 -MelonLoaderRoot $melonLoaderRoot -DotNet $dotnet
```

Evidence:

- Every Harmony target and required property was found with exactly one expected signature.
- The `PackagerConfiguration` constructor was also observed with its logged signature.
- Release build completed with zero warnings and zero errors.
- `ImprovedPackagers.dll` targets `.NETCoreApp,Version=v6.0`.
- Assembly version and `MelonInfo` version are both `2.0.1`.
- DLL SHA-256: `40234D992623FDBFF0A48568DCD0E16AA8B7CA7A34C17764120052EAB3062C2D`
- Alex package: `artifacts/ImprovedPackagers-2.0.1-il2cpp-schedule-i-0.4.6f13.zip`
- Package SHA-256: `35047B9DC9F894E5E340370F5C327AB70E7D8503448AB7F2DFEAB731ED8E47CB`
- The distributable contains no copied game, Unity, MelonLoader, Harmony, or interop dependencies.

### Runtime verification still required

The build host does not have a runnable Schedule I installation. Alex should test the packaged DLL first with only Improved Packagers enabled, then with the normal mod set. Record both logs and verify:

- No `Undefined target method`, `HarmonyException`, or missing `ImprovedPackagers.json` error.
- Packaging mode still packages.
- Unpack mode unpacks and remains selected after save/reload.
- Loading docks retain Load Only, Unload Only, and Dual behavior.
- Destroying a station removes its persisted mode.

Compare isolated and full-set logs. Treat the `PackagerConfiguration::.ctor` backend fallback as a separate mod/loader interaction unless it reproduces with only Improved Packagers installed.

### Alex full-mod-set evidence

Received `Latest.log` from the full mod set on 2026-09-17.

- Log SHA-256: `77D9F6F1B8CC3AF312F0621A64B60B721048A17D180DFF395B00FFA1AB16B1C3`
- Schedule I `0.4.6f13`, Unity `2022.3.62f2`, and MelonLoader `0.7.0 Open-Beta` are confirmed.
- MelonLoader loaded Improved Packagers `2.0.1` with DLL SHA-256 `40234D992623FDBFF0A48568DCD0E16AA8B7CA7A34C17764120052EAB3062C2D`.
- No `Undefined target method`, `HarmonyException`, or missing `ImprovedPackagers.json` error appears.
- The `PackagerConfiguration::.ctor` backend fallback remains present in this full mod set. Improved Packagers does not target this constructor; isolated comparison is still needed before attribution.
- The log contains an unrelated `DeliverySpotsPlus` coroutine exception and repeated `Worker Collision Reborn` missing-capsule messages.
- The log alone cannot prove pack/unpack behavior, save/reload persistence, dock direction behavior, or registry cleanup when a station is destroyed.

### Unpack workflow test candidate

Alex subsequently observed that a worker removed packaged bricks from the station's vanilla `OutputSlot` instead of running unpack mode and transporting the resulting loose product. This reproduces the core routing defect described in upstream PR #2 by `ecrgr`.

The f13 candidate selectively adapts that insight without taking PR #2's removed `Enable_Networked` overload or obsolete `MoveItemBehaviour.Initialize` calls:

- Restore a saved mode through `PackagingStationCanvas.SetMode(EMode)` when the UI opens.
- In unpack mode, expose `ProductSlot` through the station's existing transit `OutputSlots` list. In package mode, restore the vanilla `OutputSlot`.
- Continue using the game's normal worker transit behavior, which also gives Harder Working Employees a common slot-level contract rather than competing worker-behavior patches.
- Persist mode changes and station removal immediately.

Static API verification and the Release build pass for this candidate. Runtime testing must compare Improved Packagers alone, then the same scenario with Harder Working Employees enabled. The received full-set log identifies Harder Working Employees `2.2.3` as tested against game `0.3.4f4`; do not change that mod unless the comparison proves it still bypasses the corrected station slots on f13.

- Candidate source: `14b6e5b4aff869b51939f8fd6336cf0ae4653505`
- Candidate DLL SHA-256: `6B842125A714CC4A0C397ED8043F2011E9736EF7E6FC1B95FBA8427AB0E5B8BC`
- Candidate package: `artifacts/ImprovedPackagers-2.0.1-il2cpp-schedule-i-0.4.6f13-test2.zip`
- Candidate package SHA-256: `B13F852C4570914317E77DE08780D674761C8F8DF1648C58B03C810A88D3F62F`

Alex's isolated test of candidate 2 passed the unpack operation with Harder Working Employees disabled, but the worker did not collect or deliver the loose product despite a selected storage destination. This proves the remaining delivery defect is in Improved Packagers/f13 itself and is not attributable to HWE.

Candidate 3 adds the narrow f13 worker handoff:

- Extend `Packager.GetStationMoveItems()` only when vanilla has no valid result, selecting an unpack-mode station with loose `ProductSlot` inventory and a valid destination route.
- Suppress vanilla selection of an unpack-mode station when it would carry the still-packaged physical `OutputSlot` item away.
- Intercept `Packager.StartMoveItem(PackagingStation)` only for unpack mode and call f13's `MoveItemBehaviour.Initialize(TransitRoute, ItemInstance, int, bool)` followed by parameterless `Enable_Networked()`.
- Keep package mode and every non-packaging transit route on vanilla behavior.

- Candidate 3 source: `93aba607881bba9ac1f19fe6dc782b63f2244023`
- Candidate 3 DLL SHA-256: `B03407062D3FC9B04FDB93A77F08579E46F77C77838D23534410F002881E895F`
- Candidate 3 package: `artifacts/ImprovedPackagers-2.0.1-il2cpp-schedule-i-0.4.6f13-test3.zip`
- Candidate 3 package SHA-256: `7B6B552355BC85ECD1039876FCAE4463A84F6697F643B46A9B1CA4E4E06E8622`

Candidate 3 must pass the same isolated scenario before HWE is re-enabled. Only port HWE if isolated delivery passes and the identical HWE-enabled scenario then fails.

Candidate 3 log received on 2026-09-17:

- Log SHA-256: `6817568679C101BCFB17F01F463441D50C69D84C30C5EEC7013E0F98931924C2`
- The exact candidate DLL hash loaded on Schedule I `0.4.6f13` with HWE absent.
- The worker unpacked the packaged item but again did not collect or deliver the loose product.
- Neither the candidate's start-delivery message nor an Improved Packagers exception appeared. This shows f13 rejected the route before `StartMoveItem`, because vanilla `IsTransitRouteValid` still evaluates the physical packaged-item output slot.
- The run was HWE-free but not otherwise isolated; unrelated DeliverySpotsPlus and delivery-vehicle exceptions remain present.

Candidate 4 overrides only unpack-mode transit validation and pickup to use `ProductSlot`, while keeping vanilla behavior for every other route. It uses f13's exact destination-capacity, NPC-inventory, item-copy, quantity-change, reservation, and move APIs and emits a one-time reason if delivery remains blocked.

- Candidate 4 source: `9fa2c5ebed855f4df09ecaeef35a044db66e451e`
- Candidate 4 DLL SHA-256: `6486BC9241F989B692CE5AE1F0DB5C57D4E9B302149A6CD8676AECD1E3C1BE1A`
- Candidate 4 package: `artifacts/ImprovedPackagers-2.0.1-il2cpp-schedule-i-0.4.6f13-test4.zip`
- Candidate 4 package SHA-256: `CE0180B6B5199F765B3655CA8E548B061D669AF556C4CE84B6CC75D738D96672`

Candidate 4 log received on 2026-09-17:

- Log SHA-256: `91F57B6379A547519D93C176DC099BFEE86AD1B66C21E1BDCEADCF017079CC2F`
- Schedule I `0.4.6f13` and MelonLoader `0.7.0 Open-Beta` are confirmed.
- MelonLoader loaded the exact candidate 4 DLL SHA-256 `6486BC9241F989B692CE5AE1F0DB5C57D4E9B302149A6CD8676AECD1E3C1BE1A`.
- Harder Working Employees was absent, although other unrelated mods remained enabled.
- The worker successfully collected five 20-item stacks from `ProductSlot`.
- Immediately afterward, Improved Packagers reported `Packager inventory has no capacity for the unpacked product.` The route validator therefore cancelled the job at the source-to-destination transition instead of allowing a full worker to deliver the product already carried.

Candidate 5 preserves the in-progress route when either the station still contains the expected product or the worker already carries matching product. Free worker inventory capacity is required only before the first pickup, so multi-stack collection remains supported while a full worker can proceed to delivery. The exact `NPCInventory.GetIdenticalItemAmount(ItemInstance)` signature is included in static API verification.

- Candidate 5 source: `cfa6d8b979cc04324814c31f5103e2a5cfd883ff`
- Candidate 5 DLL SHA-256: `0566F56A67EF02A1F0928272A30D9E6DA964CAF9B82ABCBED2FE6A84D05FA208`
- Candidate 5 package: `artifacts/ImprovedPackagers-2.0.1-il2cpp-schedule-i-0.4.6f13-test5.zip`
- Candidate 5 package SHA-256: `ED9BB327265527DFD89CA6161B0AF7765743B8305C26A4D715A24BD37AEC30DD`
- Candidate 5 Release build: zero warnings and zero errors; all required f13 signatures passed.

### Pull request handoff

Keep the pull request in draft while runtime results are pending. Add the isolated and full-set log conclusions here and to the PR, then mark it ready for upstream review. Do not merge upstream or publish a fork release without fresh approval.

The requested PR account is `xboxnuker-rgb`; the current machine credential resolves to `GSVS-Dev01`, so pushing the branch requires the requested account to be authenticated or an explicit decision to use the currently authenticated fork.

### Overlapping work

Upstream PR #2 addresses an older unpacking defect but is not directly compatible with 0.4.6f13: it references removed networking helpers and obsolete method overloads. Do not cherry-pick it wholesale. Port only a specific part later if an f13 gameplay test reproduces that older defect, preserving attribution.
