# Changelog
Format based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/). Adheres to [Semantic Versioning](https://semver.org/).
## [2.0.1] - 2026-09-17
### Fixed
- Updated IL2CPP Harmony targets and station UI access for Schedule I 0.4.6f13.
- First launch no longer reports an error when `UserData/ImprovedPackagers.json` does not exist yet.
- Restored saved station modes in the UI and routed unpacked loose product from `ProductSlot` through the station's existing transit route. Routing insight adapted from PR #2 by `ecrgr` without its obsolete pre-f13 APIs.
- Updated f13 worker selection and move initialization so Packagers deliver loose product from unpack-mode stations to their selected storage destination.
- Validated unpack transit routes and pickup amounts against `ProductSlot` instead of the vanilla packaged-item output slot.
- Preserved f13's native grab, walk, place, slot-lock, and behavior-completion state machine after substituting `ProductSlot` as the unpack source.
- Returned unpack route validation, pickup, reservation, walking, placement, and cleanup to f13's native move-item implementation.
- Reasserted `ProductSlot` as the unpack station's transit output when workers select and start a delivery.
- Inferred a missing first-run station mode from f13's uniquely ready work mode, allowing an assigned Packager to claim an unpack job before `SetNPCUser` can run.
- Covered f13 transit and destination validation for both loose-product output and packaged-item input routes.
- Exposed empty and partially filled unpack-mode `PackagingSlot` capacity through `InputSlots` so configured workers can load bricks, jars, or baggies for breakdown.
## [2.0.0] - 2025-08-11
### Added
`Unpackage` - Packagers follow the pack/unpack controller of their assigned Packaging Stations
## [1.0.0] - 2025-08-08
Released
