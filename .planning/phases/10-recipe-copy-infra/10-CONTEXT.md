# Phase 10: 레시피 복사 버그 수정 + 운영 인프라 - Context

**Gathered:** 2026-04-02
**Updated:** 2026-04-03
**Status:** Ready for re-planning

<domain>
## Phase Boundary

레시피 복사/삭제가 RecipeSavePath 루트 기준으로 안정적으로 동작하고, TCP는 siteNumber 기반 경로로 레시피를 참조하며, 검사 시퀀스 실행 시 Action별 소요시간(ms)이 Trace 로그에 기록된다.

- RCP-01: 레시피 복사/삭제가 RecipeSavePath 루트 기준으로 동작 + 덮어쓰기 지원
- OPS-01: Action별 택타임 로그 기록

</domain>

<decisions>
## Implementation Decisions

### 택타임 로그
- **D-01:** 기존 Logging 시스템의 ELogType.Trace에 [TAKT] 접두사로 통합 출력. 별도 ELogType 추가 안 함
- **D-02:** Action별 개별 출력 형식 — `[TAKT] {ActionName}: {elapsed}ms` (시퀀스 요약 불필요)
- **D-03:** 항상 출력. ON/OFF 설정 불필요, SystemSetting 변경 없음
- **D-04:** 기존 Logging 시스템의 날짜별 파일 분리 + LogDeleteDay(30일) 자동 삭제 그대로 적용. 별도 보관 정책 불필요

### 레시피 경로 구조 (변경됨 — 2026-04-03)
- **D-05:** UI(OpenRecipeWindow)의 Copy/Delete/CollectRecipe는 `RecipeSavePath` 루트 기준으로 동작. Site 하위 경로 사용 안 함
- **D-06:** `Copy()`에서 siteNumber 파라미터 제거. `RecipeSavePath/{prevName}` → `RecipeSavePath/{newName}` 복사
- **D-07:** 대상 디렉터리가 없으면 Directory.CreateDirectory()로 자동 생성 (유지)
- **D-08:** 덮어쓰기 시 기존 UI 흐름 유지 — CustomMessageBox.ShowConfirmation 후 forceCopy=true로 Copy() 호출 (유지)
- **D-09:** `Delete()`는 현재 `RecipeSavePath/{recipeName}` 기준으로 동작 중 — 변경 불필요
- **D-10:** TCP쪽 경로: `"Site" + siteNumber` → `siteNumber.ToString()`으로 변경 (예: `RecipeSavePath/1/레시피명/`)
- **D-11:** UI 호출부 `CollectRecipe(1)` → `CollectRecipe()` (파라미터 없는 버전)으로 변경
- **D-12:** 사용자가 세팅에서 `RecipeSavePath`를 `Recipe\1\` 등으로 지정하면 UI와 TCP 모두 같은 폴더 참조 가능

### Claude's Discretion
- ActionBase의 Timer.Stop() 이후 ElapsedMilliseconds 로그 출력 위치 결정
- CopyFilesRecursively 내부 대상 디렉터리 생성 로직 보완 방식

</decisions>

<canonical_refs>
## Canonical References

**Downstream agents MUST read these before planning or implementing.**

### 레시피 복사/삭제 (UI)
- `WPF_Example/Utility/RecipeFileHelper.cs` — RecipeFiles.Copy(), Delete(), CopyFilesRecursively(), CollectRecipe() 구현
- `WPF_Example/UI/Recipe/OpenRecipeWindow.xaml.cs` — Btn_Copy_Click(라인 63~103), Btn_Delete_Click(라인 105~130)

### 레시피 로드 (TCP)
- `WPF_Example/Custom/SystemHandler.cs` — ProcessRecipeChange()(라인 155~178) — packet.Site 기반 경로 조합
- `WPF_Example/Sequence/SequenceHandler.cs` — LoadRecipe(int siteNumber, string name)(라인 180)

### 택타임 로그
- `WPF_Example/Sequence/Action/ActionBase.cs` — Context.Timer.Restart()/Stop() 호출 위치
- `WPF_Example/Sequence/Sequence/SequenceContext.cs` — Stopwatch Timer 인스턴스
- `WPF_Example/Utility/Logging.cs` — Logging.PrintLog() API
- `WPF_Example/Setting/SystemSetting.cs` — ELogType enum (라인 18~26), LogDeleteDay (라인 81)

### 시스템
- `WPF_Example/SystemHandler.cs` — Logging.SetLog() 초기화 (라인 60~65)
- `WPF_Example/Setting/SystemSetting.cs` — RecipeSavePath 설정 (라인 42)

</canonical_refs>

<code_context>
## Existing Code Insights

### Reusable Assets
- `Logging.PrintLog(int logID, string format, params object[] args)` — 기존 로그 출력 API, ELogType.Trace로 호출하면 됨
- `ActionBase.Context.Timer` (Stopwatch) — 이미 Restart()/Stop() 호출 중, ElapsedMilliseconds만 로그 출력 추가
- `RecipeFiles.CopyFilesRecursively()` — 디렉터리 재귀 복사 이미 구현됨
- `RecipeFiles.Delete()` — 루트 기준 삭제 이미 구현됨 (수정 불필요)
- `RecipeFiles.CollectRecipe()` — 파라미터 없는 버전이 루트 기준 스캔 (이미 존재)
- `CustomMessageBox.ShowConfirmation()` — 덮어쓰기 확인 다이얼로그 이미 구현됨

### Established Patterns
- 로그 출력: `Logging.PrintLog((int)ELogType.Trace, "[TAG] message")` 형식
- 주석: `//YYMMDD hbk` 형식
- 경로 조합: `Path.Combine(RecipeSavePath, recipeName)` (UI), `Path.Combine(basePath, siteNumber.ToString(), recipeName)` (TCP)

### Integration Points
- `OpenRecipeWindow.Btn_Copy_Click` — Copy() 호출부에서 siteNumber 제거
- `OpenRecipeWindow.Btn_Delete_Click` — 변경 불필요 (이미 루트 기준)
- `OpenRecipeWindow` — CollectRecipe(1) → CollectRecipe()로 변경
- `Custom/SystemHandler.ProcessRecipeChange` — "Site" + siteNumber → siteNumber.ToString()
- `ActionBase.Run()` — Timer.Stop() 직후 PrintLog 추가 위치

</code_context>

<specifics>
## Specific Ideas

- 사용자가 세팅에서 RecipeSavePath를 `Recipe\1\`로 설정하면 UI에서 Site1 레시피를 직접 관리하면서 TCP에서도 같은 경로로 참조 가능. Site 전환은 세팅 경로 변경으로 처리.

</specifics>

<deferred>
## Deferred Ideas

None -- discussion stayed within phase scope

</deferred>

---

*Phase: 10-recipe-copy-infra*
*Context gathered: 2026-04-02*
*Context updated: 2026-04-03*
