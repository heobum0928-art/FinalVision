# Phase 10: 레시피 복사 버그 수정 + 운영 인프라 - Research

**Researched:** 2026-04-02
**Domain:** C# WPF — 파일시스템 디렉터리 복사, Stopwatch 기반 택타임 로깅
**Confidence:** HIGH

## Summary

이 Phase는 신규 라이브러리 없이 기존 코드베이스의 두 군데만 수술적으로 수정한다. 두 태스크 모두 이미 필요한 인프라(Logging, Stopwatch, Directory API)가 완비되어 있으며, 추가해야 할 코드 줄 수는 매우 적다.

**Plan 10-01 (RCP-01):** `RecipeFiles.Copy()`는 현재 Site 경로 개념이 없다. `siteNumber` 파라미터를 추가하고 경로를 `RecipeSavePath/Site{N}/{recipeName}`으로 전환해야 한다. `CopyFilesRecursively`는 대상 루트 디렉터리가 없으면 `GetDirectories` 호출 자체가 `DirectoryNotFoundException`을 던지므로, 재귀 복사 진입 전 `Directory.CreateDirectory(targetPath)` 한 줄로 해결된다. `OpenRecipeWindow.Btn_Copy_Click`도 `Copy(SelectedRecipeName, newName)` 호출을 `Copy(SelectedRecipeName, newName, 1)` 형태로 수정해야 한다.

**Plan 10-02 (OPS-01):** `ActionBase.OnEnd()`에 이미 `Context.Timer.Stop()`이 있다. 그 직후 `Logging.PrintLog((int)ELogType.Trace, "[TAKT] {0}: {1}ms", Name, Context.Timer.ElapsedMilliseconds)`를 한 줄 추가하면 된다. ELogType.Trace 로그는 `SystemHandler` 생성자에서 이미 초기화되어 날짜별 파일 분리와 30일 자동 삭제가 동작 중이다.

**Primary recommendation:** 두 태스크 모두 단일 메서드 수정이다. 영향 범위를 최소화하기 위해 기존 메서드 시그니처를 오버로드(siteNumber 파라미터 추가)로 처리한다.

---

<user_constraints>
## User Constraints (from CONTEXT.md)

### Locked Decisions
- **D-01:** 택타임 로그는 기존 ELogType.Trace에 `[TAKT]` 접두사로 통합 출력. 별도 ELogType 추가 안 함
- **D-02:** 출력 형식: `[TAKT] {ActionName}: {elapsed}ms` (시퀀스 요약 불필요)
- **D-03:** 항상 출력. ON/OFF 설정 불필요, SystemSetting 변경 없음
- **D-04:** 기존 날짜별 파일 분리 + LogDeleteDay(30일) 자동 삭제 그대로 적용. 별도 보관 정책 불필요
- **D-05:** 같은 Site 내 복사만 지원. Site간 복사 불필요
- **D-06:** `Copy()`에 `siteNumber` 파라미터 추가하여 `RecipeSavePath/Site{N}/` 기반으로 동작
- **D-07:** 대상 Site 디렉터리가 없으면 `Directory.CreateDirectory()`로 자동 생성
- **D-08:** 덮어쓰기 시 기존 UI 흐름 유지 — `CustomMessageBox.ShowConfirmation` 후 `forceCopy=true`로 `Copy()` 호출

### Claude's Discretion
- `ActionBase.OnEnd()` 내 `Timer.Stop()` 이후 `ElapsedMilliseconds` 로그 출력 위치 결정
- `CopyFilesRecursively` 내부 대상 디렉터리 생성 로직 보완 방식

### Deferred Ideas (OUT OF SCOPE)
없음 — 논의가 Phase 범위 내에서 완결됨
</user_constraints>

---

<phase_requirements>
## Phase Requirements

| ID | Description | Research Support |
|----|-------------|------------------|
| RCP-01 | 레시피 복사 시 Site 대상 디렉터리가 없으면 자동 생성하여 복사 성공 | `Directory.CreateDirectory(targetPath)` 를 `CopyFilesRecursively` 진입 전에 추가 + `Copy()` 시그니처에 `siteNumber` 파라미터 추가 |
| OPS-01 | Action별 소요시간(ms)을 로그에 기록 (기존 Stopwatch 활용) | `ActionBase.OnEnd()` 의 `Timer.Stop()` 직후 `Logging.PrintLog` 한 줄 추가 |
</phase_requirements>

