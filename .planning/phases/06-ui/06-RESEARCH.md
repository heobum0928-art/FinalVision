# Phase 6: 레시피 관리 UI — Research

**Researched:** 2026-03-27
**Domain:** WPF/XAML — RecipeEditorWindow (팝업), PropertyTools.Wpf.PropertyGrid, RuntimeResizer ROI 드래그, SequenceHandler 레시피 저장/로드
**Confidence:** HIGH (기존 코드 전체 확인 완료)

---

<user_constraints>
## User Constraints (from CONTEXT.md)

### Locked Decisions

- **D-01**: CameraSlaveParam / CameraParam 레거시 필드 전체 제거
  - 제거 대상: `PixelToUM_Offset`, `MotorXPos`, `MotorYPos`, `PartNo`
  - 유지: `LightGroupName`, `LightLevel`, `FrameWidth`, `FrameHeight`
  - `CopyTo()` 메서드 내 해당 필드 참조도 함께 제거

- **D-02**: RecipeEditorWindow 신규 팝업 Window (`UI/Recipe/RecipeEditorWindow.xaml` + `.xaml.cs`)
  - 진입점: OpenRecipeWindow 에 "편집" 버튼 추가 → `RecipeEditorWindow.ShowDialog()`

- **D-03**: 레이아웃 — TabControl (Shot1~5) + PropertyGrid 좌측 + Canvas(이미지+ROI) 우측
  - PropertyGrid: `PropertyTools.Wpf.PropertyGrid` (기존 InspectionListView 패턴)
  - Canvas: `RuntimeResizer` + `DrawableRectangle` (Phase 3 구현 재사용)

- **D-04**: ROI 편집 — PropertyGrid 숫자 입력 + 캔버스 드래그 둘 다 지원
  - ROI=0,0,0,0 기본값 시 `new Rect(0, 0, INSPECTION_CAMERA_WIDTH, INSPECTION_CAMERA_HEIGHT)` 초기화

- **D-05**: 카메라 파라미터(Exposure, Gain) — PropertyGrid 직접 편집
  - `InspectionParam` → `CameraSlaveParam` 상속이므로 자동 표시
  - `[Category("Device|Camera")]` 어노테이션 확인/추가

- **D-06**: 레시피 저장/불러오기/복사/초기화 — 기존 SequenceHandler Site 오버로드 활용
  - `SequenceHandler.SaveRecipe(siteNumber, recipeName)` / `LoadRecipe(siteNumber, recipeName)`
  - 복사: `RecipeFiles.Handle.Copy(srcName, dstName)`
  - 초기화: ROI=0, BlobMinArea=100, BlobMaxArea=50000, DelayMs=0 리셋 후 저장

- **D-07**: 미리보기 검사 — Grab 버튼 수동 클릭 방식 (자동 재검사 없음)
  - SIMUL_MODE: VirtualCamera BackgroundImagePath 이미지 사용
  - 결과: OK=Colors.Lime, NG=Colors.Red

- **주석 규칙**: 신규/변경 코드에 `//260327 hbk` 주석 유지

### Claude's Discretion

- RecipeEditorWindow와 MainView GrabAndDisplay 연결 방식 (콜백 vs 이벤트)
- Tab 전환 시 Canvas 이미지 캐시 처리
- PropertyGrid IsReadOnly 조건 (IsEditable 플래그)
- RecipeEditorWindow 크기 및 최소/최대 사이즈

### Deferred Ideas (OUT OF SCOPE)

- Shot별 다른 시뮬레이션 이미지 (현재 공용 1장 유지)
- 레시피 파라미터 히스토리/버전 관리
- 파라미터 변경 시 자동 재검사
- Site 탭 전환 UI (Phase 7)
- TCP/IP 재설계 (Phase 5)
</user_constraints>

---

## Summary

Phase 6는 신규 파일 생성 중심의 UI 구현 Phase다. 백엔드(SequenceHandler Site 오버로드, RecipeFiles, SiteManager, InspectionParam)는 Phase 3~4에서 모두 완성됐고, Phase 6는 이를 연결하는 편집 UI 레이어만 추가한다.

