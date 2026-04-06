# Phase 13: RecipeEditorWindow - Context

**Gathered:** 2026-04-06
**Status:** Ready for planning

<domain>
## Phase Boundary

파라미터 편집 후 Save 전에 로드 시점으로 되돌리는 Reset 기능만 구현한다.

**범위 축소:** 원래 Phase 13은 RecipeEditorWindow 신규 창(RCP-02~RCP-06) 전체였으나, 사용자 결정으로 Reset 기능만 포함. RecipeEditorWindow 팝업, Edit 버튼, Grab 미리보기는 이번 Phase에서 제외.

- RCP-05 (부분): 파라미터를 편집 전 상태로 초기화하는 Reset 기능

</domain>

<decisions>
## Implementation Decisions

### 백업 시점
- **D-01:** 레시피 로드 시(LoadRecipe 시점) 자동 백업 — 편집 시작 전 상태를 메모리에 보관

### Reset 버튼 위치
- **D-02:** InspectionListView 툴바에 Reset 버튼 추가 — Save/Copy/Paste 버튼과 같은 영역에 배치하여 기존 패턴과 일관성 유지

### Reset 범위
- **D-03:** 트리에서 현재 선택된 Shot의 파라미터만 로드 시점 백업본으로 복원 (전체 Shot 일괄 아님)

### Claude's Discretion
- 백업 데이터 구조 (Dictionary, Clone 등) 구현 방식
- Reset 버튼 아이콘/텍스트 선택
- Reset 후 PropertyGrid 갱신 방식
- Reset 시 확인 다이얼로그 필요 여부

</decisions>

<canonical_refs>
## Canonical References

**Downstream agents MUST read these before planning or implementing.**

### 레시피 로드/저장
- `WPF_Example/Custom/Sequence/Inspection/InspectionRecipeManager.cs` — Shot-FAI 2계층 INI Save/Load 구조
- `WPF_Example/Custom/Sequence/Inspection/ShotConfig.cs` — ShotConfig 파라미터 (CameraSlaveParam 상속, CopyTo 메서드)
- `WPF_Example/Sequence/Param/ParamBase.cs` — ParamBase 반사 직렬화, CopyTo 패턴

### InspectionListView (Reset 버튼 추가 대상)
- `WPF_Example/UI/ControlItem/InspectionListView.xaml` — 툴바 구조 (Copy/Paste 버튼 위치: 라인 222~237)
- `WPF_Example/UI/ControlItem/InspectionListView.xaml.cs` — button_copy_Click/button_paste_Click 패턴

### PropertyGrid 편집
- `WPF_Example/UI/ControlItem/InspectionListView.xaml` — ParamEditor PropertyGrid (라인 240~246, SelectedObject 바인딩)

### 레시피 로드 흐름
- `WPF_Example/SystemHandler.cs` — ProcessRecipeChange() 레시피 로드 진입점
- `WPF_Example/Sequence/SequenceHandler.cs` — LoadRecipe() 시퀀스 레시피 로드

</canonical_refs>

<code_context>
## Existing Code Insights

### Reusable Assets
- `ShotConfig.CopyTo(ParamBase)` — Shot 파라미터 복사 메서드, 백업/복원 시 활용 가능
- `ParamBase` 반사 직렬화 — INI Save/Load 패턴으로 백업 대안 가능
- `InspectionRecipeManager.Shots` — Shot 목록 접근, 백업 대상

### Established Patterns
- 툴바 버튼: `<ToolBar>` 내 `<Button>` + `<StackPanel>` (아이콘 + 텍스트) 패턴
- 파라미터 복사: `CopyTo()` 메서드 체인 (CameraSlaveParam → ShotConfig)
- 주석: `//YYMMDD hbk` 형식

### Integration Points
- `InspectionListView.xaml` 툴바: Reset 버튼 추가 위치
- `SequenceHandler.LoadRecipe()` 또는 `InspectionRecipeManager.Load()`: 백업 수행 시점
- `PropertyGrid.SelectedObject`: Reset 후 갱신 트리거

</code_context>

<specifics>
## Specific Ideas

- 사용자가 RecipeEditorWindow 전체 범위를 Reset 기능만으로 축소 결정
- "저장 전 되돌리기" 용도 — 레시피 버전관리(저장 후 되돌리기)는 Out of Scope

</specifics>

<deferred>
## Deferred Ideas

- RecipeEditorWindow 신규 팝업 창 (RCP-02, RCP-06) — 별도 Phase로 분리 필요
- Grab 미리보기 (RCP-03) — RecipeEditorWindow와 함께 별도 Phase
- Save 버튼 별도 구현 (RCP-04) — 현재 기존 Save 흐름 사용

</deferred>

---

*Phase: 13-recipeeditorwindow*
*Context gathered: 2026-04-06*
