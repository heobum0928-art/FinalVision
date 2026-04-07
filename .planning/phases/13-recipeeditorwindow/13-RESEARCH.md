# Phase 13: RecipeEditorWindow (Reset Only) - Research

**Researched:** 2026-04-07
**Domain:** WPF C# — 메모리 내 파라미터 백업/복원 + 툴바 버튼 추가
**Confidence:** HIGH (전체 코드베이스 직접 분석)

## Summary

Phase 13의 실제 구현 범위는 CONTEXT.md에 의해 Reset 기능 하나로 축소됩니다. "레시피 로드 시점에 Shot 파라미터를 메모리에 백업하고, 툴바 Reset 버튼으로 선택된 Shot만 백업본으로 복원한다"가 전부입니다.

백업 구조는 기존 `ShotConfig.CopyTo()` 메서드를 그대로 활용합니다. `InspectionRecipeManager.Shots` 리스트와 1:1로 대응하는 `Dictionary<int, ShotConfig>` 백업 테이블을 `InspectionRecipeManager` 또는 `InspectionListView` 중 한 곳에 추가하는 형태가 됩니다. PropertyGrid 갱신은 Paste 버튼의 `UnselectAll → SelectedIndex = index` 패턴을 그대로 재사용합니다.

백업 수행 시점은 `SequenceHandler.LoadRecipe()` → `LoadFromIni()` 완료 직후입니다. 이 메서드는 `SystemHandler.LoadRecipe()`와 TCP `ProcessRecipeChange()` 양쪽에서 공통으로 호출되므로 여기서 한 번만 백업하면 모든 로드 경로를 커버합니다.

**Primary recommendation:** `InspectionRecipeManager`에 `Dictionary<int, ShotConfig> _backup` 필드와 `TakeBackup()` / `RestoreShot(int)` 메서드를 추가하고, `SequenceHandler.LoadFromIni()` 완료 후 `TakeBackup()`을 호출한다. Reset 버튼은 InspectionListView 기존 Copy/Paste `<ToolBar>` 블록 바로 아래에 새 `<ToolBar>` 블록으로 추가한다.

<user_constraints>
## User Constraints (from CONTEXT.md)

### Locked Decisions
- **D-01:** 레시피 로드 시(LoadRecipe 시점) 자동 백업 — 편집 시작 전 상태를 메모리에 보관
- **D-02:** InspectionListView 툴바에 Reset 버튼 추가 — Save/Copy/Paste 버튼과 같은 영역에 배치하여 기존 패턴과 일관성 유지
- **D-03:** 트리에서 현재 선택된 Shot의 파라미터만 로드 시점 백업본으로 복원 (전체 Shot 일괄 아님)

### Claude's Discretion
- 백업 데이터 구조 (Dictionary, Clone 등) 구현 방식
- Reset 버튼 아이콘/텍스트 선택
- Reset 후 PropertyGrid 갱신 방식
- Reset 시 확인 다이얼로그 필요 여부

### Deferred Ideas (OUT OF SCOPE)
- RecipeEditorWindow 신규 팝업 창 (RCP-02, RCP-06)
- Grab 미리보기 (RCP-03)
- Save 버튼 별도 구현 (RCP-04)
</user_constraints>

<phase_requirements>
## Phase Requirements

| ID | Description | Research Support |
|----|-------------|------------------|
| RCP-05 (partial) | RecipeEditorWindow에서 Reset 버튼으로 파라미터 기본값 초기화 | 백업 구조(ShotConfig.CopyTo), 복원 시점(LoadRecipe 후), 버튼 위치(InspectionListView 툴바) 모두 확인됨 |
| RCP-02, RCP-03, RCP-04, RCP-06 | (OUT OF SCOPE for this phase — deferred) | CONTEXT.md에 의해 전량 다음 Phase로 이연됨 |
</phase_requirements>

## Standard Stack

