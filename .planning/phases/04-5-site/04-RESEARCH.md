# Phase 4: 5개 Site 독립 운영 구조 - Research

**Researched:** 2026-03-26
**Domain:** C# WPF / FinalVisionProject — 다중 Site 레시피·결과·통계 관리
**Confidence:** HIGH (전체 코드베이스 직접 분석)

---

## Summary

Phase 4는 현재 단일 Site(ESite.DEFAULT) 구조를 5개의 독립 Site로 확장하는 작업이다. 기존 아키텍처는 `SequenceHandler`(단일 `Sequence_Inspection`) + `RecipeFiles`(글로벌 레시피 경로) + `SystemSetting.CurrentRecipeName`(단일 현재 레시피) 구조로 Site 개념이 통신 패킷의 `int Site` 필드에만 존재하고, 실제 레시피·결과·통계는 Site별로 분리되어 있지 않다.

이번 Phase의 핵심은 **(1) `SiteManager` 클래스**로 5개 Site의 컨텍스트(현재 레시피 이름, 최근 결과 이력, 통계)를 메모리에 관리하고, **(2) 레시피 파일 경로를 `Recipe/SiteN/레시피명/main.ini`** 구조로 Site별로 분리하며, **(3) `RecipeFiles`와 `SequenceHandler.LoadRecipe`가 Site를 인자로 받도록 연결**하는 것이다. UI·TCP 통신 변경은 각각 Phase 7·Phase 5 범위이므로 이번 Phase에서는 백엔드 데이터 계층만 구현한다.

`SiteStatistics`는 단순 카운터 집계(검사수/OK수/NG수/수율)이므로 외부 라이브러리 없이 C# int/double 필드로 구현한다. 결과 이력은 `Queue<bool>` 또는 `LinkedList<bool>` (최근 N건) 패턴이 적합하다.

**Primary recommendation:** `SiteManager`를 `Custom/` 하위 새 파일로 추가하고, `SystemSetting`에 `CurrentSiteIndex`(1~5)를 추가한 뒤, `RecipeFiles.GetRecipeFilePath`를 Site 인자를 받는 오버로드로 확장하라.

---

## User Constraints

CONTEXT.md 없음. 제약 사항은 REQUIREMENTS.md 및 ROADMAP.md에서 추출:

| 항목 | 제약 |
|------|------|
| Site 수 | 정확히 5개 (Site 1~5) |
| 레시피 형식 | INI 파일 (`main.ini`) — 기존 `ERecipeFileType.Ini` 유지 |
| UI 변경 | Phase 7 범위 — 이번 Phase에서 불필요 |
| TCP 통신 변경 | Phase 5 범위 — 이번 Phase에서 불필요 |
| 네임스페이스 | `FinalVisionProject` |
| LINQ 의존 | 기존 코드 일부 LINQ 사용 (`STATE.md [03-B]: DrawableList.Find → foreach (LINQ 무의존)` 참고) — 새 코드는 가능하면 foreach 사용 |

---

## Standard Stack

### Core (기존 사용 기술 — 추가 패키지 없음)

| 라이브러리/기술 | 버전 | 용도 | 근거 |
|----------------|------|------|------|
| .NET Framework / C# | 프로젝트 기존 설정 | 전체 구현 언어 | FinalVision.csproj 기존 |
| `IniFile` (프로젝트 자체 유틸) | — | 레시피 INI 저장/로드 | `ParamBase.Save/Load`, `SequenceHandler.LoadFromIni` |
| `System.Collections.Generic` | — | `Queue<T>`, `List<T>`, `Dictionary<K,V>` | 결과 이력·통계 저장 |
| `System.IO` | — | 디렉터리·파일 경로 조작 | `RecipeFiles.CollectRecipe` 패턴 |
| `INotifyPropertyChanged` | — | UI 바인딩 (SiteContext 프로퍼티) | `MainViewModel` 패턴 |

### Alternatives Considered