핵심 재사용 자산이 3개 있다. `PropertyTools.Wpf.PropertyGrid` 는 InspectionListView에서 `SelectedObject = param` 패턴으로 이미 동작 검증됨. `RuntimeResizer` + `DrawableRectangle.UpdateRect()` 는 Phase 3에서 ROI 드래그 로직이 완성됨. `GrabAndDisplay(ICameraParam, Action onComplete)` 는 MainView에 구현되어 있으며 콜백 패턴을 이미 지원함.

레거시 필드 제거(D-01)는 `CameraSlaveParam.cs`와 `CameraParam.cs` 두 파일에 걸쳐 있으며, `CopyTo()` 메서드 내 참조도 함께 제거해야 컴파일 오류가 없다. RecipeEditorWindow는 `MainView.GrabAndDisplay` 를 직접 참조할 수 없으므로 `mParentWindow.mainView.GrabAndDisplay()` 패턴 또는 전달받은 `Action<ICameraParam>` 콜백으로 연결해야 한다.

**Primary recommendation:** OpenRecipeWindow에 "편집" 버튼 추가 → RecipeEditorWindow.ShowDialog() 진입점을 먼저 구축하고, 레거시 필드 제거 → PropertyGrid/Canvas/Grab 순서로 구현한다.

---

## Standard Stack

### Core (이미 프로젝트에 설치됨)

| Library | 역할 | 근거 |
|---------|------|------|
| `PropertyTools.Wpf` (PropertyGrid) | Shot별 파라미터 편집 | InspectionListView.xaml 이미 사용 중. `SelectedObject = param` 패턴 검증됨 |
| WPF TabControl | Shot1~5 탭 | 표준 WPF — 외부 의존성 없음 |
| `RuntimeResizer` (내부 클래스) | ROI 드래그 캔버스 | Phase 3 구현 완료. `SetParam(ParamBase)` → `IsEditable=true` 조합 |
| `SequenceHandler.SaveRecipe/LoadRecipe(int, string)` | 레시피 저장/로드 | Phase 4 구현 완료 (`WPF_Example/Sequence/SequenceHandler.cs` line 180, 207) |
| `RecipeFiles.Handle.Copy(src, dst)` | 레시피 복사 | `RecipeFileHelper.cs` 기존 구현 |

**외부 패키지 추가 불필요** — 모든 의존성이 이미 프로젝트에 포함됨.

---

## Architecture Patterns

### 신규 파일 구조

```
WPF_Example/
└── UI/
    └── Recipe/
        ├── OpenRecipeWindow.xaml.cs      (기존 — "편집" 버튼 추가만)
        ├── RecipeEditorWindow.xaml        (신규)
        └── RecipeEditorWindow.xaml.cs     (신규)
```

### Pattern 1: RecipeEditorWindow 생성자 패턴

RecipeEditorWindow는 현재 Site 기준으로 InspectionParam 5개를 가져온다. MainView 참조를 생성자 인수로 받아 GrabAndDisplay를 직접 호출하는 것이 가장 단순하다 — 이벤트/콜백 추상화 불필요.

```csharp
// Source: 기존 OpenRecipeWindow.xaml.cs 패턴 응용
public RecipeEditorWindow(MainView mainView) {
    InitializeComponent();
    _mainView = mainView;
    _siteNumber = SystemHandler.Handle.Setting.CurrentSiteIndex;
    // Tab 초기화: Shot0~4의 InspectionParam 로드
    SequenceBase seq = SystemHandler.Handle.Sequences[ESequence.Inspection];
    for (int i = 0; i < 5; i++) {
        _params[i] = seq[i].Param as InspectionParam;
    }
    LoadParamsForCurrentTab();
}
```

### Pattern 2: TabControl 탭 전환 → PropertyGrid + Canvas 갱신

```csharp
private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e) {
    int idx = tabControl_shot.SelectedIndex; //260327 hbk
    if (idx < 0) return;
    InspectionParam param = _params[idx];
    // ROI=0,0,0,0 초기화
    if (param.ROI.Width == 0 && param.ROI.Height == 0) { //260327 hbk
        param.ROI = new Rect(0, 0, DeviceHandler.INSPECTION_CAMERA_WIDTH,
                             DeviceHandler.INSPECTION_CAMERA_HEIGHT);
    }
    ParamEditor.SelectedObject = param; //260327 hbk — PropertyGrid 갱신
    canvas_roi.SetParam(param);         //260327 hbk — RuntimeResizer 갱신
    canvas_roi.IsEditable = true;
}
```