### Core (신규 패키지 없음)
| 기존 자산 | 목적 | 비고 |
|-----------|------|------|
| `ShotConfig.CopyTo(ParamBase)` | Shot 파라미터 값 복사 (백업/복원 양방향) | 260401 hbk 기구현 |
| `InspectionRecipeManager.Shots` | 백업 대상 Shot 목록 접근 | `List<ShotConfig>`, 인덱스 0-based |
| `SequenceHandler.LoadFromIni()` | 백업 수행 시점 — 이 메서드 끝에서 호출 | private, 두 오버로드 모두 |
| `PropertyTools.Wpf.PropertyGrid` | 파라미터 편집 UI — Reset 후 갱신 대상 | 기존 `ParamEditor` 인스턴스 재사용 |

**설치 필요:** 없음. 기존 패키지로 충분 (v2.0 결정: 신규 NuGet 추가 금지).

## Architecture Patterns

### 백업 데이터 구조 권장안
```csharp
// InspectionRecipeManager 내부에 추가
private Dictionary<int, ShotConfig> _backup = new Dictionary<int, ShotConfig>();

public void TakeBackup()   //260407 hbk — 전체 Shot 백업 (LoadRecipe 시점)
{
    _backup.Clear();
    for (int i = 0; i < Shots.Count; i++)
    {
        var snap = new ShotConfig(Shots[i].Owner, i);
        Shots[i].CopyTo(snap);   // FAI 제외 — Shot 파라미터만
        _backup[i] = snap;
    }
}

public bool RestoreShot(int shotIndex)   //260407 hbk — 선택된 Shot 복원
{
    if (!_backup.ContainsKey(shotIndex)) return false;
    _backup[shotIndex].CopyTo(Shots[shotIndex]);
    return true;
}
```

**선택 근거:**
- `ShotConfig.CopyTo()`가 이미 FAI를 제외하고 ShotName/ZPosition/DelayMs/SimulImagePath + CameraSlaveParam 상속 필드를 모두 복사한다. 추가 리플렉션 코드 불필요.
- `new ShotConfig(owner, index)` 생성자가 이미 있어 스냅샷 인스턴스 생성이 간단하다.
- `Dictionary<int, ShotConfig>` 구조가 `shotIndex` 직접 조회를 O(1)로 지원한다.
- FAI는 런타임 검사 결과 구조이므로 Reset 범위에서 제외 (CONTEXT.md 명시 없음 — 타당한 재량 결정).

### 백업 수행 시점 — 코드 흐름
```
SystemHandler.LoadRecipe(name)
  └─ SequenceHandler.LoadRecipe(name, ERecipeFileType.Ini)
       └─ LoadFromIni(name)           ← 두 오버로드 모두 존재
            │  ... ParamBase.Load 완료 ...
            └─ [HERE] InspectionSeq.RecipeManager.TakeBackup()
```

`SequenceHandler.LoadFromIni()`는 private이므로 InspectionRecipeManager를 어디서 접근하느냐가 관건.

**접근 경로 옵션:**
- Option A: `SequenceHandler.LoadFromIni()` 내부에서 Inspection 시퀀스를 캐스팅하여 `TakeBackup()` 호출
- Option B: `SequenceHandler.OnRecipeChanged` 이벤트 구독자(예: InspectionListView)에서 `TakeBackup()` 호출
- Option C: `SequenceHandler.ExecOnLoad(name)` → `InspectionSequence.OnLoad()` 내부에서 `TakeBackup()` 호출

**권장: Option C** — `ExecOnLoad()`는 `LoadFromIni()` 완료 직후 항상 호출되며, `InspectionSequence.OnLoad()` 오버라이드가 이미 존재한다. 기존 패턴을 가장 자연스럽게 확장한다. InspectionRecipeManager는 InspectionSequence의 멤버이므로 직접 참조 가능.

### XAML 버튼 추가 위치 (XAML 라인 222~237 기준)
```xml
<!-- 기존 Copy/Paste ToolBar 블록 (라인 222~237) 바로 뒤에 추가 -->
<ToolBar>
    <Button x:Name="button_reset" Click="button_reset_Click"
            Style="{StaticResource disalbedStyle}"
            ToolTip="선택된 Shot 파라미터를 레시피 로드 시점으로 초기화">
        <StackPanel Orientation="Vertical">
            <Image Source="/Resource/undo.png" Stretch="Uniform" Width="42" Height="42"/>
            <TextBlock Text="Reset" HorizontalAlignment="Center" VerticalAlignment="Center"/>
        </StackPanel>
    </Button>
</ToolBar>
```

