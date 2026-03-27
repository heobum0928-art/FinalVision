# Phase 6: 레시피 관리 UI — Context

**Gathered:** 2026-03-27
**Status:** Ready for planning

<domain>
## Phase Boundary

Site별 Blob 파라미터 및 카메라 파라미터 레시피를 편집하는 전용 UI를 구현한다.
5개 Shot 각각의 InspectionParam(ROI/Blob/Exposure/Gain)을 시각적으로 편집하고,
레시피 저장/불러오기/복사/초기화를 한 화면에서 처리한다.

**포함:**
- 신규 RecipeEditorWindow (팝업 Window)
- Shot별 탭 (Shot1~5) + PropertyGrid 파라미터 편집
- 캔버스 ROI 드래그 (Phase 3 RuntimeResizer 재사용)
- 카메라 파라미터 편집 (Exposure, Gain)
- 레시피 저장/불러오기/복사/초기화 버튼
- Grab 버튼으로 파라미터 변경 즉시 미리보기 검사
- CameraSlaveParam 레거시 필드 제거

**제외 (다른 Phase):**
- TCP/IP 통신 재설계 (Phase 5)
- 메인 검사 UI 전면 개편 (Phase 7)
- Site 탭 전환 UI (Phase 7)
- 통계/수율 패널 (Phase 7)

</domain>

<decisions>
## Implementation Decisions

### 주석 규칙
- 신규/변경 코드에 `//260327 hbk` 주석 유지

---

### D-01: 레거시 필드 제거 (CameraSlaveParam / CameraParam)

**결정:** ECi_Dispenser 잔재 필드 전체 제거.

제거 대상 (`CameraSlaveParam.cs` 및 `CameraParam.cs`):
- `PixelToUM_Offset` — 픽셀→마이크로미터 변환 오프셋, FinalVision 미사용
- `MotorXPos` — XY 스테이지 X 좌표, FinalVision 미사용
- `MotorYPos` — XY 스테이지 Y 좌표, FinalVision 미사용
- `PartNo` — 부품 번호, FinalVision 미사용

유지 대상:
- `LightGroupName`, `LightLevel` — 조명 제어 사용 중
- `Exposure`, `Gain` (CameraParam에서) — 레시피 편집 대상
- `FrameWidth`, `FrameHeight` — 카메라 해상도 (표시용)

제거 시 ini 저장/로드에서도 해당 키 자동 무시됨 (IniFile 기반 Save/Load는 프로퍼티 존재 여부로 동작).

---

### D-02: RecipeEditorWindow 신규 생성 (진입점)

**결정:** 별도 팝업 Window로 신규 생성.

```
진입점:
  OpenRecipeWindow → "편집" 버튼 추가 → RecipeEditorWindow.ShowDialog()
  또는 InspectionListView → "편집" 버튼 추가 (직접 진입)
```

**파일 위치:**
- `UI/Recipe/RecipeEditorWindow.xaml` (신규)
- `UI/Recipe/RecipeEditorWindow.xaml.cs` (신규)

**기존 OpenRecipeWindow:** 레시피 목록 선택 (변경 없음). 선택 후 "열기" = 로드, "편집" = RecipeEditorWindow 팝업.

---

### D-03: RecipeEditorWindow 레이아웃

**결정:** TabControl (Shot1~5) + PropertyGrid 패턴

```
[RecipeEditorWindow]
┌─────────────────────────────────────────────────────┐
│ [레시피명: ______] [저장] [불러오기] [복사] [초기화] │
├─────────────────────────────────────────────────────┤
│ [Shot1] [Shot2] [Shot3] [Shot4] [Shot5]  ← TabControl│
│ ┌─────────────────────────────────────────────────┐  │
│ │ PropertyGrid                                    │  │
│ │ ROI Setting: X, Y, W, H                        │  │
│ │ Blob: MinArea / MaxArea                         │  │
│ │ Device: Exposure / Gain                         │  │
│ └─────────────────────────────────────────────────┘  │
│ [Grab] [결과: OK/NG]                                 │
└─────────────────────────────────────────────────────┘
```

