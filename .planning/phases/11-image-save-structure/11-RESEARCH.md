# Phase 11: 이미지 저장 구조 개선 - Research

**Researched:** 2026-04-03
**Domain:** C# WPF 이미지 파일 저장 / 디렉터리 경로 관리 / 검사 시퀀스 통합
**Confidence:** HIGH

---

<user_constraints>
## User Constraints (from CONTEXT.md)

### Locked Decisions

- **D-01:** 기본 경로를 `SystemSetting.ImageSavePath`로 변경. 기본값을 `D:\Log`로 설정. PropertyGrid에서 변경 가능
- **D-02:** 시간폴더 단위는 검사 1회 = 1폴더. TCP $TEST 수신 시점에 시간폴더를 생성하고 Shot1~5 이미지가 같은 폴더에 저장
- **D-03:** 최종 경로 형식: `{ImageSavePath}\{yyyyMMdd}\{HHmmss_fff}\{ShotName}_{OK|NG}.jpg`
- **D-04:** Annotated 이미지 파일명: `{ShotName}_{OK|NG}_annotated.jpg` (같은 폴더에 저장)
- **D-05:** 기존 `SaveOkImage=false`, `SaveNgImage=true` 동작 유지
- **D-06:** 원본(GrabbedImage) + Annotated(측정 오버레이) 둘 다 저장. 현재 원본 저장 로직 유지하고 Annotated 저장을 추가
- **D-07:** OK/NG 필터는 원본/Annotated 쌍 단위로 적용 (OK 미저장 시 원본+Annotated 모두 미저장)
- **D-08:** 시간폴더명에 밀리초 포함 (`HHmmss_fff`). 동일 밀리초 충돌 시 `_2`, `_3` 접미사 추가
- **D-09:** 검사 1회 시작 시점에 폴더명을 확정하고 Shot1~5 전체가 같은 폴더명 사용
- **D-10:** `WPF_Example/Utility/ImageFolderManager.cs` 신규 생성
- **D-11:** 범위는 경로 생성만 담당 — `GetSavePath()` 메서드 제공. 디스크 정리/조회는 Phase 12에서 추가
- **D-12:** `D:\Log` 하드코딩을 `SystemSetting.ImageSavePath` 참조로 교체

### Claude's Discretion

- ImageFolderManager 내부 메서드 시그니처 및 충돌 방지 구현 세부
- Action_Inspection.SaveResultImage()에서 Annotated 저장 추가 방식
- 검사 시작 시점에 폴더명을 전달하는 메커니즘 (SequenceContext 활용 등)

### Deferred Ideas (OUT OF SCOPE)

- 디스크 정리 (오래된 이미지 자동 삭제) — Phase 12에서 ImageFolderManager에 추가
- 이미지 폴더 조회/로드 UI — Phase 12 범위
- 이미지 삭제 UI — Phase 12 범위
</user_constraints>

---

<phase_requirements>
## Phase Requirements

| ID | Description | Research Support |
|----|-------------|------------------|
| IMG-01 | 검사 이미지를 날짜>시간 하위폴더 구조로 저장 (`D:\Log\{yyyyMMdd}\{HHmmss}\{ShotName}_{OK|NG}.jpg`) | ImageFolderManager.BeginInspection() + GetSavePath() 패턴으로 구현. D-03 경로 형식 확정 |
| IMG-02 | OK 이미지 기본 미저장, NG 이미지만 기본 저장 (설정에서 변경 가능) | SystemSetting.SaveOkImage(false) / SaveNgImage(true) 이미 구현됨. 기존 if 가드 유지하고 Annotated 저장에 동일 가드 적용 |
</phase_requirements>

---

## Summary

Phase 11은 두 가지 명확한 작업으로 구성된다. 첫째, `ImageFolderManager` 신규 클래스를 생성하여 날짜>시간 계층 경로 생성을 캡슐화하고 충돌 방지 로직을 내장한다. 둘째, `Action_Inspection.SaveResultImage()`를 수정하여 하드코딩된 `D:\Log` 경로를 제거하고 Annotated 이미지 저장을 추가한다.

