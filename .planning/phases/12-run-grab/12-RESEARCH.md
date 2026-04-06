# Phase 12: Run/Grab 역할 분리 + 이미지 로드/삭제 - Research

**Researched:** 2026-04-06
**Domain:** WPF C# — Button role separation, FolderBrowserDialog image load, Directory.Delete UI
**Confidence:** HIGH (all findings from direct source-code inspection)

---

<user_constraints>
## User Constraints (from CONTEXT.md)

No CONTEXT.md exists for Phase 12. Constraints come from STATE.md Decisions and project-wide rules.

### Locked Decisions (from STATE.md)
- 신규 NuGet 패키지 추가 금지 — 기존 PropertyTools.Wpf, Ookii.Dialogs.Wpf로 충분
- HIK 전용 (Basler 제거), PLC 미사용 (TCP/IP 전용), OpenCvSharp FindContours
- FAI/Halcon/에지 측정 절대 추가 금지 (Blob 유무 검사 프로젝트)

### Claude's Discretion
- 이미지 삭제 UI 위치 (ShotTabView 내부 vs 별도 창) — 단순 구현으로 ShotTabView 내 또는 MainWindow 메뉴
- Run 버튼 배치 위치 (ShotTabView 툴바 내 기존 Grab 옆에 추가)

### Deferred Ideas (OUT OF SCOPE)
- 통계 대시보드, 딥러닝, PLC, 레시피 버전관리, 택타임 트렌드 그래프
- Phase 13 (RecipeEditorWindow) 기능 일절 포함 금지
</user_constraints>

<phase_requirements>
## Phase Requirements

| ID | Description | Research Support |
|----|-------------|------------------|
| OPS-02 | Grab 버튼은 카메라 촬상+검사, Run 버튼은 로드된 이미지로 검사 테스트로 역할 분리 | ShotTabView.Btn_Grab_Click 분석 완료 — Run 경로(SimulImagePath→RunBlobOnLastGrab)와 Grab 경로(GrabImage) 분리 패턴 확인 |
| IMG-03 | 시간 폴더 선택 시 Shot1~5 이미지를 일괄 로드하여 UI에 표시 | Ookii.Dialogs.Wpf FolderBrowserDialog 사용 가능, SimulImagePath 파일명 매핑 규칙 정의 필요 |
| IMG-04 | 날짜/시간 폴더 단위로 저장된 검사 이미지 삭제 가능 | Directory.Delete(path, recursive:true) + CustomMessageBox.ShowConfirmation 패턴 확인 |
</phase_requirements>

---

## Summary

Phase 12는 세 개의 독립적인 기능을 구현한다. 신규 라이브러리 없이 기존 코드만으로 구현 가능하다.

**첫째**, ShotTabView의 "Grab" 버튼 하나가 현재 두 가지 역할(카메라 촬상 vs 로드 이미지 사용)을 혼용한다. `Btn_Grab_Click` 내부에서 `SimulImagePath` 유무로 분기하는데, 이 분기를 UI 레벨로 끌어올려 "Grab" = 항상 카메라 촬상+검사, "Run" = 항상 로드 이미지로 검사로 명확히 나눈다.

**둘째**, 현재 Shot별 이미지를 OpenFileDialog로 한 장씩 선택한다(Btn_OpenImage_Click). IMG-03은 Ookii.Dialogs.Wpf의 `VistaFolderBrowserDialog`로 시간폴더 전체를 선택하고, 폴더 내 파일명 패턴(`{ShotName}_{OK|NG}_{ss_fff}.bmp`)으로 Shot1~5를 일괄 매핑한다.

**셋째**, IMG-04의 삭제 기능은 새 창이 아닌 단순 버튼+확인 다이얼로그(`CustomMessageBox.ShowConfirmation`)로 구현한다. 날짜폴더(`yyyyMMdd`) 또는 시간폴더(`HHmm`) 단위로 `Directory.Delete(path, recursive:true)`를 호출한다.

**Primary recommendation:** ShotTabView XAML에 Run 버튼을 추가하고, Grab/Run 로직을 코드비하인드에서 명확히 분리하라. 이미지 로드/삭제는 별도 UI 패널(예: 기존 ShotTabView 툴바 오른쪽 영역 확장)로 구현하라.