**아이콘:** `/Resource/undo.png` (기존 리소스 존재 여부 확인 필요 — 없으면 `refresh.png`나 텍스트만 사용). 이미지 경로는 구현 시 확인.

### PropertyGrid 갱신 패턴 (Paste 기반 재사용)

기존 `button_paste_Click`의 갱신 로직:
```csharp
int index = treeListBox_sequence.SelectedIndex;
treeListBox_sequence.UnselectAll();
treeListBox_sequence.SelectedIndex = index;

// Paste 후 ShotTabView 강제 갱신
if (SelectedParam is InspectionParam ip)
    mParentWindow.mainView.SetParam(ESequence.Inspection, ip);
```

Reset 버튼 핸들러도 동일 패턴 적용:
```csharp
private void button_reset_Click(object sender, RoutedEventArgs e)   //260407 hbk
{
    if (SelectedParam == null) return;
    // 선택된 노드의 shotIndex 구하기
    NodeViewModel node = treeListBox_sequence.SelectedItem as NodeViewModel;
    if (node == null || node.NodeType != ENodeType.Action) return;

    // InspectionSequence.RecipeManager에서 shotIndex 얻기 + RestoreShot 호출
    // ... (구체적 접근 경로는 InspectionSequence 구조 확인 후 결정)

    // PropertyGrid 강제 갱신
    int index = treeListBox_sequence.SelectedIndex;
    treeListBox_sequence.UnselectAll();
    treeListBox_sequence.SelectedIndex = index;
    if (SelectedParam is InspectionParam ip)
        mParentWindow.mainView.SetParam(ESequence.Inspection, ip);
}
```

### shotIndex 결정 방법

`NodeViewModel.ActionID`(EAction enum)에서 Action 순번을 얻어야 한다. 기존 `Btn_start_Click`이 이 패턴을 이미 구현함:
```csharp
SequenceBase inspSeq = SystemHandler.Handle.Sequences[ESequence.Inspection];
for (int i = 0; i < inspSeq.ActionCount; i++) {
    if (inspSeq[i].ID == selNode.ActionID) { actIndex = i; break; }
}
```
`actIndex`가 곧 `shotIndex` (0-based, Shot_1=0 ... Shot_5=4).

### Anti-Patterns to Avoid

- **파일 재로드로 Reset 구현:** INI 재로드는 TCP 검사 중 동시성 문제 위험 + 불필요한 I/O. 메모리 백업이 올바른 접근.
- **전체 Shot 일괄 복원:** D-03에서 명시 금지. 선택된 Shot만 복원.
- **PropertyGrid.SelectedObject 직접 교체:** PropertyGrid가 바인딩으로 `SelectedObject={Binding SelectedItem.Param}`을 관리한다. 직접 교체 대신 treeListBox_sequence 재선택으로 바인딩 갱신.
- **새 ShotConfig를 Shots[i]에 대입:** `Shots` 리스트 참조가 여러 곳에 퍼져 있을 수 있음. 대입 대신 `CopyTo()`로 값만 복사.

## Don't Hand-Roll

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| 파라미터 값 복사 | 리플렉션으로 직접 구현 | `ShotConfig.CopyTo()` 체인 | 이미 `ParamBase → CameraSlaveParam → ShotConfig` 계층 전체 커버 |
| PropertyGrid 강제 갱신 | PropertyGrid API 직접 호출 | `UnselectAll() + SelectedIndex = index` | Paste 버튼이 이미 검증한 패턴 |
| Shot 인덱스 조회 | 별도 맵 구조 | `for` + `inspSeq[i].ID == selNode.ActionID` | `Btn_start_Click`이 이미 동일 패턴 사용 |

## Common Pitfalls