| 현재 선택 | 대안 | 트레이드오프 |
|-----------|------|-------------|
| INI 파일 (main.ini) | JSON | 기존 `ERecipeFileType.Ini`를 유지해야 하므로 INI 채택. JSON은 Phase 6에서 검토 가능 |
| `Queue<bool>` 이력 | SQLite | 이번 Phase는 메모리 내 최근 N건만 요구. DB는 과설계 |
| 수동 카운터 `int` | Interlocked | 단일 시스템 스레드(`SystemProcess`)에서 카운터 갱신 → Interlocked 불필요. 단, 미래 스레드 안전을 위해 `lock` 사용 권장 |

---

## Architecture Patterns

### 권장 파일 구조 (신규 파일)

```
WPF_Example/
└── Custom/
    └── Site/
        ├── SiteManager.cs        # 5개 SiteContext 관리, 현재 Site 전환
        ├── SiteContext.cs        # Site별 레시피명, 통계, 결과 이력
        └── SiteStatistics.cs    # 검사수/OK수/NG수/수율 계산
```

레시피 파일 디스크 구조:
```
Recipe/
├── Site1/
│   ├── RecipeA/
│   │   └── main.ini
│   └── RecipeB/
│       └── main.ini
├── Site2/
│   └── Default/
│       └── main.ini
...
└── Site5/
```

### Pattern 1: SiteManager — Singleton + Array

```csharp
// Custom/Site/SiteManager.cs
namespace FinalVisionProject.Site {
    public class SiteManager {
        public const int SITE_COUNT = 5;
        public static SiteManager Handle { get; } = new SiteManager();

        private readonly SiteContext[] _sites = new SiteContext[SITE_COUNT];
        private int _currentSiteIndex = 0; // 0-based (Site1=0 ... Site5=4)

        public int CurrentSiteIndex => _currentSiteIndex;
        public SiteContext CurrentSite => _sites[_currentSiteIndex];
        public SiteContext this[int siteIndex] => _sites[siteIndex];

        private SiteManager() {
            for (int i = 0; i < SITE_COUNT; i++) {
                _sites[i] = new SiteContext(i + 1); // siteNumber = 1~5
            }
        }

        public bool SwitchSite(int siteNumber) { // siteNumber = 1~5
            int idx = siteNumber - 1;
            if (idx < 0 || idx >= SITE_COUNT) return false;
            _currentSiteIndex = idx;
            return true;
        }
    }
}
```

### Pattern 2: SiteContext — 레시피명 + 통계 + 이력

```csharp
// Custom/Site/SiteContext.cs
namespace FinalVisionProject.Site {
    public class SiteContext : INotifyPropertyChanged {
        public int SiteNumber { get; }           // 1~5
        public string SiteName => "Site" + SiteNumber;

        private string _currentRecipeName = "Default";
        public string CurrentRecipeName {
            get => _currentRecipeName;
            set { _currentRecipeName = value; RaisePropertyChanged(nameof(CurrentRecipeName)); }
        }

        public SiteStatistics Statistics { get; } = new SiteStatistics();

        // 최근 N건 결과 이력
        private readonly Queue<bool> _resultHistory = new Queue<bool>();
        public const int MAX_HISTORY = 100;

        public void AddResult(bool isOk) {
            Statistics.Add(isOk);
            if (_resultHistory.Count >= MAX_HISTORY)
                _resultHistory.Dequeue();
            _resultHistory.Enqueue(isOk);
        }

        public IEnumerable<bool> GetRecentResults() => _resultHistory;

        public SiteContext(int siteNumber) { SiteNumber = siteNumber; }

        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
```

### Pattern 3: SiteStatistics — 단순 카운터