### Pattern 3: PropertyGrid 어노테이션 (기존 패턴)

```csharp
// Source: Action_Inspection.cs / CameraSlaveParam.cs 기존 패턴
[Category("ROI Setting")]
[Rectangle, Converter(typeof(UI.RectConverter))]
public System.Windows.Rect ROI { get; set; }

[Category("Blob")]
public double BlobMinArea { get; set; } = 100;
public double BlobMaxArea { get; set; } = 50000;

[Category("Device|Camera")]
// Exposure, Gain — CameraSlaveParam의 PropertyArray 통해 자동 표시됨
```

### Pattern 4: Grab 버튼 → GrabAndDisplay 호출

```csharp
// Source: InspectionListView.xaml.cs button_grab_Click 패턴 응용
private void Button_grab_Click(object sender, RoutedEventArgs e) { //260327 hbk
    int idx = tabControl_shot.SelectedIndex;
    if (idx < 0) return;
    ICameraParam camParam = _params[idx] as ICameraParam;
    if (camParam == null) return;
    if (SystemHandler.Handle.Sequences.IsIdle == false) return;
#if SIMUL_MODE
    _mainView.GrabAndDisplay(camParam, onComplete: () => {
        // Grab 완료 후 결과 레이블 갱신
        UpdateResultLabel(idx);
    });
#else
    _mainView.GrabAndDisplay(camParam);
#endif
}
```

### Pattern 5: 레거시 필드 제거 (D-01)

`CameraSlaveParam.cs` 에서 제거:
- 프로퍼티: `PixelToUM_Offset`, `MotorXPos`, `MotorYPos`, `PartNo`
- `ConvertPixelToMM()` 메서드 (PixelToUM_Offset 사용)
- `CopyTo()` 내 `slaveParam.PartNo`, `slaveParam.MotorXPos`, `slaveParam.MotorYPos`, `slaveParam.PixelToUM_Offset` 대입 라인

`CameraParam.cs` 에서 제거:
- 프로퍼티: `PixelToUM_Offset`, `MotorXPos`, `MotorYPos`, `PartNo`
- `CopyTo()` 내 대응 라인

**주의:** `CopyTo()` 양쪽 클래스 모두 대칭으로 제거해야 컴파일 오류가 없음.

### Anti-Patterns to Avoid

- **PropertyGrid SelectedObject를 null로 설정 후 재설정하지 않기**: Tab 전환 시 null로 클리어하면 일부 PropertyTools 버전에서 예외 발생. `SelectedObject = param` 직접 교체.
- **Canvas.IsEditable = true를 RecipeEditorWindow 생성자에서 전역 설정하지 않기**: Shot별 Tab 전환 시점에 SetParam() 이후 설정해야 DrawableList가 올바르게 구성됨.
- **GrabTask 중복 실행**: `GrabAndDisplay` 내부에 이미 `GrabTask != null` 가드가 있으나, 버튼을 비활성화하거나 IsIdle 체크를 통해 중복 클릭 방지 필요.
- **ROI 초기화를 매 Tab 전환마다 수행하지 않기**: ROI가 이미 사용자 설정된 경우 덮어쓰면 안 됨. `Width == 0 && Height == 0` 조건 체크 후에만 초기화.

---

## Don't Hand-Roll

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| Shot 파라미터 편집 UI | 커스텀 슬라이더/입력 폼 | `PropertyTools.Wpf.PropertyGrid` | 이미 프로젝트에서 검증됨. Category 어노테이션으로 그룹화 자동 처리 |
| ROI 드래그 | 마우스 이벤트 직접 구현 | `RuntimeResizer` + `DrawableRectangle.UpdateRect()` | Phase 3에서 완성. 스케일 변환, 최소 크기 검증, 점선 미리보기 포함 |
| 레시피 저장/로드 | 직접 IniFile 접근 | `SequenceHandler.SaveRecipe(int, string)` / `LoadRecipe(int, string)` | Phase 4에서 Site 경로 처리 완료 |
| 레시피 복사 | 파일 시스템 직접 조작 | `RecipeFiles.Handle.Copy(srcName, dstName)` | 디렉터리 재귀 복사, forceCopy 옵션 포함 |
| Grab + Blob 실행 | Task + OpenCvSharp 직접 | `MainView.GrabAndDisplay(ICameraParam, Action)` | Thread-safe, Dispatcher 처리, SIMUL_MODE 분기 포함 |