### Pitfall 1: CopyTo가 FAI는 복사하지 않는다
**What goes wrong:** `ShotConfig.CopyTo()`를 호출해도 `FAIs` 리스트는 복원되지 않는다.
**Why it happens:** `ShotConfig.CopyTo()`가 명시적으로 FAI를 제외하고 있음 (코드 라인 129~139 확인).
**How to avoid:** Reset 범위를 Shot 파라미터(ROI/Blob/Delay)로 제한하고 FAI는 건드리지 않는다. FAI 복원이 필요하면 별도 기획 필요.
**Warning signs:** Reset 후 FAI 설정이 바뀌었다는 사용자 보고.

### Pitfall 2: 백업이 로드 전 빈 Shots 리스트 상태에서 찍힘
**What goes wrong:** `TakeBackup()`을 `LoadFromIni()` 호출 전에 배치하면 빈 백업이 찍힌다.
**Why it happens:** `InspectionRecipeManager.Load()`가 `Shots.Clear()`로 시작하기 때문.
**How to avoid:** `ExecOnLoad()` 또는 `LoadFromIni()` 완료 직후에 `TakeBackup()` 호출. Option C(OnLoad 오버라이드)가 가장 안전.
**Warning signs:** Reset 후 모든 파라미터가 0 또는 기본값으로 초기화됨.

### Pitfall 3: 백업 없이 Reset 버튼 활성화
**What goes wrong:** 레시피 로드 전 Reset 버튼을 클릭하면 크래시 또는 무의미한 복원.
**Why it happens:** `_backup`이 비어 있을 때 `RestoreShot()` 호출.
**How to avoid:** `button_reset.IsEnabled` 를 백업 존재 여부에 연동하거나, `RestoreShot()`에서 `_backup.ContainsKey(shotIndex)` 가드 처리.
**Warning signs:** NullReferenceException on reset click before any recipe load.

### Pitfall 4: SequenceHandler.LoadFromIni가 두 개의 오버로드를 가짐
**What goes wrong:** `LoadFromIni(string name)` 에만 TakeBackup 추가하고 `LoadFromIni(int siteNumber, string name)`에는 누락.
**Why it happens:** Site 오버로드가 별도로 존재(라인 253~281).
**How to avoid:** Option C(OnLoad 오버라이드)를 사용하면 두 오버로드 모두 `ExecOnLoad()`를 거치므로 이 함정을 피할 수 있다.
**Warning signs:** TCP RecipeChange 후 Reset이 이전 레시피 상태로 복원됨.

### Pitfall 5: XAML의 -- 주석 금지 (MC3000)
**What goes wrong:** `<!-- -->`처럼 `--`가 포함된 XAML 주석이 있으면 빌드 경고 MC3000 발생.
**Why it happens:** Phase 12 accumulated context에 "XML XAML comments must not use -- (MC3000)" 명시.
**How to avoid:** XAML 주석에 `--` 사용 금지. `260407 hbk` 형식 코드 주석은 C# 코드 전용.

## Code Examples

### Pattern 1: TakeBackup + RestoreShot (신규 메서드)
```csharp
// InspectionRecipeManager.cs 에 추가
// Source: 기존 ShotConfig.CopyTo() 패턴 기반

private Dictionary<int, ShotConfig> _backup = new Dictionary<int, ShotConfig>();   //260407 hbk

public void TakeBackup()   //260407 hbk — LoadRecipe 완료 직후 호출
{
    _backup.Clear();
    for (int i = 0; i < Shots.Count; i++)
    {
        var snap = new ShotConfig(Shots[i].Owner, i);
        Shots[i].CopyTo(snap);
        _backup[i] = snap;
    }
}

public bool RestoreShot(int shotIndex)   //260407 hbk — 선택된 Shot만 복원
{
    if (!_backup.ContainsKey(shotIndex)) return false;
    _backup[shotIndex].CopyTo(Shots[shotIndex]);
    return true;
}

public bool HasBackup => _backup.Count > 0;   //260407 hbk — 버튼 활성화 가드
```

### Pattern 2: InspectionSequence.OnLoad() 오버라이드 (백업 수행 시점)
```csharp
// InspectionSequence.cs (또는 동등 클래스) — OnLoad 오버라이드에 추가
public override void OnLoad()   //260407 hbk
{
    base.OnLoad();
    RecipeManager.TakeBackup();   //260407 hbk — D-01: 로드 완료 후 즉시 백업
}
```

