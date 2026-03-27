---
phase: 04-5-site
verified: 2026-03-26T09:00:00Z
status: passed
score: 14/14 must-haves verified
re_verification: false
gaps: []
human_verification:
  - test: "5개 Site 독립 통계 런타임 검증"
    expected: "각 Site별 검사 실행 후 SiteStatistics가 다른 Site와 수치를 공유하지 않고 독립 집계"
    why_human: "런타임에 다중 Site 검사 실행이 필요하며 자동화 테스트 러너 없음"
  - test: "TCP RecipeChange 명령 end-to-end 흐름"
    expected: "TCP 클라이언트가 Site=2, RecipeName=Default 전송 시 Recipe/Site2/Default/main.ini 로드 성공 및 OK 응답"
    why_human: "TCP 클라이언트와 실행 중인 앱이 필요"
  - test: "Site 전환 후 레시피 자동 로드 UI 검증"
    expected: "UI에서 Site 전환 시 해당 Site의 CurrentRecipeName이 정확히 반영됨"
    why_human: "실행 중인 WPF UI 인터랙션이 필요"
---

# Phase 04: 5-Site 독립 관리 Verification Report

**Phase Goal:** Site 1~5 각각 독립적인 레시피/결과/통계 관리. 5개 Site 독립 레시피 로드/저장, 통계 집계 정상 동작
**Verified:** 2026-03-26T09:00:00Z
**Status:** PASSED
**Re-verification:** No — initial verification

---

## Goal Achievement

### Observable Truths

| # | Truth | Status | Evidence |
|---|-------|--------|----------|
| 1 | SiteManager.Handle 싱글톤으로 5개 SiteContext 배열을 소유한다 | VERIFIED | `SiteManager.cs` L5: `= new SiteManager()` singleton; L7: `SiteContext[] _sites = new SiteContext[SITE_COUNT]`; constructor initializes 5 entries (siteNumber 1~5) |
| 2 | SiteManager.SwitchSite(int siteNumber)가 1~5 범위 외 값에 false를 반환한다 | VERIFIED | `SiteManager.cs` L24-29: `int idx = siteNumber - 1; if (idx < 0 || idx >= SITE_COUNT) return false;` |
| 3 | SiteContext.AddResult(bool isOk)가 SiteStatistics 카운터와 Queue<bool> 이력을 동시에 갱신한다 | VERIFIED | `SiteContext.cs` L28-33: `Statistics.Add(isOk)` + `_resultHistory.Enqueue(isOk)` in same method body |
| 4 | SiteStatistics.Yield가 TotalCount=0일 때 0.0을 반환한다 | VERIFIED | `SiteStatistics.cs` L13-18: `return _totalCount == 0 ? 0.0 : (double)_okCount / _totalCount * 100.0;` |
| 5 | SiteStatistics.Add/Reset이 lock(_lock)으로 스레드 안전하게 구현된다 | VERIFIED | `SiteStatistics.cs` L21-31 (Add), L33-43 (Reset): counter mutations in `lock (_lock) { ... }`, RaisePropertyChanged outside lock |
| 6 | 모든 3개 Site 파일이 namespace FinalVisionProject.Site 아래에 위치한다 | VERIFIED | All 3 files: `namespace FinalVisionProject.Site` confirmed in SiteManager.cs L1, SiteContext.cs L4, SiteStatistics.cs L3 |
| 7 | Recipe/Site1/ ~ Recipe/Site5/ 폴더가 존재하며 Seoul_LED_MIL 레시피가 Recipe/Site1/Seoul_LED_MIL/main.ini 로 이동된다 | VERIFIED | `Recipe/Site1/Seoul_LED_MIL/main.ini` exists with `[Info] ModelName=Seoul_LED_MIL`; original `Recipe/Seoul_LED_MIL/` does not exist |
| 8 | RecipeFiles.GetRecipeFilePath(int siteNumber, string name)이 RecipeSavePath/SiteN/name/main.ini 경로를 반환한다 | VERIFIED | `RecipeFileHelper.cs` L206-209: `Path.Combine(recipeSavePath, "Site" + siteNumber, name, FILE_RECIPE + EXT_RECIPE)` |
| 9 | RecipeFiles.CollectRecipe(int siteNumber)가 Recipe/SiteN/ 하위 레시피만 수집한다 | VERIFIED | `RecipeFileHelper.cs` L214-230: `sitePath = Path.Combine(..., "Site" + siteNumber)`, scans only that directory |
| 10 | SystemSetting.CurrentSiteIndex 프로퍼티(기본값 1)가 Setting.ini에 저장/로드된다 | VERIFIED | `Custom/SystemSetting.cs` L13: `public int CurrentSiteIndex { get; set; } = 1;` with `[PropertyTools.DataAnnotations.Category("Inspection|Site")]` (base class reflection handles Save/Load) |
| 11 | 기존 GetRecipeFilePath(string name) 메서드가 그대로 유지된다 | VERIFIED | `RecipeFileHelper.cs` L195-201: original `GetRecipeFilePath(string name)` unchanged |
| 12 | SequenceHandler.LoadRecipe(int siteNumber, string name)이 Recipe/SiteN/name/main.ini 에서 로드하고 SiteContext.CurrentRecipeName을 갱신한다 | VERIFIED | `SequenceHandler.cs` L251-279: `LoadFromIni(int, string)` calls `GetRecipeFilePath(siteNumber, name)` and sets `SiteManager.Handle[siteNumber - 1].CurrentRecipeName = name` (L276) |
| 13 | ProcessRecipeChange가 packet.Site를 사용하여 Site별 레시피 로드를 수행한다 | VERIFIED | `SystemHandler.cs` L157-166: `int siteNumber = packet.Site`, `Recipes.CollectRecipe(siteNumber)`, `Sequences.LoadRecipe(siteNumber, recipeName)` |
| 14 | 기존 LoadRecipe(string name) 메서드가 그대로 유지된다 (기존 코드 호환) | VERIFIED | `SequenceHandler.cs` L157: `public bool LoadRecipe(string name, ERecipeFileType fileType = ERecipeFileType.Ini)` unchanged |