---

## Standard Stack

### Core (기존 — 신규 패키지 없음)

| 라이브러리/API | 출처 | 목적 | 비고 |
|-------------|------|------|------|
| `System.IO.Directory` | .NET BCL | 디렉터리 생성/복사 | `Directory.CreateDirectory()` 는 이미 존재하면 no-op |
| `System.IO.File.Copy(src, dst, overwrite:true)` | .NET BCL | 파일 덮어쓰기 복사 | 현재 `CopyFilesRecursively`에 이미 적용됨 |
| `System.Diagnostics.Stopwatch` | .NET BCL | 경과시간 측정 | `ActionContext.Timer`로 이미 인스턴스화됨 |
| `FinalVisionProject.Utility.Logging` | 프로젝트 내부 | 파일 로그 출력 | `PrintLog(int logID, string format, params object[] args)` |
| `FinalVisionProject.Setting.ELogType.Trace` | 프로젝트 내부 | 로그 채널 ID | `SystemHandler` 생성자에서 이미 초기화됨 |

**설치 불필요:** 모든 의존성이 기존 코드베이스에 존재한다.

---

## Architecture Patterns

### 현재 레시피 경로 구조 (버그 상태)

```
RecipeSavePath/
└── {recipeName}/          ← Copy()가 이 경로만 인식함 (Site 개념 없음)
    ├── main.ini
    └── ...
```

### 목표 레시피 경로 구조 (수정 후)

```
RecipeSavePath/
└── Site1/
    └── {recipeName}/      ← Copy(prevName, newName, siteNumber:1) 이 이 경로 기준
        ├── main.ini
        └── ...
```

### Pattern 1: Directory.CreateDirectory 선행 삽입

**What:** `CopyFilesRecursively(sourcePath, targetPath)` 진입 직전에 `Directory.CreateDirectory(targetPath)` 를 호출한다. `Directory.CreateDirectory`는 경로가 이미 존재해도 예외를 던지지 않으므로 안전하다.

**When to use:** 대상 Site 디렉터리가 존재하지 않는 경우(최초 복사), 또는 이미 존재하는 경우(덮어쓰기) 모두 동일한 코드 경로로 처리.

**현재 버그 원인:**
```csharp
// CopyFilesRecursively의 첫 줄
foreach (string dirPath in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories)) {
    Directory.CreateDirectory(dirPath.Replace(sourcePath, targetPath));
}
```
`targetPath`(예: `Recipe/Site1/newRecipe`) 자체가 없으면 `GetDirectories` 이전에 이미 파일 복사 루프에서 `DirectoryNotFoundException` 발생. 또는 대상 루트 폴더가 없는 채로 `File.Copy`가 실패.

**수정 패턴:**
```csharp
// Source: 코드 분석 (RecipeFileHelper.cs:142)
private static void CopyFilesRecursively(string sourcePath, string targetPath) {
    //260402 hbk — 대상 루트 디렉터리가 없어도 자동 생성
    Directory.CreateDirectory(targetPath);

    foreach (string dirPath in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories)) {
        Directory.CreateDirectory(dirPath.Replace(sourcePath, targetPath));
    }
    foreach (string newPath in Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories)) {
        File.Copy(newPath, newPath.Replace(sourcePath, targetPath), true);
    }
}
```

### Pattern 2: Copy() 오버로드에 siteNumber 추가

**What:** 기존 `Copy(string prevName, string newName, bool forceCopy = false)` 에 `siteNumber` 파라미터를 추가하여 경로를 `RecipeSavePath/Site{N}/` 기준으로 구성.

**기존 경로 구성:**
```csharp
string prevDirPath = Path.Combine(SystemHandler.Handle.Setting.RecipeSavePath, prevName);
string newDirPath  = Path.Combine(SystemHandler.Handle.Setting.RecipeSavePath, newName);
```

**수정 후 경로 구성:**
```csharp
// Source: 코드 분석 — GetRecipeFilePath(int siteNumber, string name) 패턴 참고
string siteDir    = Path.Combine(SystemHandler.Handle.Setting.RecipeSavePath, "Site" + siteNumber);
string prevDirPath = Path.Combine(siteDir, prevName);
string newDirPath  = Path.Combine(siteDir, newName);
```

