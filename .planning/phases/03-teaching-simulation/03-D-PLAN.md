---
phase: 03-teaching-simulation
plan: D
type: execute
wave: 3
depends_on: [03-A, 03-C]
files_modified:
  - WPF_Example/UI/ContentItem/MainView.xaml.cs
  - WPF_Example/UI/ControlItem/InspectionListView.xaml.cs
autonomous: true
requirements:
  - REQ-003
  - REQ-004

must_haves:
  truths:
    - "SIMUL 모드(#if SIMUL_MODE)에서 Grab 클릭 시 현재 Action 실행 후 다음 Action으로 자동 포커스 이동한다"
    - "Shot 5 이후 자동 진행이 멈추고 결과 스트립이 최종 상태로 유지된다"
    - "비SIMUL 모드에서 Grab 버튼은 기존 동작 그대로(선택된 Action만 단독 실행)이다"
    - "Grab 완료 후 InspectionListView의 treeListBox_sequence 선택 항목이 다음 Action으로 이동한다"
  artifacts:
    - path: "WPF_Example/UI/ControlItem/InspectionListView.xaml.cs"
      provides: "SIMUL_MODE 자동 진행"
      contains: "AdvanceToNextAction\|SIMUL_MODE"
  key_links:
    - from: "button_grab_Click"
      to: "GrabAndDisplay(camParam) 호출"
      via: "기존 코드 유지 (변경 없음)"
      pattern: "GrabAndDisplay"
    - from: "GrabAndDisplay 완료 콜백"
      to: "InspectionListView.AdvanceToNextAction()"
      via: "GrabAndDisplay에 완료 콜백 파라미터 추가 또는 eventCall=true 활용"
      pattern: "AdvanceToNextAction\|eventCall"
---

<objective>
SIMUL 모드에서 Grab 클릭 후 현재 Action이 실행되면 자동으로 다음 Action으로 포커스가 이동하는 B방식 시뮬레이션 자동 진행을 구현한다.

Purpose: 작업자가 Grab 버튼만 반복 클릭하면 Shot 1→2→3→4→5 순서로 자동 진행되어 5-Shot 시뮬레이션 결과를 빠르게 확인할 수 있어야 한다.
Output: InspectionListView.xaml.cs에 `AdvanceToNextAction()` 메서드 추가. GrabAndDisplay 완료 시 SIMUL 모드에서 자동으로 호출.
</objective>

<execution_context>
@$HOME/.claude/get-shit-done/workflows/execute-plan.md
@$HOME/.claude/get-shit-done/templates/summary.md
</execution_context>

<context>
@.planning/phases/03-teaching-simulation/03-CONTEXT.md
@.planning/phases/03-teaching-simulation/03-C-SUMMARY.md

<interfaces>
<!-- InspectionListView.xaml.cs 현재 핵심 구조 -->

public partial class InspectionListView : UserControl
{
    private MainWindow mParentWindow;
    public ParamBase SelectedParam { get; private set; } = null;

    // treeListBox_sequence — TreeListBox (PropertyTools.Wpf)
    // treeListBox_sequence.SelectedIndex — 현재 선택 인덱스
    // treeListBox_sequence.Items[i] as NodeViewModel — 각 노드
    // NodeViewModel.NodeType == ENodeType.Action → Action 노드
    // NodeViewModel.SequenceID == ESequence.Inspection
    // NodeViewModel.Param as InspectionParam → 해당 Action 파라미터

    private void button_grab_Click(object sender, RoutedEventArgs e)
    {
        // SelectedParam as ICameraParam → GrabAndDisplay(camParam) 호출
        ICameraParam camParam = SelectedParam as ICameraParam;
        mParentWindow.mainView.GrabAndDisplay(camParam);
        // 수정 대상: SIMUL_MODE 시 Grab 완료 후 AdvanceToNextAction() 호출
    }
}

