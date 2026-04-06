---
phase: 12-run-grab
plan: "03"
subsystem: ui
tags: [wpf, image-management, folder-delete, settings]

# Dependency graph
requires:
  - phase: 12-run-grab/12-01
    provides: ShotTabView Grab button removal and RUN 3-way branch logic
  - phase: 12-run-grab/12-02
    provides: ShotTabView folder load button and BackgroundImagePath flow
  - phase: 11-image-save-structure
    provides: ImageFolderManager yyyyMMdd/HHmm folder structure

provides:
  - SystemSetting.GetImageDateFolders() returning yyyyMMdd date folder list
  - ImageManageWindow with CheckBox list and recursive folder deletion
  - SettingWindow 이미지 관리 button opening ImageManageWindow

affects: [Phase 13, any future image cleanup automation]

# Tech tracking
tech-stack:
  added: []
  patterns: [PropertyGrid-window uses별도 Dialog window for extra functionality, ObservableCollection<T> with INotifyPropertyChanged item for CheckBox binding]

key-files:
  created:
    - WPF_Example/UI/Setting/ImageManageWindow.xaml
    - WPF_Example/UI/Setting/ImageManageWindow.xaml.cs
  modified:
    - WPF_Example/Setting/SystemSetting.cs
    - WPF_Example/UI/Setting/SettingWindow.xaml
    - WPF_Example/UI/Setting/SettingWindow.xaml.cs
    - WPF_Example/FinalVision.csproj

key-decisions:
  - "PropertyGrid-based SettingWindow uses separate ImageManageWindow Dialog (Alternative A) — PropertyGrid cannot host custom TabItems"
  - "DateFolderItem ViewModel uses INotifyPropertyChanged for CheckBox IsChecked two-way binding in ListBox"
  - "Directory.Delete recursive=true as specified in D-12; confirmation dialog required before delete (D-11)"

patterns-established:
  - "Pattern: New Window files in .NET Framework WPF require explicit Compile+Page entries in FinalVision.csproj"
  - "Pattern: CustomMessageBox.ShowConfirmation used for destructive operations"

requirements-completed: [IMG-04]

# Metrics
duration: 15min
completed: "2026-04-06"
---

# Phase 12 Plan 03: Image Management Tab Summary

**ImageManageWindow Dialog added to SettingWindow — lists yyyyMMdd date folders with CheckBox selection and recursive deletion via ShowConfirmation guard**

## Performance

- **Duration:** ~15 min
- **Started:** 2026-04-06
- **Completed:** 2026-04-06
- **Tasks:** 1 of 2 (Task 2 is checkpoint:human-verify — awaiting user)
- **Files modified:** 6

## Accomplishments
- `SystemSetting.GetImageDateFolders()` returns filtered yyyyMMdd directories ordered descending
- `ImageManageWindow` (new) with lb_dateFolders ListBox, checkbox binding via DateFolderItem ViewModel, Refresh button
- `Btn_DeleteFolders_Click` performs ShowConfirmation before Directory.Delete(path, true), then refreshes list
- SettingWindow.xaml updated with "이미지 관리" button; SettingWindow.xaml.cs opens ImageManageWindow.ShowDialog()
- FinalVision.csproj updated with Compile + Page entries for ImageManageWindow

## Task Commits

1. **Task 1: SystemSetting GetImageDateFolders + ImageManageWindow** - `76e6fd1` (feat)

## Files Created/Modified
- `WPF_Example/UI/Setting/ImageManageWindow.xaml` - New window: lb_dateFolders ListBox with DataTemplate CheckBox
- `WPF_Example/UI/Setting/ImageManageWindow.xaml.cs` - LoadDateFolders, Btn_DeleteFolders_Click, DateFolderItem ViewModel
- `WPF_Example/Setting/SystemSetting.cs` - Added GetImageDateFolders() method
- `WPF_Example/UI/Setting/SettingWindow.xaml` - Added 이미지 관리 button
- `WPF_Example/UI/Setting/SettingWindow.xaml.cs` - Added Btn_imageManage_Click handler
- `WPF_Example/FinalVision.csproj` - Registered ImageManageWindow Compile + Page entries

## Decisions Made
- PropertyGrid-based SettingWindow uses a separate Dialog window (Alternative A) — can't add TabItems to PropertyGrid
- DockPanel layout in SettingWindow bottom bar: 이미지 관리 button left-docked, OK/Cancel in StackPanel right

## Deviations from Plan

None - plan executed exactly as written (Alternative A was anticipated by the plan).

## Issues Encountered
- `dotnet build` does not generate WPF code-behind (.g.cs) for .NET Framework projects (pre-existing issue across whole project). Used MSBuild directly — build succeeded with warnings only, 0 errors.

## User Setup Required
None — no external service configuration required.

## Next Phase Readiness
- Phase 12 Task 1+2+3 complete. Awaiting user manual verification (Task 2 checkpoint).
- After approved: Phase 12 fully complete, Phase 13 RecipeEditorWindow can begin.

---
*Phase: 12-run-grab*
*Completed: 2026-04-06*
