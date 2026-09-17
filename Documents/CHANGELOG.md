# Changelog
Format based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/). Adheres to [Semantic Versioning](https://semver.org/).
## [2.0.1] - 2026-09-17
### Fixed
- Updated IL2CPP Harmony targets and station UI access for Schedule I 0.4.6f13.
- First launch no longer reports an error when `UserData/ImprovedPackagers.json` does not exist yet.
- Restored saved station modes in the UI and routed unpacked loose product from `ProductSlot` through the station's existing transit route. Routing insight adapted from PR #2 by `ecrgr` without its obsolete pre-f13 APIs.
- Updated f13 worker selection and move initialization so Packagers deliver loose product from unpack-mode stations to their selected storage destination.
- Validated unpack transit routes and pickup amounts against `ProductSlot` instead of the vanilla packaged-item output slot.
- Kept an unpack delivery route valid after pickup so a Packager can fill multiple inventory stacks and then walk them to the destination.
- Completed the unpack pickup state by batching available stacks and explicitly sending the Packager to the selected destination.
- Prevented a Packager carrying unpacked product from reselecting its source station and resetting the delivery route.
## [2.0.0] - 2025-08-11
### Added
`Unpackage` - Packagers follow the pack/unpack controller of their assigned Packaging Stations
## [1.0.0] - 2025-08-08
Released
