# 1.4.1 descriptor compatibility build

20 November 2026. Recovered 1.4.1 is now migrated into the active root project. Original affected files are preserved in `before-api-fix-20261120-122809`.

Changed the plant descriptor Harmony target from removed `GetDescriptors` to `CollectDescriptors(GameObject, List<Descriptor>)`. The prefix appends the custom maximum-lux descriptor to the caller's list and skips vanilla handling only for plants with a positive configured maximum. Existing list entries and vanilla handling for other plants are preserved. `nameof` and explicit argument types record the current API contract.

The project targets net48/version 1.4.1 and uses the recovered 1.4.1 toggle behavior and Steam metadata. Its build packages automatically into `bin/Debug/net48/package` and contains no installation target. The January tracked `bundle` is unchanged.

`dotnet build project.csproj` passed with zero warnings and errors. Decompiled the packaged DLL and confirmed the new target and list-appending prefix in the final merged assembly. `git diff --check` passed. See `api-fix-build.log` and `api-fix-built-patch.cs.txt`.

The four runtime package files are `DarknessNotExcluded.dll`, `mod.yaml`, `mod_info.yaml`, and `preview.png`; a debug PDB is also present. Do not use the intermediate `.mod.dll` or copy the build directory's dependency DLLs into the mod installation.

No mod was installed, copied into ONI's mods folders, or uploaded. No game was launched. Runtime verification is reserved for the owner; successful compilation and inspection do not prove that all Harmony targets or gameplay paths are compatible.
