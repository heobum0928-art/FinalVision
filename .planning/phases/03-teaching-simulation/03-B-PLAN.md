---
phase: 03-teaching-simulation
plan: B
type: execute
wave: 1
depends_on: []
files_modified:
  - WPF_Example/UI/ContentItem/RuntimeResizer/RuntimeResizer.cs
  - WPF_Example/UI/ContentItem/RuntimeResizer/DrawableRectangle.cs
autonomous: true
requirements:
  - REQ-007
  - REQ-008

must_haves:
  truths:
    - "IsEditable=true 상태에서 빈 캔버스 좌클릭 드래그 시 점선 미리보기 사각형이 그려진다"
    - "드래그 완료(MouseUp) 시 DrawableRectangle의 ROI가 드래그 범위로 업데이트된다"
    - "IsEditable=false 또는 우클릭 드래그 시 기존 스크롤 동작이 유지된다"
    - "기존 ROI 선택/이동/리사이즈 동작이 깨지지 않는다"
  artifacts:
    - path: "WPF_Example/UI/ContentItem/RuntimeResizer/RuntimeResizer.cs"
      provides: "마우스 드래그 신규 ROI 생성"
      contains: "_isDrawingNew"
    - path: "WPF_Example/UI/ContentItem/RuntimeResizer/DrawableRectangle.cs"
      provides: "UpdateRect(Rect) 메서드"
      contains: "UpdateRect"
  key_links:
    - from: "RuntimeResizer.OnMouseDown"
      to: "_isDrawingNew = true"
      via: "IsEditable && !IsSelected && LeftButton && 아무 DrawableItem에도 히트 없음"
      pattern: "_isDrawingNew"
    - from: "RuntimeResizer.OnMouseUp"
      to: "DrawableRectangle.CheckAvailable / Param.SetRect"
      via: "드래그 범위 → SelectedItem 업데이트"
      pattern: "UpdateRect\|SetRect\|OriginalRect"
---

<objective>
`RuntimeResizer`에 마우스 좌클릭 드래그로 새 ROI를 그리는 기능을 추가한다.
현재는 아무 ROI도 선택되지 않은 상태(`!IsSelected`)의 빈 영역 드래그가 스크롤로만 동작한다.

Purpose: 작업자가 이미지 위에서 마우스로 ROI 영역을 직접 지정할 수 있어야 티칭이 가능하다.
Output: RuntimeResizer에 `_isDrawingNew` 상태 + 미리보기 렌더링 + MouseUp 시 SelectedItem ROI 업데이트. DrawableRectangle에 `UpdateRect(Rect)` 메서드 추가.
</objective>

<execution_context>
@$HOME/.claude/get-shit-done/workflows/execute-plan.md
@$HOME/.claude/get-shit-done/templates/summary.md
</execution_context>

<context>
@.planning/phases/03-teaching-simulation/03-CONTEXT.md

<interfaces>
<!-- RuntimeResizer.cs 현재 필드 목록 (수정 기준선) -->

private EPickerPosition CurrentActionMode = EPickerPosition.None;
public ScaleTransform _ScaleTransform { get; set; }
public ScrollViewer ParentScrollViewer { get; set; }
private ParamBase pParam;
public IDrawableItem SelectedItem { get; private set; }
private List<IDrawableItem> DrawableList = new List<IDrawableItem>();
private bool _IsEditable = false;
private Point DownPosition;         // 이미 존재 — 드래그 시작점으로 재활용 가능
private Point ScrollStartPos;
private Point ScrollEndPos;
private Point CurrentPos;

// 추가할 필드:
// private bool _isDrawingNew = false;
// private Point _drawStartPoint;   (이미지 좌표계, ScaleTransform 역변환 후)

<!-- DrawableRectangle 인터페이스 (ROI 업데이트 대상) -->
public class DrawableRectangle : IDrawableItem
{
    private Rect OriginalRect;
    // ROI를 직접 교체하는 공개 메서드가 없음 → 신규 추가 필요:
    // public void UpdateRect(Rect newRect)  ← RuntimeResizer가 호출
    // 또는: ExecResize로 차분 계산하여 맞추는 방식 (복잡)
    // 권장: UpdateRect(Rect) 추가 후 CheckAvailable() 호출로 param에 반영

