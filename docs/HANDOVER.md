# Current Handover

## Schedule I 0.4.6f13 IL2CPP compatibility port

- Status: `PUBLISHED_V2.0.1_F13_PORTED_NEXUS_PACKAGE_PREP`
- Maintained repository: `https://github.com/xboxnuker-rgb/improved-packagers-ported`
- Merge target: `main`
- Branch: `main`
- Base: upstream `master` at `4519a1f48e3f461aa78681bb5a48bd683e2b6962`
- Current compatibility implementation: `4312df1f4cecc0564ead11b4290ae887cc589637`

### Release handoff

1. Continue from `main`; candidate 20 passed both isolated and HWE-enabled runtime tests.
2. RC3 standardizes the visible mod name, log tag, assembly, DLL, ZIP, manifest, README, and attribution as Improved Packagers PORTED.
3. The renamed RC3 install check passed: MelonLoader displayed `Improved Packagers PORTED`, `GuysWeForgotDre | Ported by GSVS UK ACM`, the shaded GSVS banner, and `f13-nexus-rc3`.
4. The owner explicitly approved pushing the polished documentation and publishing the final GitHub release on 2026-09-19.
5. Do not update or merge the upstream pull request without separate approval.
6. A separate Nexus-ready package is complete with the verified release DLL, original MIT licence, port notice and plain-text installation guide. Do not replace the live GitHub release asset when publishing it.

Never commit Schedule I, MelonLoader, generated interop, save, log, or packaged artifact files. Local build commands and reference layout are in `README.md`.

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

Candidate 5 log received on 2026-09-17:

- Log SHA-256: `340B540EDEE13A2F44FB5187261C1A9909BBD4CD4207F843417BD96555E67648`
- MelonLoader loaded the exact candidate 5 DLL SHA-256 `0566F56A67EF02A1F0928272A30D9E6DA964CAF9B82ABCBED2FE6A84D05FA208` on Schedule I `0.4.6f13`.
- The worker again collected five 20-item stacks, with no route-invalid or capacity message afterward.
- The worker remained at the station and repeatedly performed the source action instead of leaving for the destination. The custom `TakeItem()` prefix had correctly replaced the item source but also bypassed vanilla's transition to `WalkToDestination()`.

Candidate 6 batches every currently acceptable stack during one pickup, accumulates the complete `grabbedAmount`, and explicitly calls f13's public `MoveItemBehaviour.WalkToDestination()` after collection. It also resumes the destination leg if the source empties while matching product is already carried.

- Candidate 6 source: `7d3740b19aad3525e7fdd4d805035a956379d500`
- Candidate 6 DLL SHA-256: `B075A4D7D060336EB77F7C05ACF9695DB8616BB19F5F7AE5A3AD3D7A22F2B9F9`
- Candidate 6 package: `artifacts/ImprovedPackagers-2.0.1-il2cpp-schedule-i-0.4.6f13-test6.zip`
- Candidate 6 package SHA-256: `688607B492D27028E8CB55FCBC5B358F19E1BD67DD96BB0BC7470E30E7B1FD0E`
- Candidate 6 Release build: zero warnings and zero errors; exact `WalkToDestination()` API verification passed.

Candidate 6 log received on 2026-09-17:

- Log SHA-256: `97F92DD3854B0D14551A3CD5C60A79A0C0E8557AEB5806A8A2BA83927FC8405F`
- The exact candidate 6 DLL SHA-256 `B075A4D7D060336EB77F7C05ACF9695DB8616BB19F5F7AE5A3AD3D7A22F2B9F9` loaded on f13.
- The worker collected five 20-item stacks, then walked partway toward storage before returning to the same station and repeating the pickup. This indicates the source-selection layer reinitialized the route while a carried load was still present.
- No Improved Packagers Harmony or undefined-target error appeared. Existing DeliveryVehicle and DeliverySpotsPlus exceptions are unrelated.

Candidate 7 guards both `GetStationMoveItems()` and `StartMoveItem()` against that carried-load reinitialization. When the active unpack route has matching product in the worker inventory, it suppresses a new station assignment and calls `WalkToDestination()` to continue the existing delivery leg.

