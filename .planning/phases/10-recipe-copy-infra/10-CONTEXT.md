# Phase 10: 레시피 복사 버그 수정 + 운영 인프라 - Context

**Gathered:** 2026-04-02
**Status:** Ready for planning

<domain>
## Phase Boundary

레시피 복사가 Site 경로 기반으로 안정적으로 동작하고, 검사 시퀀스 실행 시 Action별 소요시간(ms)이 Trace 로그에 기록된다.

- RCP-01: 레시피 복사 시 Site 대상 디렉터리 자동 생성 + 덮어쓰기 지원
- OPS-01: Action별 택타임 로그 기록

</domain>

<decisions>
## Implementation Decisions

### 택타임 로그
- **D-01:** 기존 Logging 시스템의 ELogType.Trace에 [TAKT] 접두사로 통합 출력. 별도 ELogType 추가 안 함
- **D-02:** Action별 개별 출력 형식 — `[TAKT] {ActionName}: {elapsed}ms` (시퀀스 요약 불필요)
- **D-03:** 항상 출력. ON/OFF 설정 불필요, SystemSetting 변경 없음
- **D-04:** 기존 Logging 시스템의 날짜별 파일 분리 + LogDeleteDay(30일) 자동 삭제 그대로 적용. 별도 보관 정책 불필요

### 레시피 복사 경로
- **D-05:** 같은 Site 내 복사만 지원. Site간 복사 불필요
- **D-06:** Copy()에 siteNumber 파라미터 추가하여 Site 하위 경로(`RecipeSavePath/Site{N}/`) 기반으로 동작
- **D-07:** 대상 Site 디렉터리가 없으면 Directory.CreateDirectory()로 자동 생성
- **D-08:** 덮어쓰기 시 기존 UI 흐름 유지 — CustomMessageBox.ShowConfirmation 후 forceCopy=true로 Copy() 호출

### Claude's Discretion
- ActionBase의 Timer.Stop() 이후 ElapsedMilliseconds 로그 출력 위치 결정
- CopyFilesRecursively 내부 대상 디렉터리 생성 로직 보완 방식

</decisions>

<canonical_refs>
## Canonical References

**Downstream agents MUST read these before planning or implementing.**

### 레시피 복사
- `WPF_Example/Utility/RecipeFileHelper.cs` — RecipeFiles.Copy(), CopyFilesRecursively(), CollectRecipe(int siteNumber) 구현
- `WPF_Example/UI/Recipe/OpenRecipeWindow.xaml.cs` — Btn_Copy_Click 복사 UI 흐름 (라인 63~96)

### 택타임 로그
- `WPF_Example/Sequence/Action/ActionBase.cs` — Context.Timer.Restart()/Stop() 호출 위치
- `WPF_Example/Sequence/Sequence/SequenceContext.cs` — Stopwatch Timer 인스턴스
- `WPF_Example/Utility/Logging.cs` — Logging.PrintLog() API
- `WPF_Example/Setting/SystemSetting.cs` — ELogType enum (라인 18~26), LogDeleteDay (라인 81)

### 시스템
- `WPF_Example/SystemHandler.cs` — Logging.SetLog() 초기화 (라인 60~65)

</canonical_refs>

<code_context>
## Existing Code Insights

### Reusable Assets
- `Logging.PrintLog(int logID, string format, params object[] args)` — 기존 로그 출력 API, ELogType.Trace로 호출하면 됨
- `ActionBase.Context.Timer` (Stopwatch) — 이미 Restart()/Stop() 호출 중, ElapsedMilliseconds만 로그 출력 추가
- `RecipeFiles.CopyFilesRecursively()` — 디렉터리 재귀 복사 이미 구현됨
- `CustomMessageBox.ShowConfirmation()` — 덮어쓰기 확인 다이얼로그 이미 구현됨

### Established Patterns
- 로그 출력: `Logging.PrintLog((int)ELogType.Trace, "[TAG] message")` 형식
- 주석: `//YYMMDD hbk` 형식
- 경로 조합: `Path.Combine(RecipeSavePath, "Site" + siteNumber, recipeName)` 패턴

### Integration Points
- `OpenRecipeWindow.Btn_Copy_Click` — Copy() 호출부 수정 필요 (siteNumber 전달)
- `ActionBase.Run()` — Timer.Stop() 직후 PrintLog 추가 위치

</code_context>

<specifics>
## Specific Ideas

No specific requirements -- open to standard approaches

</specifics>

<deferred>
## Deferred Ideas

None -- discussion stayed within phase scope

</deferred>

---

*Phase: 10-recipe-copy-infra*
*Context gathered: 2026-04-02*
