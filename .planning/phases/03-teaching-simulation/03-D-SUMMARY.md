---
phase: 03-teaching-simulation
plan: D
subsystem: ui
tags: [SIMUL_MODE, auto-advance, InspectionListView, MainView, GrabAndDisplay, callback]
dependency_graph:
  requires: [03-A, 03-C]
  provides: [GrabAndDisplay.onComplete, InspectionListView.AdvanceToNextAction, SIMUL-B-mode-auto-advance]
  affects: []
tech_stack:
  added: []
  patterns: [action-callback-parameter, compile-time-preprocessor-branching, selecteditem-reference-comparison]
key_files:
  modified:
    - WPF_Example/UI/ContentItem/MainView.xaml.cs
    - WPF_Example/UI/ControlItem/InspectionListView.xaml.cs
decisions:
  - GrabAndDisplay bool eventCall replaced with Action onComplete nullable callback — minimal change, zero-cost default null
  - AdvanceToNextAction uses SelectedItem reference comparison (node == currentNode) rather than IsSelected flag — more reliable with PropertyTools.Wpf TreeListBox
  - onComplete executes on UI thread inside Dispatcher.BeginInvoke — AdvanceToNextAction needs no inner Dispatcher wrapping
  - Shot 5 auto-stop: nextIndex < 0 early-return leaves treeListBox on last action (no wrap-around)
metrics:
  duration_minutes: 10
  completed_date: 2026-03-26
  tasks_completed: 2
  files_modified: 2
---

# Phase 03 Plan D: SIMUL 모드 B방식 자동 진행 (Grab → 다음 Action 자동 포커스) Summary

GrabAndDisplay에 완료 콜백(Action onComplete) 파라미터 추가 후, SIMUL_MODE 빌드에서 Grab 완료 시 InspectionListView.AdvanceToNextAction()을 호출하여 treeListBox_sequence 선택이 Shot1→2→3→4→5 순서로 자동 이동하는 B방식 시뮬레이션 자동 진행 구현.

## Tasks Completed

| # | Name | Status | Key Changes |
|---|------|--------|-------------|
| D-1 | GrabAndDisplay에 완료 콜백 파라미터 추가 | Done | `bool eventCall = false` → `Action onComplete = null`, `onComplete?.Invoke()` 삽입(UpdateShotStrip() 이후, InvalidateVisual() 직후) |
| D-2 | InspectionListView SIMUL_MODE B방식 자동 진행 구현 | Done | `button_grab_Click` #if SIMUL_MODE/#else/#endif 분기 추가, `AdvanceToNextAction()` 메서드 추가(#if SIMUL_MODE 블록 내) |

## Decisions Made

1. `GrabAndDisplay`의 기존 미사용 `bool eventCall` 파라미터를 `Action onComplete = null`로 교체했다. 기본값 `null`로 기존 호출부 호환성을 유지한다.
2. `AdvanceToNextAction`에서 현재 선택 노드를 `treeListBox_sequence.SelectedItem as NodeViewModel`로 직접 참조하고 루프에서 `node == currentNode` (레퍼런스 비교)로 인덱스를 찾는다. `node.IsSelected` 플래그보다 안정적이다.
3. `onComplete` 콜백은 `GrabAndDisplay` 내부의 `Dispatcher.BeginInvoke` UI 스레드 블록에서 실행되므로 `AdvanceToNextAction` 내부에 추가 `Dispatcher` 래핑이 불필요하다.
4. Shot 5(Assy_Rail_Two) 이후 `nextIndex < 0`이면 즉시 return — 자동 진행 중단, treeListBox는 마지막 Action에 유지된다.

## Deviations from Plan

**1. [Rule 1 - Bug] AdvanceToNextAction 내부 Dispatcher 제거**
- **Found during:** Task D-2 구현 검토
- **Issue:** 계획의 예시 코드에서는 `AdvanceToNextAction` 내부에 `Dispatcher.BeginInvoke`를 포함했으나, `onComplete`는 이미 `GrabAndDisplay`의 `Dispatcher.BeginInvoke` UI 스레드 내부에서 호출된다. 이중 Dispatcher는 불필요하며 순서 보장이 약해진다.
- **Fix:** 계획에서 "실제 콜백 실행 스레드 확인 후 필요 없으면 Dispatcher 제거한다"는 지시에 따라 내부 Dispatcher 없이 직접 UI 조작으로 구현.
- **Files modified:** `WPF_Example/UI/ControlItem/InspectionListView.xaml.cs`

**2. [Rule 1 - Bug] IsSelected 플래그 대신 SelectedItem 레퍼런스 비교 사용**
- **Found during:** Task D-2 구현 검토
- **Issue:** 계획 예시 코드는 `node.IsSelected` 플래그로 현재 노드를 찾았으나, 계획 주의사항에서 `treeListBox_sequence.SelectedItem as NodeViewModel`이 더 안전하다고 명시.
- **Fix:** `SelectedItem` 레퍼런스로 현재 노드를 직접 가져온 뒤 루프에서 `node == currentNode`로 인덱스 탐색.
- **Files modified:** `WPF_Example/UI/ControlItem/InspectionListView.xaml.cs`

## Known Stubs

None — `AdvanceToNextAction`은 실제 `treeListBox_sequence.Items`에서 노드를 순회하며 실제 `SelectedIndex`와 `IsSelected`를 조작한다. 비SIMUL_MODE 빌드에서는 `#if SIMUL_MODE` 블록이 컴파일에서 제외되어 이전 동작 그대로다.

## Self-Check: PASSED

- File exists: `WPF_Example/UI/ContentItem/MainView.xaml.cs` — FOUND
- File exists: `WPF_Example/UI/ControlItem/InspectionListView.xaml.cs` — FOUND
- `Action onComplete = null` in GrabAndDisplay signature — FOUND (line 260)
- `onComplete?.Invoke()` in Dispatcher.BeginInvoke block — FOUND (line 318)
- `onComplete?.Invoke()` after `UpdateShotStrip()` (line 311) — CONFIRMED
- `onComplete?.Invoke()` after `canvas_main.InvalidateVisual()` (line 317) — CONFIRMED
- `//260326 hbk` on new lines in MainView.xaml.cs — CONFIRMED
- `#if SIMUL_MODE` / `#else` / `#endif` in `button_grab_Click` — FOUND (lines 222-226)
- `onComplete: () => AdvanceToNextAction()` in SIMUL branch — FOUND (line 223)
- `AdvanceToNextAction()` method inside `#if SIMUL_MODE` block — FOUND (lines 229-278)
- `treeListBox_sequence.SelectedIndex = nextIndex` — FOUND (line 270)
- `nextIndex < 0` early return (Shot 5 stop) — FOUND (line 266)
- `//260326 hbk` on all new lines in InspectionListView.xaml.cs — CONFIRMED
