---
phase: 11-image-save-structure
plan: 02
subsystem: image-save
tags: [image-save, Action_Inspection, ImageFolderManager, annotated-image, async-save]
dependency_graph:
  requires: [11-01]
  provides: [IMG-01, IMG-02]
  affects: [Action_Inspection.SaveResultImage, Action_Inspection.OnBegin]
tech_stack:
  added: []
  patterns: [Task.Factory.StartNew async save with Clone+Dispose, null guard for SIMUL mode, folder path capture in OnBegin]
key_files:
  modified:
    - WPF_Example/Custom/Sequence/Inspection/Action_Inspection.cs
decisions:
  - "_FolderPath captured once in OnBegin from InspectionSequenceContext.CurrentFolderPath (D-09: single capture per inspection)"
  - "annotated null+IsDisposed guard chosen over try/catch to handle SIMUL mode transparently"
  - "Async save pattern (Task.Factory.StartNew + Clone + Dispose) reused from SequenceBase pattern"
metrics:
  duration: "~2 minutes"
  completed: "2026-04-03"
  tasks_completed: 2
  files_modified: 1
---

# Phase 11 Plan 02: SaveResultImage Refactor to ImageFolderManager Summary

**One-liner:** SaveResultImage replaced hardcoded D:\Log flat-file with ImageFolderManager-based original+annotated pair save into time-folder using async Task.Factory.StartNew+Clone pattern.

## Tasks Completed

| Task | Name | Commit | Files |
|------|------|--------|-------|
| 1 | Add _FolderPath field and OnBegin override | bae0e46 | Action_Inspection.cs |
| 2 | Rewrite SaveResultImage with ImageFolderManager paths | 4baebb1 | Action_Inspection.cs |

## What Was Built

### Task 1: _FolderPath field + OnBegin override
Added `private string _FolderPath = ""` field to Action_Inspection and overrode `OnBegin(SequenceContext prevResult)` to capture `CurrentFolderPath` from `InspectionSequenceContext`. This ensures each Action has the correct time-folder path for the inspection cycle, set once at inspection start.

- `using FinalVisionProject.Utility` was already present (added in Plan 01)
- OnBegin calls `base.OnBegin(prevResult)` first, then casts `prevResult as InspectionSequenceContext`

### Task 2: SaveResultImage rewrite
Replaced the old flat-file approach (D:\Log\{date}\{name}_{result}_{time}.jpg) with:
1. Original image: `ImageFolderManager.GetSavePath(_FolderPath, Name, isOK)` -> `{name}_{OK|NG}.jpg`
2. Annotated image: `ImageFolderManager.GetAnnotatedSavePath(_FolderPath, Name, isOK)` -> `{name}_{OK|NG}_annotated.jpg`
3. Both saved async via `Task.Factory.StartNew` with `Clone()` + `Dispose()` in finally
4. Annotated null-guarded: `annotated != null && !annotated.IsDisposed` (SIMUL mode safe)
5. `_FolderPath` empty guard added before save logic
6. OK/NG filter (SaveOkImage/SaveNgImage) preserved at top — applies to both images as pair

## Deviations from Plan

None - plan executed exactly as written.

## Known Stubs

None. `_FolderPath` is populated from `InspectionSequenceContext.CurrentFolderPath` which is set by `ImageFolderManager.BeginInspection()` in `Clear()`. Full data flow is wired end-to-end.

## Verification Results

- `D:\Log` hardcoding: 0 occurrences (PASS)
- `ImageFolderManager.GetSavePath`: 1 occurrence (PASS)
- `ImageFolderManager.GetAnnotatedSavePath`: 1 occurrence (PASS)
- `_FolderPath` field: present (PASS)
- `OnBegin` override: present with base call + cast (PASS)
- MSBuild: succeeded with pre-existing warnings only (PASS)

## Self-Check: PASSED

- File exists: `WPF_Example/Custom/Sequence/Inspection/Action_Inspection.cs` - FOUND
- Commit bae0e46 - FOUND
- Commit 4baebb1 - FOUND
