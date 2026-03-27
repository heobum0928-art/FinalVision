---
phase: 03-teaching-simulation
verified: 2026-03-26T00:00:00Z
status: passed
score: 13/13 must-haves verified
gaps: []
human_verification:
  - test: "IsEditable=true 상태에서 빈 캔버스 좌클릭 드래그 시 노란 점선 미리보기 사각형이 화면에 그려지는지 확인"
    expected: "마우스를 드래그하는 동안 노란 점선 사각형이 캔버스에 실시간으로 표시됨"
    why_human: "WPF OnRender 런타임 렌더링은 정적 코드 분석으로 시각적 결과를 확인 불가"
  - test: "SIMUL_MODE 빌드 후 Shot 1 선택 → Grab 반복 클릭 → Shot 5까지 자동 진행 확인"
    expected: "Grab 클릭 1회마다 treeListBox에서 다음 Shot이 자동 선택됨. Shot 5 이후 멈춤."
    why_human: "SIMUL_MODE 컴파일 심볼 없이는 #if SIMUL_MODE 블록이 컴파일에서 제외됨. 런타임 동작 확인 필요."
  - test: "ComboBox에서 Shot 변경 후 원본/측정 RadioButton 전환 시 해당 이미지가 캔버스에 교체 표시되는지"
    expected: "원본 선택 시 LastOriginalImage, 측정 선택 시 LastAnnotatedImage가 캔버스에 표시됨"
    why_human: "DisplayToBackground를 통한 이미지 렌더링은 실제 Grab 실행 후 런타임에서만 확인 가능"
---

# Phase 03: Teaching Simulation Verification Report

**Phase Goal:** Teaching simulation — enable operators to set ROI via mouse drag, view per-shot original/annotated images, and simulate 5-shot inspection sequence automatically in SIMUL mode.
**Verified:** 2026-03-26
**Status:** PASSED
**Re-verification:** No — initial verification

---

## Goal Achievement

### Observable Truths