**Score: 14/14 truths verified**

---

### Required Artifacts

| Artifact | Expected | Status | Details |
|----------|----------|--------|---------|
| `WPF_Example/Custom/Site/SiteStatistics.cs` | lock 기반 스레드 안전 통계 카운터, INotifyPropertyChanged | VERIFIED | 52 lines; `_lock`, `Add`, `Reset`, `Yield`, `INotifyPropertyChanged` all present |
| `WPF_Example/Custom/Site/SiteContext.cs` | Site별 레시피명, Queue<bool> 이력, INotifyPropertyChanged | VERIFIED | 46 lines; `Queue<bool> _resultHistory`, `MAX_HISTORY=100`, `AddResult`, `Statistics` property all present |
| `WPF_Example/Custom/Site/SiteManager.cs` | 싱글톤, 5개 SiteContext 배열, SwitchSite | VERIFIED | 32 lines; `Handle` singleton, `SITE_COUNT=5`, `_sites[]`, `SwitchSite` all present |
| `WPF_Example/Utility/RecipeFileHelper.cs` | Site 오버로드 2개 추가, 기존 메서드 유지 | VERIFIED | Both `GetRecipeFilePath(int, string)` (L206) and `CollectRecipe(int)` (L214) added; originals at L195 and L173 preserved |
| `WPF_Example/Custom/SystemSetting.cs` | CurrentSiteIndex int 프로퍼티 (기본값 1) | VERIFIED | L13: `public int CurrentSiteIndex { get; set; } = 1;` with correct Category annotation |
| `Recipe/Site1/Seoul_LED_MIL/main.ini` | 마이그레이션된 INI 파일 | VERIFIED | File exists with `ModelName=Seoul_LED_MIL`; original `Recipe/Seoul_LED_MIL/` removed |
| `Recipe/Site2/Default/main.ini` | Default 레시피 템플릿 | VERIFIED | File exists with `[Info] ModelName=Default Version=1.0.0.0` |
| `Recipe/Site3/Default/main.ini` | Default 레시피 템플릿 | VERIFIED | File exists |
| `Recipe/Site4/Default/main.ini` | Default 레시피 템플릿 | VERIFIED | File exists |
| `Recipe/Site5/Default/main.ini` | Default 레시피 템플릿 | VERIFIED | File exists |
| `WPF_Example/Sequence/SequenceHandler.cs` | Site 오버로드 4개 (public 2 + private 2) | VERIFIED | L178 `LoadRecipe(int,string)`, L205 `SaveRecipe(int,string)`, L251 `LoadFromIni(int,string)`, L318 `SaveToIni(int,string)` all present |
| `WPF_Example/Custom/SystemHandler.cs` | ProcessRecipeChange Site 인자 반영 | VERIFIED | L157-173: `siteNumber = packet.Site`, `CollectRecipe(siteNumber)`, `LoadRecipe(siteNumber, recipeName)` |
| `WPF_Example/FinalVision.csproj` | 3개 Site 파일 Compile 항목 등록 | VERIFIED | L239-241: all 3 Site .cs files explicitly registered under `<!-- Phase 4: 5개 Site -->` comment |