```csharp
// Custom/Site/SiteStatistics.cs
namespace FinalVisionProject.Site {
    public class SiteStatistics : INotifyPropertyChanged {
        private int _totalCount;
        private int _okCount;
        private int _ngCount;

        public int TotalCount => _totalCount;
        public int OkCount   => _okCount;
        public int NgCount   => _ngCount;
        public double Yield  => _totalCount == 0 ? 0.0 : (double)_okCount / _totalCount * 100.0;

        public void Add(bool isOk) {
            _totalCount++;
            if (isOk) _okCount++;
            else _ngCount++;
            RaisePropertyChanged(nameof(TotalCount));
            RaisePropertyChanged(nameof(OkCount));
            RaisePropertyChanged(nameof(NgCount));
            RaisePropertyChanged(nameof(Yield));
        }

        public void Reset() {
            _totalCount = _okCount = _ngCount = 0;
            RaisePropertyChanged(nameof(TotalCount));
            RaisePropertyChanged(nameof(OkCount));
            RaisePropertyChanged(nameof(NgCount));
            RaisePropertyChanged(nameof(Yield));
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
```

### Pattern 4: RecipeFiles — Site별 경로 오버로드

기존 `GetRecipeFilePath(string name)` 는 `Recipe/{name}/main.ini` 를 반환한다.
Site 분리 후에는 `Recipe/SiteN/{name}/main.ini` 가 필요하다.

```csharp
// Utility/RecipeFileHelper.cs 에 오버로드 추가
public string GetRecipeFilePath(int siteNumber, string name) {
    string recipeSavePath = SystemHandler.Handle.Setting.RecipeSavePath;
    string siteFolderName = "Site" + siteNumber;
    return Path.Combine(recipeSavePath, siteFolderName, name, FILE_RECIPE + EXT_RECIPE);
}

public int CollectRecipe(int siteNumber) {
    string sitePath = Path.Combine(
        SystemHandler.Handle.Setting.RecipeSavePath, "Site" + siteNumber);
    // ... 기존 CollectRecipe 로직, sitePath 기준으로 반복
}
```

### Pattern 5: SequenceHandler.LoadRecipe — Site 인자 오버로드

```csharp
// SequenceHandler.cs 오버로드 추가
public bool LoadRecipe(int siteNumber, string name, ERecipeFileType fileType = ERecipeFileType.Ini) {
    bool result = LoadFromIni(siteNumber, name);
    OnRecipeChanged?.Invoke(this, new RecipeChangedEventArgs(name));
    if (result) {
        ExecOnLoad(name);
        // SiteManager 컨텍스트 갱신
        SiteManager.Handle[siteNumber - 1].CurrentRecipeName = name;
    }
    return result;
}

private bool LoadFromIni(int siteNumber, string name) {
    string recipeFile = SystemHandler.Handle.Recipes.GetRecipeFilePath(siteNumber, name);
    // ... 기존 LoadFromIni 동일 로직
}
```

### Pattern 6: SystemSetting — CurrentSiteIndex 추가

```csharp
// Custom/SystemSetting.cs (partial class)
[Category("Inspection|Site")]
public int CurrentSiteIndex { get; set; } = 1;  // 1~5, Setting.ini에 저장
```

### Pattern 7: ProcessRecipeChange — Site별 레시피 로드 연결

`Custom/SystemHandler.cs` 의 `ProcessRecipeChange`를 수정:

```csharp
private RecipeChangeResultPacket ProcessRecipeChange(RecipeChangePacket packet) {
    int siteNumber = packet.Site; // 1~5
    string recipeName = packet.RecipeName;

    bool loaded = LoadRecipe(siteNumber, recipeName);
    resultPacket.Result = loaded ? EVisionResultType.OK : EVisionResultType.NG;
    return resultPacket;
}
```

### Anti-Patterns to Avoid

- **전역 `CurrentRecipeName` 단순 교체:** `SystemSetting.CurrentRecipeName`을 Site별로 덮어쓰면 동시 명령 시 레이스 컨디션 발생. `SiteContext.CurrentRecipeName`으로 Site별 독립 관리해야 한다.
- **`CollectRecipe()` 전역 호출 유지:** `RecipeFiles.List`가 모든 Site의 레시피를 혼합하면 UI에서 Site별 필터링이 불가능해진다. Site 인자를 받는 오버로드를 추가하라.
- **통계를 `SystemHandler`에 직접 추가:** 통계는 Site별로 독립이므로 `SiteContext` 내부에 캡슐화해야 한다.
- **레시피 결과 기록을 로그 파일에만 의존:** Phase 4의 완료 기준은 메모리 내 통계 집계 동작이므로 로그와 별개로 `SiteStatistics`에 카운터를 유지해야 한다.