- Candidate 7 source: `9785c6712dbcc6d4bd27ca5379722eb4e04c6391`
- Candidate 7 DLL SHA-256: `21AB28B6DF29380E055293BFFB79F3A8F4B699A34F92DBBF32D011865B2939BC`
- Candidate 7 package: `artifacts/ImprovedPackagers-2.0.1-il2cpp-schedule-i-0.4.6f13-test7.zip`
- Candidate 7 package SHA-256: `30ABA82F85A22E7E932F53F25F1E6D9A6B1985A4803ED13502ED41C63ACA0C34`
- Candidate 7 Release build: zero warnings and zero errors; all required f13 signatures passed.

Candidate 7 runtime result (reported after test): behavior remained effectively unchanged. The worker still collected five stacks, walked partway toward storage, then returned to the source and looped. This means f13 clears `MoveItemBehaviour.assignedRoute` before the next station-selection callback, so the route-based carried-load guard cannot fire.

Candidate 8 removes that dependency. It matches the carried template to an unpack-mode assigned station, rebuilds that station's configured destination route when the active route is gone, restores `grabbedAmount` from the worker inventory, and resumes `WalkToDestination()`.

- Candidate 8 source: `b939068302c0bf4bb07a8793e2e948bdae673fda`
- Candidate 8 DLL SHA-256: `434A5983E06F3BDC0708560173094129E337614E43B19FD54EA2067F758D1016`
- Candidate 8 package: `artifacts/ImprovedPackagers-2.0.1-il2cpp-schedule-i-0.4.6f13-test8.zip`
- Candidate 8 package SHA-256: `0BC669C8AC73C2059444FAA7A7DDFDE6F5831A1944BC35BD40BADBC6CE6606AE`
- Candidate 8 Release build: zero warnings and zero errors; all required f13 signatures passed.

Candidate 8 runtime result (reported after test): behavior remained effectively unchanged. No carried-route recovery message appeared, and the worker continued the source-action loop. This indicates the repeated pickup occurs inside the active `TakeItem()` coroutine before the station-selection recovery path is reached.

Candidate 9 defers the custom pickup's `WalkToDestination()` call by one frame with `MelonCoroutines.Start`, allowing the source coroutine to return before the state transition is applied.

- Candidate 9 source: `67a27520ef8567d78c5db79d192d8a3dd05e2ecd`
- Candidate 9 DLL SHA-256: `717E1CF5EB270B1B4ABA2C3D9A757654353B8E2F5143678216288E19F8C522A1`
- Candidate 9 package: `artifacts/ImprovedPackagers-2.0.1-il2cpp-schedule-i-0.4.6f13-test9.zip`
- Candidate 9 package SHA-256: `103BD4EC65FB49D95A461EB0F324489E70496A1215D2141E540B6D8FBC5EBAA2`
- Candidate 9 Release build: zero warnings and zero errors; all required f13 signatures passed.

Candidate 9 log received as pasted text on 2026-09-17:

- Pasted log SHA-256: `5F860C75D20472D0AC1848A7F877463426021AF9F5266D10742259DDEAC0016F`
- The exact candidate 9 DLL SHA-256 `717E1CF5EB270B1B4ABA2C3D9A757654353B8E2F5143678216288E19F8C522A1` loaded on f13.
- The worker still produced five 20-item pickup messages and never produced a delivery/recovery message.
- Alex requested abandoning multi-stack carrying and matching the base-game one-stack trip behavior.

Candidate 11 removes the batch loop. Each custom ProductSlot pickup takes only one inventory-sized stack, sets `grabbedAmount` to that stack, queues the destination transition, and leaves the worker to return for the next stack after placement.

- Candidate 11 source: `dc6fa7da4c80742541d7b0418be8d4f37adf60eb`
- Candidate 11 DLL SHA-256: `C9ED3D1CBD3EC42A0F007C1DB79873A4F1512FA6D496C09C925E76653061EBB0`
- Candidate 11 package: `artifacts/ImprovedPackagers-2.0.1-il2cpp-schedule-i-0.4.6f13-test11.zip`
- Candidate 11 package SHA-256: `8E2019AE9DEE764834B46ABD3D9F99781EF0FE09441DBD7BA1571CCDB450DDD1`
- Candidate 11 Release build: zero warnings and zero errors; all required f13 signatures passed.

Candidate 11 runtime result (reported after test): the worker still walked up to the shelf, locked the slot, turned around without depositing, and continued filling/looping. This shows the remaining defect is in the vanilla `PlaceItem()` path for the loose `ProductSlot` item, not in the source quantity (including five-item jars).