---

## Standard Stack

### Core
| Library | Version | Purpose | Why Standard |
|---------|---------|---------|--------------|
| Ookii.Dialogs.Wpf | 기존 설치 | VistaFolderBrowserDialog — 시간폴더 선택 | STATE.md 잠금 결정: 신규 NuGet 금지, 이미 사용 중 |
| OpenCvSharp | 기존 설치 | Cv2.ImRead — 이미지 파일 로드 | 이미 ShotTabView.Btn_OpenImage_Click에서 사용 |
| System.IO | .NET 기본 | Directory.Delete, Directory.GetFiles | OS 파일시스템 작업, 외부 의존 없음 |

### Supporting
| Library | Version | Purpose | When to Use |
|---------|---------|---------|-------------|
| Microsoft.Win32.OpenFileDialog | .NET 기본 | Shot별 단일 파일 선택 (기존 유지) | Run 버튼 전환 후에도 Shot별 수동 지정 경로로 유지 |
| CustomMessageBox (프로젝트 내부) | — | 확인 다이얼로그 | 삭제 전 사용자 확인 필수 |

### Alternatives Considered
| Instead of | Could Use | Tradeoff |
|------------|-----------|----------|
| Ookii VistaFolderBrowserDialog | System.Windows.Forms.FolderBrowserDialog | Forms 의존 추가 필요 — Ookii가 이미 설치되어 있으므로 Ookii 사용 |
| Directory.Delete recursive | 파일별 File.Delete 루프 | 단순성 면에서 Directory.Delete가 우월; 단, 삭제 실패 시 예외 처리 필요 |

**Installation:** 신규 패키지 불필요 — 기존 의존성만 사용.

---

## Architecture Patterns

### 현재 코드 구조 파악

```
ShotTabView.xaml.cs
  ├── Btn_Grab_Click          — 현재: SimulImagePath 있으면 파일 로드, 없으면 카메라 GrabImage
  ├── Btn_OpenImage_Click     — Shot별 OpenFileDialog로 단일 이미지 선택
  ├── Btn_DeleteImage_Click   — SimulImagePath 초기화 + SetOriginalImage(null)
  └── RefreshImage()          — LastOriginalImage/LastAnnotatedImage 표시

Action_Inspection.Run()
  ├── EStep.Grab              — _GrabbedImage = camera.GrabImage() [실제모드]
  │                           — _GrabbedImage = param.GetOriginalImageClone() ?? camera.GrabImage() [SIMUL]
  └── RunBlobOnLastGrab()     — LastOriginalImage 기반으로 Blob 즉시 실행 (SIMUL 방식)

InspectionSequenceContext.Clear()
  └── ImageFolderManager.BeginInspection() — 검사 시작마다 시간폴더 생성
```

### Pattern 1: Grab/Run 버튼 역할 분리 (OPS-02)

**What:** Grab 버튼은 카메라 촬상+검사 전용. Run 버튼은 미리 로드된 이미지(SimulImagePath)로 검사 전용.

**현재 Grab_Click 로직 문제:**
```csharp
// 현재 — 한 버튼이 두 역할
if (!string.IsNullOrEmpty(param.SimulImagePath) && File.Exists(param.SimulImagePath))
    grabbed = Cv2.ImRead(param.SimulImagePath, ...);   // 로드 이미지 경로
else
    grabbed = _pDev.GrabImage(camParam);               // 카메라
```

**Phase 12 목표 분리:**
```csharp
// Grab 버튼 — 항상 카메라 촬상
private async void Btn_Grab_Click(object sender, RoutedEventArgs e)
{
    // SimulImagePath 여부와 무관하게 항상 카메라에서 GrabImage
    _pLight.ApplyLight(camParam);
    grabbed = _pDev.GrabImage(camParam);
    camParam.PutImage(grabbed);
    param.SetOriginalImage(grabbed);
    // SIMUL_MODE: RunBlobOnLastGrab 호출
}

// Run 버튼 (신규) — 로드된 이미지로 검사
private void Btn_Run_Click(object sender, RoutedEventArgs e)
{
    //260406 hbk — Run: 로드된 이미지(SimulImagePath)가 없으면 비활성 또는 경고
    if (string.IsNullOrEmpty(param.SimulImagePath)) {
        CustomMessageBox.Show("이미지 없음", "먼저 이미지를 로드하세요.", MessageBoxImage.Warning);
        return;
    }
    var mat = Cv2.ImRead(param.SimulImagePath, ImreadModes.Color);
    param.SetOriginalImage(mat);
    mat?.Dispose();
    var seq = _pSeq[ESequence.Inspection];
    if (seq != null && seq[_shotIndex] is Action_Inspection act)
        act.RunBlobOnLastGrab();
    RefreshImage();
    UpdateResultLabel();
}
```