---

### Key Link Verification

| From | To | Via | Status | Details |
|------|----|-----|--------|---------|
| `SiteContext` | `SiteStatistics` | `SiteContext.Statistics` property (`new SiteStatistics()`) | WIRED | `SiteContext.cs` L18: `public SiteStatistics Statistics { get; private set; }`, L25: `Statistics = new SiteStatistics()` |
| `SiteManager` | `SiteContext` | `_sites` 배열 (`SiteContext[SITE_COUNT]`) | WIRED | `SiteManager.cs` L7: `private readonly SiteContext[] _sites = new SiteContext[SITE_COUNT]` |
| `RecipeFiles.CollectRecipe(int)` | `Recipe/SiteN/ 디렉터리` | `Path.Combine(RecipeSavePath, "Site" + siteNumber)` | WIRED | `RecipeFileHelper.cs` L216: exact pattern present |
| `RecipeFiles.GetRecipeFilePath(int, string)` | `Recipe/SiteN/name/main.ini` | `Path.Combine(recipeSavePath, "Site" + siteNumber, name, ...)` | WIRED | `RecipeFileHelper.cs` L208: full path construction confirmed |
| `SequenceHandler.LoadFromIni(int, string)` | `RecipeFiles.GetRecipeFilePath(int, string)` | `SystemHandler.Handle.Recipes.GetRecipeFilePath(siteNumber, name)` | WIRED | `SequenceHandler.cs` L253: exact call present |
| `SequenceHandler.LoadFromIni(int, string)` | `SiteManager.Handle[siteNumber-1].CurrentRecipeName` | direct property assignment | WIRED | `SequenceHandler.cs` L276: `SiteManager.Handle[siteNumber - 1].CurrentRecipeName = name` |
| `ProcessRecipeChange` | `SequenceHandler.LoadRecipe(int, string)` | `Sequences.LoadRecipe(siteNumber, recipeName)` | WIRED | `SystemHandler.cs` L166: exact call present |

---

### Data-Flow Trace (Level 4)

| Artifact | Data Variable | Source | Produces Real Data | Status |
|----------|---------------|--------|--------------------|--------|
| `SiteStatistics.Yield` | `_totalCount`, `_okCount` | `Add(bool isOk)` mutations via `SiteContext.AddResult` | Yes — counter incremented on each call | FLOWING |
| `SiteContext.CurrentRecipeName` | `_currentRecipeName` | `LoadFromIni(int, string)` sets via `SiteManager.Handle[siteNumber-1].CurrentRecipeName = name` | Yes — populated from loaded INI file | FLOWING |
| `RecipeFiles.List` | `ObservableCollection<RecipeFileInfo>` | `CollectRecipe(int siteNumber)` scans `Recipe/SiteN/` filesystem | Yes — reads real directory entries | FLOWING |

---

### Behavioral Spot-Checks

Build verification (compile-time equivalent of behavioral check):

