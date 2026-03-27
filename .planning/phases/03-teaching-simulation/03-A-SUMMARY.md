---
phase: 03-teaching-simulation
plan: A
subsystem: inspection
tags: [image-buffer, blob-overlay, OpenCvSharp, InspectionParam, SIMUL]
dependency_graph:
  requires: []
  provides: [InspectionParam.LastOriginalImage, InspectionParam.LastAnnotatedImage, RunBlobDetection-overlay]
  affects: [03-C-PLAN.md, 03-D-PLAN.md]
tech_stack:
  added: []
  patterns: [value-tuple return, clone-on-store, compile-time preprocessor branching]
key_files:
  modified:
    - WPF_Example/Custom/Sequence/Inspection/Action_Inspection.cs
decisions:
  - SetOriginalImage called before null check on GrabImage so Grab-fail path still records null-derived state cleanly
  - SetAnnotatedImage uses lock flag (_AnnotatedImageLocked) to prevent SIMUL re-inspection from overwriting the baseline image
  - RunBlobDetection returns (bool isOk, Mat annotated) Value Tuple; caller is responsible for Dispose after Clone stored in param
  - SIMUL path uses #if SIMUL_MODE compile-time symbol to keep runtime zero-cost when not defined
metrics:
  duration_minutes: 15
  completed_date: 2026-03-26
  tasks_completed: 2
  files_modified: 1
---

# Phase 03 Plan A: InspectionParam 이미지 버퍼 + Blob 오버레이 Summary

InspectionParam에 Shot별 원본/오버레이 이미지 버퍼(LastOriginalImage/LastAnnotatedImage)를 추가하고, RunBlobDetection이 keypoint 원(OK=초록/NG=빨강) + ROI 사각형(노랑)이 그려진 오버레이 Mat을 반환하도록 수정.

## Tasks Completed

| # | Name | Status | Key Changes |
|---|------|--------|-------------|
| A-1 | InspectionParam 이미지 버퍼 추가 | Done | LastOriginalImage, LastAnnotatedImage, SetOriginalImage, SetAnnotatedImage, SetAnnotatedImageTemp, GetAnnotatedImageTemp, ResetAnnotatedImageLock, _AnnotatedImageLocked |
| A-2 | RunBlobDetection 오버레이 반환 + Run() 이미지 버퍼 저장 | Done | RunBlobDetection signature → (bool isOk, Mat annotated), EStep.Grab SetOriginalImage call, EStep.BlobDetect #if SIMUL_MODE branch, Cv2.Circle + Cv2.Rectangle overlay |

## Decisions Made

1. `SetOriginalImage` is called immediately after `GrabImage()` (before null check) so that even a grab-fail cycle records the null state correctly and no logic branch is missed.
2. `_AnnotatedImageLocked` flag on `InspectionParam` ensures the annotated image is written exactly once per real-mode inspection cycle. `SetAnnotatedImage` silently returns if the lock is already set. `ResetAnnotatedImageLock()` allows a fresh cycle to overwrite.
3. `SetAnnotatedImageTemp` provides a parallel path for SIMUL re-inspection: the canvas can be refreshed with the new overlay Mat without touching `LastAnnotatedImage`.
4. `RunBlobDetection` now returns a C# 7.0 Value Tuple `(bool isOk, Mat annotated)`. The caller in `EStep.BlobDetect` calls `Dispose()` on the local `annotated` reference after the param stores its own clone — preventing double-free.
5. The SIMUL/non-SIMUL split uses `#if SIMUL_MODE` / `#else` / `#endif` compile-time preprocessor symbols (consistent with the existing `SIMUL_MODE` usage pattern in `OnLoad()`).

## Deviations from Plan

None — plan executed exactly as written.

## Known Stubs

None — all image buffer methods are fully implemented. Data consumers (Plan C Shot Viewer UI, Plan D SIMUL auto-advance) will call `LastOriginalImage` / `LastAnnotatedImage` / `GetAnnotatedImageTemp()` from their respective plans.

## Self-Check: PASSED

- File exists: `WPF_Example/Custom/Sequence/Inspection/Action_Inspection.cs` — FOUND
- `LastOriginalImage { get; private set; }` — FOUND (line 51)
- `LastAnnotatedImage { get; private set; }` — FOUND (line 52)
- `SetOriginalImage` — FOUND (line 57, 179)
- `SetAnnotatedImage` — FOUND (line 63, 198)
- `SetAnnotatedImageTemp` — FOUND (line 75, 194)
- `GetAnnotatedImageTemp` — FOUND (line 81)
- `ResetAnnotatedImageLock` — FOUND (line 86)
- `_AnnotatedImageLocked` — FOUND (line 55, 65, 68, 88)
- `RunBlobDetection` tuple return — FOUND (line 217: `private (bool isOk, Mat annotated) RunBlobDetection`)
- `#if SIMUL_MODE` / `#else` / `#endif` branch — FOUND (lines 192-199)
- `Cv2.Circle(` — FOUND (line 273)
- `Cv2.Rectangle(` — FOUND (line 277)
- All new lines include `//260326 hbk` comments — CONFIRMED
