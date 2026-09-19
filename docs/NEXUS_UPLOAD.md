# Nexus Mods upload copy — Improved Packagers PORTED

This sheet is ready to copy into the Schedule I Nexus Mods upload form. Keep the word **PORTED** in the title so players can distinguish this compatibility build from the original.

## Main details

**Game**

Schedule I

**Mod name**

Improved Packagers PORTED - f13 IL2CPP Compatibility Port

**One-line summary**

Restores Improved Packagers on Schedule I 0.4.6f13 IL2CPP so assigned workers can unpack products, move station output and load vehicles.

**Version**

2.0.1

**Category**

Employees

**Suggested tags**

Gameplay, Employees, Utilities, AI Assisted

Use **AI Assisted**, not AI Generated: the compatibility work was human-directed, reviewed and repeatedly play-tested, with Codex used as a development assistant.

**Author display**

GuysWeForgotDre (original author) | Ported and maintained by GSVS UK ACM

## Requirements

- Schedule I 0.4.6f13 IL2CPP
- MelonLoader 0.7.0 Open-Beta
- [Mod Manager & Phone App](https://www.nexusmods.com/schedule1/mods/397) — **required**, not optional

## Full description

```text
IMPROVED PACKAGERS PORTED

Original mod by GuysWeForgotDre.
Schedule I 0.4.6f13 IL2CPP compatibility port maintained by GSVS UK ACM.

This independently maintained port restores Improved Packagers on the current f13 IL2CPP build. Assigned Packagers can follow a Packaging Station's pack/unpack setting, use the station, move loose unpacked output into storage and load vehicles parked at Loading Bays.

WHY "PORTED"?

The original release no longer works correctly on the f13 IL2CPP game build. PORTED is included throughout the mod name, DLL and log output so it is easy to distinguish this build from the original. Do not install both versions together.

REQUIREMENTS

- Schedule I 0.4.6f13 IL2CPP
- MelonLoader 0.7.0 Open-Beta
- Mod Manager & Phone App: https://www.nexusmods.com/schedule1/mods/397

Mod Manager & Phone App is REQUIRED, not merely recommended.

INSTALLATION

1. Close the game.
2. Open your Schedule I installation folder, then open the Mods folder.
3. Remove any old/original Improved Packagers DLL: ImprovedPackagers.dll, ImprovedPackagersPORTED.dll or Main-ImprovedPackagers.dll.
4. Install the requirements above.
5. Copy ImprovedPackagersPORTED.dll from the download into Schedule I/Mods.
6. Launch the game and confirm Improved Packagers PORTED is enabled in the in-game Mod Manager.

Keep only one Improved Packagers DLL in the Mods folder.

COMPATIBILITY AND TESTING

- Built for Schedule I 0.4.6f13 IL2CPP.
- Play-tested through the unpack, worker pickup and delivery cycle.
- Tested both in isolation and with HWE 2.2.3 enabled.
- This port makes no new Mono compatibility claim.

CREDITS, SOURCE AND LICENCE

Improved Packagers and its core work were created by GuysWeForgotDre:
https://github.com/GuysWeForgotDre/Improved-Packagers

The f13 IL2CPP compatibility port is maintained by GSVS UK ACM:
https://github.com/xboxnuker-rgb/improved-packagers-ported

The original project was released under the MIT License. This upload retains the original copyright and licence text. This is an independently maintained compatibility port, not an official release by the original author. Attempts were made to contact the original author before publication; no response had been received at the time of upload.

For issues, source, illustrated setup help and the latest release, use the port repository above.
```

## Main file

**File title**

Improved Packagers PORTED 2.0.1 - Schedule I 0.4.6f13 IL2CPP

**Upload filename**

`Improved-Packagers-PORTED-v2.0.1-NEXUS-Schedule-I-f13.zip`

**File description**

```text
Main file. Includes ImprovedPackagersPORTED.dll, the original MIT licence, port attribution notice and installation README. Requires MelonLoader 0.7.0 Open-Beta and Mod Manager & Phone App. Remove every older/original Improved Packagers DLL before installing.
```

**Changelog**

```text
2.0.1
- Ported Improved Packagers to Schedule I 0.4.6f13 IL2CPP.
- Updated Harmony targets and station UI access for f13.
- Restored worker selection, station operation, pickup and delivery for unpack mode.
- Added loose-product routing through the game's native transit workflow.
- Restored brick, jar and baggie loading into unpack-mode Packaging Stations.
- Standardized the visible name, DLL and log identity as Improved Packagers PORTED.
- Preserved original-author credit and added clear port-maintainer attribution.
```

## Permissions and disclosure answers

- **Original work/assets:** Based on Improved Packagers by GuysWeForgotDre. Link the original [GitHub repository](https://github.com/GuysWeForgotDre/Improved-Packagers) and [Nexus page](https://www.nexusmods.com/schedule1/mods/1142).
- **Permission basis:** The original project is published under the MIT License, which expressly permits use, modification and redistribution when its copyright and permission notice are retained. Both are included unmodified as `LICENSE.txt`.
- **Other-site upload permission:** The original Nexus page also states that uploads to other sites are permitted with credit. Do not describe the port as endorsed by or official to the original author.
- **AI disclosure:** Select **AI Assisted**. Codex assisted development and documentation; changes were human-directed and validated through source inspection, builds and in-game play-testing.
- **Donation Points:** Opt out initially. Revisit only after the original author responds and any desired arrangement is clear.
- **Adult content:** No.
- **Paid content:** No.

## Images and captions

Upload in this order:

1. `docs/images/packager-unpacking.png`  
   **Caption:** Assigned Packager actively using an unpack-mode Packaging Station.
2. `docs/images/unpackage-mode-bricks.png`  
   **Caption:** Packaging Station configured to unpackage a 20-unit brick.
3. `docs/images/handler-station-assignment.png`  
   **Caption:** Assign the Handler/Packager to a Packaging Station from the management clipboard.
4. `docs/images/install-folder.png`  
   **Caption:** Copy ImprovedPackagersPORTED.dll into the Schedule I Mods folder. Keep only this version installed.
5. `docs/images/mod-manager-enabled.png`  
   **Caption:** Confirm Improved Packagers PORTED is enabled in the in-game Mod Manager.
6. `docs/images/phone-app-required.png`  
   **Caption:** Mod Manager & Phone App is a required dependency.

Use `packager-unpacking.png` as the primary image.

## Recommended links

- **Source / bug reports:** https://github.com/xboxnuker-rgb/improved-packagers-ported
- **Original source:** https://github.com/GuysWeForgotDre/Improved-Packagers
- **Original Nexus page:** https://www.nexusmods.com/schedule1/mods/1142
- **Required dependency:** https://www.nexusmods.com/schedule1/mods/397

The maintainer-support link remains available on GitHub. Keeping the Nexus description focused on the mod, attribution and support documentation avoids making the port look like a claim over the original author's work.

## Pre-publish checklist

- [ ] Select Schedule I and the Employees category.
- [ ] Keep **PORTED** in the mod name and file title.
- [ ] Mark Mod Manager & Phone App as a required dependency.
- [ ] Select the **AI Assisted** tag/disclosure.
- [ ] Upload the Nexus ZIP from `artifacts/`, not GitHub's source-code archive.
- [ ] Upload the six images above and make `packager-unpacking.png` primary.
- [ ] Confirm the visible credit says original author GuysWeForgotDre and port maintainer GSVS UK ACM.
- [ ] Confirm `LICENSE.txt`, `NOTICE.txt` and `README.txt` appear in the file preview.
- [ ] Expect a DLL security scan or temporary manual-review quarantine.
- [ ] Preview the page, test the download once, then publish.