| Behavior | Command | Result | Status |
|----------|---------|--------|--------|
| Full solution builds with 0 errors | MSBuild Debug | `FinalVision -> WPF_Example\bin\x64\Debug\FinalVision.exe` produced; only pre-existing MSB3270/CS7035 warnings | PASS |
| Site namespace resolves in SequenceHandler | `grep "using FinalVisionProject.Site" SequenceHandler.cs` | Line 13: found | PASS |
| Site namespace resolves in SystemHandler | `grep "using FinalVisionProject.Site" SystemHandler.cs` | Line 11: found | PASS |
| SiteManager.Handle[siteNumber-1] pattern exists | `grep "SiteManager.Handle\[siteNumber" SequenceHandler.cs` | Line 276: found | PASS |
| ProcessRecipeChange has no bare LoadRecipe(recipeName) | `grep "LoadRecipe(recipeName)" SystemHandler.cs` | Not found (replaced by siteNumber variant) | PASS |
| Recipe/Seoul_LED_MIL original removed | filesystem check | NOT_AT_ROOT confirmed | PASS |

---

### Requirements Coverage

| Requirement | Source Plan | Description | Status | Evidence |
|-------------|------------|-------------|--------|----------|
| REQ-005 | 04-01, 04-02, 04-03 | 5개 Site 독립 운영 구조 | SATISFIED | SiteManager 싱글톤 + 5개 SiteContext 배열 + 독립 SiteStatistics 구현 완료 |
| REQ-008 | 04-02, 04-03 | Site별 레시피 파일 경로 분리 | SATISFIED | Recipe/SiteN/ 디렉터리 구조, GetRecipeFilePath(int, string) 오버로드, CollectRecipe(int) 오버로드 모두 구현 |

---

### Anti-Patterns Found

| File | Line | Pattern | Severity | Impact |
|------|------|---------|----------|--------|
| `SequenceHandler.cs` | L260-262 | `// 버전 불일치 (현재 미구현)` comment in `LoadFromIni(int, string)` | Info | Version mismatch handling not implemented, but same behavior as original `LoadFromIni(string)` — consistent, not a regression |

No blocker or warning-level anti-patterns detected. The version mismatch comment is an intentional deferred feature, consistent with the original method's behavior.

---

### Human Verification Required

#### 1. 5개 Site 독립 통계 런타임 검증

**Test:** 앱 실행 후 Site 1에서 OK 3건 NG 1건, Site 2에서 OK 1건 NG 2건 검사를 각각 실행한다.
**Expected:** `SiteManager.Handle[0].Statistics.Yield` = 75.0, `SiteManager.Handle[1].Statistics.Yield` = 33.3 — 서로 독립된 수치.
**Why human:** 런타임에 다중 Site 검사 실행 및 메모리 상태 확인이 필요하며 자동화 테스트 러너 없음.

#### 2. TCP RecipeChange 명령 end-to-end 흐름

**Test:** TCP 클라이언트로 `$RECIPE_CHANGE:Site=2,RecipeName=Default@` 전송.
**Expected:** `Recipe/Site2/Default/main.ini` 로드 성공, `SiteManager.Handle[1].CurrentRecipeName` = "Default", 응답 패킷 Result = OK.
**Why human:** TCP 클라이언트와 실행 중인 앱 연결이 필요.

#### 3. Site 전환 후 레시피 자동 로드 UI 검증

**Test:** 앱 실행 중 UI에서 Site 전환 (1 → 2 → 5 순서).
**Expected:** 각 Site 전환 시 해당 Site의 `CurrentRecipeName`이 UI에 정확히 반영됨. Site별 통계가 리셋 없이 각자 독립 유지됨.
**Why human:** 실행 중인 WPF UI 인터랙션이 필요.

---

### Gaps Summary

None. All 14 must-have truths verified against actual codebase. All artifacts exist, are substantive, and are correctly wired. Build succeeds with 0 errors. Human verification items are quality/integration checks, not blockers.

---

_Verified: 2026-03-26T09:00:00Z_
_Verifier: Claude (gsd-verifier)_
