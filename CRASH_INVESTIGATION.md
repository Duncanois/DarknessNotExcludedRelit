# Relit startup failure: ONI U60-764025-SCRP

Investigated 20 November 2026. No source changes or installation changes made.

## Confirmed cause

The installed Steam Relit DLL fails during Harmony patch registration. Its plant-lighting patch targets `IlluminationVulnerable.GetDescriptors`, which no longer exists in the installed game assembly.

Player.log, 10:23:05.587 UTC:

```text
HarmonyLib.HarmonyException: Patching exception in method null
System.ArgumentException: Undefined target method for patch method
DarknessNotIncluded.Plants.Patched_IlluminationVulnerable_GetDescriptors::Prefix
```

The call stack runs through `Harmony.PatchAll`, `KMod.UserMod2.OnLoad`, and `DarknessNotIncluded.Mod.OnLoad`. ONI then logs `Failed to load mod Darkness Not Excluded Relit...disabling`.

## Verified API change

Decompiled the same game type from both available game assemblies:

| Assembly | Descriptor API |
| --- | --- |
| Old references: `C:\Users\dunca\Documents\New folder\Assembly-CSharp.dll` | `List<Descriptor> GetDescriptors(GameObject go)` |
| Installed game: `E:\SteamLibrary\steamapps\common\OxygenNotIncluded\OxygenNotIncluded_Data\Managed\Assembly-CSharp.dll` | `void CollectDescriptors(GameObject go, List<Descriptor> descriptors)` |

The current API appends to a caller-provided descriptor list. The old API returned a new list.

Affected source: `src/features/Plants/IlluminationVulnerableSupportsMaxLux.cs`, class `Patched_IlluminationVulnerable_GetDescriptors`. Its name-based Harmony attribute still specifies `GetDescriptors`, and its prefix returns a replacement list through `__result`.

Required repair: target `CollectDescriptors` and add the custom maximum-lux descriptor to its supplied `descriptors` list. For plants with a configured positive maximum lux, skip the original method; otherwise allow vanilla behavior. Preserve descriptors already in the supplied list. A rename alone is insufficient because the return value and argument contract changed.

## What the logs prove

- The current game identifies itself as `U60-764025-SCRP`.
- The failing enabled mod is Steam item `3609476592`, static ID `Duncanois.DarknessNotExcludedRelit`.
- The patch failure happens during startup, before loading a colony. It does not require a particular save or a plant to exist in the colony.
- The latest Player.log subsequently reaches the main menu and ends with shutdown cleanup. It does not contain a native crash stack identifying a separate CTD.
- Player-prev.log records a crash submission and a later restart, but contains no exception stack explaining that earlier event. The confirmed Relit error above is from Player.log.
- A successful compile does not detect this break: the target method name is supplied as a string and Harmony resolves it at runtime. The earlier recovery build was a compile/decompilation validation, not runtime validation.

Other installed Darkness variants appear during mod discovery. The shown failing patch is specifically attributed to Relit; discovery alone does not establish that the other variants were enabled or caused this failure.

## Evidence and limits

Preserved in `recovery/crash-2026-11-20`: both supplied logs and decompiled old/current `IlluminationVulnerable` text. This establishes the API incompatibility between the available assemblies; the update's precise historical release timing was not independently checked.

No patch has been applied. After repairing this first blocking error, all remaining Harmony targets should be checked and the mod launched in game; the first exception may conceal later compatibility failures.