핵심 설계 과제는 "검사 1회 = 1 시간폴더" 보장이다. Shot1~5가 순차 실행되므로 각 Shot의 `SaveResultImage()` 호출 시점에 별도로 `DateTime.Now`를 찍으면 폴더명이 달라질 수 있다. 따라서 폴더명은 검사 시작 시점에 1회 확정하고 `Sequence_Inspection` 또는 `InspectionSequenceContext`에 보관한 뒤 각 Action이 참조하는 구조가 필요하다.

기존 코드에는 `SystemSetting.SaveOkImage/SaveNgImage` 가드, `Task.Factory.StartNew()` + `Clone()` 비동기 저장 패턴, `_imageLock` 동기화 패턴이 이미 구현되어 있어 재사용 가능하다. 신규 코드량은 최소화되며 변경 범위는 `SystemSetting.cs` 1줄 + `ImageFolderManager.cs` 신규 + `Action_Inspection.cs` 수정 + `Sequence_Inspection.cs` 소규모 수정에 한정된다.

**Primary recommendation:** `ImageFolderManager.BeginInspection()` 호출로 폴더명을 확정하고 그 반환값을 `InspectionSequenceContext.CurrentFolderPath`에 저장. `Action_Inspection.SaveResultImage()`는 시퀀스에서 폴더 경로를 받아 원본+Annotated 두 파일을 저장한다.

---

## Standard Stack

### Core
| Library | Version | Purpose | Why Standard |
|---------|---------|---------|--------------|
| System.IO (BCL) | .NET 4.8 | Directory.CreateDirectory, Path.Combine | 프로젝트 전체에서 이미 사용 중 |
| OpenCvSharp (Mat.SaveImage) | 기존 버전 | JPEG 저장 | Action_Inspection에서 이미 사용 중 |

### Supporting
| Library | Version | Purpose | When to Use |
|---------|---------|---------|-------------|
| System.Threading.Tasks | .NET 4.8 | Task.Factory.StartNew() 비동기 저장 | SequenceBase 패턴과 동일 — Mat.Clone() 후 백그라운드 저장 |

**신규 NuGet 패키지 추가 금지 (STATE.md 결정사항)** — 모든 구현은 기존 BCL + OpenCvSharp만 사용.

---

## Architecture Patterns

### Recommended Project Structure
```
WPF_Example/
├── Utility/
│   ├── RecipeFileHelper.cs       # 기존 유틸리티 패턴 참조
│   └── ImageFolderManager.cs     # 신규 — 경로 생성 전담
├── Setting/
│   └── SystemSetting.cs          # ImageSavePath 기본값 변경만
└── Custom/Sequence/Inspection/
    └── Action_Inspection.cs      # SaveResultImage() 수정
```

### Pattern 1: ImageFolderManager — 싱글 인스턴스 아님, static 메서드 클래스

**What:** 경로 생성만 전담하는 순수 유틸리티 클래스. 상태 없음. `BeginInspection()`과 `GetSavePath()` 두 메서드만 제공.

**When to use:** 검사 시작 시 `BeginInspection()` 1회 호출 → 반환된 폴더 경로를 컨텍스트에 저장 → 각 Shot의 `GetSavePath()` 호출.

