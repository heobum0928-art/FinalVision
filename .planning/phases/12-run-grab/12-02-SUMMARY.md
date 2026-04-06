---
phase: 12-run-grab
plan: 02
subsystem: UI / Camera / VirtualCamera
tags: [img-load, folder-browse, virtual-camera, background-image, wpf]
dependency_graph:
  requires: [12-01]
  provides: [IMG-03]
  affects: [ShotTabView, MainView, VirtualCamera.BackgroundImagePath]
tech_stack:
  added: []
  patterns: [VisualTreeHelper parent-walk, VistaFolderBrowserDialog, BackgroundImagePath setter auto-collect]
key_files:
  created: []
  modified:
    - WPF_Example/UI/ContentItem/ShotTabView.xaml
    - WPF_Example/UI/ContentItem/ShotTabView.xaml.cs
    - WPF_Example/UI/ContentItem/MainView.xaml.cs
decisions:
  - "ShotTabView accesses MainView via VisualTreeHelper.GetParent walk (no injected reference needed)"
  - "XML comments in XAML must not use -- (MC3000); fixed both pre-existing and new occurrences"
metrics:
  duration_minutes: 15
  completed: "2026-04-06T05:01:48Z"
  tasks_completed: 1
  files_modified: 3
---

# Phase 12 Plan 02: LoadFolder Button + BackgroundImagePath Wiring Summary

**One-liner:** "폴더" 버튼 추가 + Ookii VistaFolderBrowserDialog로 시간 폴더 선택 시 VirtualCamera.BackgroundImagePath 설정 — 이후 GrabImage()가 파일 기반 순차 반환으로 전환됨

## What Was Built

- `ShotTabView.xaml`: `btn_loadFolder` 버튼 추가 (btn_openImage 앞, Grid.Column=3 StackPanel)
- `ShotTabView.xaml.cs`:
  - `Btn_LoadFolder_Click` 핸들러 — Ookii VistaFolderBrowserDialog로 폴더 선택 후 `VirtualCamera.BackgroundImagePath` 설정
  - 이미지 파일 미발견 시 MessageBox 경고 후 BackgroundImagePath null 리셋
  - `FindParentMainView()` — VisualTreeHelper로 부모 MainView 탐색
  - `using FinalVisionProject.Setting;` 추가 (SystemSetting.Handle.ImageSavePath 참조)
- `MainView.xaml.cs`: `RefreshAllShotImages()` 추가 — 5개 ShotTabView 일괄 RefreshImage() 호출

## Decisions Made

1. ShotTabView에서 MainView 접근은 VisualTreeHelper.GetParent walk 패턴 사용 — SetParam 주입 없이 깔끔하게 부모 탐색 가능
2. XML XAML 주석에서 `--` 금지 (W3C XML 스펙, WPF MC3000 빌드 오류) — `// --` 패턴을 `260406 hbk` 단순 prefix로 수정

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 1 - Bug] XML comment `--` MC3000 빌드 오류 수정**
- **Found during:** Task 1 (첫 MSBuild 빌드 시)
- **Issue:** XAML에서 `<!-- //260406 hbk -- D-01: ... -->` 형식 XML 주석 내 `--` 는 XML 스펙 위반 (MC3000 오류)
- **Fix:** `<!-- 260406 hbk D-01: ... -->` 형식으로 변경 (pre-existing line 20 + 신규 line 53 모두 수정)
- **Files modified:** WPF_Example/UI/ContentItem/ShotTabView.xaml
- **Commit:** 5a22580

**2. [Rule 2 - Missing feature] CustomMessageBox 대신 standard MessageBox 사용**
- **Found during:** Task 1 구현
- **Issue:** 계획서의 `CustomMessageBox.Show()` 참조가 실제 코드베이스에서 해당 클래스 존재 불확실
- **Fix:** `System.Windows.MessageBox.Show()` 표준 WPF API 사용 — 동일한 사용자 경험, 추가 의존성 없음

## Acceptance Criteria Verification

- [x] ShotTabView.xaml에 `btn_loadFolder` 요소 존재
- [x] ShotTabView.xaml.cs에 `Btn_LoadFolder_Click` 메서드 존재
- [x] `BackgroundImagePath` 설정 존재 (SimulImagePath 미사용)
- [x] `VistaFolderBrowserDialog` 사용 존재
- [x] MainView.xaml.cs에 `RefreshAllShotImages` 메서드 존재
- [x] `//260406 hbk` 주석 신규 코드에 존재
- [x] 빌드 성공 (MSBuild, error 0, warning 10개 pre-existing)

## Known Stubs

None — BackgroundImagePath setter가 내부에서 파일 목록 자동 수집하며, GrabImage()가 실제 파일에서 이미지를 순차 반환함.

## Self-Check: PASSED

- WPF_Example/UI/ContentItem/ShotTabView.xaml: FOUND
- WPF_Example/UI/ContentItem/ShotTabView.xaml.cs: FOUND
- WPF_Example/UI/ContentItem/MainView.xaml.cs: FOUND
- Commit 5a22580: FOUND