**D-07 충족:** `CopyFilesRecursively` 앞에 `Directory.CreateDirectory(siteDir)` 추가 필요는 없다. `CopyFilesRecursively` 내부에서 `Directory.CreateDirectory(targetPath)` 가 `siteDir/newName` 전체를 한 번에 생성해 준다.

### Pattern 3: OnEnd() 에 택타임 로그 삽입

**What:** `ActionBase.OnEnd()` 에서 `Context.Timer.Stop()` 직후 로그 출력 추가.

**현재 코드:**
```csharp
// ActionBase.cs:55
public virtual void OnEnd() {
    Context.Timer.Stop();
    Context.State = EContextState.Idle;
}
```

**수정 후:**
```csharp
// 260402 hbk — OPS-01: Action별 택타임 로그 출력
public virtual void OnEnd() {
    Context.Timer.Stop();
    Logging.PrintLog((int)ELogType.Trace, "[TAKT] {0}: {1}ms", Name, Context.Timer.ElapsedMilliseconds);
    Context.State = EContextState.Idle;
}
```

**로그 출력 위치 근거:**
- `OnEnd()`는 `SequenceBase.ExecuteAction()`에서 `actionContext.State == EContextState.Finish` 또는 `EContextResult.Error` 시 반드시 1회 호출됨 (SequenceBase.cs:221, 230)
- 이 시점에 `Timer.Stop()`이 이미 완료되어 `ElapsedMilliseconds`가 정확한 값을 가짐
- Error 케이스도 포함되어 비정상 종료 Action의 소요시간도 기록됨

### Anti-Patterns to Avoid

- **Copy()에서 siteNumber 없이 CollectRecipe(int) 와 혼용:** `CollectRecipe(int siteNumber)`는 이미 `Site{N}/` 경로를 스캔하도록 구현되어 있으나, `Copy()`는 아직 Site 경로를 모른다. 이 불일치가 현재 버그의 근본 원인.
- **별도 ELogType 신설:** D-01 결정 위반. `ELogType.Trace`에 `[TAKT]` 접두사로 구분하면 충분하다.
- **SequenceContext.Timer에 로그 추가:** `ActionContext.Timer`가 Action별 타이머이고, `SequenceContext.Timer`는 시퀀스 전체 타이머다. OPS-01은 Action별 소요시간이므로 `ActionContext.Timer`(= `Context.Timer` in ActionBase)가 정확한 대상.

---

## Don't Hand-Roll

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| 디렉터리 존재 확인 후 생성 | `if (!Directory.Exists(path)) Directory.CreateDirectory(path)` | `Directory.CreateDirectory(path)` 단독 호출 | BCL이 내부적으로 exist 체크 후 생성 — TOCTOU 레이스 없음, 이미 `GetModelFilePath()` 패턴에서도 동일하게 사용 중 |
| 전용 택타임 파일/채널 | 별도 LogInfo/채널 등록 | `ELogType.Trace` + `[TAKT]` 접두사 | 로그 인프라 이미 완비, 채널 추가는 `SystemHandler` 생성자 수정 필요 (D-01 위반) |

---

## Common Pitfalls

### Pitfall 1: Copy() 호출부 누락
**What goes wrong:** `RecipeFiles.Copy()` 시그니처에 `siteNumber` 추가 후 `OpenRecipeWindow.Btn_Copy_Click`(라인 86)의 기존 호출을 업데이트하지 않으면 컴파일 오류.
**Why it happens:** 수정 대상이 두 파일(RecipeFileHelper.cs, OpenRecipeWindow.xaml.cs)에 분산됨.
**How to avoid:** Plan 10-01에서 두 파일을 동시에 수정. 오버로드 대신 기존 시그니처에 파라미터 추가 시 모든 호출부 검색 필요.
**Warning signs:** `CS7036: There is no argument given that corresponds to the required parameter 'siteNumber'` 컴파일 오류.

### Pitfall 2: forceCopy 흐름과 siteNumber 정합성
**What goes wrong:** `Btn_Copy_Click` 라인 86에서 `RecipeFiles.Handle.Copy(SelectedRecipeName, newName)` — 현재 `forceCopy` 기본값은 `false`다. 덮어쓰기 확인 후 재호출 없이 이 한 줄만 있다.
**Why it happens:** 현재 UI 코드는 `HasRecipe(newName)` 확인 후 `ShowConfirmation`을 보여주지만, 실제 `Copy()` 호출 시 `forceCopy=true`를 전달하지 않는다 — 결과적으로 덮어쓰기 의사를 확인했음에도 `Copy()`가 `false`를 반환하는 버그가 잠재됨.
**How to avoid:** `ShowConfirmation` 이후 코드 경로에서 `Copy(SelectedRecipeName, newName, siteNumber:1, forceCopy:true)` 로 호출하도록 수정.
**Warning signs:** 동일 이름 레시피 복사 시 "copy fail" 에러 메시지 표시.