**Example:**
```csharp
//260403 hbk — ImageFolderManager: 검사 1회 = 1 시간폴더 경로 생성
namespace FinalVisionProject.Utility
{
    public static class ImageFolderManager
    {
        private static readonly object _lock = new object();

        // 검사 시작 시 1회 호출 — 시간폴더 경로 확정 (충돌 시 _2, _3 접미사)
        public static string BeginInspection()
        {
            lock (_lock)
            {
                var setting = FinalVisionProject.Setting.SystemSetting.Handle;
                string dateDir = DateTime.Now.ToString("yyyyMMdd");
                string timeStr = DateTime.Now.ToString("HHmmss_fff");
                string baseDir = System.IO.Path.Combine(setting.ImageSavePath, dateDir);
                string folderPath = System.IO.Path.Combine(baseDir, timeStr);

                // 충돌 방지: 동일 밀리초 폴더가 이미 존재하면 _2, _3 접미사
                if (System.IO.Directory.Exists(folderPath))
                {
                    int suffix = 2;
                    string candidate;
                    do {
                        candidate = folderPath + "_" + suffix;
                        suffix++;
                    } while (System.IO.Directory.Exists(candidate));
                    folderPath = candidate;
                }

                System.IO.Directory.CreateDirectory(folderPath);
                return folderPath;
            }
        }

        // Shot 저장 경로 반환 — folderPath는 BeginInspection() 반환값
        public static string GetSavePath(string folderPath, string shotName, bool isOk)
        {
            string resultStr = isOk ? "OK" : "NG";
            return System.IO.Path.Combine(folderPath,
                string.Format("{0}_{1}.jpg", shotName, resultStr));
        }

        // Annotated 이미지 경로 반환
        public static string GetAnnotatedSavePath(string folderPath, string shotName, bool isOk)
        {
            string resultStr = isOk ? "OK" : "NG";
            return System.IO.Path.Combine(folderPath,
                string.Format("{0}_{1}_annotated.jpg", shotName, resultStr));
        }
    }
}
```

### Pattern 2: 폴더 경로를 Sequence 시작 시점에 확정하고 Context에 저장

**What:** `Sequence_Inspection.Start()` 오버라이드 또는 `OnStart` 이벤트 콜백에서 `ImageFolderManager.BeginInspection()`을 호출하고 `InspectionSequenceContext.CurrentFolderPath`에 저장.

**Why:** `SequenceBase.Start(int)` → `Context.Clear()` → `Command = Start` 흐름에서 `Clear()`가 상태를 리셋하므로, 폴더 경로는 Clear 이후, 첫 Action 실행 이전에 확정되어야 한다.

**실제 흐름 분석 (코드 기반):**

`SequenceBase.Start(TestPacket)` → `Start(int actionIndex)` → `Context.Clear()` → `Command = ESequenceCommmand.Start`

`Context.Clear()`는 `InspectionSequenceContext`에서 오버라이드 가능하다. 따라서 다음 두 옵션 중 하나를 선택:

- **옵션 A (권장):** `InspectionSequenceContext`에 `CurrentFolderPath` 프로퍼티 추가. `SequenceBase.Start(int)` 후 호출되는 첫 번째 Action 실행 전, 즉 시퀀스 `OnStart` 또는 `SequenceBase`의 `Begin()` 콜백 지점에서 설정. 하지만 현재 코드에 `OnStart` 훅이 없으므로 가장 단순한 방법은 `Action_Inspection.Run()`의 `EStep.Grab` 케이스에서 Shot 인덱스가 0일 때 `BeginInspection()`을 호출하는 것.
- **옵션 B:** `Sequence_Inspection`에서 `Start()` 오버라이드, 부모 호출 전 폴더 경로 확정.

**옵션 A 세부 (Shot 인덱스 0에서 초기화):**

```csharp
// Action_Inspection.Run() — EStep.Grab 케이스 상단에서
case EStep.Grab:
    // Shot 0만 폴더 초기화 (검사 1회 시작)
    if (_MyParam.ShotIndex == 0)
    {
        // 시퀀스에서 폴더 경로 받아 저장 (SequenceContext 통해 공유)
        // 또는 Action 자체에 _FolderPath 필드로 보관
    }
```

그러나 Shot1~5 Action이 각각 독립 `Action_Inspection` 인스턴스이므로 "Shot 0에서 초기화"는 다른 Shot에게 경로를 전달할 수단이 필요하다. 가장 깔끔한 방법:

**권장 메커니즘 — `InspectionSequenceContext.CurrentFolderPath`:**

```csharp
// InspectionSequenceContext에 추가
public string CurrentFolderPath { get; set; } = "";   //260403 hbk — 검사 1회 시간폴더 경로

// Clear() 오버라이드에서 초기화
public override void Clear()
{
    base.Clear();
    CurrentFolderPath = ImageFolderManager.BeginInspection();   //260403 hbk — 검사 시작 시 폴더 확정
}
```

`SequenceBase.Start(int)`에서 `Context.Clear()`를 호출하므로 자동으로 검사 시작 시마다 새 폴더 경로가 확정된다.