Candidate 12 adds an unpack-only placement implementation using f13's `ITransitEntity.InsertItemIntoInput(ItemInstance, NPC)` and `RemoveSlotLocks(NetworkObject)` APIs. It moves one actual carried stack into the selected destination, clears that quantity from the worker inventory, and leaves package-mode placement untouched.

- Candidate 12 source: `7aad20fcb6a40e58af6fcfc90a05ca05a8497194`
- Candidate 12 DLL SHA-256: `5903176C8189A343D1C774379768A524B03D5F4CE584DE4BB7FB5B1A270C0F76`
- Candidate 12 package: `artifacts/ImprovedPackagers-2.0.1-il2cpp-schedule-i-0.4.6f13-test12.zip`
- Candidate 12 package SHA-256: `E9D7C778936C67F44065E4D743649B0B8C634805311A207D7284D7212D5071CE`
- Candidate 12 Release build: zero warnings and zero errors; all required f13 signatures passed.

Candidate 12 runtime result (reported after test): behavior remained identical. The worker reached the shelf and locked its slot but did not emit the custom deposit message, indicating the game rejected the destination before `PlaceItem()`.

Candidate 13 overrides both f13 `MoveItemBehaviour.IsDestinationValid(TransitRoute, ItemInstance, ref string)` and `IsDestinationValid(TransitRoute, ItemInstance)` overloads for unpack routes, using the loose ProductSlot/carried-product validation before the explicit placement hook.

- Candidate 13 source: `dbd11a3272e3d34d218955236d33aca48d0080ef`
- Candidate 13 DLL SHA-256: `F9CA13FD01B7E87F8BB5CE84E5E46FB048DDE93449E4627459F4122FA09D24DE`
- Candidate 13 package: `artifacts/ImprovedPackagers-2.0.1-il2cpp-schedule-i-0.4.6f13-test13.zip`
- Candidate 13 package SHA-256: `23CD9DD742475D15AC221DF1393C31612B66CF22BBEE2385E8BF610282D74117`
- Candidate 13 Release build: zero warnings and zero errors; all required f13 signatures passed.

Alex additionally reported that the worker does not recognize an unpack-mode Packaging Station as a fillable destination for loading bricks/jars/baggies before breakdown. Candidate 14 addresses that separate input-side gap by overriding `PackagingStation.IsAcceptingItems` for an empty unpack-mode `PackagingSlot`, while retaining package-mode behavior and the one-stack ProductSlot delivery path.

- Candidate 14 source: `89f3c6240f0204e26759f8b98ac011bdfa885af2`
- Candidate 14 DLL SHA-256: `F9F022B03F6A9A6D3287705F05DC9E70BB10D95BE55003E315B0AFA2B1863E16`
- Candidate 14 package: `artifacts/ImprovedPackagers-2.0.1-il2cpp-schedule-i-0.4.6f13-test14.zip`
- Candidate 14 package SHA-256: `A9539AF7A16A5807E70EDFCB0BFF30FFAF5FE646A9470DF290BE21DB84964FDF`
- Candidate 14 Release build: zero warnings and zero errors; all required f13 signatures passed.

Candidate 14 runtime result (reported after test): behavior remained identical on both input and output sides. The `IsAcceptingItems` override alone did not expose the station through the actual transit input route.

Candidate 15 adds the station's `PackagingSlot` to unpack-mode `InputSlots` and handles f13 transit/destination validation when the route destination is an unpack-mode station. This is the input-side equivalent of the ProductSlot output bridge.

- Candidate 15 source: `58b5cd26c274d951ea7106616eb6d53e1092b466`
- Candidate 15 DLL SHA-256: `3104D1AFAC40350B2A11740B2F33D47DA9D5285524AC6C18B3D6C4327B666571`
- Candidate 15 package: `artifacts/ImprovedPackagers-2.0.1-il2cpp-schedule-i-0.4.6f13-test15.zip`
- Candidate 15 package SHA-256: `F243D8DE4EFBA06A4F751D13188690323BC810F257162DDF4EDD174792FD3B5D`
- Candidate 15 Release build: zero warnings and zero errors; all required f13 signatures passed.

### Native f13 control-flow inspection and candidate 16

The exact f13 IL2CPP binary and metadata were supplied and staged outside the repository:

- `GameAssembly.dll` SHA-256: `9531A85606AF7A5C545EF44895C19F3F08F77CC221479B686539FA5B72141626`
- `global-metadata.dat` SHA-256: `3A5A6E46BD8E6687F63228211978FA94E0885DCB6AE3950AA8D01F047355DE5F`
- Cpp2IL detected metadata version 31 and produced a native ISIL/diffable-C# inspection dump successfully.

The native `MoveItemBehaviour` control flow explains the repeated source/destination loop:

1. `GrabItem` sets state `Grabbing`, calls `TakeItem()`, waits, clears its coroutine, and returns the state to `Idle`.
2. On the following server tick, vanilla `OnActiveTick()` detects both a matching carried item and `grabbedAmount > 0`, then calls `WalkToDestination()`.
3. At the destination, vanilla starts `PlaceItem()`; its coroutine inserts the carried copy through `ITransitEntity.InsertItemIntoInput`, removes the carried quantity, clears its state, and disables the behavior.

Candidates 6-15 called `WalkToDestination()` before the native grab coroutine completed. The still-running coroutine then reset the state to `Idle`, while the added `PlaceItem()` prefix bypassed the native completion coroutine entirely. That combination caused the visible turn-around/reselection loop.

Candidate 16 removes the manual destination transition, carried-route recovery, `OnActiveTick()` override, and `PlaceItem()` override. The only pickup substitution retained is the required one: `TakeItem()` reads loose product from the unpack station's `ProductSlot`, sets `grabbedAmount`, inserts it into the NPC inventory, and reserves destination input slots. f13 then owns the normal wait, walk, deposit, lock cleanup, and behavior shutdown sequence.

- Candidate 16 uses vanilla's `-1` unlimited/default `maxMoveAmount` value when initializing the route.
- Candidate 16 source: `89dcf527ffbd813accb3dc4a16e151ea99deafe0`
- Candidate 16 DLL SHA-256: `178DAFD3A056DFBA3C633360E72F8EF263BD0BBD052B94125834B2B297F556AA`
- Candidate 16 package: `artifacts/ImprovedPackagers-2.0.1-il2cpp-schedule-i-0.4.6f13-test16.zip`
- Candidate 16 package SHA-256: `3C60255E8E4998D0513C58CE8B9EE1E6C0D0E3A567339A369FC8BBD475537DAD`
- Release build: zero warnings and zero errors.
- Static API verification: passed against the supplied f13 interop assembly.
- Live-game verification: pending the same isolated output-delivery and packaged-input tests.

Candidate 16 runtime result (reported after test): behavior remained similar, the worker held no product, and the expected custom pickup trace was not observed. That run stopped during station/route selection, before `TakeItem()`. Inspection also found that the remaining custom transfer hooks could disagree with f13's reservation-aware validation after a pickup, so candidate 17 removes those hooks instead of layering another state workaround.

### Native transit candidate 17

Candidate 17 removes the final custom `TakeItem()` replacement and the custom move-item route/destination validators. `StationModeRegistry` exposes `ProductSlot` as the station's transit output, and the worker-selection and start hooks now reassert that bridge immediately before calling f13's own validation. The game can therefore perform its normal source lookup, pickup, destination reservation, walk, insertion, lock cleanup, and shutdown without duplicated transfer bookkeeping.

Unpack-mode `IsAcceptingItems` remains available for empty and partially filled `PackagingSlot` inputs; item-specific compatibility and capacity are enforced by f13 against the added input slot.

The assembly logs `Compatibility build f13-native-transit-r1 loaded.` at startup so a test log can prove which binary ran.

- Candidate 17 source: `b80ecbb9e017015b627912de07034e843a848b28`
- Candidate 17 DLL SHA-256: `3A990A42E19C10B8037C85052E87BD56B5849CF3E926B8A5E8F7E42D00565731`
- Candidate 17 package: `artifacts/ImprovedPackagers-2.0.1-il2cpp-schedule-i-0.4.6f13-test17.zip`
- Candidate 17 package SHA-256: `512F42624F0A057C66ADC05180D1D2D16BDA66F5D47E6F40533FAD3E5BF35DC9`
- Release build: zero warnings and zero errors.
- Static API verification: passed against the supplied f13 interop assembly.
- Live-game verification: pending isolated output and input route tests.

### Supplied log audit and candidate 18

