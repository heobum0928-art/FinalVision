---
gsd_state_version: 1.0
milestone: v2.0
milestone_name: 레시피 편집 + 이미지 관리 + 운영 안정화
status: Executing Phase 16
stopped_at: Completed 14-01-PLAN.md
last_updated: "2026-04-13T01:36:23.059Z"
progress:
  total_phases: 7
  completed_phases: 6
  total_plans: 13
  completed_plans: 12
  percent: 92
---

# FinalVision — Project State

## Project Reference

See: .planning/PROJECT.md (updated 2026-04-02)

**Core value:** 카메라 1대 + 5-Shot 순차 촬상으로 자재 유무를 정확히 판정하고, TCP 통신으로 설비와 연동하여 자동 검사를 수행한다.
**Current focus:** Phase 16 — alive-ui

## Current Position

Phase: 16 (alive-ui) — EXECUTING
Plan: 1 of 2

## Performance Metrics

**Velocity:**

- Total plans completed: 0 (v2.0)
- Average duration: —
- Total execution time: —

**By Phase:**

| Phase | Plans | Total | Avg/Plan |
|-------|-------|-------|----------|
| - | - | - | - |

**Recent Trend:** No data yet

*Updated after each plan completion*
| Phase 10-recipe-copy-infra P02 | 15 | 1 tasks | 1 files |
| Phase 10-recipe-copy-infra P01 | 8 | 2 tasks | 3 files |
| Phase 11-image-save-structure P01 | 7 | 2 tasks | 4 files |
| Phase 11-image-save-structure P02 | 2 | 2 tasks | 1 files |
| Phase 12-run-grab P01 | 314 | 2 tasks | 4 files |
| Phase 12-run-grab P02 | 15 | 1 tasks | 3 files |
| Phase 12-run-grab P03 | 15 | 1 tasks | 6 files |
| Phase 13-recipeeditorwindow P01 | 10 | 2 tasks | 3 files |
| Phase 14-framewidth-frameheight-lightgroupname P01 | 10 | 2 tasks | 3 files |

## Accumulated Context

### Decisions

- [v1.0]: HIK 전용 (Basler 제거), PLC 미사용 (TCP/IP 전용), OpenCvSharp FindContours
- [v2.0]: 신규 NuGet 패키지 추가 금지 — 기존 PropertyTools.Wpf, Ookii.Dialogs.Wpf로 충분
- [Phase 10-recipe-copy-infra]: [10-02] ELogType.Trace 재사용 + [TAKT] 접두사로 ActionBase.OnEnd() 택타임 로그 출력 (D-01~D-04 준수)
- [Phase 10-recipe-copy-infra]: Copy() 구 시그니처(string,string,bool) 완전 제거 — siteNumber 필수 파라미터로 API 일관성 확보
- [Phase 10-recipe-copy-infra]: CopyFilesRecursively: Directory.CreateDirectory 무조건 호출(no-op 안전), if(!Exists) TOCTOU 패턴 금지
- [Phase 11-image-save-structure]: ImageFolderManager static utility (no singleton) for path generation only; collision suffix _2/_3 for millisecond uniqueness; FinalVision.csproj requires explicit Compile includes
- [Phase 11-image-save-structure]: _FolderPath captured once in OnBegin from InspectionSequenceContext.CurrentFolderPath; annotated null+IsDisposed guard for SIMUL mode; async Task.Factory.StartNew+Clone+Dispose pattern
- [Phase 12-run-grab]: RefreshShotImage extracted as MainView method for shot tab UI refresh after RunBlobOnLastGrab (D-04)
- [Phase 12-run-grab]: IsIdle guard on both BackgroundImagePath and SimulImagePath branches to prevent TCP collision
- [Phase 12-run-grab]: ShotTabView accesses MainView via VisualTreeHelper.GetParent walk — no injected reference needed
- [Phase 12-run-grab]: XML XAML comments must not use -- (MC3000); fixed both pre-existing and new occurrences
- [Phase 12-run-grab]: PropertyGrid-based SettingWindow uses separate ImageManageWindow Dialog (Alt A) — PropertyGrid cannot host custom TabItems
- [Phase 12-run-grab]: DateFolderItem ViewModel uses INotifyPropertyChanged for CheckBox IsChecked two-way binding
- [Phase 13-recipeeditorwindow]: TakeBackup uses InspectionParam CopyTo() confirmed deep copy (ROICircle=struct, ROI=struct, ERoiShape=enum)
- [Phase 13-recipeeditorwindow]: Reset button uses repair.png in separate ToolBar block; PropertyGrid refresh via Paste pattern (UnselectAll/SelectedIndex)
- [Phase 14-framewidth-frameheight-lightgroupname]: FrameWidth/FrameHeight 프로퍼티 완전 삭제 — ParamBase.Save() 리플렉션 직렬화 방지
- [Phase 14-framewidth-frameheight-lightgroupname]: LightGroupName OnLoad 제거 — INI 로드 값 유지, DeviceName만 DefaultCamera 재세팅
- [Phase 14-framewidth-frameheight-lightgroupname]: CopyTo LightGroupName 주석 해제 — RestoreShot() Reset 시 LightGroupName 복원 활성화

### Pending Todos

None yet.

### Roadmap Evolution

- Phase 14 added: 레시피 파일 설정값 버그 수정 (FrameWidth/FrameHeight/LightGroupName)
- Phase 16 added: ALIVE 상태 UI 인디케이터 — 녹색 깜빡임(하트비트 수신)/빨강(타임아웃)/회색(미연결) 표시

### Blockers/Concerns

- Phase 13: RecipeEditorWindow Grab Preview UI 결정 (별도 MiniCanvas vs ShotTabView 재사용) — Phase 13 착수 전 결정 필요
- Phase 12: Step 카운터 리셋 시점 — Action_Inspection.Run() 상태머신 내부 초기화 위치 확인 필요

## Session Continuity

Last session: 2026-04-07T06:48:31.902Z
Stopped at: Completed 14-01-PLAN.md
Resume file: None
