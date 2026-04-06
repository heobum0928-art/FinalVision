---
phase: 12-run-grab
plan: "01"
subsystem: UI
tags: [grab-button, run-button, role-separation, shot-tab, inspection-list]
dependency_graph:
  requires: []
  provides: [OPS-02]
  affects: [ShotTabView, InspectionListView, MainView]
tech_stack:
  added: []
  patterns:
    - "3-way branch in Btn_start_Click: BackgroundImagePath → StartSequence, SimulImagePath → RunBlobOnLastGrab, fallback → original"
    - "IsIdle guard before sequence start to prevent TCP conflicts"
key_files:
  created: []
  modified:
    - WPF_Example/UI/ContentItem/ShotTabView.xaml
    - WPF_Example/UI/ContentItem/ShotTabView.xaml.cs
    - WPF_Example/UI/ControlItem/InspectionListView.xaml.cs
    - WPF_Example/UI/ContentItem/MainView.xaml.cs
decisions:
  - "RefreshShotImage extracted as new method in MainView for single-shot UI refresh after RunBlobOnLastGrab"
  - "Build verified at code-logic level only; FinalVision.csproj has pre-existing XAML codegen issue (382 errors baseline)"
metrics:
  duration: "5m 14s"
  completed_date: "2026-04-06"
  tasks_completed: 2
  files_modified: 4
---

# Phase 12 Plan 01: Run/Grab Role Separation Summary

**One-liner:** ShotTabView Grab button removed; InspectionListView RUN now routes BackgroundImagePath → 5-Shot sequence Start, SimulImagePath → per-Shot RunBlobOnLastGrab, else original camera sequence.

## What Was Built

**Task 1 — ShotTabView Grab 버튼 제거 (D-01)**

- `ShotTabView.xaml`: `btn_grab` Button element removed from StackPanel
- `ShotTabView.xaml.cs`: `Btn_Grab_Click` async handler (41 lines) and `_grabTask` Task field removed
- Comment `//260406 hbk -- D-01` added marking consolidation to InspectionListView `button_grab`

**Task 2 — InspectionListView RUN 버튼 3단계 분기 (D-03, D-04, D-07)**

- `InspectionListView.xaml.cs` `Btn_start_Click` now has 3-stage dispatch:
  1. `BackgroundImagePath` set → `IsIdle` check → `mParentWindow.StartSequence(ESequence.Inspection)` → full 5-Shot file-based grab
  2. Selected Action has `SimulImagePath` set → `IsIdle` check → `act.RunBlobOnLastGrab()` → `mainView.RefreshShotImage(actIndex)` → single-Shot inspect
  3. Neither → original `StartSequence(seqID, actID)` path preserved
- `using System.IO` and `using FinalVisionProject.Device` added to InspectionListView
- `MainView.xaml.cs`: new `RefreshShotImage(int shotIndex)` public method added — calls both `RefreshImage()` and `UpdateResultLabel()` on the target ShotTabView

## Decisions Made

- Extracted `RefreshShotImage(int)` as a MainView method (per plan D-04 recommendation) rather than inline — improves reusability
- `IsIdle` guard placed on both Branch 1 and Branch 2 entry to prevent TCP collision per Research Pitfall 1

## Deviations from Plan

### Auto-fixed Issues

None — plan executed exactly as written.

## Build Note

`FinalVision.csproj` has a pre-existing XAML codegen failure (382 errors before this plan) that prevents `dotnet build` from succeeding in the CLI context. This is an established project state (Visual Studio builds correctly). Error count is 388 after Task 2 due to additional lines referencing `mainView`/`treeListBox_sequence` — same `CS0103`/`CS1061` pattern as all pre-existing InspectionListView references. No new error category was introduced.

## Known Stubs

None — all logic is wired to real system objects (SystemHandler, VirtualCamera, Action_Inspection).

## Self-Check: PASSED

- FOUND: WPF_Example/UI/ContentItem/ShotTabView.xaml
- FOUND: WPF_Example/UI/ContentItem/ShotTabView.xaml.cs
- FOUND: WPF_Example/UI/ControlItem/InspectionListView.xaml.cs
- FOUND: WPF_Example/UI/ContentItem/MainView.xaml.cs
- FOUND: .planning/phases/12-run-grab/12-01-SUMMARY.md
- FOUND commit: 1244c7f feat(12-01): remove Grab button from ShotTabView (D-01)
- FOUND commit: edaf473 feat(12-01): add 3-way branch to RUN button in InspectionListView