---

## Don't Hand-Roll

| 문제 | 빌드하면 안 되는 것 | 사용할 것 | 이유 |
|------|---------------------|-----------|------|
| INI 파일 읽기/쓰기 | 커스텀 INI 파서 | 기존 `IniFile` 유틸 | 이미 `ParamBase.Load/Save`에서 검증됨 |
| 레시피 버전 관리 | 수동 버전 문자열 비교 | `RecipeFiles.GetVersion()` | 기존 구현 재사용 |
| 프로퍼티 변경 알림 | 수동 이벤트 | `INotifyPropertyChanged` | WPF 표준 패턴, `MainViewModel`에서 이미 사용 |
| 결과 이력 FIFO | 직접 배열 인덱스 관리 | `Queue<bool>` (MaxSize 초과 시 Dequeue) | BCL 표준 — 직접 구현 불필요 |

---

## Common Pitfalls

### Pitfall 1: 기존 `LoadRecipe(string name)` 호출부가 Site 무시
**What goes wrong:** `SystemHandler.LoadRecipe(recipeName)`은 현재 Site와 무관하게 글로벌 레시피를 로드하여 Site별 분리가 실제로 동작하지 않는다.
**Why it happens:** Phase 4 이전 코드는 Site 개념이 통신 패킷에만 존재하고, 실제 로드 경로에 Site가 없다.
**How to avoid:** `SequenceHandler.LoadFromIni`에 `siteNumber` 인자를 추가하고, `GetRecipeFilePath(siteNumber, name)` 오버로드를 사용하라.
**Warning signs:** 레시피를 Site 2에서 변경해도 `Recipe/Site1/` 경로로 저장되는 현상.

### Pitfall 2: `RecipeFiles.CollectRecipe()` 가 Site 혼합 목록을 반환
**What goes wrong:** 기존 `CollectRecipe()`는 `RecipeSavePath` 하위 모든 디렉터리를 스캔한다. `Recipe/Site1/`, `Recipe/Site2/` 폴더가 생기면 이 폴더 자체가 "레시피 이름"으로 잡힌다.
**Why it happens:** `GetDirectories(recipePath, "*")`는 한 레벨만 탐색한다. `Recipe/` 아래에 `Site1` 폴더가 생기면 `Site1`을 레시피 이름으로 인식한다.
**How to avoid:** `CollectRecipe(int siteNumber)`로 `Recipe/SiteN/` 경로에서 수집하는 오버로드를 추가하라. 기존 `CollectRecipe()`는 Site 미지정 레거시 호환용으로 남겨두거나 제거한다.
**Warning signs:** `OpenRecipeWindow`에서 Site1~5 폴더명이 레시피로 표시되는 현상.

### Pitfall 3: `SiteContext.AddResult` 스레드 안전성
**What goes wrong:** `SystemProcess` 스레드(백그라운드)가 `AddResult`를 호출하고, UI 스레드가 `Statistics` 값을 읽으면 `_totalCount` 불일치가 발생할 수 있다.
**Why it happens:** 현재 `SystemProcess`는 `Thread.Sleep(1)` 루프로 실행되는 별도 스레드다.
**How to avoid:** `SiteStatistics.Add`/`Reset` 내부에 `lock(_lock)` 추가. `INotifyPropertyChanged` 이벤트는 `Dispatcher.Invoke` 없이 바인딩 엔진이 크로스스레드를 처리한다(WPF 4.5+).
**Warning signs:** `TotalCount`가 실제 검사수보다 낮거나 Yield가 100%를 초과하는 현상.

