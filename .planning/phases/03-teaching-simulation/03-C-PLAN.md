---
phase: 03-teaching-simulation
plan: C
type: execute
wave: 2
depends_on: [03-A]
files_modified:
  - WPF_Example/UI/ContentItem/MainView.xaml
  - WPF_Example/UI/ContentItem/MainView.xaml.cs
autonomous: true
requirements:
  - REQ-007

must_haves:
  truths:
    - "MainView 하단에 Shot 선택 ComboBox(Shot1~5)와 원본/측정 RadioButton이 표시된다"
    - "ComboBox 선택 시 해당 InspectionParam의 LastOriginalImage 또는 LastAnnotatedImage가 캔버스에 표시된다"
    - "결과 스트립이 항상 표시되며 OK=초록/NG=빨강/미실행=회색으로 색상이 나타난다"
    - "Shot 결과 스트립 클릭 시 ComboBox가 해당 Shot으로 연동된다"
    - "GrabAndDisplay 완료 후 해당 Shot ComboBox가 자동 선택된다"
  artifacts:
    - path: "WPF_Example/UI/ContentItem/MainView.xaml"
      provides: "Shot 뷰어 UI 레이아웃"
      contains: "comboBox_shot"
    - path: "WPF_Example/UI/ContentItem/MainView.xaml.cs"
      provides: "Shot 뷰어 이벤트 핸들러"
      contains: "RefreshShotViewer\|UpdateShotStrip"
  key_links:
    - from: "GrabAndDisplay() 완료 블록"
      to: "ComboBox_shot 자동 선택"
      via: "param.ActionName 기반으로 Shot 인덱스 계산"
      pattern: "comboBox_shot.SelectedIndex"
    - from: "ComboBox_shot SelectionChanged"
      to: "DisplayToBackground(LastOriginalImage or LastAnnotatedImage)"
      via: "radioButton_original.IsChecked 분기"
      pattern: "LastOriginalImage\|LastAnnotatedImage"
---

<objective>
MainView.xaml에 Shot 뷰어 UI(ComboBox + 원본/측정 RadioButton + 결과 스트립)를 추가한다.
ComboBox로 Shot 1~5를 선택하면 해당 Action의 이미지가 표시되고, 결과 스트립이 OK/NG/미실행 색상으로 표시된다.

Purpose: 작업자가 5-Shot 각각의 원본/측정 이미지를 전환하며 Blob 파라미터를 확인할 수 있어야 한다.
Output: MainView.xaml에 Shot 뷰어 UI 추가. MainView.xaml.cs에 이벤트 핸들러 및 결과 스트립 갱신 로직 추가.
</objective>

<execution_context>
@$HOME/.claude/get-shit-done/workflows/execute-plan.md
@$HOME/.claude/get-shit-done/templates/summary.md
</execution_context>

<context>
@.planning/phases/03-teaching-simulation/03-CONTEXT.md
@.planning/phases/03-teaching-simulation/03-A-SUMMARY.md

<interfaces>
<!-- MainView.xaml 현재 구조 (수정 대상) -->
<!-- TabItem "Main View" 내부의 Grid에 Row 구조가 있음 -->
<!-- Row 0: 툴바(ComboBox_sequence, ComboBox_viewMode, Slider, label_pos) - 42px 고정 -->
<!-- Row 1: ScrollViewer(canvas_main) + label_message 오버레이 -->
<!-- Row 2: 추가 예정 - Shot 뷰어 컨트롤 -->

<!-- SequenceHandler 상수 (Shot ComboBox 항목 정의에 사용) -->
// SequenceHandler.ACT_BOLT_ONE   = "Bolt_One_Inspect"    → "Shot 1 (Bolt One)"
// SequenceHandler.ACT_BOLT_TWO   = "Bolt_Two_Inspect"    → "Shot 2 (Bolt Two)"
// SequenceHandler.ACT_BOLT_THREE = "Bolt_Three_Inspect"  → "Shot 3 (Bolt Three)"
// SequenceHandler.ACT_ASSY_ONE   = "Assy_Rail_One_Inspect" → "Shot 4 (Assy Rail One)"
// SequenceHandler.ACT_ASSY_TWO   = "Assy_Rail_Two_Inspect" → "Shot 5 (Assy Rail Two)"