---

## Runtime State Inventory

> 이 Phase는 레거시 필드 제거(rename/refactor 성격)를 포함하므로 런타임 상태 확인이 필요.

| Category | Items Found | Action Required |
|----------|-------------|------------------|
| Stored data | `main.ini` 레시피 파일에 `PixelToUM_Offset`, `MotorXPos`, `MotorYPos`, `PartNo` 키가 저장되어 있을 수 있음 | 코드 변경만 — IniFile은 프로퍼티 존재 여부로 동작하므로 로드 시 미존재 키 자동 무시. 기존 ini 파일 편집 불필요 |
| Live service config | 없음 — TCP/IP 통신은 Phase 5 대상, 현재 레시피 편집과 무관 | 없음 |
| OS-registered state | 없음 — 스케줄러/서비스 등록 없음 | 없음 |
| Secrets/env vars | 없음 — 레거시 필드는 ini 파일 키일 뿐, 시크릿/환경변수 아님 | 없음 |
| Build artifacts | `WPF_Example/obj/` 내 generated .g.cs 파일들 — 빌드 시 자동 재생성 | 수동 조치 불필요 |

**핵심 결론:** 레거시 필드 제거는 순수 코드 편집이며 데이터 마이그레이션이 불필요하다. IniFile 기반 Save/Load는 프로퍼티 존재 여부로 동작하므로 기존 레시피 파일에 구 키가 남아있어도 로드 시 자동으로 무시된다.

---

## Common Pitfalls

### Pitfall 1: RuntimeResizer의 ScaleTransform과 ScrollViewer 연결 미설정
**What goes wrong:** RecipeEditorWindow 내 Canvas를 신규 생성할 때 `_ScaleTransform`과 `ParentScrollViewer`를 초기화하지 않으면 마우스 좌표 계산이 NullReferenceException 또는 오동작 발생.
**Why it happens:** RuntimeResizer 생성자는 이 두 속성을 설정하지 않음. MainView에서는 코드비하인드에서 명시적으로 설정함 (`canvas_main._ScaleTransform = ScaleTransform`, `canvas_main.ParentScrollViewer = scrollViewer`).
**How to avoid:** RecipeEditorWindow XAML에서 RuntimeResizer 인스턴스 후 Loaded 이벤트에서 ScaleTransform과 내부 ScrollViewer(또는 Window 자체)를 연결. 스케일 기능이 불필요하다면 단위 ScaleTransform (`ScaleX=1, ScaleY=1`) 고정 사용.
**Warning signs:** 드래그 시 ROI 위치가 실제 클릭 위치와 크게 어긋남.

### Pitfall 2: CopyTo() 메서드에서 제거된 필드 참조 잔존
**What goes wrong:** `PixelToUM_Offset`, `MotorXPos`, `MotorYPos`, `PartNo` 프로퍼티를 삭제했지만 `CopyTo()` 메서드 내 `slaveParam.PartNo = this.PartNo` 같은 대입 라인이 남아 있으면 컴파일 오류.
**Why it happens:** CameraSlaveParam과 CameraParam 두 클래스 각각의 `CopyTo()`가 서로 상대 클래스 필드를 참조하는 교차 패턴으로 작성됨.
**How to avoid:** D-01 제거 작업 시 두 파일의 프로퍼티 선언과 CopyTo() 내 대입 라인을 동시에 처리. 빌드로 즉시 확인.