### Pitfall 3: OnEnd() override 시 base.OnEnd() 미호출
**What goes wrong:** `ActionBase.OnEnd()`를 override 하는 하위 클래스가 있다면, base의 `[TAKT]` 로그가 출력되지 않는다.
**Why it happens:** C# virtual override는 명시적 `base.OnEnd()` 호출 없이는 부모 로직이 실행되지 않음.
**How to avoid:** 코드베이스 검색 결과 `Action_Inspection.cs`는 `OnEnd()`를 override하지 않는다(현재 코드 확인). `OnCreate`, `OnLoad`, `Run`만 override. 추가 구현 시 `base.OnEnd()` 호출 패턴 문서화 필요.
**Warning signs:** 특정 Action에서 `[TAKT]` 로그 미출력.

---

## Code Examples

### Plan 10-01: RecipeFiles.Copy() 수정 완성 형태

```csharp
// Source: RecipeFileHelper.cs — 수정 대상 메서드
// //260402 hbk — D-06: siteNumber 파라미터 추가, D-07: 대상 디렉터리 자동 생성
public bool Copy(string prevName, string newName, int siteNumber, bool forceCopy = false) {
    string siteDir    = Path.Combine(SystemHandler.Handle.Setting.RecipeSavePath, "Site" + siteNumber);
    string prevDirPath = Path.Combine(siteDir, prevName);
    string newDirPath  = Path.Combine(siteDir, newName);

    if (Directory.Exists(newDirPath) && (forceCopy == false)) {
        return false;
    }

    CopyFilesRecursively(prevDirPath, newDirPath);
    return true;
}

private static void CopyFilesRecursively(string sourcePath, string targetPath) {
    //260402 hbk — D-07: 대상 루트 디렉터리 자동 생성 (이미 존재해도 안전)
    Directory.CreateDirectory(targetPath);

    foreach (string dirPath in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories)) {
        Directory.CreateDirectory(dirPath.Replace(sourcePath, targetPath));
    }
    foreach (string newPath in Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories)) {
        File.Copy(newPath, newPath.Replace(sourcePath, targetPath), true);
    }
}
```

### Plan 10-01: OpenRecipeWindow.Btn_Copy_Click 수정 지점

```csharp
// Source: OpenRecipeWindow.xaml.cs:86 — forceCopy 분기 정합성 수정
// 덮어쓰기 미확인 경로:
RecipeFiles.Handle.Copy(SelectedRecipeName, newName, 1)

// 덮어쓰기 확인 후 경로 (HasRecipe true 분기):
RecipeFiles.Handle.Copy(SelectedRecipeName, newName, 1, forceCopy: true)
```

### Plan 10-02: ActionBase.OnEnd() 수정 완성 형태

```csharp
// Source: ActionBase.cs:55
public virtual void OnEnd() {
    Context.Timer.Stop();
    //260402 hbk — OPS-01: Action별 택타임 Trace 로그 출력
    Logging.PrintLog((int)ELogType.Trace, "[TAKT] {0}: {1}ms", Name, Context.Timer.ElapsedMilliseconds);
    Context.State = EContextState.Idle;
}
```

**예상 로그 출력 예시 (Trace/{date}_Trace.log):**
```
14:23:05:3,[TAKT] Bolt_One: 125ms
14:23:05:5,[TAKT] Bolt_Two: 88ms
14:23:05:7,[TAKT] Assy_Rail_One: 102ms
```

---

## State of the Art

| 현재 상태 | 수정 후 | 영향 |
|----------|---------|------|
| `Copy(prevName, newName, forceCopy)` — Site 경로 없음 | `Copy(prevName, newName, siteNumber, forceCopy)` — Site 하위 경로 기준 | `CollectRecipe(int siteNumber)` 와 경로 정합성 완성 |
| `OnEnd()` 에 로그 없음 | `[TAKT] {Name}: {ms}ms` 출력 | 운영 중 택타임 분석 가능 |
| 대상 Site 디렉터리 없으면 복사 실패 | `Directory.CreateDirectory(targetPath)` 선행 삽입 | 최초 Site 디렉터리 생성 자동화 |