| # | Truth | Status | Evidence |
|---|-------|--------|----------|
| 1 | InspectionParam에 LastOriginalImage/LastAnnotatedImage 프로퍼티가 존재한다 | VERIFIED | Action_Inspection.cs lines 51-52: `public Mat LastOriginalImage { get; private set; }` / `public Mat LastAnnotatedImage { get; private set; }` |
| 2 | Grab 후 LastOriginalImage가 저장된다 | VERIFIED | Action_Inspection.cs line 179: `_MyParam.SetOriginalImage(_GrabbedImage);` — EStep.Grab 블록에서 GrabImage() 직후 호출 |
| 3 | BlobDetect 완료 후 LastAnnotatedImage(오버레이 포함 Mat)가 저장된다 (비SIMUL) | VERIFIED | Action_Inspection.cs line 198: `#else` 분기에서 `_MyParam.SetAnnotatedImage(annotated);` 호출 |
| 4 | SIMUL 재검사 시 LastAnnotatedImage는 갱신되지 않는다 | VERIFIED | Action_Inspection.cs lines 192-195: `#if SIMUL_MODE` 분기에서 `SetAnnotatedImageTemp(annotated)` 만 호출 — `SetAnnotatedImage` 호출 없음. `_AnnotatedImageLocked` 플래그로 추가 보호. |
| 5 | RunBlobDetection이 keypoints를 반환하고 오버레이 Mat을 생성한다 | VERIFIED | Action_Inspection.cs line 217: `private (bool isOk, Mat annotated) RunBlobDetection(...)` — Cv2.Circle(line 273) + Cv2.Rectangle(line 277) + OK/NG 색상 분기(line 265) 모두 구현 |
| 6 | IsEditable=true 상태에서 빈 캔버스 좌클릭 드래그 시 드래그 미리보기 로직이 동작한다 | VERIFIED | RuntimeResizer.cs lines 229-237: `_isDrawingNew = true` 분기, lines 549-568: OnRender에서 dashPen 점선 사각형 코드 |
| 7 | 드래그 완료(MouseUp) 시 DrawableRectangle의 ROI가 드래그 범위로 업데이트된다 | VERIFIED | RuntimeResizer.cs lines 441-477: `_isDrawingNew` 분기에서 `dr.UpdateRect(...)` + `dr.CheckAvailable(EPickerPosition.None)` 호출. DrawableRectangle.cs lines 244-255: `UpdateRect` 구현 확인 |
| 8 | IsEditable=false 또는 우클릭 드래그 시 기존 스크롤 동작이 유지된다 | VERIFIED | RuntimeResizer.cs lines 238-243: `else` 분기에서 기존 `CaptureMouse()` + `ScrollStartPos` 저장 경로 유지 |
| 9 | MainView 하단에 Shot 선택 ComboBox(Shot1~5)와 원본/측정 RadioButton이 표시된다 | VERIFIED | MainView.xaml lines 61-116: Grid Row 2에 comboBox_shot, radioButton_original, radioButton_annotated, stripBorder_Shot1~5 모두 존재 |
| 10 | ComboBox 선택 시 해당 InspectionParam의 LastOriginalImage 또는 LastAnnotatedImage가 캔버스에 표시된다 | VERIFIED | MainView.xaml.cs lines 427-433: ComboBox_shot_SelectionChanged → RefreshShotViewer(idx). Lines 467-480: RefreshShotViewer에서 `param.LastOriginalImage` / `param.LastAnnotatedImage` 분기 후 DisplayToBackground 호출 |
| 11 | 결과 스트립이 OK=초록/NG=빨강/미실행=회색으로 색상이 나타난다 | VERIFIED | MainView.xaml.cs lines 482-512: UpdateShotStrip()에서 EContextResult.Pass→Lime, Fail→Red, default→Gray 분기 |
| 12 | GrabAndDisplay 완료 후 해당 Shot ComboBox가 자동 선택된다 | VERIFIED | MainView.xaml.cs lines 303-311: `Array.IndexOf(SHOT_ACTION_NAMES, param.ActionName)` 후 `comboBox_shot.SelectedIndex = shotIdx` + `UpdateShotStrip()` 호출 |
| 13 | SIMUL 모드에서 Grab 완료 후 다음 Action으로 자동 포커스 이동한다 | VERIFIED | InspectionListView.xaml.cs lines 221-226: `#if SIMUL_MODE` 분기에서 `onComplete: () => AdvanceToNextAction()` 전달. Lines 229-278: `AdvanceToNextAction()` 메서드 구현 — SelectedItem 레퍼런스 비교로 다음 ENodeType.Action 탐색 후 treeListBox.SelectedIndex 업데이트 |

**Score: 13/13 truths verified**

---

### Required Artifacts

