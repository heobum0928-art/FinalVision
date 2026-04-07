---
phase: 13-recipeeditorwindow
plan: "01"
subsystem: inspection-reset
tags: [backup, restore, reset, InspectionParam, UI]
dependency_graph:
  requires: []
  provides: [shot-param-backup, shot-param-restore, reset-button-ui]
  affects: [Sequence_Inspection, InspectionListView]
tech_stack:
  added: []
  patterns: [Dictionary-backup, CopyTo-deep-copy, UnselectAll-PropertyGrid-refresh]
key_files:
  created: []
  modified:
    - WPF_Example/Custom/Sequence/Inspection/Sequence_Inspection.cs
    - WPF_Example/UI/ControlItem/InspectionListView.xaml
    - WPF_Example/UI/ControlItem/InspectionListView.xaml.cs
decisions:
  - "TakeBackup uses InspectionParam CopyTo() confirmed as deep copy (ROICircle=struct, ROI=struct, ERoiShape=enum, rest=primitives/string)"
  - "RestoreShot uses _backup[shotIndex].CopyTo(target) — value-copy pattern, not reference replace"
  - "Reset button placed in separate ToolBar block after Copy/Paste, uses repair.png icon (no undo.png available)"
  - "PropertyGrid refresh via UnselectAll/SelectedIndex pattern reused from Paste handler"
metrics:
  duration_minutes: 10
  completed_date: "2026-04-07"
  tasks_completed: 2
  files_modified: 3
---

# Phase 13 Plan 01: Shot Parameter Backup and Reset Summary

**One-liner:** Shot parameter backup-on-load with per-shot restore via InspectionListView Reset button using InspectionParam CopyTo deep-copy.

## What Was Built

레시피 로드 시점에 Sequence_Inspection이 모든 Shot의 InspectionParam을 Dictionary에 스냅샷하고, InspectionListView 툴바의 Reset 버튼 클릭 시 선택된 Shot만 로드 시점 값으로 복원한다.

## Tasks Completed

| Task | Name | Commit | Files |
|------|------|--------|-------|
| 1 | Add backup/restore methods to Sequence_Inspection | e99b1bc | Sequence_Inspection.cs |
| 2 | Add Reset button to InspectionListView + click handler | 188f829 | InspectionListView.xaml, InspectionListView.xaml.cs |

## Key Implementation Details

### Task 1 — Sequence_Inspection

- Added `private Dictionary<int, InspectionParam> _backup` field
- Added `TakeBackup()` — clears and rebuilds snapshot for all ActionCount shots via `CopyTo()`
- Added `RestoreShot(int shotIndex)` — bounds-checked, restores only the target shot
- Added `HasBackup` property — guards Reset button when no recipe is loaded
- Modified `OnLoad()` — calls `TakeBackup()` after `base.OnLoad()`, covering both `LoadFromIni` overloads via `ExecOnLoad`
- Deep-copy safety confirmed: `UI.Circle` is `struct` (DrawableCircle.cs line 15), `System.Windows.Rect` is WPF struct, `ERoiShape` is enum — `CopyTo()` already performs full deep copy

### Task 2 — InspectionListView

- Added Reset `<ToolBar>` block after Copy/Paste toolbar in XAML, using `repair.png` icon
- `button_reset_Click` handler:
  - Guards: selected node must be ENodeType.Action + ESequence.Inspection
  - Finds shotIndex by ActionID match (same pattern as Btn_start_Click)
  - Casts to `Sequence_Inspection`, checks `HasBackup`
  - Shows confirmation dialog (Paste pattern)
  - Calls `RestoreShot(shotIndex)`, shows error on failure
  - Refreshes PropertyGrid via UnselectAll/SelectedIndex
  - Refreshes ShotTabView via `SetParam(ESequence.Inspection, ip)`
  - Updates statusBar with "Reset Shot_N 완료"
- No `--` in XAML comments (MC3000 rule observed)

## Deviations from Plan

None — plan executed exactly as written.

## Known Stubs

None — all data paths are wired. Backup is populated from real InspectionParam objects on recipe load.

## Self-Check: PASSED

- `Sequence_Inspection.cs` TakeBackup/RestoreShot/HasBackup: FOUND (lines 148, 163, 172)
- `InspectionListView.xaml` button_reset: FOUND (line 239)
- `InspectionListView.xaml.cs` button_reset_Click: FOUND (line 305)
- Commit e99b1bc: FOUND
- Commit 188f829: FOUND
- MSBuild: 0 errors
