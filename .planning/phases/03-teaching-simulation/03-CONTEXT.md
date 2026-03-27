# Phase 3: 티칭 UI + 시뮬레이션 자동 실행 — Context

**Gathered:** 2026-03-26
**Status:** Ready for planning

> **스코프 노트:** 로드맵 Phase 3 원제("OpenCV Blob Detection 검사 알고리즘")는
> Phase 2에서 흡수 완료. Phase 3는 **티칭 + 시뮬레이션 모드**로 재정의.

<domain>
## Phase Boundary

작업자가 파라미터를 설정(티칭)하고, TCP 없이 5-Shot을 시뮬레이션으로 자동 실행하여
결과를 확인·검증할 수 있는 UI/흐름을 구현한다.

**포함:**
- Blob 검출 결과를 캔버스에 원 오버레이로 시각화
- SIMUL 모드에서 Grab 버튼 → Shot 순차 자동 진행 (B방식)
- Shot별 원본/측정 이미지 버퍼 + ComboBox 뷰어
- Shot별 OK/NG 결과 스트립 (실검사 + 시뮬레이션 공용)
- 파라미터 변경 후 Grab 재클릭으로 재검사 (별도 버튼 없음)
- **마우스 드래그로 새 ROI 생성** (RuntimeResizer 신규 기능 — 현재 없음)

**제외 (다른 Phase):**
- 5-Site 분리 운영 (Phase 4)
- TCP 통신 재설계 (Phase 5)
- 레시피 편집 전용 UI 페이지 (Phase 6)
- 메인 UI 전면 개편 (Phase 7)

</domain>

<decisions>
## Implementation Decisions

### 주석 규칙
- 신규/변경 코드에 `//260326 hbk // 설명` 형식 주석 유지

---

### 1. Blob 오버레이 (캔버스 원 표시)

**결정:** Grab → BlobDetect 완료 시, 검출된 keypoint 위치에 원을 캔버스에 오버레이.

- 구현: `DrawableCircle` (기존 RuntimeResizer 인프라 활용) 또는 `OpenCvSharp.Cv2.Circle()` 로 Mat에 직접 그려서 표시
- 오버레이 대상: `canvas_main` (MainView의 기존 캔버스)
- `LastAnnotatedImage` = 원 오버레이가 그려진 결과 Mat (실검사 시 저장, 이후 잠금)
- 표시 방식: Claude's Discretion (RuntimeResizer IDrawableItem vs Mat 직접 그리기)

---

### 2. Simulation 모드 트리거 — **B방식**

**결정:** 별도 "Run Simulation" 버튼 없음. 기존 Grab 버튼 활용.

```
SIMUL 모드 (#define SIMULATION_MODE) 동작:
  InspectionListView에서 Action 선택 → Grab 클릭
  → 현재 Action 실행 (Grab → BlobDetect → 결과 표시)
  → 자동으로 다음 Action으로 포커스 이동
  → Grab 다시 클릭 → 다음 Shot 실행
  → Shot 5 이후 종료

비SIMUL 모드: 기존 Grab 동작 그대로 (선택된 Action만 단독 실행)
```

- `#define SIMULATION_MODE` 컴파일 상수 (기존 VirtualCamera BackgroundImagePath 방식 유지)
- 시뮬레이션 이미지: **공용 1장** (`SystemSetting.SimulImagePath`) — 5개 Shot 공용
- Grab 재클릭 = 재검사: BackgroundImagePath가 설정된 경우 동일 이미지 재사용
  → 파라미터 변경 후 Grab 재클릭만으로 재검사 가능 (별도 Test 버튼 없음)

---

### 3. Shot별 이미지 버퍼

**결정:** `InspectionParam`에 두 가지 이미지 버퍼 추가.

```csharp
public Mat LastOriginalImage { get; private set; }    // Grab 시 저장 (항상 최신 원본)
public Mat LastAnnotatedImage { get; private set; }   // 검사 완료 시 저장 (잠금)
```