// SHOT_INDEX_BOLT_ONE   = 0 ~ SHOT_INDEX_ASSY_TWO = 4

<!-- InspectionParam 이미지 버퍼 (Plan A에서 추가됨) -->
// InspectionParam.LastOriginalImage  → Mat (항상 최신 Grab 원본)
// InspectionParam.LastAnnotatedImage → Mat (BlobDetect 오버레이, 잠금)

<!-- GrabAndDisplay 현재 시그니처 -->
// public async void GrabAndDisplay(ICameraParam param, bool eventCall = false)
// param.ActionName → SequenceHandler.ACT_BOLT_ONE 등과 비교하여 Shot 인덱스 파악

<!-- SystemHandler.Handle.Sequences 구조 -->
// pSeq[ESequence.Inspection] → SequenceBase
// seq[shotIndex] → ActionBase (인덱스로 Action 접근)
// seq[shotIndex].Param as InspectionParam → 해당 Shot의 파라미터

<!-- ActionBase 프로퍼티 -->
// ActionBase.Name → SequenceHandler.ACT_BOLT_ONE 등 문자열
// ActionBase.Context.Result → EContextResult (Pass/Fail/None/Error)
</interfaces>
</context>

<tasks>

<task type="auto">
  <name>Task C-1: MainView.xaml Shot 뷰어 UI 추가</name>
  <files>WPF_Example/UI/ContentItem/MainView.xaml</files>
  <read_first>
    - WPF_Example/UI/ContentItem/MainView.xaml (전체)
  </read_first>
  <action>
**목표:** TabItem "Main View" 내부의 Grid에 Row 2를 추가하고 Shot 뷰어 컨트롤을 삽입한다.

**1. Grid.RowDefinitions 수정** — 기존 2개 Row에 Row 2 추가:

```xml
<Grid.RowDefinitions>
    <RowDefinition Height="42"></RowDefinition>   <!-- 기존 툴바 -->
    <RowDefinition Height="10*"></RowDefinition>  <!-- 기존 캔버스 -->
    <RowDefinition Height="Auto"></RowDefinition> <!-- 신규 Shot 뷰어 컨트롤 -->  <!--260326 hbk-->
</Grid.RowDefinitions>
```

**2. Row 2에 Shot 뷰어 Grid 추가** — `</ScrollViewer>` 닫힌 태그 바로 뒤, `</Grid>` 닫히기 전에 삽입:

```xml
<!--260326 hbk Shot 뷰어 컨트롤 시작-->
<Grid Grid.Column="0" Grid.Row="2" Margin="0,2,0,0">
    <Grid.RowDefinitions>
        <RowDefinition Height="36"></RowDefinition>   <!--Shot 선택 + 이미지 종류-->
        <RowDefinition Height="36"></RowDefinition>   <!--결과 스트립-->
    </Grid.RowDefinitions>

    <!-- Row 0: Shot 선택 ComboBox + 원본/측정 RadioButton -->
    <StackPanel Grid.Row="0" Orientation="Horizontal" VerticalAlignment="Center" Margin="4,2">
        <Label Content="Shot:" FontSize="14" VerticalAlignment="Center" Margin="0,0,4,0"/>
        <ComboBox x:Name="comboBox_shot" Width="200" FontSize="14" Margin="0,0,8,0"
                  SelectionChanged="ComboBox_shot_SelectionChanged"/>  <!--260326 hbk-->
        <RadioButton x:Name="radioButton_original" Content="원본" FontSize="14"
                     GroupName="ImageMode" IsChecked="True"
                     VerticalAlignment="Center" Margin="0,0,8,0"
                     Checked="RadioButton_imageMode_Checked"/>         <!--260326 hbk-->
        <RadioButton x:Name="radioButton_annotated" Content="측정" FontSize="14"
                     GroupName="ImageMode"
                     VerticalAlignment="Center"
                     Checked="RadioButton_imageMode_Checked"/>         <!--260326 hbk-->
    </StackPanel>

    <!-- Row 1: 결과 스트립 (Shot 1~5 OK/NG/미실행 표시) -->
    <UniformGrid Grid.Row="1" Rows="1" Columns="5" Margin="4,2">  <!--260326 hbk-->
        <Border x:Name="stripBorder_Shot1" Background="Gray" Margin="2,0"
                MouseLeftButtonDown="StripBorder_MouseLeftButtonDown" Tag="0"
                CornerRadius="3">
            <Label Content="Shot 1" FontSize="12" HorizontalAlignment="Center"
                   VerticalAlignment="Center" Foreground="White"/>
        </Border>
        <Border x:Name="stripBorder_Shot2" Background="Gray" Margin="2,0"
                MouseLeftButtonDown="StripBorder_MouseLeftButtonDown" Tag="1"
                CornerRadius="3">
            <Label Content="Shot 2" FontSize="12" HorizontalAlignment="Center"
                   VerticalAlignment="Center" Foreground="White"/>
        </Border>
        <Border x:Name="stripBorder_Shot3" Background="Gray" Margin="2,0"
                MouseLeftButtonDown="StripBorder_MouseLeftButtonDown" Tag="2"
                CornerRadius="3">
            <Label Content="Shot 3" FontSize="12" HorizontalAlignment="Center"
                   VerticalAlignment="Center" Foreground="White"/>
        </Border>
        <Border x:Name="stripBorder_Shot4" Background="Gray" Margin="2,0"
                MouseLeftButtonDown="StripBorder_MouseLeftButtonDown" Tag="3"
                CornerRadius="3">
            <Label Content="Shot 4" FontSize="12" HorizontalAlignment="Center"
                   VerticalAlignment="Center" Foreground="White"/>
        </Border>
        <Border x:Name="stripBorder_Shot5" Background="Gray" Margin="2,0"
                MouseLeftButtonDown="StripBorder_MouseLeftButtonDown" Tag="4"
                CornerRadius="3">
            <Label Content="Shot 5" FontSize="12" HorizontalAlignment="Center"
                   VerticalAlignment="Center" Foreground="White"/>
        </Border>
    </UniformGrid>
</Grid>
<!--260326 hbk Shot 뷰어 컨트롤 끝-->
```

**주의:**
- `label_message`는 Row 1에 Canvas 오버레이로 있으므로 `Grid.Row="1"`인 채로 유지한다. Row 번호는 그대로다.
- RowDefinition 3번째 항목 `Height="Auto"`는 내용 크기에 맞게 자동으로 늘어남. 약 72px.
  </action>
  <verify>
    <automated>grep -n "comboBox_shot\|radioButton_original\|radioButton_annotated\|stripBorder_Shot\|UniformGrid" "WPF_Example/UI/ContentItem/MainView.xaml"</automated>
  </verify>
  <acceptance_criteria>
    - `comboBox_shot` 이름 존재
    - `radioButton_original` 이름 존재
    - `radioButton_annotated` 이름 존재
    - `stripBorder_Shot1` ~ `stripBorder_Shot5` 5개 Border 존재
    - `StripBorder_MouseLeftButtonDown` 이벤트 핸들러 참조 존재
    - `ComboBox_shot_SelectionChanged` 이벤트 핸들러 참조 존재
    - `RadioButton_imageMode_Checked` 이벤트 핸들러 참조 존재
    - `Tag="0"` ~ `Tag="4"` 각 Border에 존재
    - `<!--260326 hbk` 주석 포함
  </acceptance_criteria>
  <done>XAML에 Shot 뷰어 컨트롤(ComboBox + RadioButton + 결과 스트립 5개) 추가. Visual Studio에서 디자이너 오류 없음.</done>