The nine supplied runtime logs separated two distinct failures:

- The 06:09 through 07:20 sessions used earlier candidates that reached the custom pickup and repeatedly logged 20-item collections.
- The 07:28, 08:33, and 08:46 sessions failed the mod's entire Harmony initialization. Harmony reported that prefix parameter `templateItem` did not match f13's `IsDestinationValid(..., ItemInstance item, ...)` parameter name. Those builds could not record UI mode changes or run any other patch.
- The 08:48 session positively loaded candidate 17 (`f13-native-transit-r1`) without a Harmony exception, but contained no station-mode, route-waiting, or delivery-start message. The assigned worker therefore remained on the Package default before `SetNPCUser` could run.

Candidate 18 resolves that first-run deadlock. When no explicit or sticky mode exists, `PackagingStationBehaviour.IsStationReady` now asks f13 which mode is uniquely ready. It infers and retains Unpackage only when Unpackage is ready and Package is not; it does not persist a guess for an idle or ambiguous station. The same resolver is used when station work executes and when an NPC user is assigned. Runtime logs identify both the inferred mode and the build.

The API verifier now optionally checks parameter names as well as parameter types for Harmony methods that inject original arguments, preventing the earlier `templateItem`/`item` failure from passing static verification again.

- Candidate 18 source: `f36e4f81876c0b53787d35383f206e448f2809f8`
- Candidate 18 DLL SHA-256: `0A6FD0C2DC70460C7226ED0FE462CCEDD80485EEED6602512DA4C51CA658F611`
- Candidate 18 package: `artifacts/ImprovedPackagers-2.0.1-il2cpp-schedule-i-0.4.6f13-test18.zip`
- Candidate 18 package SHA-256: `FC971C9FC89A8144234448F1984C9F4BBA2317EBBDF177F8A5B1FE1623668B6D`
- Published prerelease: `v2.0.1-test18-alpha`
- Release URL: `https://github.com/xboxnuker-rgb/improved-packagers-ported/releases/tag/v2.0.1-test18-alpha`
- Release build: zero warnings and zero errors.
- Static API and Harmony parameter-name verification: passed against the supplied f13 interop assembly.
- Live-game result: failed before the station operation. With Harder Working Employees disabled, Alex observed the assigned worker standing at the Packaging Station without operating it. This is earlier than the older candidates that unpacked, filled the worker inventory, and then failed to deposit.
- The two HWE-free candidate-18 startup logs (`26-9-18_13-17-36.log`, SHA-256 `598D3257CD3F6BA95A2A9813E01B079E2DF26EDE0B89E3994624C68AF7EB5A2C`; and `26-9-18_13-24-58.log`, SHA-256 `46350019E7826522E2051EEF1C642A879A2F7CA1EAF7D9DEACA0C678C8EC13D7`) confirm `f13-native-transit-r2` loaded without an Improved Packagers Harmony exception. Neither records a station-mode line or the game's `Starting packaging`/`Packaging done!` messages. Later sessions contain Harder Working Employees and must not be treated as isolated evidence.

### Native station-operation trace and candidate 19

Fresh Cpp2IL inspection of the exact f13 native binary established the station-operation chain: `Packager.GetStationToAttend()` selects a station, `Packager.StartPackaging()` enables `PackagingStationBehaviour`, readiness and arrival gates lead to `BeginPackaging()`, and only the completed work calls `PackagingStation.PackSingleInstance()`. Improved Packagers replaces that final call with `Unpack()` in Unpackage mode. Because candidate 18 stopped with the worker at the bench, the remaining failure is before `PackSingleInstance()`, not in the transfer/deposit code exercised by candidates 4-15.

Candidate 19 retains candidate 18 behavior and adds state-change-only diagnostics around that native chain. The trace reports station selection, both Package and Unpackage readiness states, ownership/in-use rejection, navigation reachability, active behavior movement/arrival state, `StartPackaging`, `BeginPackaging`, `PackSingleInstance`, `Unpack`, move-item selection/rejection, and the three native station slots. Repeated identical ticks are deduplicated to avoid another multi-megabyte blind log.