<!-- MainView.GrabAndDisplay 시그니처 (Plan C 적용 후) -->
// public async void GrabAndDisplay(ICameraParam param, bool eventCall = false)
// eventCall 파라미터는 현재 미사용. 콜백 전달 메커니즘이 없으므로
// 다음 두 가지 중 하나를 선택:
// 방법 A: GrabAndDisplay에 Action callback 파라미터 추가
//         → GrabAndDisplay(camParam, onComplete: () => AdvanceToNextAction())
// 방법 B: GrabAndDisplay를 await하는 방식으로 변경
//         → button_grab_Click을 async로 만들고 await Task 기반으로 처리
// 권장: 방법 A (최소 변경, 기존 eventCall 파라미터 대체)

<!-- SequenceHandler 상수 (Shot 순서 파악에 사용) -->
// SHOT_INDEX_BOLT_ONE=0 ~ SHOT_INDEX_ASSY_TWO=4

<!-- treeListBox_sequence 노드 구조 -->
// Items[0] = Root (Recipe 이름)
// Items[1] = SEQ_INSPECTION (Sequence 노드)
// Items[2] = Bolt_One_Inspect (Action 노드, NodeType=Action)
// Items[3] = Bolt_Two_Inspect
// Items[4] = Bolt_Three_Inspect
// Items[5] = Assy_Rail_One_Inspect
// Items[6] = Assy_Rail_Two_Inspect
// → Action 노드만 순서대로 찾아 다음 인덱스로 이동
</interfaces>
</context>

<tasks>

<task type="auto">
  <name>Task D-1: GrabAndDisplay에 완료 콜백 파라미터 추가</name>
  <files>WPF_Example/UI/ContentItem/MainView.xaml.cs</files>
  <read_first>
    - WPF_Example/UI/ContentItem/MainView.xaml.cs (Plan C 적용 후 최신 상태 — UpdateShotStrip() 호출 및 comboBox_shot 자동 선택 코드가 이미 존재함)
  </read_first>
  <action>
**목표:** `GrabAndDisplay`가 완료됐을 때 호출되는 콜백을 전달받을 수 있도록 시그니처를 확장한다.
기존 `eventCall` 파라미터는 미사용 상태이므로 콜백 파라미터로 교체한다.

**GrabAndDisplay 시그니처 변경:**

현재:
```csharp
public async void GrabAndDisplay(ICameraParam param, bool eventCall = false)
```

변경:
```csharp
public async void GrabAndDisplay(ICameraParam param, Action onComplete = null)   //260326 hbk // 완료 콜백 추가
```

**기존 호출부 검색:** 파일 내 `GrabAndDisplay(` 호출이 있는지 grep으로 확인. `eventCall` 인자를 전달하는 호출이 있으면 `null`로 교체한다.

**콜백 호출 위치:** `GrabAndDisplay` 메서드 내부의 `Dispatcher.BeginInvoke` 블록에서 **반드시 아래 순서**를 지킨다:

```csharp
// Plan C에서 삽입된 UpdateShotStrip() 및 comboBox_shot 자동 선택 코드가 이미 이 블록에 있음
// onComplete 호출은 UpdateShotStrip() 이후, canvas_main.InvalidateVisual() 직후에 배치   //260326 hbk

canvas_main.InvalidateVisual();   // 기존 코드 (이미 존재)
onComplete?.Invoke();             //260326 hbk // null 안전 호출 — UpdateShotStrip() 뒤, InvalidateVisual() 뒤
```

삽입 순서 확인:
1. `label_message.Content = resultStr;` (기존)
2. `comboBox_shot.SelectedIndex = shotIdx;` + `UpdateShotStrip();` (Plan C 삽입)
3. `canvas_main.InvalidateVisual();` (기존 또는 Plan C 삽입)
4. `onComplete?.Invoke();` ← **여기** (이번 Task 추가)

**주의:** `onComplete` 호출은 Dispatcher UI 스레드 안에 있으므로 UI 조작이 가능하다.