### Pitfall 3: OpenRecipeWindow가 Site 오버로드 없이 `CollectRecipe()` 호출
**What goes wrong:** OpenRecipeWindow 생성자에서 `SystemHandler.Handle.Recipes.CollectRecipe()` (Site 없음)를 호출하므로 전역 레시피 목록을 로드함. RecipeEditorWindow에서 저장한 Site별 레시피가 목록에 즉시 반영되지 않을 수 있음.
**Why it happens:** OpenRecipeWindow는 Phase 4 이전에 작성됐고 Site 오버로드를 모름.
**How to avoid:** RecipeEditorWindow에서 저장 버튼 클릭 후 부모 OpenRecipeWindow가 필요하다면 `CollectRecipe(siteNumber)` 오버로드로 교체. 또는 RecipeEditorWindow는 독립 Save만 하고 목록 새로고침은 OpenRecipeWindow 재오픈으로 처리.

### Pitfall 4: PropertyGrid SelectedObject 갱신 시 이전 탭의 수정사항이 손실
**What goes wrong:** Tab 전환 시 PropertyGrid의 SelectedObject를 교체하면, 변경 중이던 값이 PropertyGrid 내부 버퍼에서 커밋되지 않은 채 버려질 수 있음.
**Why it happens:** PropertyGrid에 따라 TextBox 등에 포커스가 있는 상태에서 SelectedObject를 교체하면 편집 완료 이벤트가 발생하지 않음.
**How to avoid:** Tab 전환 이벤트에서 `Keyboard.ClearFocus()` 또는 `FocusManager.SetFocusedElement(this, null)` 를 먼저 호출해 진행 중인 편집을 커밋시킨 후 SelectedObject 교체.

### Pitfall 5: GrabAndDisplay가 null MainView 참조로 호출
**What goes wrong:** RecipeEditorWindow는 MainView에 직접 접근할 수 없으므로 생성 시점에 mainView 참조를 주입받아야 함.
**Why it happens:** RecipeEditorWindow는 별도 팝업 Window이므로 `mParentWindow.mainView` 접근이 불가.
**How to avoid:** `new RecipeEditorWindow(mParentWindow.mainView)` 형태로 MainView를 생성자 파라미터로 전달하거나, `mParentWindow.mainView.GrabAndDisplay` 를 Action 형태로 캡처해서 전달.

---

## Code Examples

### PropertyGrid 사용 (기존 패턴 — InspectionListView.xaml)

```xml
<!-- Source: InspectionListView.xaml line 226-232 -->
<propToolsWpf:PropertyGrid x:Name="ParamEditor"
    CategoryControlTemplate="{StaticResource CategoryControlTemplate1}"
    CategoryHeaderTemplate="{StaticResource HeaderTemplate1}"
    TabHeaderTemplate="{StaticResource HeaderTemplate2}"
    DescriptionIcon="/Resource/lightbulb.png"
    SelectedObject="{Binding SelectedItem.Param, ElementName=treeListBox_sequence}">
</propToolsWpf:PropertyGrid>
```

RecipeEditorWindow에서는 코드비하인드에서 직접 설정:
```csharp
// Source: InspectionListView.xaml.cs 패턴 응용
ParamEditor.SelectedObject = _params[tabControl_shot.SelectedIndex]; //260327 hbk
```

### RuntimeResizer 초기화 패턴 (MainView.xaml.cs 검증됨)

```csharp
// Source: MainView.xaml.cs Constructor + MainView_Loaded
canvas_roi.RenderTransform = _ScaleTransform;         //260327 hbk
canvas_roi._ScaleTransform = _ScaleTransform;         //260327 hbk
canvas_roi.ParentScrollViewer = scrollViewer_roi;     //260327 hbk
canvas_roi.IsEditable = true;                         //260327 hbk
canvas_roi.SetParam(param);                           //260327 hbk
```

### 레거시 필드 제거 패턴 (CameraSlaveParam.cs)

제거 대상 프로퍼티 (Category `"General|AOI"` 블록 내):
```csharp
// 삭제 대상 — CameraSlaveParam.cs 23~29 라인
public double PixelToUM_Offset { get; set; }  // 제거
public double MotorXPos { get; set; }          // 제거
public double MotorYPos { get; set; }          // 제거
public int PartNo { get; set; }                // 제거
// 유지: FrameWidth, FrameHeight
```

