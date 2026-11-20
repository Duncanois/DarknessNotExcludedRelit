# Darkness Not Excluded Relit recovery audit

20 November 2026. Audit and staged reconstruction only; existing tracked source and installed mods remain unchanged.

## Result

The supplied Steam DLL is readable managed C#/.NET code. Recovery succeeded without replacing the original source with decompiler output.

The reconstructed package builds successfully against the installed game libraries: **zero warnings, zero errors**. After decompilation with the same tool and settings, **all 205 code files match the Steam DLL**, including bundled PLib, excluding assembly metadata. This supports recovery of the published implementation; it is not a byte-identical rebuild or a gameplay test.

The actual code gap is much smaller than expected: the January source and Steam build contain the same 55 decompiled mod code files; **54 match exactly and one differs**. No mod types were added or removed.

## Inputs and preservation

- Active repository: `C:\Users\dunca\Documents\Game Modding\Oxygen Not Included\DarknessNotExcludedRelit`.
- Original HEAD: `b83df126e939c7d7c75e1963ae9f90994b48fc97`, 8 January 2026, `Fixed space reveal crash`.
- Working tree was clean before this audit. Origin: `https://github.com/Duncanois/DarknessNotExcludedRelit.git`. No fetch, push, branch change, or commit was performed.
- Steam input: `C:\Users\dunca\Documents\Klei\OxygenNotIncluded\mods\Steam\3609476592`.
- Steam package contains four files: `DarknessNotExcluded.dll`, `mod.yaml`, `mod_info.yaml`, `preview.png`. No PDB was supplied.
- Complete recovery directory: `C:\Users\dunca\Documents\Game Modding\Oxygen Not Included\DarknessNotExcludedRelit-Recovery-20261120-121435`.
- Backups there: `repository.bundle` (all local refs/history), `january-tracked-source.zip`, `january-bundle`, and `steam-3609476592`. `input-hashes.json` records preserved input hashes.

| Input | DLL bytes | SHA-256 |
| --- | ---: | --- |
| Steam 1.4.1 | 408064 | `5F82A02DBF50F5F9036D6E9A7415291CBB296EA19E78D1B2FC32D26BBE560DB7` |
| Repository bundle 1.4.0 | 408576 | `5DB66F7F922A2DA8821601A20160A5A2F5131576D563B176F7B467FEE674840B` |
| Reconstructed 1.4.1 | 408064 | `92285F159E8269B58EC3EA71D47E25CF6EEAF1734ED6BA7405D73C04A1A54192` |