**InspectionListView에서 호출 시 예:**
```csharp
// SIMUL_MODE 시
mParentWindow.mainView.GrabAndDisplay(camParam, onComplete: () => AdvanceToNextAction());
// 비SIMUL 시
mParentWindow.mainView.GrabAndDisplay(camParam);
```
  </action>
  <verify>
    <automated>grep -n "onComplete\|Action onComplete\|onComplete?.Invoke" "WPF_Example/UI/ContentItem/MainView.xaml.cs"</automated>
  </verify>
  <acceptance_criteria>
    - `GrabAndDisplay(ICameraParam param, Action onComplete = null)` 시그니처 존재
    - `onComplete?.Invoke()` 호출이 Dispatcher.BeginInvoke 블록 내에 존재
    - `onComplete?.Invoke()` 호출이 `UpdateShotStrip()` 호출 이후에 위치함
    - `onComplete?.Invoke()` 호출이 `canvas_main.InvalidateVisual()` 직후에 위치함
    - 기존 `eventCall` 파라미터 제거됨 (또는 새 파라미터와 공존 허용)
    - `//260326 hbk` 주석 포함
  </acceptance_criteria>
  <done>GrabAndDisplay에 onComplete 콜백 파라미터 추가. UpdateShotStrip() 이후, InvalidateVisual() 직후 위치 확인. 기존 호출부 호환성 유지(기본값 null). 빌드 성공.</done>
</task>

<task type="auto">
  <name>Task D-2: InspectionListView SIMUL_MODE B방식 자동 진행 구현</name>
  <files>WPF_Example/UI/ControlItem/InspectionListView.xaml.cs</files>
  <read_first>
    - WPF_Example/UI/ControlItem/InspectionListView.xaml.cs (전체)
    - WPF_Example/Custom/Sequence/SequenceHandler.cs (ACT_* 상수, SHOT_INDEX_* 상수)
  </read_first>
  <action>
**0. 프리프로세서 상수 확인 (먼저 실행)**

실제 상수명을 확인한다:
```bash
grep -rn "SIMUL_MODE\|SIMULATION_MODE" WPF_Example/ --include="*.cs" --include="*.csproj"
```
csproj 또는 기존 소스에서 `SIMUL_MODE`가 확인되면 `#if SIMUL_MODE`를 사용한다.
`SIMULATION_MODE`만 보이면 `#if SIMULATION_MODE`를 사용한다. 이 계획에서는 `SIMUL_MODE`를 기준으로 기술하지만 실제 확인 결과를 우선한다.

**1. AdvanceToNextAction 메서드 추가** — `button_grab_Click` 메서드 바로 아래에 추가:

```csharp
#if SIMUL_MODE
// SIMUL_MODE B방식: Grab 완료 후 다음 Action으로 포커스 이동   //260326 hbk
private void AdvanceToNextAction()   //260326 hbk
{
    this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(() => {   //260326 hbk
        // treeListBox_sequence에서 현재 선택된 Action 노드 인덱스 파악   //260326 hbk
        int currentIndex = -1;   //260326 hbk
        int nextIndex = -1;      //260326 hbk

        for (int i = 0; i < treeListBox_sequence.Items.Count; i++)   //260326 hbk
        {
            NodeViewModel node = treeListBox_sequence.Items[i] as NodeViewModel;   //260326 hbk
            if (node == null) continue;   //260326 hbk

            if (node.IsSelected && node.NodeType == ENodeType.Action)   //260326 hbk
            {
                currentIndex = i;   //260326 hbk
                break;              //260326 hbk
            }
        }

        if (currentIndex < 0) return;   //260326 hbk // 선택된 Action 없음

        // 다음 Action 노드 탐색 (currentIndex+1 이후에서 ENodeType.Action 찾기)   //260326 hbk
        for (int i = currentIndex + 1; i < treeListBox_sequence.Items.Count; i++)   //260326 hbk
        {
            NodeViewModel node = treeListBox_sequence.Items[i] as NodeViewModel;   //260326 hbk
            if (node == null) continue;   //260326 hbk
            if (node.NodeType == ENodeType.Action && node.SequenceID == ESequence.Inspection)   //260326 hbk
            {
                nextIndex = i;   //260326 hbk
                break;           //260326 hbk
            }
        }

        if (nextIndex < 0) return;   //260326 hbk // Shot 5 이후 → 더 이상 진행 없음

        // 다음 Action 선택   //260326 hbk
        treeListBox_sequence.UnselectAll();   //260326 hbk
        treeListBox_sequence.SelectedIndex = nextIndex;   //260326 hbk
        NodeViewModel nextNode = treeListBox_sequence.Items[nextIndex] as NodeViewModel;   //260326 hbk
        if (nextNode != null)   //260326 hbk
        {
            nextNode.IsSelected = true;   //260326 hbk
            treeListBox_sequence.ScrollIntoView(nextNode);   //260326 hbk // 스크롤하여 보이게 함
        }
    }));   //260326 hbk
}
#endif
```

