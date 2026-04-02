---
phase: 10-recipe-copy-infra
plan: 01
subsystem: ui
tags: [recipe, copy, site, wpf, csharp]

# Dependency graph
requires:
  - phase: 04-5-site
    provides: CollectRecipe(int siteNumber), GetRecipeFilePath(int, string), Site 경로 구조
provides:
  - "RecipeFiles.Copy(prevName, newName, int siteNumber, bool forceCopy=false) — Site 하위 경로 기준 복사"
  - "CopyFilesRecursively — 대상 루트 디렉터리 자동 생성 (Directory.CreateDirectory 선행)"
  - "OpenRecipeWindow.Btn_Copy_Click — siteNumber=1 + forceCopy 정합성 수정"
  - "InspectionListView.MenuItem_Save_As_Click — siteNumber=1 + forceCopy 정합성 수정"
affects: [recipe-editor, site-switch, recipe-load]

# Tech tracking
tech-stack:
  added: []
  patterns:
    - "Copy() siteNumber 오버로드: 기존 시그니처 교체, Site 하위 경로(RecipeSavePath/Site{N}/) 기준"
    - "Directory.CreateDirectory(targetPath) 선행 호출: TOCTOU 방지, if (!Exists) 패턴 사용 금지"
    - "HasRecipe/forceCopy 분기: try 블록 안에서 처리, 두 분기 각각 별도 Copy() 호출"

key-files:
  created: []
  modified:
    - WPF_Example/Utility/RecipeFileHelper.cs
    - WPF_Example/UI/Recipe/OpenRecipeWindow.xaml.cs
    - WPF_Example/UI/ControlItem/InspectionListView.xaml.cs

key-decisions:
  - "Copy() 구 시그니처(string, string, bool) 완전 제거 — siteNumber 필수 파라미터로 API 일관성 확보"
  - "CopyFilesRecursively: if (!Directory.Exists) 패턴 금지, Directory.CreateDirectory 무조건 호출(no-op 안전)"
  - "InspectionListView도 동일 패턴으로 수정 — 빌드 오류 방지(Rule 3 auto-fix)"

patterns-established:
  - "Recipe Copy 패턴: HasRecipe 체크 → forceCopy 분기 → Copy(name1, name2, siteNumber, forceCopy) 두 경로"

requirements-completed: [RCP-01]

# Metrics
duration: 8min
completed: 2026-04-02
---

# Phase 10 Plan 01: Recipe Copy Infra Summary

**RecipeFiles.Copy()를 Site 경로 기반(Site{N}/ 하위)으로 교체하고, CopyFilesRecursively에 대상 디렉터리 자동 생성을 추가하여 레시피 복사 시 경로 불일치와 forceCopy 누락 버그를 해소한다**

## Performance

- **Duration:** ~8 min
- **Started:** 2026-04-02T23:41:00Z
- **Completed:** 2026-04-02T23:43:57Z
- **Tasks:** 2 (+ 1 auto-fix deviation)
- **Files modified:** 3

## Accomplishments

- Copy() 시그니처를 `Copy(prevName, newName, int siteNumber, bool forceCopy=false)`로 교체하여 Site 하위 경로 기준 동작 확보
- CopyFilesRecursively 본문 첫 줄에 `Directory.CreateDirectory(targetPath)` 추가 — 대상 Site 디렉터리 미존재 시 자동 생성
- OpenRecipeWindow.Btn_Copy_Click 및 InspectionListView.MenuItem_Save_As_Click 양쪽 호출부 siteNumber=1 + forceCopy 정합성 수정

## Task Commits

1. **Task 1: RecipeFiles.Copy() siteNumber 오버로드 + CopyFilesRecursively 대상 디렉터리 자동 생성** - `ea19bd1` (feat)
2. **Task 2: OpenRecipeWindow + InspectionListView Copy() 호출부 siteNumber + forceCopy 정합성 수정** - `a831b2c` (feat)

## Files Created/Modified

- `WPF_Example/Utility/RecipeFileHelper.cs` — Copy() 시그니처 교체, CopyFilesRecursively Directory.CreateDirectory 추가
- `WPF_Example/UI/Recipe/OpenRecipeWindow.xaml.cs` — Btn_Copy_Click siteNumber=1, forceCopy 분기 수정
- `WPF_Example/UI/ControlItem/InspectionListView.xaml.cs` — MenuItem_Save_As_Click siteNumber=1, CollectRecipe(1) 수정

## Decisions Made

- Copy() 구 시그니처(string, string, bool) 완전 제거: siteNumber를 필수 파라미터로 만들어 호출부 누락 방지
- if (!Directory.Exists) 패턴 금지: Directory.CreateDirectory는 이미 존재하면 no-op이므로 TOCTOU 레이스 방지를 위해 무조건 호출

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 3 - Blocking] InspectionListView.xaml.cs에서 구 Copy() 시그니처 호출 수정**
- **Found during:** Task 2 (전체 Copy() 호출부 검증)
- **Issue:** `WPF_Example/UI/ControlItem/InspectionListView.xaml.cs` 85번째 줄에서 구 시그니처 `Copy(curRecipe, newName)` 사용 — 컴파일 오류 발생 예정
- **Fix:** HasRecipe 분기 추가 + `Copy(curRecipe, newName, 1)` / `Copy(curRecipe, newName, 1, forceCopy: true)` 로 수정, `CollectRecipe()` → `CollectRecipe(1)` 수정
- **Files modified:** WPF_Example/UI/ControlItem/InspectionListView.xaml.cs
- **Verification:** `grep -rn "RecipeFiles.Handle.Copy(" WPF_Example/` — 모든 호출부 siteNumber 포함 확인
- **Committed in:** a831b2c (Task 2 commit에 포함)

---

**Total deviations:** 1 auto-fixed (Rule 3 blocking)
**Impact on plan:** 빌드 성공에 필수적인 수정. 계획 범위 내 동일 패턴 적용으로 scope creep 없음.

## Issues Encountered

None

## User Setup Required

None - no external service configuration required.

## Next Phase Readiness

- 레시피 복사가 Site 경로 기반으로 안정적으로 동작함
- Plan 02(택타임 로그)와 독립적으로 완료됨
- 빌드 성공 전제: 구 Copy() 시그니처 호출부가 모두 제거됨

---
*Phase: 10-recipe-copy-infra*
*Completed: 2026-04-02*