각 `Action_Inspection.SaveResultImage()`는 `OwnerSequence.Context` 또는 부모로부터 폴더 경로를 받아 사용한다. ActionBase에서 소속 Sequence에 접근하는 방법을 확인해야 한다.

### Pattern 3: SaveResultImage — 원본 + Annotated 쌍 저장

```csharp
//260403 hbk — ImageFolderManager 기반 경로로 원본+Annotated 저장
private void SaveResultImage(Mat image, bool isOK)
{
    if (image == null) return;

    var setting = SystemSetting.Handle;
    if (isOK && !setting.SaveOkImage) return;
    if (!isOK && !setting.SaveNgImage) return;

    string folderPath = GetCurrentFolderPath();   // 시퀀스 컨텍스트에서 취득
    if (string.IsNullOrEmpty(folderPath)) return;

    // 원본 이미지 비동기 저장
    Mat imgClone = image.Clone();
    string origPath = ImageFolderManager.GetSavePath(folderPath, Name, isOK);
    Task.Factory.StartNew((obj) => {
        var mat = obj as Mat;
        mat.SaveImage(origPath);
        mat.Dispose();
    }, imgClone);

    // Annotated 이미지 비동기 저장
    Mat annotated = _MyParam.LastAnnotatedImage;
    if (annotated != null && !annotated.IsDisposed)
    {
        Mat annotatedClone = annotated.Clone();
        string annotatedPath = ImageFolderManager.GetAnnotatedSavePath(folderPath, Name, isOK);
        Task.Factory.StartNew((obj) => {
            var mat = obj as Mat;
            mat.SaveImage(annotatedPath);
            mat.Dispose();
        }, annotatedClone);
    }
}
```

### Anti-Patterns to Avoid

- **DateTime.Now를 SaveResultImage() 내부에서 호출:** Shot마다 다른 시간이 찍혀 같은 검사가 여러 폴더에 분산됨. 반드시 검사 시작 시점에 1회 확정.
- **Directory.CreateDirectory()를 SaveResultImage()마다 호출:** BeginInspection()에서 이미 생성했으므로 중복. 단, 성능 무관(no-op 안전)하므로 방어적으로 재호출은 허용.
- **Annotated 이미지를 SIMUL_MODE에서 저장 시도:** `LastAnnotatedImage`는 실운영에서만 `SetAnnotatedImage()`로 잠금 저장. SIMUL에서는 null 또는 이전 값. null 가드 필수.
- **lock 없이 `LastAnnotatedImage` 접근:** `_imageLock`이 `InspectionParam`에 있으나 `LastAnnotatedImage`의 setter는 `SetAnnotatedImage()` 내부만 사용. Clone 시 null 체크 후 Clone 필요.

---

## Don't Hand-Roll

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| 폴더명 충돌 방지 | 타임스탬프만으로 고유성 보장 시도 | `HHmmss_fff` + suffix `_2`, `_3` 루프 | 같은 밀리초에 2회 실행 시 충돌. Directory.Exists 체크 루프가 충분 |
| 비동기 이미지 저장 | 동기 SaveImage() | Task.Factory.StartNew() + Clone() | 이미 SequenceBase.SaveResultImage()에서 검증된 패턴 |
| JPEG 저장 | 자체 인코더 | Mat.SaveImage(path) | OpenCvSharp JPEG 저장 이미 프로젝트 전체에서 사용 |

**Key insight:** 충돌 방지를 위한 `Directory.Exists` 루프는 경쟁 조건(TOCTOU)이 이론적으로 존재하나, 단일 프로세스 내 직렬 검사 흐름에서 실제 충돌 가능성은 밀리초 이내 2회 연속 검사뿐이므로 `lock(_lock)` + `Directory.Exists` 루프가 충분하다.

---

## Runtime State Inventory

이 Phase는 신규 코드 추가 + 기존 코드 수정이며 rename/refactor가 아니다. 그러나 `SystemSetting.ImageSavePath` 기본값 변경(`AppDomain...Image` → `D:\Log`)이 영구 설정 파일(Setting.ini)에 영향을 줄 수 있으므로 확인한다.

