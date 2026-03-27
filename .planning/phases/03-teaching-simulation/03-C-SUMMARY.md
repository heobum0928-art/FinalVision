---
phase: 03-teaching-simulation
plan: C
subsystem: ui
tags: [shot-viewer, combobox, result-strip, MainView, WPF]
dependency_graph:
  requires: [03-A]
  provides: [MainView.comboBox_shot, MainView.UpdateShotStrip, Shot-viewer-UI]
  affects: [03-D-PLAN.md]
tech_stack:
  added: []
  patterns: [suppress-event-flag, dispatcher-begininvoke, array-indexof-lookup]
key_files:
  modified:
    - WPF_Example/UI/ContentItem/MainView.xaml
    - WPF_Example/UI/ContentItem/MainView.xaml.cs
decisions:
  - _suppressShotComboEvent flag pattern used to prevent recursive ComboBox_shot_SelectionChanged when code changes SelectedIndex
  - Array.IndexOf(SHOT_ACTION_NAMES, param.ActionName) used in GrabAndDisplay for O(5) lookup — simple and readable
  - UpdateShotStrip is public so Plan D (SIMUL auto-advance) can call it after sequence completes
  - RefreshShotViewer dispatches via BeginInvoke even when already on UI thread (e.g. RadioButton check) — consistent with existing DisplayToBackground call pattern
metrics:
  duration_minutes: 12
  completed_date: 2026-03-26
  tasks_completed: 2
  files_modified: 2
---

# Phase 03 Plan C: Shot 뷰어 UI (ComboBox + RadioButton + 결과 스트립) Summary

MainView.xaml 하단에 Shot 선택 ComboBox(Shot1~5) + 원본/측정 RadioButton + OK/NG/미실행 색상 결과 스트립 5칸을 추가하고, GrabAndDisplay 완료 시 해당 Shot이 자동 선택되는 Shot 뷰어 UI 구현.

## Tasks Completed

| # | Name | Status | Key Changes |
|---|------|--------|-------------|
| C-1 | MainView.xaml Shot 뷰어 UI 추가 | Done | Grid Row 2(Auto) 추가, comboBox_shot + radioButton_original/annotated + stripBorder_Shot1~5 UniformGrid 삽입 |
| C-2 | MainView.xaml.cs Shot 뷰어 이벤트 핸들러 구현 | Done | SHOT_ACTION_NAMES/SHOT_DISPLAY_NAMES 배열, ComboBox_shot_SelectionChanged, RadioButton_imageMode_Checked, StripBorder_MouseLeftButtonDown, RefreshShotViewer, GetInspectionParam, UpdateShotStrip, GrabAndDisplay 자동 선택 추가 |

## Decisions Made

1. `_suppressShotComboEvent` 플래그로 코드가 `SelectedIndex`를 변경할 때 `ComboBox_shot_SelectionChanged`가 재진입하는 것을 방지한다. `StripBorder_MouseLeftButtonDown`과 `GrabAndDisplay` 양쪽에서 사용된다.
2. `Array.IndexOf(SHOT_ACTION_NAMES, param.ActionName)` — 5개 요소 배열이므로 LINQ 없이 단순 IndexOf 사용. 기존 코드베이스의 LINQ 최소 사용 패턴과 일치한다.
3. `UpdateShotStrip()`를 `public`으로 선언하여 Plan D (SIMUL 자동 실행 완료 이벤트)에서 외부 호출 가능하게 한다.
4. `RefreshShotViewer`는 `Dispatcher.BeginInvoke`로 `DisplayToBackground`를 호출한다 — UI 스레드에서 이미 실행 중이더라도 기존 `GrabAndDisplay` 패턴과 일관성 유지.

## Deviations from Plan

None — plan executed exactly as written.

## Known Stubs

None — Shot ComboBox는 `SHOT_DISPLAY_NAMES` 5개 항목으로 완전히 초기화되며, 결과 스트립은 `seq[i].Context.Result`에서 실제 데이터를 읽는다. `RefreshShotViewer`는 `InspectionParam.LastOriginalImage` / `LastAnnotatedImage`를 직접 표시한다 (Plan A에서 구현됨).

## Self-Check: PASSED

- File exists: `WPF_Example/UI/ContentItem/MainView.xaml` — FOUND
- File exists: `WPF_Example/UI/ContentItem/MainView.xaml.cs` — FOUND
- `comboBox_shot` in XAML — FOUND (line 70)
- `radioButton_original` in XAML — FOUND (line 72)
- `radioButton_annotated` in XAML — FOUND (line 76)
- `stripBorder_Shot1` ~ `stripBorder_Shot5` in XAML — FOUND (lines 84, 90, 96, 102, 108)
- `UniformGrid` in XAML — FOUND (line 83)
- `Tag="0"` ~ `Tag="4"` in XAML — FOUND (lines 85, 91, 97, 103, 109)
- `ComboBox_shot_SelectionChanged` in XAML — FOUND (line 71)
- `RadioButton_imageMode_Checked` in XAML — FOUND (lines 75, 79)
- `StripBorder_MouseLeftButtonDown` in XAML — FOUND (lines 85, 91, 97, 103, 109)
- `<!--260326 hbk` comments in XAML — FOUND
- `SHOT_ACTION_NAMES` array (5 elements) in .cs — FOUND (lines 106-112)
- `SHOT_DISPLAY_NAMES` array (5 elements) in .cs — FOUND (lines 114-120)
- `_suppressShotComboEvent` in .cs — FOUND (line 122)
- `ComboBox_shot_SelectionChanged` handler in .cs — FOUND (line 426)
- `RadioButton_imageMode_Checked` handler in .cs — FOUND (line 435)
- `StripBorder_MouseLeftButtonDown` handler in .cs — FOUND (line 443)
- `RefreshShotViewer` method in .cs — FOUND (line 467)
- `UpdateShotStrip` public method in .cs — FOUND (line 482)
- `GetInspectionParam` method in .cs — FOUND (line 458)
- `comboBox_shot.SelectedIndex = shotIdx` in GrabAndDisplay — FOUND (line 308)
- `UpdateShotStrip()` call in GrabAndDisplay — FOUND (line 311)
- All new lines include `//260326 hbk` comments — CONFIRMED