### Pitfall 4: `SaveToIni` / `LoadFromIni` 경로 불일치
**What goes wrong:** 저장은 `GetRecipeFilePath(siteNumber, name)`으로 하고, 로드는 기존 `GetRecipeFilePath(name)`(Site 없음)으로 하면 파일을 찾지 못한다.
**Why it happens:** 두 오버로드가 반환하는 경로가 다르다.
**How to avoid:** `LoadFromIni`와 `SaveToIni`의 Site 있는 버전을 pair로 항상 같이 수정하라. 단위 테스트로 경로 일치 검증.

### Pitfall 5: ESite enum에 Site 1~5 미추가
**What goes wrong:** `ResourceMap`의 `ESite` 에 `Site1=1`만 있고 `Site2~5`가 없으면 TCP 패킷 파싱 시 `ActionList[site]` KeyNotFoundException 발생.
**Why it happens:** 현재 `ESite`에는 `DEFAULT = 1`만 정의되어 있다.
**How to avoid:** Phase 5(TCP 재설계) 전에 `ESite`를 미리 확장하거나, `SiteManager`에서 `int siteNumber`를 직접 사용하여 enum 의존성을 줄인다.

---

## Code Examples

### 기존 LoadFromIni (참조용)
```csharp
// Source: WPF_Example/Sequence/SequenceHandler.cs L196-L226
private bool LoadFromIni(string name) {
    if (string.IsNullOrEmpty(name)) return false;
    string recipeFile = SystemHandler.Handle.Recipes.GetRecipeFilePath(name);
    if (File.Exists(recipeFile) == false) return false;
    IniFile loadFile = new IniFile(recipeFile);
    // ... Param 로드
    pSetting.CurrentRecipeName = name;
    return true;
}
```

**Site 확장 포인트:** `GetRecipeFilePath(name)` → `GetRecipeFilePath(siteNumber, name)` 교체 + `SiteManager.Handle[siteNumber-1].CurrentRecipeName = name` 추가.

### 기존 GetRecipeFilePath (참조용)
```csharp
// Source: WPF_Example/Utility/RecipeFileHelper.cs L195-L200
public string GetRecipeFilePath(string name) {
    string recipeSavePath = SystemHandler.Handle.Setting.RecipeSavePath;
    recipeSavePath = Path.Combine(recipeSavePath, name);
    string recipeFile = Path.Combine(recipeSavePath, FILE_RECIPE + EXT_RECIPE);
    return recipeFile;
}
```

**Site 확장 오버로드 패턴:**
```csharp
public string GetRecipeFilePath(int siteNumber, string name) {
    string recipeSavePath = SystemHandler.Handle.Setting.RecipeSavePath;
    return Path.Combine(recipeSavePath, "Site" + siteNumber, name, FILE_RECIPE + EXT_RECIPE);
}
```

### 기존 ProcessRecipeChange (참조용)
```csharp
// Source: WPF_Example/Custom/SystemHandler.cs L150-L177
private RecipeChangeResultPacket ProcessRecipeChange(RecipeChangePacket packet) {
    int siteNumber = packet.Site;          // 이미 Site 번호 존재
    string recipeName = packet.RecipeName;
    // 현재: Site 무시하고 글로벌 LoadRecipe 호출
    if (Recipes.HasRecipe(recipeName) == false) { resultPacket.Result = EVisionResultType.NG; }
    else if ((Setting.CurrentRecipeName != recipeName) && LoadRecipe(recipeName)) {
        resultPacket.Result = EVisionResultType.OK;
    }
}
```

**Phase 4 이후 수정 방향:** `LoadRecipe(siteNumber, recipeName)` 호출 + `SiteContext` 갱신.

---

## State of the Art