</task>

<task type="auto">
  <name>Task C-2: MainView.xaml.cs Shot 뷰어 이벤트 핸들러 구현</name>
  <files>WPF_Example/UI/ContentItem/MainView.xaml.cs</files>
  <read_first>
    - WPF_Example/UI/ContentItem/MainView.xaml.cs (전체)
    - WPF_Example/Custom/Sequence/SequenceHandler.cs (ACT_* 상수, SHOT_INDEX_* 상수 확인)
    - WPF_Example/Custom/Sequence/Inspection/Action_Inspection.cs (Plan A 완료 후 — InspectionParam 구조)
    - WPF_Example/Custom/Sequence/SequenceBase.cs 또는 SequenceHandler.cs (pSeq 인덱서, ActionCount, seq[i] 패턴 확인)
  </read_first>
  <action>
**1. 필드 추가** — MainView 클래스 상단 기존 필드 선언 블록에 추가:

```csharp
// Shot 뷰어 관련 필드   //260326 hbk
private static readonly string[] SHOT_ACTION_NAMES = {   //260326 hbk
    SequenceHandler.ACT_BOLT_ONE,    //260326 hbk // 인덱스 0
    SequenceHandler.ACT_BOLT_TWO,    //260326 hbk // 인덱스 1
    SequenceHandler.ACT_BOLT_THREE,  //260326 hbk // 인덱스 2
    SequenceHandler.ACT_ASSY_ONE,    //260326 hbk // 인덱스 3
    SequenceHandler.ACT_ASSY_TWO,    //260326 hbk // 인덱스 4
};

private static readonly string[] SHOT_DISPLAY_NAMES = {   //260326 hbk
    "Shot 1 (Bolt One)",       //260326 hbk
    "Shot 2 (Bolt Two)",       //260326 hbk
    "Shot 3 (Bolt Three)",     //260326 hbk
    "Shot 4 (Assy Rail One)",  //260326 hbk
    "Shot 5 (Assy Rail Two)",  //260326 hbk
};

private bool _suppressShotComboEvent = false;   //260326 hbk // 코드에서 SelectedIndex 변경 시 이벤트 억제
```

**2. MainView_Loaded 수정** — 기존 초기화 끝 부분에 추가:

```csharp
// Shot ComboBox 초기화   //260326 hbk
comboBox_shot.Items.Clear();   //260326 hbk
foreach (string name in SHOT_DISPLAY_NAMES)   //260326 hbk
    comboBox_shot.Items.Add(name);   //260326 hbk
if (comboBox_shot.Items.Count > 0)    //260326 hbk
    comboBox_shot.SelectedIndex = 0;  //260326 hbk
```

**3. 이벤트 핸들러 추가** — 기존 `private void ComboBox_viewMode_SelectionChanged` 메서드 아래에 추가:

```csharp
// Shot ComboBox 선택 변경 → 해당 Shot 이미지 표시   //260326 hbk
private void ComboBox_shot_SelectionChanged(object sender, SelectionChangedEventArgs e)   //260326 hbk
{
    if (_suppressShotComboEvent) return;   //260326 hbk
    int idx = comboBox_shot.SelectedIndex;   //260326 hbk
    if (idx < 0 || idx >= SHOT_ACTION_NAMES.Length) return;   //260326 hbk
    RefreshShotViewer(idx);   //260326 hbk
}

// 원본/측정 RadioButton 전환   //260326 hbk
private void RadioButton_imageMode_Checked(object sender, RoutedEventArgs e)   //260326 hbk
{
    int idx = comboBox_shot.SelectedIndex;   //260326 hbk
    if (idx < 0) return;   //260326 hbk
    RefreshShotViewer(idx);   //260326 hbk
}

// 결과 스트립 Border 클릭 → ComboBox 연동   //260326 hbk
private void StripBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)   //260326 hbk
{
    if (sender is Border border && border.Tag is string tagStr)   //260326 hbk
    {
        if (int.TryParse(tagStr, out int shotIdx))   //260326 hbk
        {
            _suppressShotComboEvent = true;   //260326 hbk
            comboBox_shot.SelectedIndex = shotIdx;   //260326 hbk
            _suppressShotComboEvent = false;   //260326 hbk
            RefreshShotViewer(shotIdx);        //260326 hbk
        }
    }
}

// Shot 인덱스에 해당하는 InspectionParam 조회   //260326 hbk
private InspectionParam GetInspectionParam(int shotIndex)   //260326 hbk
{
    if (pSeq == null) return null;   //260326 hbk
    SequenceBase seq = pSeq[ESequence.Inspection];   //260326 hbk
    if (seq == null || shotIndex < 0 || shotIndex >= seq.ActionCount) return null;   //260326 hbk
    return seq[shotIndex].Param as InspectionParam;   //260326 hbk
}

// Shot 뷰어 갱신 — 선택된 Shot의 원본 or 측정 이미지 표시   //260326 hbk
private void RefreshShotViewer(int shotIndex)   //260326 hbk
{
    InspectionParam param = GetInspectionParam(shotIndex);   //260326 hbk
    if (param == null) return;   //260326 hbk

    bool showOriginal = radioButton_original.IsChecked == true;   //260326 hbk
    Mat imgToShow = showOriginal ? param.LastOriginalImage : param.LastAnnotatedImage;   //260326 hbk

    // 이미지가 없으면 캔버스 검정   //260326 hbk
    this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(() => {   //260326 hbk
        DisplayToBackground(imgToShow);   //260326 hbk // 기존 메서드 재사용
    }));   //260326 hbk
}

// 결과 스트립 색상 업데이트 (외부에서 호출 가능)   //260326 hbk
public void UpdateShotStrip()   //260326 hbk
{
    if (pSeq == null) return;   //260326 hbk
    SequenceBase seq = pSeq[ESequence.Inspection];   //260326 hbk
    if (seq == null) return;   //260326 hbk

    Border[] stripBorders = {   //260326 hbk
        stripBorder_Shot1, stripBorder_Shot2, stripBorder_Shot3,   //260326 hbk
        stripBorder_Shot4, stripBorder_Shot5   //260326 hbk
    };

    for (int i = 0; i < stripBorders.Length && i < seq.ActionCount; i++)   //260326 hbk
    {
        EContextResult result = seq[i].Context.Result;   //260326 hbk
        SolidColorBrush bg;   //260326 hbk
        switch (result)   //260326 hbk
        {
            case EContextResult.Pass:
                bg = new SolidColorBrush(Colors.Lime);   //260326 hbk // OK = 초록
                break;
            case EContextResult.Fail:
                bg = new SolidColorBrush(Colors.Red);    //260326 hbk // NG = 빨강
                break;
            default:
                bg = new SolidColorBrush(Colors.Gray);   //260326 hbk // 미실행 = 회색
                break;
        }
        stripBorders[i].Background = bg;   //260326 hbk
    }
}
```

**4. GrabAndDisplay 수정** — `label_message.Content = resultStr;` 직후에 추가:

```csharp
// Shot ComboBox 자동 선택   //260326 hbk
int shotIdx = Array.IndexOf(SHOT_ACTION_NAMES, param.ActionName);   //260326 hbk
if (shotIdx >= 0)   //260326 hbk
{
    _suppressShotComboEvent = true;        //260326 hbk
    comboBox_shot.SelectedIndex = shotIdx; //260326 hbk
    _suppressShotComboEvent = false;       //260326 hbk
}
UpdateShotStrip();   //260326 hbk // 결과 스트립 갱신
```