**Run 버튼 활성화 조건:** `SimulImagePath`가 비어있지 않고 파일이 존재할 때만 활성. `RefreshImage()` 호출 시 버튼 상태 갱신.

**When to use:** Run = 이전에 로드된 이미지로 검사 재실행. Grab = 카메라에서 새로 촬상.

### Pattern 2: 시간폴더 일괄 로드 (IMG-03)

**What:** Ookii VistaFolderBrowserDialog로 시간폴더(`HHmm`)를 선택하면, 폴더 내 파일을 Shot1~5에 매핑한다.

**파일명 패턴 (Phase 11에서 확정):**
```
{folderPath}\{ShotName}_{OK|NG}_{ss_fff}.bmp   ← GetOriginSavePath 형식
{folderPath}\{ShotName}_{OK|NG}_capture_{ss_fff}.jpg  ← GetCaptureSavePath 형식
```

**ShotName은 Action 이름 (= `Name` 속성)** — `Action_Inspection` 생성 시 전달되는 name (예: `Shot1`, `Shot2`, ... `Shot5`). 실제 이름은 코드에서 확인 필요.

**매핑 로직:**
```csharp
// 신규 버튼: Btn_LoadFolder_Click (ShotTabView or 별도 위치)
private void Btn_LoadFolder_Click(object sender, RoutedEventArgs e)
{
    //260406 hbk — IMG-03: Ookii FolderBrowserDialog로 시간폴더 선택
    var dlg = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog {
        Description = "시간 폴더 선택 (HHmm)",
        UseDescriptionForTitle = true,
    };
    if (dlg.ShowDialog() != true) return;
    string folder = dlg.SelectedPath;

    var seq = _pSeq[ESequence.Inspection];
    if (seq == null) return;

    for (int i = 0; i < seq.ActionCount; i++) {
        var act = seq[i] as Action_Inspection;  // null-safe
        if (act == null) continue;
        var p = act.Param as InspectionParam;
        if (p == null) continue;

        // {ShotName}_*.bmp 검색 (원본 이미지 우선)
        string[] files = Directory.GetFiles(folder, p.ProcessName + "_*.bmp");
        if (files.Length == 0)
            files = Directory.GetFiles(folder, p.ProcessName + "_*_capture_*.jpg");
        if (files.Length == 0) continue;

        // 가장 최근 파일 선택 (동일 초 내 복수 파일 대비)
        string best = files.OrderByDescending(f => f).First();
        p.SimulImagePath = best;
        var mat = Cv2.ImRead(best, ImreadModes.Color);
        p.SetOriginalImage(mat);
        mat?.Dispose();
    }

    // 전체 ShotTabView 갱신 요청 — 각 탭 RefreshImage 필요
    // MainWindow 또는 부모 View에서 BroadcastRefresh() 호출
}
```

**중요 제약:** `LoadFolder` 버튼은 ShotTabView(Shot별 탭)가 아닌 상위 UI(MainView 또는 툴바 공통 영역)에 배치해야 5개 Shot을 한번에 로드할 수 있다. ShotTabView 내부에서 로드하면 현재 탭의 Shot만 처리된다.

**Alternative:** ShotTabView 내부에 배치하되 `_pSeq[ESequence.Inspection]` 전체 Action을 순회하면 모든 Shot 처리 가능 — 현재 코드 패턴과 일치하므로 이 방법 권장.

### Pattern 3: 이미지 폴더 삭제 UI (IMG-04)

**What:** 날짜폴더(`yyyyMMdd`) 또는 시간폴더(`HHmm`) 단위로 확인 다이얼로그 후 삭제.

