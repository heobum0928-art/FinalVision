---
phase: 06-ui
plan: 01
subsystem: ui
tags: [wpf, camera-param, refactor, legacy-cleanup]

# Dependency graph
requires: []
provides:
  - CameraSlaveParam에서 ECi_Dispenser 레거시 필드(PixelToUM_Offset, MotorXPos, MotorYPos, PartNo) 제거
  - CameraSlaveParam에서 ConvertPixelToMM 메서드 제거
  - CameraParam에서 동일 레거시 필드 제거
  - CopyTo() 대칭 정리 완료
affects: [RecipeEditorWindow, InspectionParam, PropertyGrid 표시]

# Tech tracking
tech-stack:
  added: []
  patterns:
    - "레거시 제거 패턴: 필드 완전 삭제 + //260327 hbk 제거 이유 주석"

key-files:
  created: []
  modified:
    - WPF_Example/Sequence/Param/CameraSlaveParam.cs
    - WPF_Example/Sequence/Param/CameraParam.cs

key-decisions:
  - "레거시 필드는 주석 처리가 아닌 완전 삭제 — PropertyGrid에 노출되지 않도록"
  - "삭제 위치에 //260327 hbk 코멘트 블록으로 변경 이유 기록"

patterns-established:
  - "레거시 제거: 활성 프로퍼티 라인 완전 삭제, 제거 위치에 날짜+담당자 주석 삽입"

requirements-completed: []

# Metrics
duration: 12min
completed: 2026-03-27
---

# Phase 06 Plan 01: CameraSlaveParam/CameraParam 레거시 필드 제거 Summary

**ECi_Dispenser 잔재 레거시 필드(PixelToUM_Offset, MotorXPos, MotorYPos, PartNo, ConvertPixelToMM)를 CameraSlaveParam/CameraParam에서 완전 제거하여 RecipeEditorWindow PropertyGrid 불필요 필드 노출 방지**

## Performance

- **Duration:** 12min
- **Started:** 2026-03-27T07:05:00Z
- **Completed:** 2026-03-27T07:17:00Z
- **Tasks:** 2/2
- **Files modified:** 2

## Accomplishments

### Task 1: CameraSlaveParam.cs 레거시 필드 및 메서드 제거 (commit: 5b37199)

- `public double PixelToUM_Offset` 프로퍼티 제거
- `public double MotorXPos` 프로퍼티 제거
- `public double MotorYPos` 프로퍼티 제거
- `public int PartNo` 프로퍼티 제거
- `public virtual double ConvertPixelToMM(double pixel)` 메서드 제거
- `CopyTo()` 내 CameraSlaveParam 분기: PartNo/MotorXPos/MotorYPos/PixelToUM_Offset 대입 제거
- `CopyTo()` 내 CameraParam 분기: 동일 4개 필드 대입 제거
- FrameWidth, FrameHeight, LightGroupName, LightLevel 유지
- 모든 변경에 `//260327 hbk` 주석 포함

### Task 2: CameraParam.cs 레거시 필드 제거 + 빌드 확인 (commit: 18d65c8)

- `public double PixelToUM_Offset` 프로퍼티 제거
- `public double MotorXPos` 프로퍼티 제거
- `public double MotorYPos` 프로퍼티 제거
- `public int PartNo` 프로퍼티 제거
- `CopyTo()` 내 CameraSlaveParam/CameraParam 분기 레거시 대입 4개 제거
- msbuild 빌드 성공 (오류 0건, 기존 경고만 존재)

## Commits

| Task | Commit | Description |
|------|--------|-------------|
| 1 | 5b37199 | feat(06-ui-01): CameraSlaveParam 레거시 필드 및 메서드 제거 |
| 2 | 18d65c8 | feat(06-ui-01): CameraParam 레거시 필드 제거 + 빌드 확인 |

## Verification Results

- `grep "PixelToUM_Offset" WPF_Example/Sequence/Param/CameraSlaveParam.cs` → 0 matches
- `grep "MotorXPos" WPF_Example/Sequence/Param/CameraSlaveParam.cs` → 0 matches
- `grep "MotorYPos" WPF_Example/Sequence/Param/CameraSlaveParam.cs` → 0 matches
- `grep "PartNo" WPF_Example/Sequence/Param/CameraSlaveParam.cs` → 0 matches
- `grep "ConvertPixelToMM" WPF_Example/Sequence/Param/CameraSlaveParam.cs` → 0 matches
- `grep "PixelToUM_Offset" WPF_Example/Sequence/Param/CameraParam.cs` → 0 matches
- `grep "MotorXPos" WPF_Example/Sequence/Param/CameraParam.cs` → 0 matches
- `grep "MotorYPos" WPF_Example/Sequence/Param/CameraParam.cs` → 0 matches
- `grep "PartNo" WPF_Example/Sequence/Param/CameraParam.cs` → 0 matches
- Cross-file grep: 0 matches in WPF_Example/Sequence/Param/
- msbuild FinalVision.sln /p:Configuration=Debug → 빌드 성공

## Deviations from Plan

None — plan executed exactly as written.

## Known Stubs

None.

## Self-Check: PASSED

- `WPF_Example/Sequence/Param/CameraSlaveParam.cs` — FOUND
- `WPF_Example/Sequence/Param/CameraParam.cs` — FOUND
- Commit 5b37199 — FOUND
- Commit 18d65c8 — FOUND
