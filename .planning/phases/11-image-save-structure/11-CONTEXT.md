# Phase 11: 이미지 저장 구조 개선 - Context

**Gathered:** 2026-04-03
**Status:** Ready for planning

<domain>
## Phase Boundary

검사 이미지가 날짜>시간 하위폴더 계층으로 저장되고, NG만 기본 저장된다. 검사 1회(5 Shot)가 1개 시간폴더에 묶여 저장되며, 원본+Annotated 이미지를 모두 저장한다.

- IMG-01: 검사 이미지를 날짜>시간 하위폴더 구조로 저장
- IMG-02: OK 이미지 기본 미저장, NG 이미지만 기본 저장 (설정에서 변경 가능)

</domain>

<decisions>
## Implementation Decisions

### 저장 경로 구조
- **D-01:** 기본 경로를 `SystemSetting.ImageSavePath`로 변경. 기본값을 `D:\Log`로 설정. PropertyGrid에서 변경 가능
- **D-02:** 시간폴더 단위는 검사 1회 = 1폴더. TCP $TEST 수신 시점에 시간폴더를 생성하고 Shot1~5 이미지가 같은 폴더에 저장
- **D-03:** 최종 경로 형식: `{ImageSavePath}\{yyyyMMdd}\{HHmmss_fff}\{ShotName}_{OK|NG}.jpg`
- **D-04:** Annotated 이미지 파일명: `{ShotName}_{OK|NG}_annotated.jpg` (같은 폴더에 저장)

### OK/NG 저장 정책
- **D-05:** 기존 `SaveOkImage=false`, `SaveNgImage=true` 동작 유지
- **D-06:** 원본(GrabbedImage) + Annotated(측정 오버레이) 둘 다 저장. 현재 원본 저장 로직 유지하고 Annotated 저장을 추가
- **D-07:** OK/NG 필터는 원본/Annotated 쌍 단위로 적용 (OK 미저장 시 원본+Annotated 모두 미저장)

### 동시성/충돌 방지
- **D-08:** 시간폴더명에 밀리초 포함 (`HHmmss_fff`). 동일 밀리초 충돌 시 `_2`, `_3` 접미사 추가
- **D-09:** 검사 1회 시작 시점에 폴더명을 확정하고 Shot1~5 전체가 같은 폴더명 사용

### ImageFolderManager 설계
- **D-10:** `WPF_Example/Utility/ImageFolderManager.cs` 신규 생성
- **D-11:** 범위는 경로 생성만 담당 — `GetSavePath()` 메서드 제공. 디스크 정리/조회는 Phase 12에서 추가
- **D-12:** `D:\Log` 하드코딩을 `SystemSetting.ImageSavePath` 참조로 교체

### Claude's Discretion
- ImageFolderManager 내부 메서드 시그니처 및 충돌 방지 구현 세부
- Action_Inspection.SaveResultImage()에서 Annotated 저장 추가 방식
- 검사 시작 시점에 폴더명을 전달하는 메커니즘 (SequenceContext 활용 등)

</decisions>

<canonical_refs>
## Canonical References

**Downstream agents MUST read these before planning or implementing.**

### 이미지 저장 (현재 구현)
- `WPF_Example/Custom/Sequence/Inspection/Action_Inspection.cs` — SaveResultImage() 라인 458~481, EStep.SaveImage 라인 266~269
- `WPF_Example/Sequence/Sequence/SequenceBase.cs` — SaveResultImage() 라인 341~364 (레거시 저장 로직)

### 설정
- `WPF_Example/Setting/SystemSetting.cs` — ImageSavePath(라인 60), SaveOkImage/SaveNgImage(라인 95~96), GetResultImageSavePath(라인 142~148)

### 유틸리티 위치
- `WPF_Example/Utility/RecipeFileHelper.cs` — 같은 Utility 폴더에 ImageFolderManager 배치 예정

### 시퀀스 컨텍스트
- `WPF_Example/Sequence/Sequence/SequenceContext.cs` — ResultImage, Timer 등 검사 컨텍스트

</canonical_refs>

<code_context>
## Existing Code Insights

### Reusable Assets
- `SystemSetting.ImageSavePath` — 기존 설정 프로퍼티, 기본값만 `D:\Log`로 변경하면 됨
- `SystemSetting.SaveOkImage/SaveNgImage` — OK/NG 필터 이미 구현됨
- `SystemSetting.GetLogSavePath(ELogType.Image, ...)` — 기존 경로 조합 로직 (참고용)
- `Action_Inspection._GrabbedImage` — 원본 이미지 참조
- `InspectionParam.LastAnnotatedImage` / `GetAnnotatedImageTemp()` — Annotated 이미지 접근

### Established Patterns
- 로그/이미지 저장: `Directory.CreateDirectory()` 후 `image.SaveImage(filePath)` (OpenCvSharp Mat)
- 비동기 저장: `Task.Factory.StartNew()` + `Clone()` (SequenceBase 패턴, 스레드 안전)
- 주석: `//YYMMDD hbk` 형식

### Integration Points
- `Action_Inspection.SaveResultImage()` — ImageFolderManager.GetSavePath() 호출로 교체
- `Action_Inspection.Run()` EStep.SaveImage — 경로 생성 시점
- `SystemSetting.ImageSavePath` 기본값 변경 (`AppDomain.CurrentDomain.BaseDirectory + "Image"` → `@"D:\Log"`)

</code_context>

<specifics>
## Specific Ideas

- 검사 1회 시작 시 ImageFolderManager가 폴더 경로를 생성하고, Shot1~5가 모두 같은 경로에 저장되어야 Phase 12에서 폴더 단위 로드/삭제가 자연스러움
- Annotated 이미지는 `_annotated` 접미사로 구분하여 같은 폴더에 저장

</specifics>

<deferred>
## Deferred Ideas

- 디스크 정리 (오래된 이미지 자동 삭제) — Phase 12에서 ImageFolderManager에 추가
- 이미지 폴더 조회/로드 UI — Phase 12 범위
- 이미지 삭제 UI — Phase 12 범위

</deferred>

---

*Phase: 11-image-save-structure*
*Context gathered: 2026-04-03*