```csharp
// Btn_DeleteFolder_Click — 날짜 또는 시간폴더 삭제
private void Btn_DeleteFolder_Click(object sender, RoutedEventArgs e)
{
    //260406 hbk — IMG-04: 폴더 삭제 (날짜/시간 단위)
    var dlg = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog {
        Description = "삭제할 날짜 또는 시간 폴더 선택",
        UseDescriptionForTitle = true,
        SelectedPath = SystemSetting.Handle.ImageSavePath,
    };
    if (dlg.ShowDialog() != true) return;
    string folder = dlg.SelectedPath;

    var result = CustomMessageBox.ShowConfirmation(
        "폴더 삭제 확인",
        $"'{Path.GetFileName(folder)}' 폴더를 삭제합니까?\n(하위 파일 모두 삭제됨)",
        MessageBoxButton.YesNo);
    if (result != MessageBoxResult.Yes) return;

    try {
        Directory.Delete(folder, recursive: true);   //260406 hbk
        Logging.PrintLog((int)ELogType.Trace, "[IMG] 폴더 삭제 완료: {0}", folder);
    }
    catch (Exception ex) {
        CustomMessageBox.Show("삭제 실패", ex.Message, MessageBoxImage.Error);
    }
}
```

**배치 위치:** 삭제 기능은 Shot별 탭과 무관하므로 MainView 툴바 또는 SystemSetting 창에 별도 배치가 자연스럽다. 계획(12-03)에 따라 별도 삭제 UI로 구현.

### Anti-Patterns to Avoid
- **`Btn_Grab_Click`에서 SimulImagePath 분기 유지:** 분리 이후 Grab 버튼에는 SimulImagePath 조건문을 두지 않는다. 역할 혼용 재발 방지.
- **Run 버튼에서 카메라 GrabImage 호출:** Run은 절대 카메라에 접근하지 않는다.
- **Directory.Delete 전 확인 생략:** 데이터 손실 방지를 위해 항상 CustomMessageBox.ShowConfirmation 후 삭제.
- **시간폴더 선택 시 파일명 매핑 실패 무시:** 매핑 실패한 Shot은 SimulImagePath를 변경하지 않는다 (기존 값 유지).
- **Run 버튼 항상 활성:** SimulImagePath 없을 때 Run 버튼 비활성화 또는 경고 필수.

---

## Don't Hand-Roll

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| 폴더 선택 다이얼로그 | 커스텀 트리뷰 폴더 선택 UI | `Ookii.Dialogs.Wpf.VistaFolderBrowserDialog` | 이미 프로젝트에 설치됨, 네이티브 Vista 스타일 |
| 삭제 확인 | 커스텀 YesNo 창 | `CustomMessageBox.ShowConfirmation` | 프로젝트 표준 패턴 (InspectionListView.xaml.cs에서 동일 사용) |
| 파일 패턴 매핑 | 복잡한 파서 | `Directory.GetFiles(folder, pattern)` + LINQ OrderByDescending | System.IO 기본 기능으로 충분 |

**Key insight:** 세 기능 모두 기존 코드 패턴의 조합으로 구현 가능하다. 새 아키텍처나 라이브러리가 필요 없다.

---

## Common Pitfalls

### Pitfall 1: Step 카운터 리셋 시점 (STATE.md Blocker)
**What goes wrong:** Run 버튼으로 검사 실행 시 `Action_Inspection.Run()`의 `Step` 카운터가 이전 상태로 남아있으면 `EStep.Grab`을 건너뛰고 엉뚱한 Step부터 시작한다.
**Why it happens:** `RunBlobOnLastGrab()`은 시퀀스 상태머신 외부에서 직접 호출된다. `OnBegin()`이 호출되지 않으므로 Step이 리셋되지 않는다.
**How to avoid:** `RunBlobOnLastGrab()`은 Step 상태에 의존하지 않고 독립적으로 Blob+SaveImage+FinishAction을 직접 수행하는 구조다 — 이미 올바르게 설계됨. 단, Run 버튼 구현 시 시퀀스 `Start()`를 호출하지 말고 `RunBlobOnLastGrab()` 직접 호출 방식을 유지해야 한다.
**Warning signs:** Run 실행 후 `label_result`가 갱신되지 않거나 SaveImage가 이중 실행됨.

