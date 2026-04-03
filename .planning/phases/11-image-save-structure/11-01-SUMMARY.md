---
phase: 11-image-save-structure
plan: 01
subsystem: image-save
tags: [image-save, folder-management, inspection-context, system-setting]
dependency_graph:
  requires: []
  provides: [ImageFolderManager.BeginInspection, InspectionSequenceContext.CurrentFolderPath]
  affects: [WPF_Example/Utility/ImageFolderManager.cs, WPF_Example/Custom/Sequence/Inspection/Sequence_Inspection.cs, WPF_Example/Setting/SystemSetting.cs]
tech_stack:
  added: []
  patterns: [static-utility-class, lock-collision-suffix, path-combine-hierarchy]
key_files:
  created:
    - WPF_Example/Utility/ImageFolderManager.cs
  modified:
    - WPF_Example/Custom/Sequence/Inspection/Sequence_Inspection.cs
    - WPF_Example/Setting/SystemSetting.cs
    - WPF_Example/FinalVision.csproj
decisions:
  - "ImageFolderManager is static (no singleton) per D-10; path generation only, no disk cleanup per D-11"
  - "Collision suffix uses _2, _3 pattern rather than timestamp retry to guarantee ordering"
  - "FinalVision.csproj requires explicit Compile includes — ImageFolderManager.cs added to project file"
metrics:
  duration: "~7 minutes"
  completed: "2026-04-03T06:26:48Z"
  tasks_completed: 2
  files_modified: 4
---

# Phase 11 Plan 01: ImageFolderManager + InspectionSequenceContext Wiring Summary

## One-liner

Static ImageFolderManager utility with date>time folder creation (millisecond collision suffix) wired into InspectionSequenceContext.Clear() so every inspection cycle gets a unique time-folder.

## What Was Built

### Task 1: ImageFolderManager static utility class

New file `WPF_Example/Utility/ImageFolderManager.cs` in `FinalVisionProject.Utility`:

- `BeginInspection()`: reads `SystemSetting.Handle.ImageSavePath` for base path, creates `{date}/{time_ms}` folder with `_2`, `_3` collision suffix inside `lock(_lock)`. Returns the created folder path.
- `GetSavePath(folderPath, shotName, isOk)`: returns `{shotName}_{OK|NG}.jpg` path (D-03).
- `GetAnnotatedSavePath(folderPath, shotName, isOk)`: returns `{shotName}_{OK|NG}_annotated.jpg` path (D-04).

### Task 2: InspectionSequenceContext + SystemSetting wiring

- `InspectionSequenceContext.CurrentFolderPath` property added (`= ""` default).
- `Clear()` override calls `ImageFolderManager.BeginInspection()` and assigns result to `CurrentFolderPath`.
- `SystemSetting.ImageSavePath` default changed from `BaseDirectory + "Image"` to `@"D:\Log"` (D-01).
- `using FinalVisionProject.Utility;` added to Sequence_Inspection.cs.

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 3 - Blocking] ImageFolderManager.cs not in FinalVision.csproj Compile includes**
- **Found during:** Task 2 build verification
- **Issue:** FinalVision.csproj uses explicit `<Compile Include=...>` entries. New file was not listed, causing CS0103 compile error.
- **Fix:** Added `<Compile Include="Utility\ImageFolderManager.cs" />` before the `RecipeFileHelper.cs` entry in FinalVision.csproj.
- **Files modified:** WPF_Example/FinalVision.csproj
- **Commit:** e8b7f37

## Commits

| Task | Commit | Message |
|------|--------|---------|
| Task 1 | f240b0c | feat(11-01): add ImageFolderManager static utility class |
| Task 2 | e8b7f37 | feat(11-01): wire ImageFolderManager into InspectionSequenceContext and update defaults |

## Build Result

Build succeeded with pre-existing warnings only (CS0219, CS0169, CS0414 in unrelated files). No new errors introduced.

## Known Stubs

None. All methods have full implementations. `CurrentFolderPath` is set in `Clear()` — data flows from `BeginInspection()` to the property. Plan 02 will consume `CurrentFolderPath` for actual image saving.

## Self-Check: PASSED