- `LastOriginalImage`: Grab 할 때마다 덮어쓰기 (VirtualCamera 재사용 시 동일 이미지)
- `LastAnnotatedImage`: 실검사(BlobDetect) 완료 시 1회 저장, 이후 변경 금지
  - SIMUL 재검사(파라미터 튜닝) 시에는 업데이트 안 함 — 캔버스 오버레이만 갱신
  - 실운영 TCP TEST 커맨드 실행 시에만 갱신

---

### 4. Shot 뷰어 UI (MainView.xaml 추가)

**결정:** 기존 MainView.xaml에 요소 추가. 별도 탭 페이지 없음.

```
[ComboBox: Shot1 ▼] [○ 원본] [○ 측정]   ← Shot 선택 + 이미지 종류 토글
[Canvas — 선택된 Shot 이미지 표시]
[결과 스트립]
  [Shot1: OK▌] [Shot2: NG▌] [Shot3: OK▌] [Shot4: OK▌] [Shot5: OK▌]
[label_message]
```

**ComboBox 동작:**
- 항목: Shot 1 (Bolt_One) ~ Shot 5 (Assy_Rail_Two) — SequenceHandler 상수 기반
- 선택 변경 시 → 해당 Action의 InspectionParam에서 이미지 로드 → 캔버스 표시
- 원본/측정 라디오 토글 → `LastOriginalImage` or `LastAnnotatedImage` 표시

**결과 스트립:**
- 항상 표시 (SIMUL + 실운영 공용)
- 각 Shot: Border + Label, OK=초록, NG=빨강, 미실행=회색
- Shot 클릭 → ComboBox 연동 (해당 Shot으로 전환)

**연동 없는 상태:**
- GrabAndDisplay 완료 → 해당 Shot ComboBox 자동 선택
- 검사 미완료 Shot은 결과 스트립 회색 유지

---

### 5. 재검사 (파라미터 튜닝)

**결정:** 별도 버튼 없음 — 기존 Grab 버튼이 재검사.

```
작업자 워크플로우:
  1. InspectionListView에서 Shot N 선택
  2. DeviceSelector → 이미지 디렉토리 로드 (BackgroundImagePath 설정)
  3. InspectionParam에서 BlobMinArea/MaxArea 조정
  4. Grab 클릭 → 동일 원본 이미지 재획득 → Blob 재검출
  5. 캔버스 오버레이 갱신 확인
  6. 만족 시 레시피 저장
```

---

### 6. ROI 마우스 드래그 신규 생성

**결정:** 빈 캔버스 영역에서 마우스 드래그로 새 ROI를 그릴 수 있어야 한다.

**현재 상태 (확인 완료):**
- `RuntimeResizer.OnMouseDown`: `!IsSelected` 상태에서 드래그 → 이미지 스크롤만 됨
- ROI 이동/리사이즈(8방향 핸들)만 가능, 신규 생성 없음

**추가 구현:**
```
RuntimeResizer.OnMouseDown (IsEditable && !IsSelected && 좌클릭):
  → _isDrawingNew = true, _drawStartPoint = 마우스 좌표

OnMouseMove (_isDrawingNew):
  → 미리보기 사각형 그리기 (점선 또는 반투명)

OnMouseUp (_isDrawingNew):
  → 드래그 범위 → Rect 계산
  → SelectedItem(DrawableRectangle)의 Rect 업데이트
  → param.ROI = 계산된 Rect → InvalidateVisual
  → _isDrawingNew = false
```

- 대상: `RuntimeResizer.cs` + `DrawableRectangle` Rect 업데이트
- 기존 스크롤: 우클릭 드래그 또는 IsEditable=false 일 때만 동작
- 주석: `//260326 hbk // ROI 마우스 드래그 신규 생성`

---

### Claude's Discretion