Filesystem timestamps indicate local download/copy activity, not a verified Workshop publishing date. The supplied package is used as the recovery authority. The public [Workshop page](https://steamcommunity.com/sharedfiles/filedetails/?id=3609476592) was consulted for context; public page timestamps alone do not authenticate these local bytes as the most recent server copy.

## Published changes recovered

### Source behavior

`src/features/InGameConfig/DarknessToggleButton.cs`:

- Left-click still toggles darkness.
- The published build removes the right-click handler which opened the in-game options dialog.
- Both tooltips remove the right-click instructions.
- The disable tooltip uses three periods instead of the previous ellipsis character.

The options dialog implementation itself remains present. Recovery does not remove the regular mod settings UI.

All other mod code matches the compiled January source after decompilation, including configuration defaults, custom light shapes, visibility/occlusion, space reveal batching, equipment lighting, darkness effects, plant handling, and options presets. These are compiled-code comparisons, not runtime assurances.

### Build and package metadata

| Setting | January | Steam |
| --- | --- | --- |
| Target framework | .NET Framework 4.7.1 | .NET Framework 4.8 |
| Assembly/file version | 1.4.0.0 | 1.4.1.0 |
| Package version | 1.4.0 | 1.4.1 |
| Minimum game build | 999999 | 737790 |
| API/content | 2 / ALL | 2 / ALL |
| Bundled PLib | 4.19.0.0 | 4.19.0.0 |
| Mod title | Darkness Not Excluded | Darkness Not Excluded Relit |
| Static ID | nevir.DarknessNotExcluded | Duncanois.DarknessNotExcludedRelit |

The published `mod.yaml` title is `Darkness Not Excluded Relit`, and its static ID is `Duncanois.DarknessNotExcludedRelit`. Published metadata is copied unchanged into the staged candidate. Preview hashes are identical between the January bundle and Steam package.

The Steam assembly informational version is `1.4.1+b83df126e939c7d7c75e1963ae9f90994b48fc97`. This anchors its build to the existing January commit identifier but does not encode uncommitted laptop changes. The January bundled DLL instead identifies `f96d7c97ac7559b05f7359a5de73bcda74935474`, so source was separately compiled and compared rather than relying only on that bundle.

## Validation

ILSpy command line version `11.2.0.10050`, C# 7.3 output, with explicit dependency resolution paths. The DLL was inspected statically; its mod entry point was not run.

1. Decompiled the preserved Steam and January bundled DLLs. Of 211 extracted project/code/resource files, 207 matched. Differences: framework project declaration, darkness toggle code, assembly metadata, and `ILRepack.List` build provenance.
2. Compiled the unchanged January source in an isolated project using its old `C:\Users\dunca\Documents\New folder` references. It passed; the legacy Newtonsoft.Json 7.0.1 NuGet dependency produced NU1903 warnings.
3. Decompiled that source build and compared all 55 mod code files to Steam: 54 exact matches and the one darkness toggle change described above. The file sets matched in both directions.
4. Compiled the unchanged January source against installed game libraries at `E:\SteamLibrary\steamapps\common\OxygenNotIncluded\OxygenNotIncluded_Data\Managed`, targeting net48. It passed. This checks compilation against the installed assemblies, not Harmony patch binding or game behavior.
5. Reconstructed a candidate from the original source, changing only the published toggle behavior. A new, isolated project supplies the recovered framework/version, configurable game references, PLib dependency, and explicit packaging target. It references the game's Newtonsoft.Json.dll rather than downloading a second copy. `dotnet build project.csproj -t:Package` passed with zero warnings/errors.
6. Decompiled the reconstructed merged DLL. All 205 code files, including bundled PLib, matched Steam exactly. Assembly metadata, compiler identity/provenance, dependency reference versions, module identifiers, and byte identity are outside this equality claim.

Raw whole-assembly decompiler output is also retained. Building that unedited output directly failed on three PLib `SerializeField` attributes emitted on properties. The verified candidate instead preserves the original mod source and uses the original PLib NuGet package; it does not rely on rebuilding decompiled PLib.

Evidence in the external recovery directory includes `source-comparison.json`, `decompiled-comparison.json`, `reconstructed-comparison.json`, `dependency-hashes.json`, `steam-assembly-references.json`, `source-audit-build.log`, `current-game-build.log`, `decompiled-build.log`, and `reconstructed-build.log`.

## Deliverables and continuation

- `recovery/Published-1.4.1-source.zip`: reconstructed project with original source layout/comments, recovered toggle change, original license/assets, published mod metadata, and build instructions. No auto-install target.
- `recovery/Steam-1.4.1-decompiled.zip`: raw ILSpy project/code/resources for reference, without build outputs. This is evidence, not the preferred continuation project.
- `recovery/evidence`: comparison manifests, input hashes, and build logs.
- `recovery/January-to-published-source.patch`: the narrow source change in the staged candidate. `git apply --check` passed; the patch has not been applied.
- External `reconstructed-1.4.1/artifacts/package`: validated four-file package. It has not been installed or uploaded.

To continue in the active repository, use the reconstructed archive as a reviewed recovery candidate: apply its narrow source change and build configuration, and reconcile the tracked `bundle` separately. The original root project still targets net471, has old hardcoded references and unquoted paths, and automatically replaces the local installed mod. Its tracked bundle is still the January version. No root build or installation target was executed during this audit.

Before publishing a future build, test mod loading, settings, the darkness toggle, save loading, visibility/space reveal, equipment lights, plant/effect behavior, and Expanded Lights interaction in game. Recovery does not establish that existing runtime bugs are fixed.

Comments and source layout survive because the January source was available. The DLL alone cannot recover the laptop's exact project file, uncompiled edits, comments that existed only on that laptop, or absent development assets.