- PropertyGrid: `PropertyTools.Wpf.PropertyGrid` (기존 InspectionListView ParamEditor 패턴 그대로)
- Tab 선택 시 해당 Shot의 `InspectionParam` → PropertyGrid.SelectedObject = param

---

### D-04: ROI 편집 방식

**결정:** PropertyGrid 숫자 입력만 지원 (드래그 편집 제외).

- PropertyGrid의 `ROI` 프로퍼티(X, Y, Width, Height)를 직접 숫자 입력으로 편집
- ROI 저장: RecipeEditorWindow에서 "저장" 버튼 클릭 시 `SequenceHandler.SaveRecipe(siteNumber, recipeName)` 호출

---

### D-05: 카메라 파라미터 편집

**결정:** Exposure, Gain을 PropertyGrid에 직접 편집.

- `InspectionParam`이 `CameraSlaveParam` 상속 → Exposure, Gain 자동 PropertyGrid 표시
- `[Category("Device|Camera")]` Exposure, Gain 어노테이션 확인/추가
- 레거시 필드(D-01) 제거 후 PropertyGrid에서 불필요 항목 사라짐

---

### D-06: 레시피 저장/불러오기/복사/초기화

**결정:** 기존 `SequenceHandler` Site 오버로드 활용 (Phase 4 구현).

```csharp
// 저장
SequenceHandler.SaveRecipe(siteNumber, recipeName);

// 불러오기
OpenRecipeWindow.ShowDialog() → 선택 후 SequenceHandler.LoadRecipe(siteNumber, recipeName);

// 복사
RecipeFiles.Handle.Copy(srcName, dstName);

// 초기화
각 InspectionParam 프로퍼티를 기본값으로 리셋 → 저장
```

RecipeEditorWindow는 현재 `SystemHandler.Handle.Setting.CurrentSiteIndex` 기준으로 동작.

---

### D-07: 미리보기 검사 (실시간 파라미터 확인)

**결정:** 기존 Grab 버튼 방식 유지 (Phase 3와 동일).

- RecipeEditorWindow 내 Grab 버튼 → 현재 탭 Shot의 GrabAndDisplay 실행
- 파라미터 변경 후 Grab 재클릭으로 재검사 (자동 재검사 없음)
- SIMUL_MODE: VirtualCamera BackgroundImagePath 이미지 사용
- 결과 표시: OK=초록, NG=빨강 (기존 색상 규칙 유지)

---

### Claude's Discretion

- RecipeEditorWindow와 MainView GrabAndDisplay 연결 방식 (콜백 vs 이벤트)
- Tab 전환 시 Canvas 이미지 캐시 처리
- PropertyGrid IsReadOnly 조건 (IsEditable 플래그)
- RecipeEditorWindow 크기 및 최소/최대 사이즈

</decisions>

<canonical_refs>
## Canonical References

**Downstream agents MUST read these before planning or implementing.**

### 기존 UI 구조 (반드시 확인)
- `WPF_Example/UI/Recipe/OpenRecipeWindow.xaml.cs` — 기존 레시피 목록 창 (진입점 추가 대상)
- `WPF_Example/UI/ControlItem/InspectionListView.xaml.cs` — PropertyGrid(ParamEditor) 패턴, Grab 버튼
- `WPF_Example/UI/ControlItem/InspectionListView.xaml` — PropertyTools.Wpf.PropertyGrid 사용법
- `WPF_Example/UI/ContentItem/MainView.xaml.cs` — GrabAndDisplay(), RefreshShotViewer(), UpdateShotStrip()

### 파라미터 구조
- `WPF_Example/Custom/Sequence/Inspection/Action_Inspection.cs` — InspectionParam 클래스 정의
- `WPF_Example/Sequence/Param/CameraSlaveParam.cs` — **레거시 필드 제거 대상**
- `WPF_Example/Sequence/Param/CameraParam.cs` — **레거시 필드 제거 대상** (Copy/복사 관련)