| 현재 구조 | Phase 4 이후 구조 | 변경 시점 | 영향 |
|-----------|-------------------|-----------|------|
| `ESite.DEFAULT = 1` (단일) | `ESite.Site1~Site5` 또는 `int siteNumber` 직접 사용 | Phase 4 | ResourceMap 갱신 필요 |
| `Recipe/{name}/main.ini` | `Recipe/SiteN/{name}/main.ini` | Phase 4 | 기존 레시피 디렉터리 마이그레이션 필요 |
| `SystemSetting.CurrentRecipeName` (전역) | `SiteContext.CurrentRecipeName` (Site별) | Phase 4 | `ParamBase.GetExternalFilePath`의 `CurrentRecipeName` 참조 검토 필요 |
| 통계 없음 | `SiteStatistics` (Site별 검사수/OK/NG/수율) | Phase 4 | 신규 |

**Deprecated/outdated:**
- `RecipeFiles.CollectRecipe()` (Site 없는 버전): Phase 4 이후 Site 인자 버전으로 교체. 기존 버전은 `OpenRecipeWindow` 호환을 위해 일시 유지 후 Phase 6에서 제거.

---

## Runtime State Inventory

> 이 Phase는 파일 경로 구조 변경(레거시 `Recipe/{name}/` → `Recipe/SiteN/{name}/`)을 포함한다.

| 카테고리 | 발견된 항목 | 필요 조치 |
|----------|------------|-----------|
| 저장 데이터 | `Recipe/` 하위 기존 레시피 폴더 (현재 `Recipe/Seoul_LED_MIL/` 존재 확인됨) | 기존 레시피는 `Recipe/Site1/레시피명/` 으로 수동 이동 또는 마이그레이션 스크립트 작성 |
| 라이브 서비스 설정 | 없음 — TCP VisionServer는 재시작 시 재초기화 | 없음 |
| OS 등록 상태 | 없음 — Task Scheduler/서비스 등록 없음 | 없음 |
| 시크릿/환경변수 | `Setting.ini` 의 `CurrentRecipeName` 키 — Site별 관리로 전환 시 호환성 유지 필요 | `SystemSetting`에 `CurrentSiteIndex` 추가, `CurrentRecipeName`은 레거시 유지 |
| 빌드 아티팩트 | `WPF_Example/bin/` 하위 — 재빌드 시 자동 갱신 | 없음 |

**기존 레시피 마이그레이션:** `Recipe/Seoul_LED_MIL/` → `Recipe/Site1/Seoul_LED_MIL/` 이동이 필요하다. 이는 코드 변경이 아닌 **디렉터리 이동 작업**이며 플랜에 별도 태스크로 포함되어야 한다.

---

## Environment Availability

> Phase 4는 순수 C# 코드 변경이다. 외부 도구·서비스 의존성 없음.