### Pattern 3: button_reset_Click (InspectionListView.xaml.cs)
```csharp
private void button_reset_Click(object sender, RoutedEventArgs e)   //260407 hbk
{
    NodeViewModel node = treeListBox_sequence.SelectedItem as NodeViewModel;
    if (node == null || node.NodeType != ENodeType.Action) return;
    if (node.SequenceID != ESequence.Inspection) return;

    // shotIndex 결정 (Btn_start_Click 기존 패턴 재사용)
    SequenceBase inspSeq = SystemHandler.Handle.Sequences[ESequence.Inspection];
    if (inspSeq == null) return;
    int shotIndex = -1;
    for (int i = 0; i < inspSeq.ActionCount; i++)
    {
        if (inspSeq[i].ID == node.ActionID) { shotIndex = i; break; }
    }
    if (shotIndex < 0) return;

    // RestoreShot 호출
    // InspectionSequence 접근 경로는 구현 시 확인 필요
    bool ok = /* InspectionSeq.RecipeManager.RestoreShot(shotIndex) */ false;
    if (!ok)
    {
        CustomMessageBox.Show("Reset", "백업 데이터가 없습니다. 레시피를 먼저 로드하세요.", MessageBoxImage.Warning);
        return;
    }

    // PropertyGrid 강제 갱신 (Paste 패턴 재사용)
    int index = treeListBox_sequence.SelectedIndex;
    treeListBox_sequence.UnselectAll();
    treeListBox_sequence.SelectedIndex = index;
    if (SelectedParam is InspectionParam ip)
        mParentWindow.mainView.SetParam(ESequence.Inspection, ip);

    mParentWindow.statusBar.Model.SetText($"Reset Shot_{shotIndex + 1} 완료");
}
```

## Open Questions

1. **InspectionRecipeManager 접근 경로**
   - What we know: `InspectionRecipeManager`는 `InspectionSequence`의 멤버일 가능성이 높으나, 이 클래스 파일을 직접 확인하지 않았다.
   - What's unclear: `SystemHandler.Handle.Sequences[ESequence.Inspection]`을 캐스팅하면 `InspectionSequence`가 나오는지, `RecipeManager` 프로퍼티가 public인지.
   - Recommendation: 구현 Wave 0에서 `InspectionSequence.cs` 파일 확인 필수. `SequenceBase` 캐스팅 패턴 또는 `SystemHandler` 접근자 추가 고려.

2. **undo.png 리소스 존재 여부**
   - What we know: `/Resource/` 폴더에 `copy.png`, `paste.png`, `camera.png`, `folder.png` 등이 존재한다.
   - What's unclear: `undo.png` 또는 `refresh.png`가 있는지 확인 안 됨.
   - Recommendation: 구현 시 리소스 폴더 확인 후 적절한 아이콘 선택. 없으면 텍스트만으로 충분.

3. **확인 다이얼로그 여부 (Claude 재량)**
   - What we know: Paste 버튼은 `CustomMessageBox.ShowConfirmation()`을 사용한다.
   - What's unclear: Reset도 같은 패턴이 필요한지는 Claude 재량 영역.
   - Recommendation: 오작동 방지를 위해 Paste와 동일하게 `MessageBoxButton.OKCancel` 확인 다이얼로그 추가를 권장.

## Project Constraints (from CLAUDE.md)

CLAUDE.md가 존재하지 않음. 대신 MEMORY.md 및 accumulated context에서 추출:

| 제약 | 출처 |
|------|------|
| 주석 형식: `//YYMMDD hbk` (예: `//260407 hbk`) | MEMORY.md feedback_comments.md |
| FAI/Halcon/에지 측정 추가 절대 금지 | MEMORY.md feedback_no_fai.md, REQUIREMENTS.md Out of Scope |
| 신규 NuGet 패키지 추가 금지 | STATE.md Decisions v2.0 |
| XAML 주석에 `--` 사용 금지 (MC3000) | STATE.md Phase 12 accumulated context |
| `FinalVision.csproj` explicit Compile includes 필요 | STATE.md Phase 11 accumulated context |