| Category | Items Found | Action Required |
|----------|-------------|------------------|
| Stored data | Setting.ini의 `ImageSavePath` 값 — 기존 설치에서는 이미 저장된 경로가 있을 수 있음 | 코드 기본값만 변경. ini 파일이 이미 존재하면 `Load()`가 저장값을 읽어 기본값을 덮어씀 — 기존 경로 유지됨. 신규 설치 시만 `D:\Log` 적용 |
| Live service config | 없음 | — |
| OS-registered state | 없음 | — |
| Secrets/env vars | 없음 | — |
| Build artifacts | `D:\Log` 하드코딩이 `Action_Inspection.SaveResultImage()` 라인 471에 존재 | 코드 수정으로 제거 |

---

## Common Pitfalls

### Pitfall 1: Shot 0이 먼저 실행된다는 보장 없음
**What goes wrong:** `InspectionSequenceContext.Clear()`에서 `BeginInspection()`을 호출하면 `Start()` 시점에 폴더가 확정된다 — 이것이 올바른 패턴. 반면 Shot 0의 `Run()`에서 초기화하면 SequenceBase가 actionIndex를 지정해서 시작할 경우(Shot 0이 아닌 곳부터 시작) 미초기화 상태가 될 수 있다.
**Why it happens:** `SequenceBase.Start(EAction)` 오버로드는 특정 Action부터 시작 가능.
**How to avoid:** `Context.Clear()` 오버라이드에서 폴더 경로 확정. Start() → Clear() 순서가 보장됨.
**Warning signs:** 폴더 경로가 빈 문자열이거나 이전 검사 경로를 재사용.

### Pitfall 2: SIMUL_MODE에서 LastAnnotatedImage null
**What goes wrong:** SIMUL 모드에서 `SetAnnotatedImage()`는 호출되지 않고 `SetAnnotatedImageTemp()`만 호출됨. `LastAnnotatedImage`는 null 또는 이전 실운영 검사 값.
**Why it happens:** 코드 설계 — SIMUL은 LastAnnotatedImage 잠금 유지.
**How to avoid:** SaveResultImage()에서 `LastAnnotatedImage != null && !IsDisposed` 체크 후 Clone. SIMUL에서 Annotated 저장 시도해도 null 가드로 스킵.
**Warning signs:** NullReferenceException 또는 disposed Mat 접근 예외.

### Pitfall 3: 비동기 저장 중 Mat 해제
**What goes wrong:** `Task.Factory.StartNew()` 실행 전에 `_GrabbedImage`가 다음 Step에서 Dispose되면 저장 중 오류.
**Why it happens:** 상태머신이 계속 실행되고 다음 검사 사이클에서 Mat을 재사용.
**How to avoid:** 반드시 `Mat.Clone()` 후 Task에 전달. Task 완료 시 Clone Dispose. SequenceBase 패턴과 동일.
**Warning signs:** "ObjectDisposedException" 또는 저장된 이미지가 손상됨.

### Pitfall 4: ActionBase에서 소속 Sequence Context 접근
**What goes wrong:** `Action_Inspection`이 `InspectionSequenceContext.CurrentFolderPath`를 읽으려면 소속 Sequence에 접근해야 하는데, `ActionBase`에 Sequence 역참조가 없을 수 있음.
**Why it happens:** 현재 코드에서 `ActionBase`는 독립 실행 단위. `SequenceBase`에서 Action을 호출하지만 역참조는 확인 필요.
**How to avoid:** 대안: `Action_Inspection.SaveResultImage()`에 `string folderPath` 파라미터를 추가하고, 호출자 `Run()` 케이스에서 폴더 경로를 전달. 또는 `Action_Inspection`에 `public string FolderPath` 프로퍼티를 두고 Sequence가 Start 시 각 Action에 설정. 후자가 더 명확.

---

## Code Examples