- Candidate 19 source: `0a0767d2a60bd9ad3e44d6e0efd378ff87cd53df`
- Build identity: `f13-diagnostic-r3`
- Candidate DLL SHA-256: `66F8498E54D3646A755791742FD35C0297E50E27DD71CD1786502E11E7B0C9C7`
- Candidate package: `artifacts/ImprovedPackagers-2.0.1-il2cpp-schedule-i-0.4.6f13-test19-diagnostic.zip`
- Candidate package SHA-256: `688B28B859C313B50C2546CA34E9F31F37EEB5C031B1B8926931D39FD11A2618`
- Package contents: one root-level `ImprovedPackagers.dll`; no game, loader, Harmony, Unity, or interop dependency.
- Release build: zero warnings and zero errors with .NET SDK `8.0.425`.
- Static API and Harmony parameter-name verification: all required signatures passed against `Assembly-CSharp.dll` SHA-256 `0D2EB364F3E84120AF7CCC9FA6BAFD597D42D495EBACC3A260CB4CA0CF0513DA`.
- Compiled assembly inspection confirms version `2.0.1.0`, build identity `f13-diagnostic-r3`, and the diagnostic hook strings.
- Live-game result: with HWE disabled, the worker completed one unpack operation and then stopped without delivery. The trace proves `PackSingleInstance()` redirected to `Unpack()`, `ProductSlot` increased from 0 to 20, and the packaged `OutputSlot` decreased from 20 to 19. `GetStationMoveItems()` then selected that exact station, but the `StartMoveItem` patch never ran.
- Test 19 log: `26-9-19_0-5-33.log`, SHA-256 `7F4A1761DD7CA716CF07CD09EE0F4E97ACEE84D7E513C4E4BE0554B5A900CD43`.

### Inlined move initialization and candidate 20

The Test 19 trace isolated a native compiler detail that earlier source-level reasoning missed. In f13, `Packager.UpdateBehaviour()` calls `GetStationMoveItems()` but then contains the body of `StartMoveItem(PackagingStation)` inline. It directly reads native station offset `+808` (`OutputSlot`) and calls `MoveItemBehaviour.Initialize(...)`; it does not invoke the patchable `StartMoveItem` method. Therefore the selection postfix correctly returned the unpack station, but the inlined code still initialized movement with the packaged item instead of the loose `ProductSlot` item.

Candidate 20 patches the exact non-inlined boundary that f13 still calls: `MoveItemBehaviour.Initialize(TransitRoute route, ItemInstance _itemToRetrieveTemplate, int _maxMoveAmount, bool _skipPickup)`. When and only when `route.Source` is an unpack-mode Packaging Station containing loose product, the prefix replaces `_itemToRetrieveTemplate` with `ProductSlot.ItemInstance`. The existing `OutputSlots` bridge already exposes `ProductSlot`, so f13 continues through its own pickup, inventory, reservation, walk, placement, lock cleanup, and behavior shutdown code.

- Candidate 20 source: `d8835d23e2a292590a08c393181aa65463375b40`
- Build identity: `f13-inline-init-r4`
- Candidate DLL SHA-256: `52782B7C97B81D95D05F7C3FBF8D01C7DD568E27B5655E27B3D95678522F2EF7`
- Candidate package: `artifacts/ImprovedPackagers-2.0.1-il2cpp-schedule-i-0.4.6f13-test20-inline-init.zip`
- Candidate package SHA-256: `38791D47D46D543AA4401F24A4C6C0BCC92AB5C8625FBE810BEF9D025209AF9D`
- Release build: zero warnings and zero errors with .NET SDK `8.0.425`.
- Static API verification now checks the exact four `Initialize` parameter names and the `TransitRoute.Source` property; all required f13 signatures passed.
- Compiled assembly inspection confirms version `2.0.1.0`, build identity `f13-inline-init-r4`, and the product-bridge trace string.
- Isolated live-game result: **PASS** with Harder Working Employees disabled. The worker delivered the 20 loose product left by Test 19, returned, completed further unpack operations, and initialized later deliveries through the product bridge. The packaged output count fell from 19 through 14 during the observed run, with repeated `PackSingleInstance`, `Unpack`, station-selection, and product-bridge traces.
- Passing isolated log: `26-9-19_0-29-15.log`/`Latest.log`, SHA-256 `FA2EE6A16CE3A81C2D7A1FF2FA04AF34EFD577522855E5F23ED5C5B07137DC62`.
- HWE compatibility result: **PASS** with Harder Working Employees `2.2.3` active. The trace records repeated unpack operations, `ProductSlot` station selections, and product-bridge initialization while HWE's behavior monitor and move-item hooks were active. No Improved Packagers Harmony, undefined-target, or product-bridge error appeared.
- Passing HWE-enabled log: `26-9-19_0-35-54.log`/`Latest.log`, SHA-256 `C0C5F511C30FD143433AB969EE4FA45D15D695522408D123EF447213A44BE1E6`.
- Tested HWE DLL SHA-256: `937051445C63675EA4D13619911F388AA841D657D403446FA3E7E75FD75CBD87`.