제거 대상 메서드 (CameraSlaveParam.cs):
```csharp
// 삭제 대상
public virtual double ConvertPixelToMM(double pixel) { ... }  // PixelToUM_Offset 의존
```

CopyTo() 내 제거 대상 라인 (CameraSlaveParam.cs 197~204):
```csharp
slaveParam.PartNo = this.PartNo;       // 제거
slaveParam.MotorXPos = this.MotorXPos; // 제거
slaveParam.MotorYPos = this.MotorYPos; // 제거
slaveParam.FrameWidth = this.FrameWidth;   // 유지
slaveParam.FrameHeight = this.FrameHeight; // 유지
slaveParam.PixelToUM_Offset = this.PixelToUM_Offset; // 제거
```

### 레시피 저장/초기화 (SequenceHandler 오버로드)

```csharp
// Source: Sequence/SequenceHandler.cs line 180, 207 (Phase 4 구현됨)
// 저장
SystemHandler.Handle.Sequences.SaveRecipe(_siteNumber, _recipeName); //260327 hbk

// 초기화 후 저장
foreach (var p in _params) {                            //260327 hbk
    p.ROI = new Rect(0, 0, 0, 0);                      //260327 hbk — 저장값 0,0,0,0
    p.BlobMinArea = 100;                                //260327 hbk
    p.BlobMaxArea = 50000;                              //260327 hbk
    p.DelayMs = 0;                                      //260327 hbk
}
SystemHandler.Handle.Sequences.SaveRecipe(_siteNumber, _recipeName); //260327 hbk
ParamEditor.SelectedObject = _params[tabControl_shot.SelectedIndex]; //260327 hbk — 갱신
```

---

## Environment Availability

Step 2.6: SKIPPED (외부 의존성 없음 — 신규 UI 파일 생성 및 기존 코드 편집만 포함)

---

## Validation Architecture

### Test Framework

| Property | Value |
|----------|-------|
| Framework | 없음 — 프로젝트 내 자동화 테스트 인프라 미확인 |
| Config file | 없음 |
| Quick run command | 빌드 성공 확인: `msbuild FinalVision.sln /p:Configuration=Debug` |
| Full suite command | 앱 실행 후 수동 검증 시나리오 |

### Phase Requirements → Test Map

| 요구사항 | Behavior | Test Type | 명령 / 방법 |
|---------|----------|-----------|-------------|
| 레거시 필드 제거 | CameraSlaveParam/CameraParam 빌드 오류 없음 | 빌드 | `msbuild` 오류 0건 |
| RecipeEditorWindow 팝업 진입 | OpenRecipeWindow → 편집 버튼 → 팝업 | Manual | 앱 실행, 레시피 선택 후 편집 버튼 클릭 |
| Shot 탭 전환 | Tab 선택 시 PropertyGrid + Canvas 갱신 | Manual | 탭 1~5 순차 클릭, PropertyGrid 내용 확인 |
| ROI 드래그 | 캔버스 드래그 → InspectionParam.ROI 업데이트 | Manual | 드래그 후 PropertyGrid ROI 값 변경 확인 |
| PropertyGrid 편집 | ROI/Blob/DelayMs 숫자 수정 | Manual | 값 변경 후 저장, 재로드 후 유지 확인 |
| 레시피 저장 | SaveRecipe(site, name) 호출 → ini 파일 생성 | Manual | 저장 후 Recipe/Site{N}/{name}/main.ini 파일 확인 |
| 레시피 불러오기 | LoadRecipe 후 PropertyGrid 값 갱신 | Manual | 저장된 레시피 로드 후 값 일치 확인 |
| 레시피 복사 | Copy() → 새 디렉터리 생성 | Manual | 복사 후 Recipe/Site{N}/{newName}/ 존재 확인 |
| 초기화 | 기본값 리셋 | Manual | 초기화 버튼 후 BlobMinArea=100 확인 |
| Grab 미리보기 | GrabAndDisplay 호출 → 이미지 표시 | Manual | Grab 버튼 클릭 후 캔버스 이미지 갱신 |
| SIMUL_MODE Grab | VirtualCamera 이미지 사용 | Manual | SIMUL_MODE 빌드 후 Grab 실행 |