### 기존 SaveResultImage() — 수정 전 (Action_Inspection.cs 라인 457~481)
```csharp
// 이미지 저장 — D:\Log\{날짜}\{Shot명}_{OK|NG}_{시간}.jpg   //260326 hbk
private void SaveResultImage(Mat image, bool isOK)   //260326 hbk
{
    if (image == null) return;

    var setting = SystemSetting.Handle;
    if (isOK && !setting.SaveOkImage) return;    //260326 hbk
    if (!isOK && !setting.SaveNgImage) return;   //260326 hbk

    try
    {
        string dateDir  = DateTime.Now.ToString("yyyyMMdd");
        string timeStr  = DateTime.Now.ToString("HHmmss_fff");
        string resultStr = isOK ? "OK" : "NG";
        string dir = System.IO.Path.Combine(@"D:\Log", dateDir);   // 하드코딩 교체 대상
        System.IO.Directory.CreateDirectory(dir);
        string filePath = System.IO.Path.Combine(dir,
            string.Format("{0}_{1}_{2}.jpg", Name, resultStr, timeStr));
        image.SaveImage(filePath);
    }
    catch (Exception ex)
    {
        Logging.PrintLog((int)ELogType.Error, string.Format("SaveImage Error: {0}", ex.Message));
    }
}
```

### SystemSetting.ImageSavePath 기본값 변경 (1줄)
```csharp
// 변경 전
public string ImageSavePath { get; set; } = AppDomain.CurrentDomain.BaseDirectory + @"Image";

// 변경 후 (라인 60)
public string ImageSavePath { get; set; } = @"D:\Log";   //260403 hbk — 기본 저장 경로를 D:\Log로 변경
```

### 비동기 저장 패턴 (SequenceBase.cs 라인 347~363에서 검증됨)
```csharp
Task.Factory.StartNew((object obj) => {
    Mat resultImage = obj as Mat;
    // ... 저장 로직
    resultImage.Dispose();   // Clone이므로 Task 내에서 Dispose 안전
}, Context.ResultImage.Clone());   // 원본이미지 변경 우려 → Clone 전달
```

---

## State of the Art

| Old Approach | Current Approach | When Changed | Impact |
|--------------|------------------|--------------|--------|
| `D:\Log` 하드코딩 경로 | `SystemSetting.ImageSavePath` 참조 | Phase 11 (이번) | 운영자가 PropertyGrid에서 경로 변경 가능 |
| `{Shot명}_{OK|NG}_{시간}.jpg` 단일 파일 | `{yyyyMMdd}\{HHmmss_fff}\{Shot명}_{OK|NG}.jpg` 계층 구조 | Phase 11 (이번) | Phase 12 폴더 단위 조회/삭제 기반 마련 |
| 원본만 저장 | 원본 + Annotated 쌍 저장 | Phase 11 (이번) | 검사 결과 시각적 재현 가능 |

---

## Open Questions

1. **ActionBase에서 소속 Sequence에 역참조 가능 여부**
   - What we know: `SequenceBase`의 `Actions[]` 배열이 Action을 보유하지만 Action에서 Sequence로의 역참조 코드가 있는지 확인 안 됨
   - What's unclear: `ActionBase` 생성자 또는 `OnLoad()` 시점에 Sequence 참조가 주입되는지
   - Recommendation: 플래너가 Plan 작성 전 `ActionBase.cs`를 확인할 것. 역참조 없으면 `Action_Inspection`에 `string FolderPath` 필드를 두고 `Sequence_Inspection.Start()` 오버라이드에서 각 Action에 설정하는 방법 사용

2. **SIMUL_MODE에서 Annotated 이미지 저장 정책**
   - What we know: SIMUL에서 `LastAnnotatedImage`는 null 또는 이전 실운영 값
   - What's unclear: SIMUL 중 이미지 저장 자체를 막아야 하는지, 원본만 저장해야 하는지
   - Recommendation: null 가드로 자연스럽게 처리 — Annotated가 없으면 원본만 저장. 별도 SIMUL 분기 불필요

---

## Environment Availability

