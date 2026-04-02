# Architecture Research — FinalVision v2.0

## Existing Component Boundaries

- **SystemHandler** (singleton) — 전체 서브시스템 오케스트레이션
- **SequenceHandler** — SequenceBase > ActionBase (Action_Inspection) 파이프라인
- **ShotTabView** — Shot별 탭 UI (canvas, sliders, Grab 버튼)
- **OpenRecipeWindow** — 레시피 목록 관리 (선택/복사/삭제)
- **RecipeFileHelper** — IniFile 기반 레시피 load/save, Site{N} 경로
- **MainView** — GrabAndDisplay() 카메라 grab + 표시 파이프라인
- **Action_Inspection** — Grab → BlobDetect → SaveImage → End 상태머신

## New Components

| Component | Purpose | Key Methods |
|-----------|---------|-------------|
| RecipeEditorWindow | Shot별 파라미터 편집 팝업 | TabControl + PropertyGrid + Grab preview |
| ImageFolderManager | 이미지 저장 경로 관리 (날짜>시간 계층) | GetSavePath(), DeleteFolder(), LoadShotImages() |
| TactTimeLogger | 택타임 로그 출력 | LogActionTime(), LogSequenceTime() |

## Modified Components

| Component | Change | Reason |
|-----------|--------|--------|
| Action_Inspection.SaveResultImage | `@"D:\Log"` 하드코딩 → ImageFolderManager.GetSavePath() | 날짜>시간 폴더 구조 |
| ShotTabView | Grab/Run 버튼 역할 분리 | Grab=카메라, Run=로드 이미지 테스트 |
| OpenRecipeWindow | "Edit" 버튼 추가 → RecipeEditorWindow 팝업 | 레시피 편집 진입점 |
| RecipeFiles.Copy | Site 경로 오버로드 추가 | 복사 시 디렉터리 미생성 버그 수정 |
| SystemSetting | SaveOkImage 기본값 false | NG만 기본 저장 |

## Data Flows

### 레시피 편집
OpenRecipeWindow → Edit 버튼 → RecipeEditorWindow(mainView) → TabControl Shot 전환 → PropertyGrid.SelectedObject = InspectionParam[idx] → Grab → GrabAndDisplay → Save → SequenceHandler.SaveRecipe

### 이미지 관리
Action_Inspection.SaveResultImage → ImageFolderManager.GetSavePath(date, time) → `D:\Log\{yyyyMMdd}\{HHmmss}\{ShotName}_{OK|NG}.jpg`
이미지 로드: FolderBrowserDialog → ImageFolderManager.LoadShotImages(folderPath) → Shot1~5 매핑 → ShotTabView 표시

### 택타임 로그
SequenceBase.OnFinish → TactTimeLogger.LogActionTime(ActionContext.Timer.ElapsedMilliseconds)

## Key Findings

- `InspectionParam.CopyTo()` 이미 구현 — RecipeEditorWindow 백업/복원 즉시 활용 가능
- `ActionContext.Timer` / `SequenceContext.Timer` — Stopwatch 존재, 로그 출력만 없음
- `Action_Inspection.SaveResultImage()` — `@"D:\Log"` 하드코딩 → 교체 필요
- `RecipeFiles.Copy()` — Site 경로 무시하고 flat 경로 복사하는 버그 존재
- `ShotTabView.Btn_Grab_Click` — SimulImagePath 분기 + 카메라 Grab 혼재 → Run/Grab 분리 핵심

## Build Order (의존성 기반)

1. **레시피 복사 버그 수정** — 선행 필수, 독립
2. **인프라 유틸리티** (ImageFolderManager, TactTimeLogger) — 이후 기능이 의존
3. **이미지 저장 구조 변경** (SaveResultImage 경로) — UI 없이 테스트 가능
4. **택타임 로그 연결** — Timer 구독
5. **Run/Grab 분리 + 이미지 폴더 로드** — ShotTabView 수정
6. **이미지 삭제 UI**
7. **RecipeEditorWindow** — 모든 인프라 완성 후 마지막

## Open Questions

- `SequenceBase.MainExecute()` 내부 Timer.Start() 호출 실제 확인 필요
- RecipeEditorWindow Grab Preview: 별도 MiniCanvas vs ShotTabView 팝업 재사용 결정 필요

## Confidence: HIGH
실제 코드 전수 분석 기반