### Wave 0 Gaps

자동화 테스트 인프라가 없으므로 Wave 0 추가 작업 없음. 모든 검증은 수동 시나리오 기반.

---

## Open Questions

1. **RecipeEditorWindow에서 MainView.GrabAndDisplay 접근 방법**
   - What we know: RecipeEditorWindow는 팝업 Window이고 mParentWindow가 없음. InspectionListView는 `mParentWindow.mainView.GrabAndDisplay()` 패턴 사용.
   - What's unclear: ShowDialog() 이전에 mainView 참조를 어디서 얻는가.
   - Recommendation: OpenRecipeWindow가 RecipeEditorWindow를 생성하는 시점에 `mParentWindow.mainView` 를 생성자로 주입. 또는 `RecipeEditorWindow(MainView mainView)` 생성자 오버로드.

2. **PropertyGrid SelectedObject 교체 시 편집 중인 값 커밋 보장**
   - What we know: WPF TextBox는 포커스 이탈 시 LostFocus 이벤트로 바인딩 업데이트. PropertyGrid 내부 처리는 PropertyTools 버전에 따라 다름.
   - What's unclear: 현재 PropertyTools.Wpf 버전에서 SelectedObject 교체 시 미커밋 값 처리 여부.
   - Recommendation: Tab 전환 이벤트 핸들러에서 `Keyboard.ClearFocus()` 선행 호출 패턴 적용.

3. **OpenRecipeWindow Site 오버로드 미사용 문제**
   - What we know: OpenRecipeWindow는 `CollectRecipe()` (Site 없음) 를 사용. Phase 4에서 `CollectRecipe(int siteNumber)` 오버로드가 추가됨.
   - What's unclear: RecipeEditorWindow 저장 후 목록 갱신이 필요한지.
   - Recommendation: RecipeEditorWindow는 독립 저장만 담당. 목록 갱신이 필요하다면 OpenRecipeWindow.Window_Loaded에서 Site 오버로드로 교체.

---

## Sources

### Primary (HIGH confidence)
- 직접 코드 분석: `WPF_Example/UI/ControlItem/InspectionListView.xaml` — PropertyGrid 패턴
- 직접 코드 분석: `WPF_Example/UI/ContentItem/RuntimeResizer/RuntimeResizer.cs` — Phase 3 구현
- 직접 코드 분석: `WPF_Example/UI/ContentItem/MainView.xaml.cs` — GrabAndDisplay 시그니처
- 직접 코드 분석: `WPF_Example/Sequence/SequenceHandler.cs` — SaveRecipe/LoadRecipe(int, string) 오버로드 line 180, 207
- 직접 코드 분석: `WPF_Example/Sequence/Param/CameraSlaveParam.cs` — 레거시 필드 현황
- 직접 코드 분석: `WPF_Example/Sequence/Param/CameraParam.cs` — 레거시 필드 현황
- 직접 코드 분석: `WPF_Example/Custom/Sequence/Inspection/Action_Inspection.cs` — InspectionParam 구조
- 직접 코드 분석: `WPF_Example/Custom/Device/DeviceHandler.cs` — INSPECTION_CAMERA_WIDTH/HEIGHT 상수
- 직접 코드 분석: `WPF_Example/UI/Recipe/OpenRecipeWindow.xaml.cs` — 기존 레시피 창 구조

### Secondary (MEDIUM confidence)
- .planning/phases/06-ui/06-CONTEXT.md — 사용자 결정 사항 (D-01~D-07)
- .planning/STATE.md — Phase 3~4 완료 결정 이력

---

## Metadata

**Confidence breakdown:**
- Standard stack: HIGH — 모든 라이브러리가 이미 프로젝트에 존재하고 동작이 검증됨
- Architecture: HIGH — 기존 코드에서 재사용 패턴 직접 확인됨
- Pitfalls: HIGH — RuntimeResizer 초기화, CopyTo 필드 대칭 제거, PropertyGrid 포커스 커밋은 코드에서 직접 식별

**Research date:** 2026-03-27
**Valid until:** 2026-04-27 (안정적인 내부 코드 기반 — 외부 API 의존 없음)