| Artifact | Expected | Status | Details |
|----------|----------|--------|---------|
| `WPF_Example/Custom/Sequence/Inspection/Action_Inspection.cs` | InspectionParam 이미지 버퍼 + Blob 오버레이 생성 | VERIFIED | 330 lines. LastOriginalImage, LastAnnotatedImage, SetOriginalImage, SetAnnotatedImage, SetAnnotatedImageTemp, GetAnnotatedImageTemp, ResetAnnotatedImageLock, _AnnotatedImageLocked, RunBlobDetection tuple return, Cv2.Circle, Cv2.Rectangle — 모두 존재 |
| `WPF_Example/UI/ContentItem/RuntimeResizer/RuntimeResizer.cs` | 마우스 드래그 신규 ROI 생성 | VERIFIED | 574 lines. _isDrawingNew(line 61), _drawStartPoint(line 62), OnMouseDown 분기(lines 227-243), OnMouseMove 분기(lines 253-257), OnMouseUp 분기(lines 441-477), OnRender dashPen(lines 548-568) — 모두 존재 |
| `WPF_Example/UI/ContentItem/RuntimeResizer/DrawableRectangle.cs` | UpdateRect(Rect) 메서드 | VERIFIED | 276 lines. UpdateRect(lines 244-255) — OriginalRect = newRect, UpdatePicker(), MIN_ROI_WIDTH/HEIGHT 보장 존재 |
| `WPF_Example/UI/ContentItem/MainView.xaml` | Shot 뷰어 UI 레이아웃 | VERIFIED | 121 lines. Grid Row 2(Height=Auto), comboBox_shot(line 70), radioButton_original(line 72), radioButton_annotated(line 76), stripBorder_Shot1~5(lines 84-113), UniformGrid(line 83), Tag="0"~"4" — 모두 존재 |
| `WPF_Example/UI/ContentItem/MainView.xaml.cs` | Shot 뷰어 이벤트 핸들러 | VERIFIED | SHOT_ACTION_NAMES(line 106), SHOT_DISPLAY_NAMES(line 114), _suppressShotComboEvent(line 122), GrabAndDisplay signature `Action onComplete = null`(line 260), ComboBox_shot_SelectionChanged(line 427), RadioButton_imageMode_Checked(line 435), StripBorder_MouseLeftButtonDown(line 443), GetInspectionParam(line 458), RefreshShotViewer(line 467), UpdateShotStrip(line 482), onComplete?.Invoke()(line 318) — 모두 존재 |
| `WPF_Example/UI/ControlItem/InspectionListView.xaml.cs` | SIMUL_MODE 자동 진행 | VERIFIED | #if SIMUL_MODE 분기 button_grab_Click(lines 221-226), AdvanceToNextAction() 메서드(lines 229-278), treeListBox_sequence.SelectedIndex = nextIndex(line 270), nextIndex < 0 early return(line 266) — 모두 존재 |

---

### Key Link Verification

| From | To | Via | Status | Details |
|------|----|-----|--------|---------|
| Action_Inspection.Run() EStep.Grab | InspectionParam.SetOriginalImage() | GrabImage() 반환 후 즉시 호출 | WIRED | Action_Inspection.cs line 178-179: `_GrabbedImage = _Camera.GrabImage();` 직후 `_MyParam.SetOriginalImage(_GrabbedImage);` |
| Action_Inspection.Run() EStep.BlobDetect (비SIMUL) | InspectionParam.SetAnnotatedImage() | RunBlobDetection 반환 Mat으로 호출 | WIRED | lines 190-198: var (isOk, annotated) = RunBlobDetection(...); `#else` 분기에서 `_MyParam.SetAnnotatedImage(annotated);` |
| Action_Inspection.Run() EStep.BlobDetect (SIMUL) | SetAnnotatedImageTemp | SIMUL_MODE #if 분기 경유, LastAnnotatedImage 변경 없음 | WIRED | lines 192-195: `#if SIMUL_MODE` 분기에서 `_MyParam.SetAnnotatedImageTemp(annotated);` |
| RuntimeResizer.OnMouseDown | _isDrawingNew = true | IsEditable && !IsSelected && LeftButton && DrawableList.Count > 0 | WIRED | lines 227-237: 조건 분기 정확히 구현됨 |
| RuntimeResizer.OnMouseUp | DrawableRectangle.UpdateRect / CheckAvailable | 드래그 범위 → SelectedItem 업데이트 | WIRED | lines 456-476: rw/rh > MIN_ROI 검사 후 `dr.UpdateRect(...)` + `dr.CheckAvailable(EPickerPosition.None)` |
| GrabAndDisplay() 완료 블록 | comboBox_shot.SelectedIndex 자동 선택 | param.ActionName 기반으로 Shot 인덱스 계산 | WIRED | lines 303-310: `Array.IndexOf(SHOT_ACTION_NAMES, param.ActionName)` → `comboBox_shot.SelectedIndex = shotIdx` |
| ComboBox_shot SelectionChanged | DisplayToBackground(LastOriginalImage or LastAnnotatedImage) | radioButton_original.IsChecked 분기 | WIRED | lines 427-432→467-479: SelectionChanged → RefreshShotViewer → param.LastOriginalImage/LastAnnotatedImage → DisplayToBackground |
| button_grab_Click (SIMUL) | GrabAndDisplay(camParam, onComplete: AdvanceToNextAction) | #if SIMUL_MODE 분기 | WIRED | lines 221-226: `#if SIMUL_MODE` 블록에서 콜백 전달 |
| GrabAndDisplay 완료 콜백 | InspectionListView.AdvanceToNextAction() | onComplete?.Invoke() — UpdateShotStrip() 이후, InvalidateVisual() 직후 | WIRED | lines 311-318: UpdateShotStrip() → InvalidateVisual() → `onComplete?.Invoke()` 순서 확인 |