    public virtual void CheckAvailable(EPickerPosition moveType)
    // CheckAvailable → Param.SetRect(Owner, Name, OriginalRect) 호출하여 파라미터에 반영
}

<!-- OnMouseDown 현재 분기 구조 (수정 위치) -->
// IsEditable=false → CaptureMouse + ScrollStartPos 저장 → return
// IsEditable=true:
//   SelectedItem != null → CurrentActionMode 계산
//   DrawableList 순회 → 히트 항목 → SelectedItem 설정
//   !IsSelected → CaptureMouse + ScrollStartPos (← 여기를 수정)

<!-- OnRender 현재 구조 (미리보기 추가 위치) -->
// protected override void OnRender(DrawingContext dc)
// foreach (IDrawableItem item) { item.Render(dc); }  ← 이 직후에 미리보기 사각형 추가
</interfaces>
</context>

<tasks>

<task type="auto">
  <name>Task B-1: DrawableRectangle에 UpdateRect 메서드 추가</name>
  <files>WPF_Example/UI/ContentItem/RuntimeResizer/DrawableRectangle.cs</files>
  <read_first>
    - WPF_Example/UI/ContentItem/RuntimeResizer/DrawableRectangle.cs (전체)
  </read_first>
  <action>
`DrawableRectangle` 클래스의 `ExecResize` 메서드 바로 아래에 다음 메서드를 추가한다.
모든 신규 라인에 `//260326 hbk // 설명` 형식 주석 필수.

```csharp
public void UpdateRect(System.Windows.Rect newRect)   //260326 hbk // 마우스 드래그 신규 ROI 적용
{
    // 최소 크기 보장   //260326 hbk
    if (newRect.Width < DeviceHandler.MIN_ROI_WIDTH)   //260326 hbk
        newRect.Width = DeviceHandler.MIN_ROI_WIDTH;   //260326 hbk
    if (newRect.Height < DeviceHandler.MIN_ROI_HEIGHT) //260326 hbk
        newRect.Height = DeviceHandler.MIN_ROI_HEIGHT; //260326 hbk

    OriginalRect = newRect;   //260326 hbk // ROI 전체 교체
    UpdatePicker();           //260326 hbk // 핸들 위치 갱신
    // 파라미터에 반영은 RuntimeResizer.OnMouseUp에서 CheckAvailable() 호출로 처리
}
```

`CheckAvailable()` 내부에서 `Param.SetRect(Owner, Name, OriginalRect)`가 이미 호출되므로
`UpdateRect` 완료 후 `CheckAvailable(EPickerPosition.None)` 호출하면 파라미터에 자동 반영된다.
  </action>
  <verify>
    <automated>grep -n "UpdateRect" "WPF_Example/UI/ContentItem/RuntimeResizer/DrawableRectangle.cs"</automated>
  </verify>
  <acceptance_criteria>
    - `public void UpdateRect(System.Windows.Rect newRect)` 메서드 존재
    - 내부에서 `OriginalRect = newRect` 및 `UpdatePicker()` 호출
    - 최소 크기(`MIN_ROI_WIDTH`, `MIN_ROI_HEIGHT`) 보장 로직 존재
    - `//260326 hbk` 주석 포함
  </acceptance_criteria>
  <done>DrawableRectangle에 UpdateRect 메서드 추가 완료. 빌드 성공.</done>
</task>

<task type="auto">
  <name>Task B-2: RuntimeResizer 드래그 신규 ROI 생성 구현</name>
  <files>WPF_Example/UI/ContentItem/RuntimeResizer/RuntimeResizer.cs</files>
  <read_first>
    - WPF_Example/UI/ContentItem/RuntimeResizer/RuntimeResizer.cs (전체)
    - WPF_Example/UI/ContentItem/RuntimeResizer/DrawableRectangle.cs (Task B-1 완료 후)
  </read_first>
  <action>
**1. 필드 추가** — 클래스 상단 기존 필드 선언 블록 끝에 추가:

```csharp
private bool _isDrawingNew = false;   //260326 hbk // 신규 ROI 드래그 중 플래그
private Point _drawStartPoint;        //260326 hbk // 드래그 시작 이미지 좌표 (ScaleTransform 역변환)
```

**2. OnMouseDown 수정** — `!IsSelected` 분기(마지막 `if (!IsSelected)` 블록) 수정:

현재 코드:
```csharp
if (!IsSelected) {
    this.CaptureMouse();
    ScrollStartPos = e.GetPosition(ParentScrollViewer);
}
```

변경 후:
```csharp
if (!IsSelected) {   //260326 hbk
    // IsEditable + 좌클릭 + DrawableList에 ROI 존재 → 신규 ROI 드래그 시작   //260326 hbk
    if (IsEditable && e.LeftButton == MouseButtonState.Pressed && DrawableList.Count > 0)   //260326 hbk
    {
        _isDrawingNew = true;   //260326 hbk // 신규 ROI 드래그 시작
        // 이미지 좌표계로 변환 (ScaleTransform 역변환)   //260326 hbk
        _drawStartPoint = new Point(
            e.GetPosition(this).X / _ScaleTransform.ScaleX,   //260326 hbk
            e.GetPosition(this).Y / _ScaleTransform.ScaleY);  //260326 hbk
        this.CaptureMouse();   //260326 hbk // 드래그 중 마우스 캡처
    }
    else   //260326 hbk // 우클릭 or IsEditable=false or ROI 없음 → 기존 스크롤
    {
        this.CaptureMouse();
        ScrollStartPos = e.GetPosition(ParentScrollViewer);
    }
}
```

**3. OnMouseMove 수정** — `this.IsMouseCaptured` 블록 내부:

현재: 스크롤만 수행
추가 위치: `if (IsEditable == false) return;` 라인 **위**에 삽입:

```csharp
if (_isDrawingNew)   //260326 hbk // 신규 ROI 드래그 미리보기 — 스크롤 없이 캔버스만 갱신
{
    this.InvalidateVisual();   //260326 hbk // OnRender에서 미리보기 사각형 그리기
    return;                    //260326 hbk
}
```

**4. OnMouseUp 수정** — `this.ReleaseMouseCapture();` 직후에 추가:

```csharp
if (_isDrawingNew)   //260326 hbk // 신규 ROI 드래그 완료 → SelectedItem ROI 업데이트
{
    _isDrawingNew = false;   //260326 hbk

    // 현재 마우스 이미지 좌표   //260326 hbk
    Point endPt = new Point(
        CurrentPos.X / _ScaleTransform.ScaleX,   //260326 hbk
        CurrentPos.Y / _ScaleTransform.ScaleY);  //260326 hbk

    // 드래그 범위 → Rect 계산 (음수 폭/높이 방지)   //260326 hbk
    double rx = Math.Min(_drawStartPoint.X, endPt.X);   //260326 hbk
    double ry = Math.Min(_drawStartPoint.Y, endPt.Y);   //260326 hbk
    double rw = Math.Abs(endPt.X - _drawStartPoint.X);  //260326 hbk
    double rh = Math.Abs(endPt.Y - _drawStartPoint.Y);  //260326 hbk

    if (rw > DeviceHandler.MIN_ROI_WIDTH && rh > DeviceHandler.MIN_ROI_HEIGHT)   //260326 hbk // 최소 크기 이상일 때만 적용
    {
        // SelectedItem이 없으면 첫 번째 DrawableRectangle 선택   //260326 hbk
        IDrawableItem target = SelectedItem ?? DrawableList.Find(d => d is DrawableRectangle);   //260326 hbk
        if (target is DrawableRectangle dr)   //260326 hbk
        {
            dr.UpdateRect(new System.Windows.Rect(rx, ry, rw, rh));   //260326 hbk // ROI 교체
            dr.CheckAvailable(EPickerPosition.None);                   //260326 hbk // Param.SetRect 반영
            SelectedItem = dr;                                         //260326 hbk // 선택 상태로 전환
            OnSelectionItemChanged?.Invoke(this, new SelectionChangedCallbackArg(SelectedItem));   //260326 hbk
        }
    }
    this.InvalidateVisual();   //260326 hbk
    return;   //260326 hbk
}
```

