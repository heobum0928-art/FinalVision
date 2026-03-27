# Phase 2 Plan: HIK 카메라 단일화 및 5-Shot 시퀀스 구조

## 목표
HIK 카메라 1대로 5개 포지션을 순차 촬상하는 검사 시퀀스 구현

## 기반 코드 분석
- 기존 `CornerAlignSequence` → `InspectionSequence` 로 교체
- 기존 4개 Corner Action → 5-Shot Action으로 교체
- 카메라 이름: `CORNER_ALIGN_CAMERA` → `INSPECTION_CAMERA`

---

## Task 1: 카메라 상수 및 등록 업데이트
**파일**: `Custom/Device/DeviceHandler.cs`
- `CORNER_ALIGN_CAMERA` → `INSPECTION_CAMERA`
- 카메라 해상도 상수 HIK 카메라 사양에 맞게 조정 (기존 유지 가능)

---

## Task 2: ShotConfig 모델 생성
**파일**: `Custom/Sequence/Inspection/ShotConfig.cs` (신규)

```csharp
public class ShotConfig {
    public int ShotIndex;       // 1~5
    public int GrabDelayMs;     // 촬상 전 딜레이 (ms)
    public Rect ROI;            // 검사 관심 영역
}
```

---

## Task 3: InspectionSequence 생성
**파일**: `Custom/Sequence/Inspection/Sequence_Inspection.cs` (신규)
- 기존 `CornerAlignSequence` 구조 참고하여 작성
- Shot 1→5 순차 실행 구조
- `InspectionSequenceContext`: 5개 Shot 결과 보관

---

## Task 4: Action_Grab 생성 (Shot 1개 단위)
**파일**: `Custom/Sequence/Inspection/Action_Grab.cs` (신규)
- 기존 `CornerAlignInspectionAction` 구조 참고
- Step: Delay → Grab → Done
- HIK 카메라 SoftwareTrigger 사용
- 결과: `Mat GrabbedImage` 보관
- SIMUL_MODE: VirtualCamera 이미지 사용

---

## Task 5: SequenceHandler 업데이트
**파일**: `Custom/Sequence/SequenceHandler.cs`
- `RegisterSequences()`: CornerAlignSequence → InspectionSequence 교체
- `RegisterActions()`: 5개 Grab Action 등록 (Shot1~5)
- 기존 Corner 상수 → Inspection 상수로 교체

---

## Task 6: Custom/Device/DeviceHandler.cs 카메라 등록 확인
**파일**: `Custom/SystemHandler.cs` 내 카메라 등록 부분
- `SetRequiredDevice()` 호출 시 HIK 타입, INSPECTION_CAMERA 이름으로 등록 확인

---

## 완료 기준
- [ ] `InspectionSequence`, `Action_Grab` 클래스 생성 완료
- [ ] Shot 1~5 순차 실행 구조 작동
- [ ] SIMUL_MODE 에서 빌드 및 실행 성공
- [ ] 기존 Corner 시퀀스 코드는 유지 (삭제 X, 추후 정리)