### Pitfall 2: 시간폴더 일괄 로드 시 ProcessName vs Name 불일치
**What goes wrong:** 파일명 패턴에서 Shot 이름이 `Action.Name`인데 `InspectionParam.ProcessName`과 다를 경우 매핑 실패.
**Why it happens:** `ProcessName`은 `OnLoad()`에서 `Param.OwnerName`으로 설정된다. 레시피 로드 전이나 OnLoad 호출 전에는 빈 문자열일 수 있다.
**How to avoid:** 매핑 시 `Action_Inspection.Name` (= `ActionBase.Name`) 을 기준으로 검색한다. `ProcessName`이 설정된 이후라면 동일값이나, 안전을 위해 `seq[i].Name`을 사용.
**Warning signs:** `Directory.GetFiles` 결과가 항상 빈 배열.

### Pitfall 3: Directory.Delete 중 이미지가 열려있는 경우
**What goes wrong:** ShotTabView의 `_bgStream`이 삭제 대상 폴더의 이미지를 메모리 스트림으로 참조 중이면 `Directory.Delete`가 `IOException`으로 실패한다.
**Why it happens:** `DisplayToBackground()`에서 `_bgStream`이 파일의 내용을 `OnLoad`로 캐시하므로 파일 핸들은 없지만, 드물게 잠금이 발생할 수 있다.
**How to avoid:** WPF `BitmapCacheOption.OnLoad`는 파일 핸들을 즉시 닫으므로 실제로는 문제 없다. 단, 삭제 전 `try/catch IOException`을 반드시 포함하고 오류 메시지를 표시.
**Warning signs:** 삭제 버튼 클릭 후 "파일이 다른 프로세스에서 사용 중" 오류.

### Pitfall 4: Run 버튼과 TCP 자동 검사 충돌
**What goes wrong:** TCP로 검사 요청이 들어오는 중에 Run 버튼을 누르면 `_pSeq.IsIdle` 체크를 통과하지 못한다.
**Why it happens:** `RunBlobOnLastGrab()`은 시퀀스 외부 직접 호출이므로 `IsIdle` 여부를 체크하지 않으면 동시 실행될 수 있다.
**How to avoid:** Run 버튼 클릭 핸들러 진입 시 `_pSeq.IsIdle` 확인 — 기존 Grab 버튼과 동일 패턴. 비활성 시 반환.

### Pitfall 5: ImageFolderManager.BeginInspection() — Run 모드에서 불필요 폴더 생성
**What goes wrong:** Run 버튼이 시퀀스 `Start()`를 통해 실행되면 `InspectionSequenceContext.Clear()`가 호출되어 `BeginInspection()`이 실행되고 빈 시간폴더가 생성된다.
**Why it happens:** `Clear()`는 `Start()` 내부에서 항상 호출된다.
**How to avoid:** Run 모드는 반드시 `RunBlobOnLastGrab()` 직접 호출로만 구현한다. 시퀀스 `Start()`를 통하지 않으면 `Clear()`가 호출되지 않는다.

---

## Code Examples

### Ookii VistaFolderBrowserDialog 사용 패턴
```csharp
// Source: 기존 프로젝트 RecipeFiles 등에서 확인된 패턴
var dlg = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog {
    Description = "시간 폴더 선택",
    UseDescriptionForTitle = true,
    SelectedPath = SystemSetting.Handle.ImageSavePath,
};
if (dlg.ShowDialog() == true) {
    string folder = dlg.SelectedPath;
    // 처리 로직
}
```

### 기존 CustomMessageBox.ShowConfirmation 패턴
```csharp
// Source: InspectionListView.xaml.cs line 85
var result = CustomMessageBox.ShowConfirmation(
    "제목", "메시지 내용", MessageBoxButton.YesNo);
if (result != MessageBoxResult.Yes) return;
```

### RunBlobOnLastGrab 직접 호출 패턴 (기존 확인)
```csharp
// Source: ShotTabView.xaml.cs Btn_OpenImage_Click
var seq = _pSeq[ESequence.Inspection];
if (seq != null && seq[_shotIndex] is Action_Inspection act)
    act.RunBlobOnLastGrab();
RefreshImage();
UpdateResultLabel();
```