### 레시피 저장/로드 (Phase 4 구현)
- `WPF_Example/Custom/Sequence/SequenceHandler.cs` — LoadRecipe(int, string), SaveRecipe(int, string) 오버로드
- `WPF_Example/Utility/RecipeFileHelper.cs` — RecipeFiles.Handle.Copy(), HasRecipe(), CollectRecipe(int)
- `WPF_Example/Custom/Setting/SystemSetting.cs` — CurrentSiteIndex, CurrentRecipeName

### 카메라 / 디바이스
- `WPF_Example/Custom/Device/DeviceHandler.cs` — INSPECTION_CAMERA_WIDTH/HEIGHT 상수
- `WPF_Example/Device/Camera/VirtualCamera.cs` — BackgroundImagePath (SIMUL Grab)

### 프로젝트 구조
- `.planning/phases/03-teaching-simulation/03-CONTEXT.md` — ROI 드래그, Shot 뷰어 결정사항
- `.planning/phases/04-5-site/04-CONTEXT.md` (있다면) — Site 레시피 저장 구조
- `.planning/ROADMAP.md` — Phase 6 작업 항목 및 완료 기준

</canonical_refs>

<code_context>
## Existing Code Insights

### Reusable Assets
- `PropertyTools.Wpf.PropertyGrid` (ParamEditor): InspectionListView에 이미 구현됨. `SelectedObject = param` 패턴으로 재사용
- `RuntimeResizer` + `DrawableRectangle.UpdateRect()`: Phase 3에서 ROI 드래그 구현됨. Canvas에 올려서 그대로 재사용
- `GrabAndDisplay(ICameraParam, Action onComplete)`: MainView에 구현됨. RecipeEditorWindow에서 호출 또는 인터페이스화
- `SequenceHandler.SaveRecipe(int, string)` / `LoadRecipe(int, string)`: Phase 4에서 구현됨
- `RecipeFiles.Handle.Copy()`, `HasRecipe()`: 기존 OpenRecipeWindow에서 사용 중

### Established Patterns
- PropertyGrid: `[Category("Group|SubGroup")]`, `[ReadOnly(true)]`, `[Browsable(false)]` 어노테이션으로 표시 제어
- 레시피 저장: `main.ini` IniFile 기반 (ParamBase.Save/Load)
- 에러 표시: `CustomMessageBox.Show()` 패턴
- 색상 규칙: OK=`Colors.Lime`, NG=`Colors.Red`, 미실행=`Colors.Gray`

### Integration Points
- `MainWindow.PopupView(EPageType.Recipe)` → OpenRecipeWindow → RecipeEditorWindow (연결 추가 필요)
- `SystemHandler.Handle.Sequences[ESequence.Inspection]` → 5개 Action 접근 → InspectionParam 참조
- `SystemHandler.Handle.Setting.CurrentSiteIndex` → 현재 Site 기준 저장/로드

</code_context>

<specifics>
## Specific Ideas

- RecipeEditorWindow 진입: OpenRecipeWindow에서 레시피 선택 후 "편집" 버튼
- Tab 이름: "Shot 1 (Bolt One)" ~ "Shot 5 (Assy Rail Two)" (SequenceHandler 상수 기반)
- ROI 초기화: ROI.Width==0 && ROI.Height==0 → `new Rect(0, 0, DeviceHandler.INSPECTION_CAMERA_WIDTH, DeviceHandler.INSPECTION_CAMERA_HEIGHT)`
- 레거시 필드 ini 키: 기존 main.ini에 저장된 값은 제거 후 로드 시 무시됨 (IniFile 방식)
- 초기화(Reset) 동작: 각 InspectionParam의 ROI=0, BlobMinArea=100, BlobMaxArea=50000, DelayMs=0으로 리셋

</specifics>

<deferred>
## Deferred Ideas

- Shot별 다른 시뮬레이션 이미지 (현재 공용 1장 유지)
- 레시피 파라미터 히스토리/버전 관리
- 파라미터 변경 시 자동 재검사 (현재: Grab 수동 클릭 방식 유지)
- Site 탭 전환 UI (Phase 7)
- TCP/IP 재설계 (Phase 5 — 별도 진행 예정)

</deferred>

---

*Phase: 06-ui*
*Context gathered: 2026-03-27 via discuss-phase --auto*
