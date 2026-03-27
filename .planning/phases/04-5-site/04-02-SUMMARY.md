---
phase: 04-5-site
plan: 02
subsystem: recipe
tags: [recipe, site, directory-structure, ini, system-setting, csharp]

# Dependency graph
requires:
  - phase: 03-teaching-ui
    provides: RecipeFileHelper existing CollectRecipe()/GetRecipeFilePath(string) methods used as base
provides:
  - Recipe/Site1~5/ directory structure with Seoul_LED_MIL migrated to Site1
  - RecipeFiles.GetRecipeFilePath(int siteNumber, string name) overload
  - RecipeFiles.CollectRecipe(int siteNumber) overload
  - SystemSetting.CurrentSiteIndex property (default 1, saved/loaded via Setting.ini)
affects: [04-03, sequence-handler, site-selection-ui]

# Tech tracking
tech-stack:
  added: []
  patterns:
    - "Site-indexed recipe paths: Recipe/SiteN/name/main.ini"
    - "Method overloading for backward-compatible Site extension (no deletion of original methods)"
    - "partial class pattern for SystemSetting custom properties"

key-files:
  created:
    - Recipe/Site1/Seoul_LED_MIL/main.ini (migrated from Recipe/Seoul_LED_MIL/)
    - Recipe/Site2/Default/main.ini
    - Recipe/Site3/Default/main.ini
    - Recipe/Site4/Default/main.ini
    - Recipe/Site5/Default/main.ini
  modified:
    - WPF_Example/Utility/RecipeFileHelper.cs
    - WPF_Example/Custom/SystemSetting.cs

key-decisions:
  - "Site overloads added as method overloads — original GetRecipeFilePath(string) and CollectRecipe() kept untouched for backward compatibility"
  - "CollectRecipe(int siteNumber) clears and repopulates List from Recipe/SiteN/ only — avoids mixing Site data"
  - "System.IO prefix removed from CollectRecipe(int) body since 'using System.IO' already in RecipeFileHelper.cs header"
  - "CurrentSiteIndex placed in Custom/SystemSetting.cs partial class with [Category('Inspection|Site')] — auto Save/Load via base class reflection"

patterns-established:
  - "Site path pattern: Path.Combine(RecipeSavePath, 'Site' + siteNumber, name, 'main.ini')"
  - "Site overload naming: same method name with int siteNumber as first parameter"

requirements-completed: [REQ-005, REQ-008]

# Metrics
duration: 15min
completed: 2026-03-26
---

# Phase 04 Plan 02: Site Recipe Directory Structure and SystemSetting.CurrentSiteIndex Summary

**Recipe/Site1~5/ 디렉터리 구조 생성, Seoul_LED_MIL 레시피 Site1으로 마이그레이션, RecipeFileHelper Site 오버로드 2개 추가, SystemSetting.CurrentSiteIndex(기본값 1) 추가 — 빌드 성공 (오류 0건)**

## Performance

- **Duration:** ~15 min
- **Started:** 2026-03-26T07:30:00Z
- **Completed:** 2026-03-26T07:45:00Z
- **Tasks:** 2
- **Files modified:** 2 (+ 5 new ini files, 5 new directories)

## Accomplishments
- Recipe/Site1~5/ 5개 폴더 생성 완료, Seoul_LED_MIL 레시피가 Recipe/Site1/Seoul_LED_MIL/ 로 이동 (원본 삭제)
- Site2~Site5 Default/main.ini (INI 최소 포맷) 생성
- RecipeFileHelper.cs에 GetRecipeFilePath(int, string) 및 CollectRecipe(int) 오버로드 2개 추가 (기존 메서드 완전 유지)
- Custom/SystemSetting.cs에 CurrentSiteIndex(int, 기본값 1) 프로퍼티 추가 — [Category("Inspection|Site")] 어노테이션으로 Setting.ini 자동 저장/로드
- MSBuild Debug 빌드: 오류 0건 (기존 pre-existing 경고만 존재)

## Task Commits

(No git repository — changes applied directly to working tree)

1. **Task 1: Recipe 디렉터리 구조 생성 및 기존 레시피 마이그레이션** - directory operations via bash
2. **Task 2: RecipeFileHelper.cs Site 오버로드 추가 + SystemSetting.CurrentSiteIndex 추가** - source code edits

## Files Created/Modified

- `D:\Project\FinalVision\Recipe\Site1\Seoul_LED_MIL\main.ini` - 기존 레시피 마이그레이션 (하위 폴더 포함: DEFAULT, SPRA_LOAD, SPRA_UNLOAD)
- `D:\Project\FinalVision\Recipe\Site2\Default\main.ini` - Default 레시피 템플릿 (INI 최소 포맷)
- `D:\Project\FinalVision\Recipe\Site3\Default\main.ini` - Default 레시피 템플릿
- `D:\Project\FinalVision\Recipe\Site4\Default\main.ini` - Default 레시피 템플릿
- `D:\Project\FinalVision\Recipe\Site5\Default\main.ini` - Default 레시피 템플릿
- `WPF_Example\Utility\RecipeFileHelper.cs` - GetRecipeFilePath(int, string) 및 CollectRecipe(int) 오버로드 추가
- `WPF_Example\Custom\SystemSetting.cs` - CurrentSiteIndex 프로퍼티 추가 (using PropertyTools.DataAnnotations 추가)

## Decisions Made

- CollectRecipe(int siteNumber) 내부에서 `List.Clear()` 후 해당 Site 폴더만 스캔 — 전체 레시피 목록을 Site별로 독립 관리하는 패턴 확립
- CurrentSiteIndex는 base class 반사(Reflection) 기반 Save/Load가 Int32 타입을 자동 처리하므로 별도 저장 코드 불필요

## Deviations from Plan

None - plan executed exactly as written.

## Issues Encountered

- msbuild CLI가 PATH에 없어 MSBuild.exe 절대 경로를 사용: `/c/Program Files/Microsoft Visual Studio/2022/Community/MSBuild/Current/Bin/MSBuild.exe`. 빌드 자체는 정상 완료 (오류 0건).

## User Setup Required

None - no external service configuration required.

## Next Phase Readiness

- Plan 03 (SequenceHandler Site 확장)이 GetRecipeFilePath(siteNumber, name) 호출 가능 — 의존성 충족
- CollectRecipe(siteNumber) 호출로 Site별 레시피 목록 로드 가능
- SystemSetting.CurrentSiteIndex로 현재 활성 Site 추적 가능
- 레시피 디렉터리 구조 변경 완료 — 기존 코드(GetRecipeFilePath(string), CollectRecipe()) 호환성 유지

---
*Phase: 04-5-site*
*Completed: 2026-03-26*

## Self-Check: PASSED

- FOUND: Recipe/Site1/Seoul_LED_MIL/main.ini
- FOUND: Recipe/Site2/Default/main.ini
- FOUND: Recipe/Site3/Default/main.ini
- FOUND: Recipe/Site4/Default/main.ini
- FOUND: Recipe/Site5/Default/main.ini
- OK: Original Recipe/Seoul_LED_MIL removed (no longer exists)
- FOUND: WPF_Example/Utility/RecipeFileHelper.cs (with both new overloads and original methods)
- FOUND: WPF_Example/Custom/SystemSetting.cs (with CurrentSiteIndex)
- FOUND: .planning/phases/04-5-site/04-02-SUMMARY.md
- Build: 오류 0건 (MSBuild Debug)