---

## Open Questions

없음 — CONTEXT.md의 결정(D-01~D-08)이 모든 불확실 지점을 해소했다.

---

## Environment Availability

Step 2.6: SKIPPED (순수 코드 수정 Phase, 외부 도구/서비스 의존성 없음)

---

## Validation Architecture

> `workflow.nyquist_validation` 키가 config.json에 없으므로 enabled로 취급.

### Test Framework

| Property | Value |
|----------|-------|
| Framework | 없음 — 이 프로젝트는 자동화 테스트 프레임워크(xUnit/MSTest/NUnit)가 미설치 |
| Config file | 없음 |
| Quick run command | 빌드 컴파일: `msbuild WPF_Example/FinalVision.csproj` |
| Full suite command | 동일 — 자동화 테스트 없음, 수동 검증으로 대체 |

### Phase Requirements → Test Map

| Req ID | Behavior | Test Type | Automated Command | 파일 존재 |
|--------|----------|-----------|-------------------|---------|
| RCP-01 | Site 디렉터리 없는 경로에 레시피 복사 성공 | manual-smoke | 컴파일 후 UI에서 복사 실행 | N/A |
| RCP-01 | 기존 레시피 경로에 덮어쓰기 복사 성공 | manual-smoke | 동일 이름으로 복사 시도 후 덮어쓰기 확인 | N/A |
| OPS-01 | 검사 실행 후 Trace 로그에 `[TAKT]` 라인 기록 | manual-smoke | 검사 1회 실행 후 `Trace/*.log` 파일 확인 | N/A |

**Manual-only justification:** WPF 프로젝트에 단위 테스트 인프라 미존재. 빌드 성공 + 런타임 수동 검증이 현 프로젝트 표준.

### Sampling Rate

- **Per task commit:** `msbuild` 빌드 성공 확인
- **Per wave merge:** 빌드 성공 + 각 Success Criteria 수동 확인
- **Phase gate:** 3개 Success Criteria 전부 충족 후 `/gsd:verify-work`

### Wave 0 Gaps

없음 — 기존 테스트 인프라가 없으므로 Wave 0 신규 파일 생성 불필요. 빌드 성공이 최소 검증 기준.

---

## Sources

### Primary (HIGH confidence)

- `WPF_Example/Utility/RecipeFileHelper.cs` — `Copy()`, `CopyFilesRecursively()`, `CollectRecipe(int)` 구현 직접 검토
- `WPF_Example/UI/Recipe/OpenRecipeWindow.xaml.cs` — `Btn_Copy_Click` 전체 흐름 (라인 63~96) 직접 검토
- `WPF_Example/Sequence/Action/ActionBase.cs` — `OnBegin()`, `OnEnd()`, `Context.Timer` 사용 패턴 직접 검토
- `WPF_Example/Sequence/Sequence/SequenceBase.cs` — `OnEnd()` 호출 위치(라인 221, 230) 직접 검토
- `WPF_Example/Utility/Logging.cs` — `PrintLog(int, string, params object[])` API 직접 검토
- `WPF_Example/Setting/SystemSetting.cs` — `ELogType` enum (라인 18~26), `LogDeleteDay` (라인 81) 직접 검토
- `WPF_Example/SystemHandler.cs` — `Logging.SetLog()` 초기화 (라인 60~65) 직접 검토
- `WPF_Example/Sequence/Sequence/SequenceContext.cs` — `ActionContext.Timer: Stopwatch` 인스턴스 직접 검토

### Secondary (MEDIUM confidence)

없음 — 이 Phase는 외부 라이브러리/패턴 조사가 불필요하다. 모든 결정 근거가 코드베이스 직접 분석에 기반한다.

---

## Metadata

**Confidence breakdown:**
- Standard stack: HIGH — 모든 API가 현재 코드에서 이미 사용 중, 직접 확인
- Architecture: HIGH — 수정 대상 메서드와 호출 경로를 소스 코드에서 직접 추적
- Pitfalls: HIGH — 실제 코드의 현재 버그 경로(forceCopy=false 누락, 대상 디렉터리 미생성)를 소스에서 직접 확인

**Research date:** 2026-04-02
**Valid until:** 2026-05-02 (안정적인 .NET BCL API, 빠른 변경 없음)