---

### Data-Flow Trace (Level 4)

| Artifact | Data Variable | Source | Produces Real Data | Status |
|----------|---------------|--------|--------------------|--------|
| RefreshShotViewer (MainView.xaml.cs) | LastOriginalImage / LastAnnotatedImage | InspectionParam — SetOriginalImage (EStep.Grab), SetAnnotatedImage (EStep.BlobDetect 비SIMUL) | YES — SimpleBlobDetector.Detect() 결과 + GrabImage() 원본 저장 | FLOWING |
| UpdateShotStrip (MainView.xaml.cs) | EContextResult result | seq[i].Context.Result — ActionContext.Result, FinishAction() 호출 시 설정 | YES — Action_Inspection.Run() EStep.End에서 FinishAction(_IsOK ? Pass : Fail) | FLOWING |
| AdvanceToNextAction (InspectionListView.xaml.cs) | treeListBox_sequence.SelectedIndex | 실제 treeListBox_sequence.Items 컬렉션 순회 + SelectedItem 레퍼런스 비교 | YES — 런타임 TreeListBox 데이터에서 직접 읽음 | FLOWING |

---

### Behavioral Spot-Checks

Step 7b: SKIPPED — WPF 데스크탑 응용 프로그램으로 서버 실행 없이 단독 테스트 불가. 모든 핵심 동작은 런타임 UI 상호작용이 필요하므로 Human Verification으로 위임.

---

### Requirements Coverage

| Requirement | Source Plan(s) | Description | Status | Evidence |
|-------------|---------------|-------------|--------|----------|
| REQ-003 | 03-D | 5-Shot 검사 시퀀스 — Shot 위치 정보 레시피화(ROI), 순차 실행 지원 | PARTIALLY SATISFIED | Plan D의 SIMUL 자동 진행(Shot1→5 순서)이 구현됨. AdvanceToNextAction()으로 Shot 순서 자동 이동. 단, REQ-003의 전체 항목(PLC 명령 수신, 전체 NG 집계)은 다른 Phase 범위. |
| REQ-004 | 03-A, 03-D | OpenCV Blob Detection — SimpleBlobDetector 구현, Blob 오버레이 표시 | SATISFIED (Phase 범위 내) | Action_Inspection.cs: SimpleBlobDetector.Create(), Detect(), Cv2.Circle(OK=초록/NG=빨강), Cv2.Rectangle(ROI=노랑). isOk = (keypoints.Length == 1) 판정 로직. |
| REQ-007 | 03-A, 03-B, 03-C | UI 개선 — Blob 결과 오버레이 표시, 실시간 결과 표시, 레시피 편집 UI(ROI 조정) | SATISFIED (Phase 범위 내) | MainView.xaml Shot 뷰어(comboBox_shot, radioButton_original/annotated, 결과 스트립). RuntimeResizer 마우스 드래그 ROI 편집. DrawableRectangle.UpdateRect(). |
| REQ-008 | 03-B | 레시피 관리 — ROI 포함 레시피 항목 | SATISFIED (Phase 범위 내) | RuntimeResizer 드래그 완료 시 dr.CheckAvailable(EPickerPosition.None) → Param.SetRect() 호출로 ROI가 파라미터에 반영되고 레시피 저장 경로로 이어짐. |

**주의:** REQ-003, REQ-007, REQ-008의 전체 스펙(PLC 통신, Site 탭, 통계 대시보드, JSON 레시피 CRUD)은 이후 Phase에서 구현 예정. 이번 Phase에서 클레임된 항목만 위의 범위로 검증됨.

---

### Anti-Patterns Found

