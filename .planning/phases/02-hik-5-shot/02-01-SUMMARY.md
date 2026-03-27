---
phase: 02-hik-5-shot
plan: 01
subsystem: vision-sequence
tags: [inspection, tcp, blob-detection, 5-shot, hik-camera]
dependency_graph:
  requires: []
  provides: [ESequence.Inspection, Action_Inspection, SimpleBlobDetector, TCP-RESULT-OK/NG]
  affects: [SequenceHandler, ResourceMap, SystemSetting, DeviceHandler, VisionResponsePacket]
tech_stack:
  added: [SimpleBlobDetector (OpenCvSharp)]
  patterns: [EStep state machine, CameraSlaveParam inheritance, TCP TEST→RESULT flow]
key_files:
  created:
    - WPF_Example/Custom/Sequence/Inspection/Sequence_Inspection.cs
    - WPF_Example/Custom/Sequence/Inspection/Action_Inspection.cs
  modified:
    - WPF_Example/Sequence/Param/CameraSlaveParam.cs
    - WPF_Example/Utility/RecipeFileHelper.cs
    - WPF_Example/Custom/Define/ID.cs
    - WPF_Example/Custom/Device/DeviceHandler.cs
    - WPF_Example/Setting/SystemSetting.cs
    - WPF_Example/Custom/TcpServer/ResourceMap.cs
    - WPF_Example/TcpServer/VisionResponsePacket.cs
    - WPF_Example/Custom/Sequence/SequenceHandler.cs
    - WPF_Example/Sequence/Sequence/SequenceContext.cs
    - WPF_Example/UI/ContentItem/MainView.xaml.cs
    - WPF_Example/FinalVision.csproj
  deleted:
    - WPF_Example/Custom/Sequence/Corner/Sequence_CornerAlign.cs
    - WPF_Example/Custom/Sequence/Corner/Action_CornerAlign_Inspection.cs
    - WPF_Example/Custom/Sequence/Corner/Action_CornerAlign_Calibration.cs
decisions:
  - ETestType enum in ResourceMap.cs aligned with EAction enum (both use Bolt_One=1..Assy_Rail_Two=5)
  - CornerAlignSequenceContext overloaded constructor in SequenceContext.cs removed (was dead code blocking build)
  - CORNER_ALIGN_CAMERA_WIDTH/HEIGHT renamed → INSPECTION_CAMERA_WIDTH/HEIGHT; MainView.xaml.cs updated accordingly
  - TestResultPacket.DieCount/ROICount/Data properties retained (not removed) to avoid downstream compile risk
metrics:
  duration: ~25 minutes
  completed: 2026-03-26
  tasks_completed: 2
  files_modified: 11
  files_created: 2
  files_deleted: 3
---

# Phase 2 Plan 1: HIK 카메라 단일화 및 5-Shot 시퀀스 구조 Summary

**One-liner:** CornerAlign 시퀀스를 Inspection 시퀀스로 전면 교체하여 TCP TEST:1,type,null@ 수신 → SimpleBlobDetector Grab+Blob+OK/NG+저장 → RESULT:1,type,OK@ 응답하는 5-Shot end-to-end 플로우 구현

---

## Tasks Completed

### Task 1: 260325 hbk 제거 + 기반 설정 변경

- **CameraSlaveParam.cs:** `_isLoading` 필드, DeviceName setter guard (`if (_isLoading) return`), `Load()` 오버라이드 전체 제거. base.Load()가 부모에서 처리됨.
- **RecipeFileHelper.cs:** Copy() 메서드의 ini 경로 치환 블록(260325 hbk) 제거. `CopyFilesRecursively()` 뒤 바로 `return true;`로 종료.
- **ID.cs:** ESequence → `Inspection=1`, EAction → 5종 (Bolt_One_Inspection=1 ~ Assy_Rail_Two_Inspection=5).
- **DeviceHandler.cs:** `CORNER_ALIGN_CAMERA` → `INSPECTION_CAMERA`, 관련 WIDTH/HEIGHT 상수 rename. RegisterRequiredDevices() 업데이트.
- **SystemSetting.cs:** ServerPort 기본값 `2505` → `7701`. `SaveOkImage`, `SaveNgImage`, `SimulImagePath` 프로퍼티 추가.
- **ResourceMap.cs:** ETestType 5종 교체, Initialize() 매핑 교체 (INSPECTION_CAMERA + SEQ_INSPECTION + ACT_BOLT_ONE~ACT_ASSY_TWO).
- **VisionResponsePacket.cs:** TestResult Convert() 단순화 — `RESULT:site,type,OK/NG`만 출력. DieCount/ROICount/Data 관련 라인 제거.