## Environment Availability

Step 2.6: SKIPPED — 이 Phase는 코드/XAML 변경만 포함. 외부 런타임 의존성 없음.

## Validation Architecture

`workflow.nyquist_validation` 키가 `.planning/config.json`에 없음 → 활성화 상태로 처리.

이 프로젝트는 WPF 데스크톱 애플리케이션이며 별도의 자동화 테스트 프레임워크(xUnit, NUnit 등)가 코드베이스에 없다.

### Test Framework
| Property | Value |
|----------|-------|
| Framework | 없음 — WPF 수동 검증 |
| Config file | 해당 없음 |
| Quick run command | 빌드 + 수동 UI 조작 |
| Full suite command | 빌드 성공 확인 |

### Phase Requirements → Test Map
| Req ID | Behavior | Test Type | Automated Command | File Exists? |
|--------|----------|-----------|-------------------|--------------|
| RCP-05 (D-01) | 레시피 로드 후 _backup에 Shot 파라미터 저장됨 | manual | 빌드 후 레시피 로드 → HasBackup 확인 | ❌ Wave 0 불필요 (수동) |
| RCP-05 (D-02) | 툴바에 Reset 버튼 표시됨 | manual | 빌드 + UI 확인 | ❌ Wave 0 불필요 (수동) |
| RCP-05 (D-03) | Shot 선택 후 Reset → 해당 Shot 파라미터만 복원, 나머지 Shot 불변 | manual | 빌드 후 파라미터 편집 → Reset 클릭 → 값 확인 | ❌ Wave 0 불필요 (수동) |

### Wave 0 Gaps
- None — 자동화 테스트 인프라 불필요. 빌드 성공 + 수동 검증으로 충분.

## Sources

### Primary (HIGH confidence)
- 직접 코드 분석: `WPF_Example/UI/ControlItem/InspectionListView.xaml` — 툴바 구조 (라인 182~238)
- 직접 코드 분석: `WPF_Example/UI/ControlItem/InspectionListView.xaml.cs` — Copy/Paste/Grab 버튼 패턴
- 직접 코드 분석: `WPF_Example/Custom/Sequence/Inspection/ShotConfig.cs` — CopyTo 구현 (라인 129~139)
- 직접 코드 분석: `WPF_Example/Custom/Sequence/Inspection/InspectionRecipeManager.cs` — Load/Save 구조
- 직접 코드 분석: `WPF_Example/Sequence/SequenceHandler.cs` — LoadFromIni 두 오버로드, ExecOnLoad 흐름
- 직접 코드 분석: `WPF_Example/Sequence/Param/ParamBase.cs` — CopyTo 기반 클래스
- 직접 코드 분석: `WPF_Example/SystemHandler.cs` — LoadRecipe / ProcessRecipeChange 진입점

### Secondary (MEDIUM confidence)
- `WPF_Example/Custom/SystemHandler.cs` 라인 155~178 — ProcessRecipeChange TCP 경로 확인 (grep 결과)
- `.planning/STATE.md` accumulated context — Phase 12 패턴 (MC3000, VisualTreeHelper, IsIdle guard)

### Tertiary (LOW confidence)
- `InspectionSequence.cs` 내부 구조 — 직접 확인 안 됨 (Open Question 1)
- `/Resource/undo.png` 존재 여부 — 직접 확인 안 됨 (Open Question 2)

## Metadata

**Confidence breakdown:**
- Standard stack: HIGH — 신규 패키지 없음, 기존 코드 직접 확인
- Architecture (백업 구조): HIGH — CopyTo 패턴이 이미 존재하고 동작 검증됨
- Architecture (백업 시점): HIGH — ExecOnLoad 흐름이 두 LoadFromIni 오버로드를 모두 커버함을 코드로 확인
- Architecture (PropertyGrid 갱신): HIGH — Paste 패턴이 이미 동작 중인 것을 코드로 확인
- Pitfalls: HIGH — 코드 직접 분석 기반
- InspectionSequence 접근 경로: LOW — 파일 미확인

**Research date:** 2026-04-07
**Valid until:** 2026-05-07 (코드베이스 안정적, 30일)