### Nexus RC1 presentation build

The first Nexus release candidate keeps candidate 20's tested runtime logic unchanged. It adds a one-time green startup banner with `STAYING PORTED BY` above a block-letter `GSVS`, plus the mod version, f13/IL2CPP target, build identity, original-author credit, and port-maintainer credit. The normal compatibility identity line remains after the banner for machine-readable log checks.

- RC source: `3673233bc7255b40d301ff03e248c76cd5b9614c`
- Build identity: `f13-nexus-rc1`
- RC DLL SHA-256: `490BC63B9053397770E7D96410513062798BB23F89E36F9223DBE7733105FB05`
- RC package: `artifacts/ImprovedPackagers-2.0.1-il2cpp-schedule-i-0.4.6f13-nexus-rc1.zip`
- RC package SHA-256: `5960A593159D02E36BFBE4AC5D55A4D3CBDF4CBBB691A6C94C731CB8A2A7CE45`
- Package contents: one root-level `ImprovedPackagers.dll`; no copied game, loader, Harmony, Unity, interop, log, or save files.
- Release build: zero warnings and zero errors; all exact f13 API and Harmony parameter-name checks passed.
- Compiled assembly inspection confirms version `2.0.1.0`, `f13-nexus-rc1`, and all requested banner/attribution text.
- Live-game verification: pending the normal-mod-set banner and extended-playtest check.

### Nexus RC2 shaded banner

RC2 replaces the flat green RC1 artwork with an original six-row beveled GSVS design. It uses the same ANSI 256-colour technique observed in the installed BFG banner: progressively darker purple rows create depth, acid green highlights the heading and values, white carries the metadata, and the terminal supplies the black background. No worker or routing logic changed.

- RC2 source: `763e43864f04deca578ddd633a5aec6a4829392c`
- Build identity: `f13-nexus-rc2`
- RC2 DLL SHA-256: `4578D4C3508CF23716B3F8A708237F3E5F83F7E59BAAE242D78AF62C11C3CA41`
- RC2 package: `artifacts/ImprovedPackagers-2.0.1-il2cpp-schedule-i-0.4.6f13-nexus-rc2.zip`
- RC2 package SHA-256: `40C71AC78C756733675EFE98BE6D6F0A6D9A4494F74D6373DE5E1C0420534FC8`
- Release build: zero warnings and zero errors; all exact f13 API checks passed.
- Compiled assembly inspection confirms the RC2 identity, purple and green ANSI palette codes, and requested heading.
- Live-game verification: pending visual inspection and a final functionality check.

### GitHub download clarity requirement

The final GitHub release must not require non-technical users to understand the Assets disclosure or distinguish the installable mod from GitHub's autogenerated source archives.

- Put a prominent `⬇️ DOWNLOAD IMPROVED PACKAGERS` link at the top of both `README.md` and the release description.
- Link directly to the named installable ZIP asset, not the release page or either `Source code` archive.
- Immediately state: extract/copy `ImprovedPackagersPORTED.dll` into the game's `Mods` folder.
- Use a short, human-readable asset filename and repeat it beside the link.
- If the rendered release page is still ambiguous, add an annotated screenshot with an arrow pointing to the correct ZIP.
- Add the direct link only after the final release asset exists so it cannot silently point at RC/test content.

### Nexus RC3 standardized PORTED identity

RC3 changes release identity and packaging only; candidate 20's passing worker/routing implementation and RC2's approved shaded banner remain unchanged.