**주의사항:**
- `param.ActionName` — `ICameraParam` 또는 `ParamBase`의 `ActionName` 프로퍼티가 `SequenceHandler.ACT_BOLT_ONE` 등과 동일한 문자열을 반환해야 한다. 실제 프로퍼티 이름이 `ActionName`이 아닐 경우 `OwnerName` 또는 유사 프로퍼티를 확인하고 사용한다.
- `SequenceBase.ActionCount` 프로퍼티가 없으면 `seq.Count` 또는 반복문에서 `try-catch` 사용.
- `seq[i].Context` — `InspectionActionContext`이므로 `.Result` 프로퍼티가 `EContextResult`이어야 함. 실제 Context 구조 확인 후 적용.
- `DisplayToBackground(imgToShow)` — 이미 `private bool DisplayToBackground(Mat img)` 로 존재. 이미지가 null이면 검정 캔버스로 돌아가므로 그대로 사용한다.
- `StripBorder_MouseLeftButtonDown`에서 `border.Tag`는 XAML에서 `Tag="0"` 처럼 문자열로 선언됐으므로 `tagStr`를 `string`으로 받고 `int.TryParse` 사용.
- `pSeq[ESequence.Inspection]` 인덱서 패턴은 SequenceHandler.cs에서 확인한 타입을 따른다 (read_first에서 로드).
  </action>
  <verify>
    <automated>grep -n "ComboBox_shot_SelectionChanged\|RadioButton_imageMode_Checked\|StripBorder_MouseLeftButtonDown\|RefreshShotViewer\|UpdateShotStrip\|SHOT_ACTION_NAMES\|_suppressShotComboEvent" "WPF_Example/UI/ContentItem/MainView.xaml.cs"</automated>
  </verify>
  <acceptance_criteria>
    - `SHOT_ACTION_NAMES` 배열 (5개 요소) 존재
    - `SHOT_DISPLAY_NAMES` 배열 (5개 요소) 존재
    - `ComboBox_shot_SelectionChanged` 이벤트 핸들러 존재
    - `RadioButton_imageMode_Checked` 이벤트 핸들러 존재
    - `StripBorder_MouseLeftButtonDown` 이벤트 핸들러 존재
    - `RefreshShotViewer(int shotIndex)` 메서드 존재
    - `UpdateShotStrip()` public 메서드 존재
    - `GetInspectionParam(int shotIndex)` 메서드 존재
    - GrabAndDisplay 내에 `comboBox_shot.SelectedIndex = shotIdx` 존재
    - GrabAndDisplay 내에 `UpdateShotStrip()` 호출 존재
    - `//260326 hbk` 주석 모든 신규 라인에 존재
  </acceptance_criteria>
  <done>Shot 뷰어 이벤트 핸들러 구현 완료. ComboBox 선택/RadioButton 전환/결과 스트립 클릭/GrabAndDisplay 자동 선택 동작. 빌드 성공.</done>
</task>

</tasks>

<verification>
- Visual Studio 2022 전체 빌드 성공 (오류 0개)
- 런타임: MainView 하단에 Shot ComboBox + RadioButton + 결과 스트립 5칸이 표시됨
- Grab 실행 후 해당 Shot의 ComboBox가 자동 선택되고 결과 스트립 색상이 변경됨
- ComboBox 수동 선택 시 해당 Shot LastOriginalImage가 캔버스에 표시됨
- RadioButton "측정" 선택 시 LastAnnotatedImage (오버레이 포함)가 표시됨
</verification>

<success_criteria>
- MainView.xaml에 comboBox_shot, radioButton_original/annotated, stripBorder_Shot1~5 추가
- MainView.xaml.cs에 이벤트 핸들러 5개 + 헬퍼 메서드 3개 추가
- GrabAndDisplay 후 해당 Shot이 자동 선택되고 결과 스트립 갱신됨
- 빌드 성공, 기존 comboBox_sequence/viewMode 동작 회귀 없음
</success_criteria>

<output>
완료 후 `.planning/phases/03-teaching-simulation/03-C-SUMMARY.md` 생성
</output>