- Blob 오버레이 구현 방식: RuntimeResizer `IDrawableItem` vs OpenCvSharp `Cv2.Circle()` Mat 직접 그리기
- 결과 스트립 WPF 레이아웃 구체 구현 (WrapPanel vs UniformGrid vs StackPanel)
- ComboBox 항목 바인딩 방식 (정적 리스트 vs SequenceHandler 동적 조회)
- `LastAnnotatedImage` 잠금 메커니즘 (플래그 vs private setter)
- SIMULATION_MODE #define 배치 위치 (Preprocessor.cs, App.xaml.cs, 또는 기존 위치)

</decisions>

<canonical_refs>
## Canonical References

**Downstream agents MUST read these before planning or implementing.**

### 기존 UI 구조 (반드시 확인)
- `WPF_Example/UI/ContentItem/MainView.xaml.cs` — GrabAndDisplay, canvas_main, label_message
- `WPF_Example/UI/ControlItem/InspectionListView.xaml.cs` — Grab 버튼, Action 선택, IsEditable
- `WPF_Example/UI/ContentItem/RuntimeResizer/DrawableRectangle.cs` — IDrawableItem 패턴
- `WPF_Example/UI/ContentItem/RuntimeResizer/DrawableCircle.cs` — 원 오버레이 (존재 여부 확인)
- `WPF_Example/UI/ContentItem/RuntimeResizer/RuntimeResizer.cs` — OnMouseDown/Move/Up 패턴 (ROI 드래그 신규 생성 추가 대상)

### Phase 2 구현 파일 (티칭/시뮬 대상)
- `WPF_Example/Custom/Sequence/Inspection/Action_Inspection.cs` — InspectionParam, BlobDetect, EStep
- `WPF_Example/Custom/Sequence/Inspection/Sequence_Inspection.cs` — Sequence 구조
- `WPF_Example/Custom/Sequence/SequenceHandler.cs` — ACT_BOLT_ONE ~ ACT_ASSY_TWO 상수
- `WPF_Example/Custom/Device/DeviceHandler.cs` — INSPECTION_CAMERA, GrabImage()

### 카메라 / 시뮬레이션
- `WPF_Example/Device/Camera/VirtualCamera.cs` — BackgroundImagePath, GrabImage()
- `WPF_Example/Custom/SystemSetting.cs` — SimulImagePath, SaveOkImage, SaveNgImage

### DeviceSelector (티칭 이미지 로드)
- `WPF_Example/UI/Device/DeviceSelector.xaml.cs` — MenuItem_LoadImage_Click, BackgroundImagePath

### 프로토콜 / 프로젝트
- `.planning/phases/02-hik-5-shot/02-CONTEXT.md` — Phase 2 확정 결정사항
- `.planning/ROADMAP.md` — 전체 Phase 구조

</canonical_refs>

<specifics>
## Specific Ideas

- ComboBox 항목 이름: "Shot 1 (Bolt One)" ~ "Shot 5 (Assy Rail Two)" (SequenceHandler 상수 기반)
- 결과 스트립: OK=`Colors.Lime`, NG=`Colors.Red`, 미실행=`Colors.Gray` (기존 label_message 색상 규칙 통일)
- SIMUL B방식: Grab 완료 후 `InspectionListView.treeListBox_sequence.SelectedIndex++` 또는 NextAction 포인터
- Blob 오버레이 원: keypoint.Size = 검출된 Blob 반지름, 색상 = OK:초록, NG:빨강
- LastAnnotatedImage 잠금: 실운영 시(`!SIMULATION_MODE`) BlobDetect 후 저장, SIMUL 재검사 시 저장 안 함

</specifics>

<deferred>
## Deferred Ideas

- Shot별 다른 시뮬레이션 이미지 (공용 1장 유지, 향후 테스트 강화 시)
- 결과 이력 테이블 (최근 N건 검사 결과 리스트) — Phase 7 Main UI
- 5개 Shot 이미지 동시 표시 패널 — Phase 7
- Site 탭 (Site 1~5 전환) — Phase 4

</deferred>

---

*Phase: 03-teaching-simulation*
*Context gathered: 2026-03-26 via discuss-phase*