### Shot 이름 기반 파일 검색 패턴
```csharp
// ImageFolderManager.GetOriginSavePath 형식:
// {folderPath}\{shotName}_{OK|NG}_{ss_fff}.bmp
string[] files = Directory.GetFiles(folder, act.Name + "_*.bmp");
if (files.Length > 0) {
    string latest = files.OrderByDescending(f => f).First();
    param.SimulImagePath = latest;
    using (var mat = Cv2.ImRead(latest, ImreadModes.Color)) {
        param.SetOriginalImage(mat);
    }
}
```

---

## Runtime State Inventory

> 이 Phase는 rename/refactor 없음 — Runtime State Inventory 해당 없음.

---

## Environment Availability

| Dependency | Required By | Available | Version | Fallback |
|------------|------------|-----------|---------|----------|
| Ookii.Dialogs.Wpf | IMG-03, IMG-04 폴더 선택 | 확인 필요 (STATE.md에서 승인됨) | 기존 설치 | System.Windows.Forms.FolderBrowserDialog (비권장) |
| OpenCvSharp | IMG-03 이미지 로드 | 확인됨 | 기존 설치 | — |
| System.IO | IMG-04 Directory.Delete | 확인됨 (.NET 기본) | — | — |

**확인 사항:** Ookii.Dialogs.Wpf가 실제 프로젝트 참조에 포함되어 있는지 .csproj에서 확인 권장. STATE.md에 "기존 PropertyTools.Wpf, Ookii.Dialogs.Wpf로 충분"이라 명시되어 있으므로 설치된 것으로 간주함.

---

## Validation Architecture

> config.json에 `workflow.nyquist_validation` 키 없음 → 포함 처리.

이 Phase는 UI 버튼 동작과 파일시스템 작업이 주다. 자동화 테스트보다 수동 확인이 적합하다.

### Test Framework
| Property | Value |
|----------|-------|
| Framework | 없음 (WPF 수동 검증, 단위 테스트 인프라 없음) |
| Config file | 없음 |
| Quick run command | 빌드 후 UI 수동 클릭 |
| Full suite command | 빌드 후 전체 시나리오 수동 실행 |

### Phase Requirements → Test Map
| Req ID | Behavior | Test Type | Automated Command | File Exists? |
|--------|----------|-----------|-------------------|-------------|
| OPS-02 | Grab 클릭 → 카메라 촬상 실행 | manual | — | ❌ 수동만 |
| OPS-02 | Run 클릭(이미지 있음) → RunBlobOnLastGrab 실행 | manual | — | ❌ 수동만 |
| OPS-02 | Run 클릭(이미지 없음) → 경고 표시, 카메라 미접근 | manual | — | ❌ 수동만 |
| IMG-03 | 시간폴더 선택 → Shot1~5 SimulImagePath 매핑 | manual | — | ❌ 수동만 |
| IMG-03 | 매핑 후 각 ShotTabView 이미지 표시 갱신 | manual | — | ❌ 수동만 |
| IMG-04 | 날짜폴더 삭제 → Directory.Delete 성공 | manual | — | ❌ 수동만 |
| IMG-04 | 취소 클릭 → 삭제 안 됨 | manual | — | ❌ 수동만 |

### Sampling Rate
- **Per task:** 빌드 성공 확인 + 해당 버튼 수동 클릭
- **Per wave:** 전체 시나리오 수동 실행 (Grab→Run→LoadFolder→DeleteFolder 순서)
- **Phase gate:** 4가지 Success Criteria 모두 수동 확인 후 완료 처리

### Wave 0 Gaps
없음 — 테스트 파일 구조 없음. 수동 검증 전용.

---

## State of the Art

| Old Approach | Current Approach | When Changed | Impact |
|--------------|------------------|--------------|--------|
| Grab 버튼이 카메라/파일 두 역할 혼용 | Grab=카메라 전용, Run=파일 전용 | Phase 12 | 역할 명확화, 현장 오조작 방지 |
| Shot별 이미지 파일을 개별 OpenFileDialog로 선택 | 시간폴더 선택 → Shot1~5 일괄 매핑 | Phase 12 | 작업 속도 향상 (5번 클릭 → 1번) |

---

