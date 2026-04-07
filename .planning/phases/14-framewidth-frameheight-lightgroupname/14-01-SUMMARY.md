---
phase: 14-framewidth-frameheight-lightgroupname
plan: "01"
subsystem: Param / Sequence
tags: [bugfix, ini, lightgroupname, framewidth, frameheight, recipe]
dependency_graph:
  requires: []
  provides: [clean-ini-serialization, lightgroupname-restore-on-reset]
  affects: [CameraSlaveParam, CameraParam, Sequence_Inspection]
tech_stack:
  added: []
  patterns: [legacy-field-removal, copyto-restore-activation]
key_files:
  created: []
  modified:
    - WPF_Example/Sequence/Param/CameraSlaveParam.cs
    - WPF_Example/Sequence/Param/CameraParam.cs
    - WPF_Example/Custom/Sequence/Inspection/Sequence_Inspection.cs
decisions:
  - "FrameWidth/FrameHeight: 프로퍼티 완전 삭제 (주석 처리 아님) — ParamBase.Save() 리플렉션이 public property를 모두 직렬화하므로 삭제만이 INI 오염 방지 가능"
  - "LightGroupName OnLoad: _MyParam.LightGroupName = DefaultLight 제거 — DeviceName은 항상 DefaultCamera로 재설정하되 LightGroupName은 INI에서 로드된 값 그대로 유지"
  - "CopyTo LightGroupName: 주석 해제 활성화 — RestoreShot()이 CopyTo를 호출하므로 백업 시점 LightGroupName이 Reset 시 복원됨"
metrics:
  duration: "~10 minutes"
  completed_date: "2026-04-07"
  tasks_completed: 2
  files_modified: 3
---

# Phase 14 Plan 01: FrameWidth/FrameHeight/LightGroupName INI 버그 수정 Summary

INI 직렬화 오염(FrameWidth/FrameHeight=0 저장), LightGroupName OnLoad 강제 덮어쓰기, Reset 시 LightGroupName 미복원 — 3가지 버그를 CameraSlaveParam.cs / CameraParam.cs / Sequence_Inspection.cs 3개 파일 수정으로 완전 해결.

## Tasks Completed

| Task | Name | Commit | Files |
|------|------|--------|-------|
| 1 | FrameWidth/FrameHeight 레거시 필드 완전 제거 | 59f73a5 | CameraSlaveParam.cs, CameraParam.cs |
| 2 | LightGroupName OnLoad 덮어쓰기 제거 + CopyTo 복원 활성화 | f421f04 | Sequence_Inspection.cs, CameraSlaveParam.cs, CameraParam.cs |

## What Was Built

**Task 1 — FrameWidth/FrameHeight 제거:**
- CameraSlaveParam.cs: `[System.ComponentModel.Browsable(false)] public int FrameWidth/FrameHeight` 프로퍼티 2개 제거
- CameraSlaveParam.cs: CopyTo() 내 `slaveParam.FrameWidth/FrameHeight = this.FrameWidth/FrameHeight` 대입 2개 제거
- CameraSlaveParam.cs: CopyTo() 내 CameraParam 분기 `camParam.FrameWidth/FrameHeight = ...` 대입 2개 제거
- CameraParam.cs: 동일한 패턴으로 FrameWidth/FrameHeight 프로퍼티 2개 및 CopyTo 내 대입 2개 제거
- 각 위치에 `//260407 hbk — FrameWidth/FrameHeight 레거시 필드 제거 (INI 오염 방지)` 주석 추가

**Task 2 — LightGroupName 버그 수정:**
- Sequence_Inspection.OnLoad(): `_MyParam.LightGroupName = DefaultLight;` 라인 제거, 주석을 DeviceName 전용으로 갱신
- CameraSlaveParam.CopyTo(): CameraSlaveParam 분기와 CameraParam 분기 각각에서 `LightGroupName = this.LightGroupName` 주석 해제
- CameraParam.CopyTo(): CameraParam 분기에서 `camParam.LightGroupName = this.LightGroupName` 주석 해제

## Decisions Made

1. **FrameWidth/FrameHeight 삭제 방식**: `[Browsable(false)]` 어트리뷰트로는 불충분 — `ParamBase.Save()`가 리플렉션으로 모든 `public` 프로퍼티를 INI에 직렬화하므로 프로퍼티 자체를 삭제해야만 INI 오염을 방지할 수 있다.

2. **OnLoad DeviceName 유지**: `_MyParam.DeviceName = DefaultCamera` 라인은 유지 — 카메라 디바이스는 항상 런타임 기본 카메라로 바인딩되어야 한다. LightGroupName과 다르게 DeviceName은 INI 값을 따르지 않는다.

3. **CopyTo LightGroupName 활성화**: `RestoreShot(shotIndex)` → `_backup[shotIndex].CopyTo(target)` 흐름에서 LightGroupName이 복원되도록 주석 해제. DeviceName은 여전히 주석 유지 (DeviceName은 CameraMasterParam 경로로만 관리).

## Verification Results

- `grep FrameWidth CameraSlaveParam.cs` → 주석 1건만 (프로퍼티/대입 0건)
- `grep FrameWidth CameraParam.cs` → 주석 1건만 (프로퍼티/대입 0건)
- `grep "_MyParam.LightGroupName = DefaultLight" Sequence_Inspection.cs` → OnLoad에 없음, OnCreate에만 존재
- `grep "slaveParam.LightGroupName = this.LightGroupName" CameraSlaveParam.cs` → 활성 코드로 존재
- MSBuild FinalVision.sln /p:Configuration=Debug → FinalVision.exe 빌드 성공 (0 errors)

## Deviations from Plan

None — plan executed exactly as written.

## Known Stubs

None.

## Self-Check: PASSED

- /d/Project/FinalVision/WPF_Example/Sequence/Param/CameraSlaveParam.cs — FOUND (modified)
- /d/Project/FinalVision/WPF_Example/Sequence/Param/CameraParam.cs — FOUND (modified)
- /d/Project/FinalVision/WPF_Example/Custom/Sequence/Inspection/Sequence_Inspection.cs — FOUND (modified)
- Commit 59f73a5 — FOUND
- Commit f421f04 — FOUND