- RC3 source: `4312df1f4cecc0564ead11b4290ae887cc589637`
- Melon/display/log name: `Improved Packagers PORTED`
- Assembly and install filename: `ImprovedPackagersPORTED.dll`
- Melon author: `GuysWeForgotDre | Ported by GSVS UK ACM`
- Build identity: `f13-nexus-rc3`
- Final asset name: `Improved-Packagers-PORTED-v2.0.1-Schedule-I-f13.zip`
- RC3 DLL SHA-256: `D3068A5C50D24568D5101C0B514C35D8BE8D2E8295B4829EF01F7A4A3366F24B`
- RC3 package SHA-256: `69B32F9FE577B8EABDC3CB36ED0489183F9477345ED842B142D6CFE2F987B0EC`
- Package contents: one root-level `ImprovedPackagersPORTED.dll`.
- README download counter: live Shields.io badge for tag `v2.0.1-f13-ported`, labelled `downloaded` and linked directly to the installable ZIP.
- Release build: zero warnings and zero errors; all exact f13 API checks passed.
- Compiled metadata inspection confirms the assembly name, display name, version, author attribution, build identity, and banner credit.
- Live-game verification: **passed** after the filename/display-name transition. The installed file and loaded assembly were both `ImprovedPackagersPORTED.dll`; MelonLoader showed the standardized display name and full author/porter attribution, and the startup log used `[Improved Packagers PORTED]` with build identity `f13-nexus-rc3`.
- Public documentation includes the supplied installation, dependency, enabled-state, assignment, unpackage-mode, and active-worker screenshots under `docs/images/`.
- Mod Manager & Phone App is documented as required. The Patreon badge/link is optional maintainer support and does not alter the original-author attribution.
- Published stable GitHub release: `v2.0.1-f13-ported` at `https://github.com/xboxnuker-rgb/improved-packagers-ported/releases/tag/v2.0.1-f13-ported`.
- Release tag commit: `066cebeff8965283f36f7af917f1f7288122004d`.
- The direct ZIP URL and Shields.io `downloaded` badge both returned HTTP 200 after publication; the ZIP response type was `application/octet-stream`.

### Nexus upload package

- Package: `artifacts/Improved-Packagers-PORTED-v2.0.1-NEXUS-Schedule-I-f13.zip`
- Package SHA-256: `00840EF35DE17E63AC640BEF8FA46AD2816DDC4945946A0B3B0CD520E9614BC8`
- Package size: 15,079 bytes.
- DLL SHA-256: `D3068A5C50D24568D5101C0B514C35D8BE8D2E8295B4829EF01F7A4A3366F24B`, exactly matching the tested and published RC3 DLL.
- Root contents: `ImprovedPackagersPORTED.dll`, `LICENSE.txt`, `NOTICE.txt`, and `README.txt`; there is no nested archive or password.
- Copy-ready listing fields, permissions guidance, image order, captions and checklist: `docs/NEXUS_UPLOAD.md`.
- Polished long-form rich-text master (about 1,450 words): `docs/NEXUS_DESCRIPTION.md`.
- Current Nexus rules require disclosure of generative-AI-assisted code through the available `AI-Generated Content` tag. Do not select the `Nexus Mods Turns 25` event tag because its 2026 rules prohibit generative AI in code, assets or dialogue.
- The upload is prepared locally but has not been submitted or published on Nexus Mods.

### Upstream maintenance model

- `origin` is the maintained port at `xboxnuker-rgb/improved-packagers-ported`.
- `upstream` is GuysWeForgotDre's original `Improved-Packagers` repository.
- Both histories share upstream commit `4519a1f48e3f461aa78681bb5a48bd683e2b6962` as their merge base; this is a true descendant/fork history rather than a detached code copy.
- Keep compatibility and presentation changes as reviewable commits on the maintained branch. Fetch and merge future upstream work normally, resolving only genuine overlapping changes.
- Preserve the original MIT licence and GuysWeForgotDre attribution in source and binary distributions even if GSVS UK ACM becomes the active compatibility maintainer.

### Pull request handoff

Keep the pull request in draft while runtime results are pending. Add the isolated and full-set log conclusions here and to the PR, then mark it ready for upstream review. Do not merge upstream or publish a fork release without fresh approval.

The requested PR account is `xboxnuker-rgb`; the current machine credential resolves to `GSVS-Dev01`, so pushing the branch requires the requested account to be authenticated or an explicit decision to use the currently authenticated fork.

### Overlapping work

Upstream PR #2 addresses an older unpacking defect but is not directly compatible with 0.4.6f13: it references removed networking helpers and obsolete method overloads. Do not cherry-pick it wholesale. Port only a specific part later if an f13 gameplay test reproduces that older defect, preserving attribution.