**2. button_grab_Click 수정** — 기존 `mParentWindow.mainView.GrabAndDisplay(camParam);` 라인을:

```csharp
//260326 hbk // SIMUL_MODE: Grab 완료 후 다음 Action 자동 선택 (B방식)
#if SIMUL_MODE
mParentWindow.mainView.GrabAndDisplay(camParam, onComplete: () => AdvanceToNextAction());   //260326 hbk
#else
mParentWindow.mainView.GrabAndDisplay(camParam);   //260326 hbk
#endif
```

**주의사항:**
- `treeListBox_sequence`는 `PropertyTools.Wpf.TreeListBox`. `UnselectAll()`이 없으면 `SelectedIndex = -1`로 초기화한다.
- `AdvanceToNextAction` 내부 Dispatcher 호출은 이미 UI 스레드(GrabAndDisplay onComplete → Dispatcher.BeginInvoke 내부)에서 실행되므로 이중 Dispatcher가 될 수 있다. 실제 콜백 실행 스레드 확인 후 필요 없으면 Dispatcher 제거한다.
- `node.IsSelected` — `NodeViewModel` 프로퍼티. `treeListBox_sequence.SelectedItem`과 비교하는 방식이 더 안전하다면: `treeListBox_sequence.SelectedItem as NodeViewModel`로 현재 선택을 가져온다.
- `onComplete: () => AdvanceToNextAction()` — Plan C에서 삽입한 `UpdateShotStrip()` 이후 실행된다 (Task D-1에서 순서 확정).
  </action>
  <verify>
    <automated>grep -n "AdvanceToNextAction\|SIMUL_MODE\|onComplete" "WPF_Example/UI/ControlItem/InspectionListView.xaml.cs"</automated>
  </verify>
  <acceptance_criteria>
    - `AdvanceToNextAction()` 메서드 존재 (`#if SIMUL_MODE` 블록 내)
    - `button_grab_Click` 내에 `#if SIMUL_MODE`/`#else`/`#endif` 분기 존재
    - SIMUL 분기에서 `onComplete: () => AdvanceToNextAction()` 전달 존재
    - `treeListBox_sequence.SelectedIndex = nextIndex` 존재
    - `//260326 hbk` 주석 모든 신규 라인에 존재
  </acceptance_criteria>
  <done>SIMUL_MODE 빌드 시 Grab 완료 후 다음 Action으로 treeListBox 포커스 자동 이동. 비SIMUL_MODE 빌드 시 기존 동작 유지. 빌드 성공.</done>
</task>

</tasks>

<verification>
- Visual Studio 2022 전체 빌드 성공 (`SIMUL_MODE` 정의 여부 양쪽 모두)
- SIMUL_MODE 정의 시: Grab 클릭 → Action_Inspection 실행 → treeListBox에서 다음 Shot 자동 선택됨
- Shot 5(Assy_Rail_Two) Grab 후 더 이상 자동 진행 없음 (멈춤)
- 비SIMUL_MODE: Grab 클릭 시 기존 동작 그대로 (단독 Action만 실행, 자동 진행 없음)
</verification>

<success_criteria>
- GrabAndDisplay 시그니처 확장: `Action onComplete = null` 파라미터
- onComplete 삽입 위치: UpdateShotStrip() 이후, canvas_main.InvalidateVisual() 직후
- AdvanceToNextAction(): treeListBox에서 현재 선택 이후 다음 ENodeType.Action을 찾아 선택
- button_grab_Click: SIMUL_MODE #if 조건 분기 추가
- Shot 5 이후 자동 진행 중단
- 빌드 성공, 기존 비SIMUL 동작 회귀 없음
</success_criteria>

<output>
완료 후 `.planning/phases/03-teaching-simulation/03-D-SUMMARY.md` 생성
</output>