**5. OnRender 수정** — `foreach (IDrawableItem item in DrawableList)` 루프 **아래**에 추가:

```csharp
// 신규 ROI 드래그 미리보기 사각형   //260326 hbk
if (_isDrawingNew)   //260326 hbk
{
    Point curImgPt = new Point(
        CurrentPos.X / _ScaleTransform.ScaleX,   //260326 hbk
        CurrentPos.Y / _ScaleTransform.ScaleY);  //260326 hbk

    double px = Math.Min(_drawStartPoint.X, curImgPt.X);   //260326 hbk
    double py = Math.Min(_drawStartPoint.Y, curImgPt.Y);   //260326 hbk
    double pw = Math.Abs(curImgPt.X - _drawStartPoint.X);  //260326 hbk
    double ph = Math.Abs(curImgPt.Y - _drawStartPoint.Y);  //260326 hbk

    if (pw > 1 && ph > 1)   //260326 hbk
    {
        // 점선 Pen 생성   //260326 hbk
        Pen dashPen = new Pen(Brushes.Yellow, 1);               //260326 hbk
        dashPen.DashStyle = new DashStyle(new double[] { 4, 4 }, 0);   //260326 hbk
        dc.DrawRectangle(null, dashPen,
            new System.Windows.Rect(px, py, pw, ph));   //260326 hbk // 점선 미리보기
    }
}
```

**주의사항:**
- `OnRender`의 `dc.PushTransform(this._ScaleTransform)` 이후 코드에 추가하므로 좌표는 이미지 좌표계를 쓴다. _drawStartPoint, curImgPt가 이미 역변환된 이미지 좌표이므로 그대로 전달한다.
- `DrawableList.Find(d => d is DrawableRectangle)` — System.Linq 필요 시 using 확인. 아니면 `foreach`로 첫 번째 DrawableRectangle 탐색.
  </action>
  <verify>
    <automated>grep -n "_isDrawingNew\|_drawStartPoint\|UpdateRect\|dashPen" "WPF_Example/UI/ContentItem/RuntimeResizer/RuntimeResizer.cs"</automated>
  </verify>
  <acceptance_criteria>
    - `_isDrawingNew` 필드 선언 존재
    - `_drawStartPoint` 필드 선언 존재
    - OnMouseDown에서 `_isDrawingNew = true` 분기 존재
    - OnMouseMove에서 `_isDrawingNew` 체크 후 `InvalidateVisual()` 존재
    - OnMouseUp에서 `_isDrawingNew = false` 및 `UpdateRect(` 호출 존재
    - OnRender에서 `dashPen` 사용 점선 사각형 그리기 존재
    - `//260326 hbk` 주석이 모든 신규 라인에 존재
  </acceptance_criteria>
  <done>RuntimeResizer에서 IsEditable=true 상태 좌클릭 드래그 시 신규 ROI 그리기 동작. 기존 스크롤/이동/리사이즈 동작 유지. 빌드 성공.</done>
</task>

</tasks>

<verification>
- Visual Studio 2022 전체 빌드 성공 (오류 0개)
- IsEditable=true 상태에서 빈 캔버스 좌클릭 드래그 시 노란 점선 사각형 표시됨 (런타임 확인)
- MouseUp 후 해당 DrawableRectangle의 ROI가 갱신되고 파란 핸들(Picker) 표시됨
- IsEditable=false 상태에서 기존 스크롤 동작 유지됨
</verification>

<success_criteria>
- RuntimeResizer: `_isDrawingNew`, `_drawStartPoint` 필드 추가
- OnMouseDown/Move/Up 수정으로 신규 ROI 드래그 생성 가능
- DrawableRectangle: `UpdateRect(Rect)` 메서드 추가
- OnRender에 노란 점선 미리보기 사각형 렌더링
- 빌드 성공, 기존 기능(선택/이동/리사이즈/스크롤) 회귀 없음
</success_criteria>

<output>
완료 후 `.planning/phases/03-teaching-simulation/03-B-SUMMARY.md` 생성
</output>
