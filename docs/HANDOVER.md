# Current Handover

## Schedule I 0.4.6f13 IL2CPP compatibility port

- Status: `IN_PROGRESS`
- Branch: `fix/schedule-i-0.4.6f13-compat`
- Base: upstream `master` at `4519a1f48e3f461aa78681bb5a48bd683e2b6962`
- Planned pull request account: `xboxnuker-rgb`

### Scope

Port Improved Packagers 2.0.0 to Schedule I 0.4.6f13 IL2CPP, produce a locally installable 2.0.1 DLL package, and submit a draft upstream pull request. Mono source remains present, but this work makes no new Mono compatibility claim.

### Reference evidence

- Supplied archive: out-of-tree `MelonLoader.zip`
- Archive SHA-256: `A2B9B715ECD56747E59C9EF8FF949A4898BD10ED0D3A62321D7ECE701B7EBB67`
- `Assembly-CSharp.dll` SHA-256: `0D2EB364F3E84120AF7CCC9FA6BAFD597D42D495EBACC3A260CB4CA0CF0513DA`
- Schedule I: `0.4.6f13`
- Unity: `2022.3.62f2`
- MelonLoader: `0.7.0 Open-Beta`, net6
- No reference binaries or logs belong in Git.

### Confirmed API drift

- `PackagingStationCanvas.SetIsOpen(PackagingStation, bool)` was removed; `Open(PackagingStation)` is the replacement.
- `PackagingStationCanvas.PackagingStation` was removed; `Station` is the replacement property.
- `PackagingStation.DestroyItem()` was removed; `Destroy()` is the replacement lifecycle method.
- `PackagerConfiguration(ConfigurationReplicator, IConfigurable, Packager)` still exists. Improved Packagers does not patch it, so the logged backend fallback must be attributed through an isolated runtime test rather than a speculative change.

### Overlapping work

Upstream PR #2 addresses an older unpacking defect but is not directly compatible with 0.4.6f13: it references removed networking helpers and obsolete method overloads. Do not cherry-pick it wholesale.

### Required completion evidence

- API verification script passes against the supplied f13 assembly.
- Release IL2CPP build succeeds and contains no copied game dependencies.
- Packaged DLL metadata and SHA-256 are recorded here.
- Alex completes the isolated and normal-mod-set runtime checklist from the artifact package.
- Draft PR includes exact static/build evidence and clearly marks runtime verification status.