Step 2.6: SKIPPED (no external dependencies — pure C# code/file structure changes only)

---

## Validation Architecture

> `config.json`에 `nyquist_validation` 키 없음 → 미정의 = enabled로 처리. 단, 이 프로젝트는 별도 자동화 테스트 인프라(pytest/xUnit 설정 파일)가 없다.

### Test Framework

| 속성 | 값 |
|------|-----|
| Framework | 없음 — 수동 검증 (xUnit/NUnit 미설정) |
| Config file | 없음 |
| Quick run command | (해당 없음 — Visual Studio에서 Debug 실행 후 수동 확인) |
| Full suite command | (해당 없음) |

### Phase Requirements → Test Map

| 요구사항 | 동작 | 테스트 유형 | 검증 방법 |
|----------|------|------------|-----------|
| REQ-005: Site 1~5 독립 레시피 로드/저장 | `SiteManager.Handle[n].CurrentRecipeName` 가 Site별 독립 | manual | 앱 실행 후 Site1에 RecipeA 로드, Site2에 RecipeB 로드, 서로 독립 확인 |
| REQ-005: 통계 집계 | `SiteStatistics.TotalCount/OkCount/NgCount/Yield` 정확성 | manual | OK 3건 NG 1건 입력 후 Yield=75% 확인 |
| REQ-008: Site별 레시피 파일 구조 | `Recipe/SiteN/레시피명/main.ini` 생성 확인 | manual | SaveRecipe 후 디스크 경로 확인 |

### Wave 0 Gaps

- 자동화 테스트 파일 없음 — 수동 검증으로 대체
- xUnit/NUnit 도입은 이번 Phase 범위 밖 (Phase 4 완료 기준: 수동 동작 확인)

---

## Open Questions

1. **`ParamBase.GetExternalFilePath`의 `CurrentRecipeName` 참조**
   - What we know: `ParamBase.GetExternalFilePath`는 `SystemHandler.Handle.Setting.CurrentRecipeName`을 사용하여 외부 파일 경로를 결정한다.
   - What's unclear: Phase 4에서 `CurrentRecipeName`을 `SiteContext`로 이동하면 `GetExternalFilePath`의 Site 컨텍스트 연결 방식 결정이 필요하다.
   - Recommendation: `SystemSetting.CurrentRecipeName`을 `SiteManager.Handle.CurrentSite.CurrentRecipeName`으로 프록시하는 프로퍼티로 유지하거나, `GetExternalFilePath`에 Site 인자를 추가한다.

2. **`ESite` enum vs `int siteNumber` 직접 사용**
   - What we know: 현재 `ResourceMap`은 `ESite` enum을 사용하지만, `ESite`에 `DEFAULT=1`만 있다.
   - What's unclear: Site 1~5를 `ESite.Site1~Site5`로 추가할지, `ResourceMap`의 Site 키를 `int`로 변경할지.
   - Recommendation: Phase 4(백엔드 구조)에서는 `int siteNumber`(1~5)를 직접 사용하고, `ESite` enum 확장은 Phase 5(TCP 재설계) 시 함께 처리한다. `SiteManager` 내부는 0-based 배열 인덱스를 사용하고, 외부 API는 1-based siteNumber로 통일한다.

3. **기존 레시피 마이그레이션 시점**
   - What we know: `Recipe/Seoul_LED_MIL/` 폴더가 존재하며, 새 경로는 `Recipe/Site1/Seoul_LED_MIL/`이다.
   - What's unclear: 개발 중 기존 레시피를 즉시 이동할지, 호환 코드패스(Site 없는 경로 폴백)를 임시 유지할지.
   - Recommendation: 간단하게 플랜 Wave 0에서 디렉터리를 수동 이동한다. 폴백 코드패스는 복잡도만 증가시킨다.

---

## Sources

### Primary (HIGH confidence)

- 직접 코드 분석: `WPF_Example/Sequence/SequenceHandler.cs` — LoadRecipe/LoadFromIni 구조
- 직접 코드 분석: `WPF_Example/Utility/RecipeFileHelper.cs` — 레시피 파일 경로 구조
- 직접 코드 분석: `WPF_Example/Custom/Sequence/Inspection/Action_Inspection.cs` — InspectionParam 구조
- 직접 코드 분석: `WPF_Example/Custom/TcpServer/ResourceMap.cs` — ESite·ETestType 정의
- 직접 코드 분석: `WPF_Example/TcpServer/VisionRequestPacket.cs` — 패킷 Site 필드 구조
- 직접 코드 분석: `WPF_Example/SystemHandler.cs` — ProcessRecipeChange·ProcessTest 플로우
- 직접 코드 분석: `WPF_Example/Setting/SystemSetting.cs` — CurrentRecipeName·RecipeSavePath
- `.planning/REQUIREMENTS.md`, `.planning/ROADMAP.md`, `.planning/STATE.md` — Phase 범위 제약

### Secondary (MEDIUM confidence)

- 없음 (전체 분석이 코드베이스 직접 읽기 기반)

### Tertiary (LOW confidence)

- 없음

---

## Metadata

**Confidence breakdown:**
- Standard stack: HIGH — 기존 코드베이스 직접 확인, 추가 외부 라이브러리 불필요
- Architecture: HIGH — 기존 Singleton+partial class 패턴 일관 적용
- Pitfalls: HIGH — 코드 분석에서 직접 발견된 문제점

**Research date:** 2026-03-26
**Valid until:** 2026-04-26 (코드베이스가 변경되지 않는 한)
