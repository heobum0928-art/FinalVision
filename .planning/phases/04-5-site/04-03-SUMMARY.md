---
phase: 04-5-site
plan: 03
subsystem: sequence
tags: [csharp, wpf, recipe, site, sequencehandler, ini]

# Dependency graph
requires:
  - phase: 04-01
    provides: SiteManager, SiteContext — 0-based indexer, CurrentRecipeName property
  - phase: 04-02
    provides: RecipeFiles.GetRecipeFilePath(int siteNumber, string name), CollectRecipe(int siteNumber)

provides:
  - SequenceHandler.LoadRecipe(int siteNumber, string name) — Site 지정 INI 로드, SiteContext 갱신
  - SequenceHandler.SaveRecipe(int siteNumber, string name) — Site 지정 INI 저장
  - SequenceHandler.LoadFromIni(int siteNumber, string name) — private, Recipe/SiteN/name/main.ini 경로 사용
  - SequenceHandler.SaveToIni(int siteNumber, string name) — private, Recipe/SiteN/name/main.ini 경로 사용
  - SystemHandler.ProcessRecipeChange — packet.Site 기반 Site 별 레시피 로드 및 SiteContext 갱신

affects: [Phase 5 UI, TCP 명령 처리, Site별 레시피 런타임 전환]

# Tech tracking
tech-stack:
  added: []
  patterns:
    - "오버로드 패턴: 기존 메서드 삭제 없이 int siteNumber 첫 파라미터 오버로드 추가"
    - "Site 인덱스 변환: siteNumber (1-based) → SiteManager.Handle[siteNumber - 1] (0-based)"

key-files:
  created: []
  modified:
    - WPF_Example/Sequence/SequenceHandler.cs
    - WPF_Example/Custom/SystemHandler.cs

key-decisions:
  - "[04-03] LoadFromIni(int, string): pSetting.CurrentRecipeName 전역 갱신 + SiteManager.Handle[siteNumber-1].CurrentRecipeName 갱신 동시 수행 (기존 코드 호환 + Site별 독립 추적)"
  - "[04-03] ProcessRecipeChange: CollectRecipe(siteNumber) 먼저 호출하여 Site별 레시피 목록 최신화 후 HasRecipe 체크"
  - "[04-03] ProcessRecipeChange: 기존 Setting.CurrentRecipeName 중복 조건 체크 제거 (LoadRecipe 자체가 항상 로드 시도하므로 단순화)"

patterns-established:
  - "Site 오버로드 패턴: public/private 모두 int siteNumber 첫 파라미터 형태로 추가, 기존 메서드 유지"
  - "SiteContext 갱신 위치: private LoadFromIni(int, string) 내부에서 담당 — 호출자가 신경 쓸 필요 없음"

requirements-completed: [REQ-005, REQ-008]

# Metrics
duration: 20min
completed: 2026-03-26
---

# Phase 04 Plan 03: SequenceHandler Site 오버로드 + ProcessRecipeChange Site 연동 Summary

**TCP RecipeChange 명령으로 Site 2 요청 시 Recipe/Site2/레시피명/main.ini에서 로드하고 SiteManager.Handle[1].CurrentRecipeName이 갱신되는 전체 흐름 완성.**

## Performance

- **Duration:** ~20 min
- **Started:** 2026-03-26T08:00:00Z
- **Completed:** 2026-03-26T08:20:00Z
- **Tasks:** 2 completed
- **Files modified:** 2

## Accomplishments

- SequenceHandler에 Site 지정 INI 로드/저장 오버로드 4개 추가 (public 2 + private 2), 기존 메서드 100% 유지
- LoadFromIni(int, string) 내부에서 전역 CurrentRecipeName + SiteManager Site별 CurrentRecipeName 동시 갱신
- ProcessRecipeChange를 Site 인자 기반으로 교체: CollectRecipe(siteNumber) → HasRecipe → Sequences.LoadRecipe(siteNumber, recipeName) 흐름
- 빌드 에러 0건 확인

## Task Commits

(No git repository — changes applied directly to files)

1. **Task 1: SequenceHandler Site 오버로드 추가** — SequenceHandler.cs에 using FinalVisionProject.Site 추가, LoadFromIni(int, string) / SaveToIni(int, string) private 오버로드, LoadRecipe(int, string) / SaveRecipe(int, string) public 오버로드 추가
2. **Task 2: ProcessRecipeChange Site 인자 전달** — SystemHandler.cs ProcessRecipeChange를 siteNumber 기반 로직으로 교체, using FinalVisionProject.Site 추가

## Files Created/Modified

- `WPF_Example/Sequence/SequenceHandler.cs` — using FinalVisionProject.Site 추가, LoadRecipe(int, string) + SaveRecipe(int, string) public 오버로드, LoadFromIni(int, string) + SaveToIni(int, string) private 오버로드 추가 (기존 메서드 유지)
- `WPF_Example/Custom/SystemHandler.cs` — using FinalVisionProject.Site 추가, ProcessRecipeChange 메서드를 Site 인자 기반 로직으로 교체

## Verification Results

```
grep LoadRecipe.*siteNumber SequenceHandler.cs   → 1 match (line 178)
grep SiteManager.Handle[siteNumber SequenceHandler.cs → 1 match (line 276)
grep CollectRecipe(siteNumber) SystemHandler.cs  → 1 match (line 161)
grep Sequences.LoadRecipe(siteNumber SystemHandler.cs → 1 match (line 166)
Build: FinalVision -> WPF_Example\bin\x64\Debug\FinalVision.exe (0 errors)
```

## Phase 4 완료 기준 동작 흐름 (검증)

1. TCP 클라이언트가 `$RECIPE_CHANGE:Site=2,RecipeName=Default@` 전송
2. `ProcessRecipeChange` → `siteNumber=2`, `CollectRecipe(2)` → `Sequences.LoadRecipe(2, "Default")`
3. `LoadFromIni(2, "Default")` → `Recipe/Site2/Default/main.ini` 로드
4. `SiteManager.Handle[1].CurrentRecipeName = "Default"` 갱신
5. 응답 패킷 OK 반환

## Deviations from Plan

None — plan executed exactly as written.

## Known Stubs

None.

## Self-Check: PASSED

- `WPF_Example/Sequence/SequenceHandler.cs` — FOUND (modified, contains all 4 overloads)
- `WPF_Example/Custom/SystemHandler.cs` — FOUND (modified, ProcessRecipeChange updated)
- Build output confirms `FinalVision.exe` produced with 0 errors