| File | Line | Pattern | Severity | Impact |
|------|------|---------|----------|--------|
| Action_Inspection.cs | 192-199 | `#if SIMUL_MODE` — 컴파일 타임 심볼 | Info | SIMUL_MODE 미정의 빌드에서 `SetAnnotatedImageTemp` 경로 비활성화. 의도된 설계. 영향 없음. |
| MainView.xaml.cs | 422-424 | `ComboBox_viewMode_SelectionChanged` 핸들러 본문이 비어 있음 | Warning | 기존 Phase 코드 — 이번 Phase 범위 밖. Shot 뷰어 기능에 영향 없음. |
| DrawableRectangle.cs | 260-263 | `CheckAvailable` 내부 경계 클램핑 로직 4줄이 주석 처리됨 | Warning | MAX_ROI_WIDTH/HEIGHT 경계 검사 비활성화 상태. 이번 Phase 변경 아님. ROI 이상 크기 방지 미작동 가능. 기능 차단 수준 아님. |

**Blocker anti-patterns: 없음**

---

### Human Verification Required

#### 1. 마우스 드래그 ROI 시각적 미리보기

**Test:** 앱 실행 후 InspectionListView에서 Shot을 선택하고 IsEditable=true(Teaching 모드)로 전환. MainView 캔버스 빈 공간에서 좌클릭 드래그.
**Expected:** 드래그 중 노란 점선 사각형이 실시간으로 그려짐. 마우스 버튼 해제 후 해당 범위로 ROI 사각형(파란 테두리 + Picker 핸들)이 업데이트됨.
**Why human:** WPF OnRender/MouseMove 렌더링 결과는 런타임 UI에서만 확인 가능.

#### 2. SIMUL_MODE 빌드 자동 진행

**Test:** csproj에 `<DefineConstants>SIMUL_MODE</DefineConstants>` 추가 빌드. InspectionListView에서 Shot 1 선택 후 button_grab 반복 클릭.
**Expected:** Grab 1회 완료 → Shot 2 자동 선택. Grab 반복 → Shot 3→4→5 순서 이동. Shot 5 Grab 후 더 이상 자동 이동 없음. 결과 스트립 OK=초록/NG=빨강으로 갱신.
**Why human:** SIMUL_MODE 컴파일 심볼이 프로젝트에 정의된 환경에서만 AdvanceToNextAction() 블록이 활성화됨.

#### 3. Shot 뷰어 원본/측정 이미지 전환

**Test:** SIMUL 이미지 경로 설정 후 Shot 1~5 순서로 Grab 실행. 결과 스트립에서 Shot 클릭. radioButton_original/annotated 전환.
**Expected:** 원본 선택 시 Grab 이미지(배경 없는 원본), 측정 선택 시 Blob 원(OK=초록/NG=빨강) + ROI 사각형(노랑)이 그려진 오버레이 이미지 표시.
**Why human:** 실제 VirtualCamera Grab 실행 및 DisplayToBackground 렌더링 결과는 런타임에서만 확인 가능.

---

### Gaps Summary

갭 없음. 모든 필수 구현 항목이 실제 코드베이스에서 확인됨.

4개 계획(A/B/C/D) 전체가 SUMMARY에 기록된 내용과 실제 파일 내용이 일치함:
- Plan A: Action_Inspection.cs — 이미지 버퍼 + 오버레이 생성 완전 구현
- Plan B: RuntimeResizer.cs + DrawableRectangle.cs — 드래그 ROI 생성 완전 구현
- Plan C: MainView.xaml + MainView.xaml.cs — Shot 뷰어 UI + 이벤트 핸들러 완전 구현
- Plan D: MainView.xaml.cs + InspectionListView.xaml.cs — onComplete 콜백 + SIMUL 자동 진행 완전 구현

Human Verification 3개 항목은 WPF 런타임 UI 동작 특성상 정적 코드 분석으로 대체 불가능하며, 코드 품질 문제가 아닌 시각적/실행적 확인 사항임.

---

_Verified: 2026-03-26_
_Verifier: Claude (gsd-verifier)_