Step 2.6: SKIPPED (no external dependencies — 순수 C# BCL + 기존 OpenCvSharp 사용, 신규 도구/서비스 없음)

---

## Validation Architecture

`workflow.nyquist_validation` 키 없음 → enabled로 처리.

### Test Framework
| Property | Value |
|----------|-------|
| Framework | 없음 — 프로젝트에 자동화 테스트 인프라 없음 (WPF 앱, .NET 4.8) |
| Config file | none |
| Quick run command | 수동: 앱 빌드 후 시뮬 검사 1회 실행, 경로 확인 |
| Full suite command | 수동: 빌드 + 실제 TCP $TEST 수신 + 저장 경로/파일 확인 |

### Phase Requirements → Test Map
| Req ID | Behavior | Test Type | Automated Command | File Exists? |
|--------|----------|-----------|-------------------|-------------|
| IMG-01 | 검사 후 `D:\Log\{yyyyMMdd}\{HHmmss_fff}\{ShotName}_{OK|NG}.jpg` 파일 생성 | manual-smoke | 수동: 앱 실행, 검사 1회 후 파일 탐색기로 경로 확인 | ❌ Wave 0 |
| IMG-01 | 같은 검사의 Shot1~5가 동일 시간폴더에 저장 | manual-smoke | 수동: 폴더 내 파일 5개(또는 2개 NG) 확인 | ❌ Wave 0 |
| IMG-01 | 동시 검사(밀리초 충돌) 시 `_2` 접미사 폴더 생성 | manual-smoke | 수동: 시뮬에서 빠른 연속 검사 2회 후 폴더 2개 확인 | ❌ Wave 0 |
| IMG-02 | SaveOkImage=false 기본값에서 OK Shot 저장 안 됨 | manual-smoke | 수동: OK 결과 검사 후 폴더 없거나 NG 파일만 존재 확인 | ❌ Wave 0 |
| IMG-02 | SaveOkImage=true 설정 후 OK 이미지도 저장됨 | manual-smoke | 수동: 설정 변경 후 OK 검사 → 원본+Annotated 2파일 확인 | ❌ Wave 0 |

**Manual-only justification:** 프로젝트에 자동화 테스트 프레임워크 없음. WPF + 카메라 하드웨어 의존 구조로 단위 테스트 설정 비용이 Phase 범위 초과.

### Sampling Rate
- **Per task commit:** 빌드 성공 (컴파일 에러 없음) 확인
- **Per wave merge:** 수동 smoke test — 앱 실행, 검사 1회, 파일 경로 확인
- **Phase gate:** IMG-01/IMG-02 success criteria 5개 항목 모두 수동 통과 후 `/gsd:verify-work`

### Wave 0 Gaps
- [ ] 자동화 테스트 없음 — 수동 검증으로 대체 (기존 프로젝트 관행과 동일)

---

## Sources

### Primary (HIGH confidence)
- `WPF_Example/Custom/Sequence/Inspection/Action_Inspection.cs` — 현재 SaveResultImage() 구현, 하드코딩 위치, _imageLock 패턴 직접 확인
- `WPF_Example/Setting/SystemSetting.cs` — ImageSavePath 기본값, SaveOkImage/SaveNgImage 위치 직접 확인
- `WPF_Example/Sequence/Sequence/SequenceBase.cs` — Start() → Context.Clear() 흐름, Task.Factory.StartNew 패턴 직접 확인
- `WPF_Example/Sequence/Sequence/SequenceContext.cs` — ActionContext, SequenceContext 구조 직접 확인
- `WPF_Example/Custom/Sequence/Inspection/Sequence_Inspection.cs` — InspectionSequenceContext.Clear() 오버라이드 지점 확인
- `WPF_Example/Custom/Sequence/SequenceHandler.cs` — 5 Shot Action 등록 구조 확인

### Secondary (MEDIUM confidence)
- `.planning/phases/11-image-save-structure/11-CONTEXT.md` — 모든 결정사항 D-01~D-12 출처

### Tertiary (LOW confidence)
- ActionBase → Sequence 역참조 가능성: ActionBase.cs 미확인. Plan 작성 전 확인 권장.

---

## Metadata

**Confidence breakdown:**
- Standard Stack: HIGH — 기존 BCL + OpenCvSharp, 신규 의존성 없음
- Architecture: HIGH — 기존 코드에서 패턴 직접 확인, Context.Clear() 훅 위치 확실
- Pitfalls: HIGH — SIMUL/null/비동기 패턴 모두 기존 코드에서 직접 확인

**Research date:** 2026-04-03
**Valid until:** Phase 12 착수 전까지 (안정적 도메인)