## Open Questions

1. **Shot Action 이름 (`Action_Inspection.Name`)의 실제 값**
   - What we know: `Action_Inspection` 생성 시 `name` 파라미터로 전달됨. `ProcessName`은 `OnLoad()`에서 `Param.OwnerName`으로 설정.
   - What's unclear: 실제 이름이 `Shot1`인지 `Bolt_One`인지 정확히 모름 — Sequence_Inspection 생성자 호출 코드(`SystemHandler` 초기화 부분)에서 확인 필요.
   - Recommendation: Plan 12-02 작업 시 `SystemHandler` 초기화 코드에서 `Action_Inspection` 생성 시 전달되는 name 파라미터 확인 후 GetFiles 패턴 문자열 확정.

2. **Run 버튼 배치 위치 — ShotTabView 내부 vs 공통 영역**
   - What we know: `LoadFolder`(IMG-03)는 5개 Shot 모두 처리하므로 공통 영역이 자연스럽다. 그러나 `ShotTabView` 내부에서도 `_pSeq` 전체 순회로 구현 가능하다.
   - What's unclear: MainView 툴바에 배치하면 UI가 어디에 연결되는지 (MainWindow 구조 추가 확인 필요).
   - Recommendation: Plan 12-01(Run 버튼)은 ShotTabView 내 Grab 옆에 추가. Plan 12-02(폴더 로드)는 ShotTabView 내 Shot 루프 순회로 구현 — 현재 코드 패턴과 일치하고 별도 창 불필요.

3. **Step 카운터 리셋 (STATE.md Blocker 항목)**
   - What we know: STATE.md가 "Action_Inspection.Run() 상태머신 내부 초기화 위치 확인 필요"를 Blocker로 명시함.
   - What's unclear: Run 모드(`RunBlobOnLastGrab`)는 상태머신(`Step` 카운터) 외부이므로 이 Blocker는 Run 버튼 구현에는 해당 없다. 단, Grab 버튼이 시퀀스 `Start()`를 통해 실행될 경우 Step 리셋은 `OnBegin()`에서 처리됨 — `ActionBase.OnBegin()`이 Step 리셋하는지 확인 권장.
   - Recommendation: Plan 12-01에서 Grab 버튼이 `_pSeq.Start(EAction.xxx)` 경로를 쓰지 않고 기존 직접 GrabImage 패턴을 유지한다면 Blocker 해당 없음.

---

## Sources

### Primary (HIGH confidence)
- `WPF_Example/UI/ContentItem/ShotTabView.xaml.cs` — Btn_Grab_Click, Btn_OpenImage_Click, RefreshImage, RunBlobOnLastGrab 호출 패턴 직접 확인
- `WPF_Example/Custom/Sequence/Inspection/Action_Inspection.cs` — EStep 상태머신, RunBlobOnLastGrab, SimulImagePath 분기 직접 확인
- `WPF_Example/Custom/Sequence/Inspection/Sequence_Inspection.cs` — InspectionSequenceContext.Clear() → ImageFolderManager.BeginInspection() 직접 확인
- `WPF_Example/Utility/ImageFolderManager.cs` — GetOriginSavePath/GetCaptureSavePath 파일명 패턴 직접 확인
- `WPF_Example/UI/ControlItem/InspectionListView.xaml.cs` — CustomMessageBox.ShowConfirmation 사용 패턴 직접 확인
- `.planning/STATE.md` — 잠금 결정 및 Blocker 항목 확인

### Secondary (MEDIUM confidence)
- `WPF_Example/Setting/SystemSetting.cs` — ImageSavePath, SaveOriginImage/SaveGoodImage/SaveNGImage 설정 확인
- `.planning/REQUIREMENTS.md` — OPS-02, IMG-03, IMG-04 요구사항 원문 확인

---

## Metadata

**Confidence breakdown:**
- Standard stack: HIGH — 신규 라이브러리 없음, 기존 코드만 사용
- Architecture: HIGH — 소스 코드 직접 분석 기반, 기존 패턴 연장선
- Pitfalls: HIGH — 실제 코드 흐름에서 도출된 구체적 위험요소

**Research date:** 2026-04-06
**Valid until:** 2026-05-06 (코드베이스 변경 시 재검토)