### Task 2: Sequence_Inspection + Action_Inspection 신규 생성 및 SequenceHandler 교체

- **Sequence_Inspection.cs (신규):** CornerAlignSequence 패턴 클론. `InspectionSequenceContext`, `Sequence_Inspection` 클래스. OnCreate()에서 카메라 초기화, OnLoad()에서 조명 적용.
- **Action_Inspection.cs (신규):** `InspectionParam` (CameraSlaveParam 상속, ROI/BlobMinArea/BlobMaxArea/DelayMs/ProcessName 프로퍼티). `Action_Inspection` (ActionBase 상속, EStep.Grab→BlobDetect→SaveImage→End 상태머신). `RunBlobDetection()` SimpleBlobDetector filterByArea만 사용, `keypoints.Length == 1` → OK. `SaveResultImage()` SystemSetting.SaveOkImage/SaveNgImage 조건부 저장.
- **SequenceHandler.cs:** SEQ_INSPECTION + ACT_BOLT_ONE~ACT_ASSY_TWO const, SHOT_INDEX_* const. RegisterSequences/RegisterActions/InitializeSequences 전면 교체.
- **FinalVision.csproj:** Corner 3개 파일 항목 제거, Inspection 2개 파일 항목 추가.
- **Corner 폴더 3개 파일 삭제.**

---

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 1 - Bug] SequenceContext.cs의 CornerAlignSequenceContext 잔존 참조 제거**
- **Found during:** Task 2 build verification
- **Issue:** `SequenceContext.cs`에 `private CornerAlignSequenceContext source;` 필드와 `public SequenceContext(CornerAlignSequenceContext source)` 오버로드 생성자가 남아 있었음. Corner 파일 삭제 후 빌드 에러 2개 발생.
- **Fix:** 해당 필드와 오버로드 생성자를 제거. 기존 유일 생성자 `SequenceContext(SequenceBase source)`는 유지.
- **Files modified:** `WPF_Example/Sequence/Sequence/SequenceContext.cs`

**2. [Rule 1 - Bug] MainView.xaml.cs의 CORNER_ALIGN_CAMERA_WIDTH/HEIGHT 참조 수정**
- **Found during:** Task 2 build verification (MSBuild WPF 전처리 단계)
- **Issue:** `MainView.xaml.cs` 라인 94-95에서 `DeviceHandler.CORNER_ALIGN_CAMERA_WIDTH`, `CORNER_ALIGN_CAMERA_HEIGHT` 참조. DeviceHandler.cs 상수 rename으로 인해 빌드 에러 발생.
- **Fix:** `INSPECTION_CAMERA_WIDTH` / `INSPECTION_CAMERA_HEIGHT`로 교체. `//260326 hbk` 주석 추가.
- **Files modified:** `WPF_Example/UI/ContentItem/MainView.xaml.cs`

---

## Build Result

```
경고 27개
오류 0개
경과 시간: 00:00:02.88
```

빌드 성공 (0 errors). 27개 경고는 모두 기존 코드의 미사용 필드/버전 형식 문제 — 이번 작업과 무관.

---

## Known Stubs

없음. 모든 구현 완료.

---

## Self-Check: PASSED

- Sequence_Inspection.cs: 존재 확인
- Action_Inspection.cs: 존재 확인
- SequenceHandler.cs: `SEQ_INSPECTION` 포함 확인
- ResourceMap.cs: `Bolt_One_Inspection` 포함 확인
- ID.cs: `Bolt_One_Inspection` 포함 확인
- SystemSetting.cs: ServerPort=7701, SaveOkImage, SaveNgImage 포함 확인
- Build: 0 errors 확인
