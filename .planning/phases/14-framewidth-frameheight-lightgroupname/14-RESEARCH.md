# Phase 14: 레시피 파일 설정값 버그 수정 (FrameWidth/FrameHeight/LightGroupName) - Research

**Researched:** 2026-04-07
**Domain:** WPF C# — INI 레시피 직렬화 (ParamBase.Save/Load) + 카메라 파라미터 상속 구조
**Confidence:** HIGH (전체 코드베이스 직접 분석)

---

## Summary

Phase 14는 레시피 INI 파일에 잘못된 값이 저장되거나 로드 시 잘못 덮어쓰이는 세 가지 버그를 수정한다.

**Bug 1 — FrameWidth/FrameHeight 저장 오염:** `CameraSlaveParam`과 `CameraParam` 두 클래스 모두 `FrameWidth`, `FrameHeight` 필드를 `public int`로 선언한다. `ParamBase.Save()`는 `GetProperties()`로 모든 `public` 프로퍼티를 반사적으로 저장하므로, 이 필드들이 `[System.ComponentModel.Browsable(false)]`로 UI 숨김 처리돼 있어도 INI에 `FrameWidth=0`, `FrameHeight=0`으로 기록된다. `InspectionParam`은 `CameraSlaveParam`을 상속하므로 동일하게 영향받는다. 이 값들은 "레거시 미사용 필드"(주석 260327 hbk, 260330 hbk)로, 저장 자체가 불필요하다.

**Bug 2 — LightGroupName 덮어쓰기:** 레시피 로드 순서는 (1) `LoadFromIni()` — INI의 `LightGroupName` 값을 각 Action의 `InspectionParam`에 설정 → (2) `ExecOnLoad()` — `Sequence_Inspection.OnLoad()`가 `_MyParam.LightGroupName = DefaultLight`로 `CameraMasterParam`을 재설정 후, `CameraMasterParam.set` 프로퍼티가 `ChildList`의 `CameraSlaveParam`(= `InspectionParam`)에 값을 전파한다. 즉, 레시피 파일에 저장된 실제 LightGroupName은 OnLoad 이후 `DefaultLight` 상수로 항상 덮어써진다. 그러나 현재 코드베이스에서 LightGroupName이 `DefaultLight`가 아닌 다른 값으로 쓰이는 경우가 존재하는지(예: "WAFER" 분기, SystemHandler.cs line 109) 별도 확인이 필요하다.

**Bug 3 — CopyTo에서 LightGroupName 미복사:** `CameraSlaveParam.CopyTo()` 및 `CameraParam.CopyTo()`에서 `//slaveParam.LightGroupName = this.LightGroupName;` 라인이 주석 처리돼 있다. Reset 기능(Phase 13)의 `RestoreShot()`은 `_backup[shotIndex].CopyTo(target)`을 호출하는데, 이때 LightGroupName이 복원되지 않는다.

**Primary recommendation:** (1) `FrameWidth`/`FrameHeight`를 직렬화에서 제외하도록 `ParamBase.Save()`에 Browsable(false) 프로퍼티 스킵 로직을 추가하거나, 해당 필드에 직렬화 제외 어트리뷰트를 추가한다. (2) `Sequence_Inspection.OnLoad()`의 `DefaultLight` 강제 재설정 로직이 실제로 필요한지 판단하고, 필요하다면 LightGroupName을 레시피에 저장하지 않도록 Save에서도 제외한다. (3) `CopyTo()`의 LightGroupName 주석 라인 처리 방향을 결정한다.

---

## Project Constraints (from CLAUDE.md)

CLAUDE.md가 존재하지 않으므로, STATE.md의 누적 결정 사항을 적용한다.

| Constraint | Source | Rule |
|------------|--------|------|
| 신규 NuGet 패키지 추가 금지 | STATE.md [v2.0] | PropertyTools.Wpf, Ookii.Dialogs.Wpf로 충분 |
| 주석 스타일 | MEMORY.md | `//YYMMDD hbk` 형식 유지 |
| FAI/Halcon/에지 측정 절대 추가 금지 | MEMORY.md | Blob 유무 검사 프로젝트 |
| HIK 전용 | STATE.md [v1.0] | Basler 제거됨 |

---

## Bug Analysis (코드베이스 직접 분석)

### Bug 1: FrameWidth / FrameHeight가 INI에 0으로 저장됨

**파일:** `WPF_Example/Sequence/Param/CameraSlaveParam.cs` (lines 22-24), `CameraParam.cs` (lines 74-76)

```csharp
[System.ComponentModel.Browsable(false)]   //260330 hbk — PropertyGrid 숨김 (레거시 미사용 필드)
public int FrameWidth { get; set; }
[System.ComponentModel.Browsable(false)]   //260330 hbk
public int FrameHeight { get; set; }
```

`ParamBase.Save()`는 `BindingFlags.Instance | BindingFlags.Public`으로 모든 public 프로퍼티를 반사 열거한다. `[Browsable(false)]`는 PropertyGrid 표시만 제어하며 직렬화와 무관하다. 따라서 `FrameWidth=0`, `FrameHeight=0`이 INI에 기록된다.

기존 레시피 파일(`Recipe/Site1/Seoul_LED_MIL/main.ini`)에서도 확인:
```ini
[Param0]
FrameWidth=0
FrameHeight=0
```

이 값들은 "260327 hbk — 레거시 필드 대입 제거"라는 주석이 달린 `CopyTo()`의 두 줄(slaveParam.FrameWidth = this.FrameWidth 등)에서도 나타난다. 레거시 필드이므로 INI 오염 및 불필요한 저장을 제거해야 한다.

**수정 선택지:**

| 방법 | 코드 변경 위치 | 장점 | 단점 |
|------|---------------|------|------|
| A. ParamBase.Save()에서 Browsable(false) 프로퍼티 스킵 | ParamBase.cs 한 곳 | 모든 파라미터 클래스에 일괄 적용 | 다른 Browsable(false) 필드가 의도적으로 저장되고 있을 경우 부작용 |
| B. 해당 필드에 [Obsolete]나 커스텀 무시 어트리뷰트 추가 | CameraSlaveParam.cs, CameraParam.cs | 명시적 | 어트리뷰트 정의 필요 |
| C. FrameWidth/FrameHeight 프로퍼티 완전 제거 | CameraSlaveParam.cs, CameraParam.cs | 가장 깔끔 | CopyTo()에서 참조하고 있으므로 해당 줄도 함께 제거 필요 |

**권장:** 방법 C. 레거시 필드이며 어디서도 실제로 읽히거나 의미있게 쓰이지 않는다. `CopyTo()`의 `slaveParam.FrameWidth = this.FrameWidth;` 줄도 함께 제거한다.

**영향 파일:**
- `WPF_Example/Sequence/Param/CameraSlaveParam.cs` — 필드 선언 제거 + CopyTo() 내 대입 제거
- `WPF_Example/Sequence/Param/CameraParam.cs` — 동일

---

### Bug 2: LightGroupName이 OnLoad에서 DefaultLight로 항상 덮어써짐

**로드 흐름:**

```
LoadRecipe(name)
  └─ LoadFromIni(name)             ← INI에서 각 Action Param 로드 (LightGroupName 포함)
  └─ ExecOnLoad(name)
       └─ Sequence_Inspection.OnLoad()
            ├─ _MyParam.LightGroupName = DefaultLight;   ← 덮어쓰기!
            └─ _MyParam.DeviceName = DefaultCamera;
                (CameraMasterParam.set LightGroupName → ChildList 전파)
```

**코드 위치:** `Sequence_Inspection.cs` lines 127-129:

```csharp
//260407 hbk — INI Load가 ChildList에 레거시 값(Corner_Align 등) 덮어쓰므로 기본값 재세팅
_MyParam.LightGroupName = DefaultLight;
_MyParam.DeviceName = DefaultCamera;
```

이 재설정은 260407에 의도적으로 추가됐다. 주석은 "INI Load가 ChildList에 레거시 값(Corner_Align 등) 덮어쓰므로 기본값 재세팅"이라고 설명한다. 레거시 레시피 파일에 저장된 `LightGroupName=DEFAULT` 같은 잘못된 값이 로드될 때 방어 목적이었던 것으로 보인다.

**문제의 성격:**

현재 모든 InspectionParam의 `LightGroupName`은 `DefaultLight` 상수 하나로만 사용되어야 한다면 Bug 2는 의도된 동작이다. 그러나 Phase 14의 이름에 LightGroupName이 명시된 것으로 보아, 다음 두 시나리오 중 하나가 버그의 원인이다:

- **시나리오 A:** LightGroupName을 레시피마다 다르게 저장/로드해야 하는데 OnLoad가 강제 재설정한다 → OnLoad의 재설정 라인을 제거하고, Save/Load 흐름이 LightGroupName을 올바르게 처리하도록 한다.
- **시나리오 B:** LightGroupName은 항상 DefaultLight여야 하는데, 레거시 INI 파일에 잘못된 값이 저장돼 있어서 로드 후 예상치 못한 동작이 생긴다 → OnLoad 재설정은 유지하되, Save 시에 LightGroupName을 저장하지 않도록 제외한다.

`SystemHandler.cs` line 109에 `if (camParam.LightGroupName == "WAFER")` 분기가 존재하므로, 현재 프로젝트에서도 LightGroupName이 "WAFER"와 기본값 두 가지로 나뉘어 사용된다. 그러므로 **시나리오 A** 가능성이 높다: 레시피마다 LightGroupName을 달리 저장해야 하는데, OnLoad가 덮어쓰고 있다.

**영향 파일:**
- `WPF_Example/Custom/Sequence/Inspection/Sequence_Inspection.cs` — OnLoad()의 LightGroupName 재설정 라인 처리

---

### Bug 3: CopyTo()에서 LightGroupName 미복사 (Reset 버그)

**코드 위치:** `CameraSlaveParam.cs` lines 178-179, `CameraParam.cs` lines 248-249:

```csharp
//slaveParam.LightGroupName = this.LightGroupName;   ← 주석 처리됨
slaveParam.LightLevel = this.LightLevel;
```

Phase 13의 Reset 기능은 `_backup[shotIndex].CopyTo(target)`로 백업에서 복원한다. LightGroupName이 CopyTo에서 복사되지 않으므로, Reset 후에도 LightGroupName은 현재 값(변경된 값)으로 남는다.

**왜 주석 처리됐는가:** 코드 이력상 확실하지 않으나, `CameraMasterParam`이 `LightGroupName` 설정 시 `ChildList` 전체에 전파하는 구조(CameraMasterParam.cs line 31-32)가 있으므로, 개별 CopyTo에서 복사하면 MasterParam과 불일치가 생길 것을 우려해 주석 처리한 것으로 추정된다.

**그러나:** InspectionParam(CameraSlaveParam)에서 LightGroupName을 개별적으로 복사하는 것은 Reset 컨텍스트에서 올바른 동작이다. OnLoad 후에는 모든 ChildList의 LightGroupName이 동일한 DefaultLight로 설정되므로, Reset 시 백업된 값으로 복원해야 한다면 이 주석을 해제해야 한다.

**단, Bug 2 해결 방향에 따라 이 수정의 필요성이 달라진다.**
- Bug 2를 시나리오 A로 해결(OnLoad 재설정 제거)하면 → LightGroupName이 INI에서 정상 로드 → Reset 시 원래 INI 값으로 복원이 의미있음 → CopyTo 주석 해제 필요
- Bug 2를 시나리오 B로 해결(LightGroupName을 저장 제외)하면 → LightGroupName은 항상 DefaultLight → CopyTo 주석 해제 불필요

---

## Architecture Patterns

### ParamBase 직렬화 메커니즘

`ParamBase.Save()`는 Reflection으로 public 프로퍼티를 열거해 타입별로 INI에 저장한다. `ParamBase.Load()`는 동일 프로퍼티 이름으로 INI에서 읽어 `prop.SetValue(this, value)`로 설정한다.

**직렬화 제어 방법:**
```csharp
// 현재: Browsable(false) 사용 — 직렬화와 무관
[System.ComponentModel.Browsable(false)]
public int FrameWidth { get; set; }

// 올바른 제외 방법: ParamBase.Save()에서 Browsable 체크 추가
// 또는 필드 자체 제거
```

### CameraMasterParam → CameraSlaveParam 전파 패턴

```csharp
// CameraMasterParam.LightGroupName setter
set {
    _LightGroupName = value;
    foreach (CameraSlaveParam camParam in ChildList) {
        camParam.LightGroupName = _LightGroupName;   // 모든 자식에 전파
    }
}
```

`Sequence_Inspection`의 `_MyParam`은 `CameraMasterParam`이다. `_MyParam.LightGroupName = X`를 설정하면 `ChildList`(= 5개 Action_Inspection의 InspectionParam)에 모두 전파된다. 따라서 LightGroupName을 Shot별로 다르게 설정하려면 `CameraMasterParam`을 거치지 않고 각 `InspectionParam`에 직접 설정해야 한다.

---

## Don't Hand-Roll

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| 직렬화 제외 마킹 | 커스텀 어트리뷰트 클래스 | 기존 `[Browsable(false)]` 체크 또는 필드 제거 | 신규 패키지 추가 금지 결정과 일치 |
| 레거시 필드 유지 | FrameWidth/Height를 null-safe wrapper로 감싸기 | 단순 필드 제거 | 레거시임이 주석으로 명시됨 |

---

## Common Pitfalls

### Pitfall 1: Browsable(false)를 직렬화 제외로 오해
**What goes wrong:** `[Browsable(false)]`는 PropertyGrid에서 숨길 뿐, `ParamBase.Save()`의 Reflection 루프는 모든 `public` 프로퍼티를 열거하므로 INI에 그대로 저장된다.
**Why it happens:** Browsable 어트리뷰트가 UI와 직렬화 두 가지 역할을 할 것이라고 기대
**How to avoid:** ParamBase.Save()의 루프에 `BrowsableAttribute` 체크를 추가하거나, 레거시 필드는 완전히 제거

### Pitfall 2: OnLoad 재설정이 INI 로드를 덮어쓰는 순서 문제
**What goes wrong:** LoadFromIni()가 먼저 INI에서 LightGroupName을 설정하고, 이후 ExecOnLoad()가 다시 DefaultLight로 재설정한다.
**Why it happens:** OnLoad의 재설정이 필요한 이유(레거시 INI 방어)가 있었으나, 정상 INI에 대해서도 무조건 덮어쓴다.
**How to avoid:** 정상 INI에서 로드된 값을 신뢰하거나, LightGroupName을 INI에서 아예 저장/로드하지 않는 방향 중 하나를 선택

### Pitfall 3: CopyTo에서 LightGroupName 복사 시 MasterParam과 불일치
**What goes wrong:** InspectionParam(CameraSlaveParam)의 LightGroupName을 직접 복사하면, CameraMasterParam._LightGroupName과 값이 달라질 수 있다.
**Why it happens:** MasterParam의 전파 구조 때문에 설정 경로가 두 개 존재
**How to avoid:** Bug 2 해결 후, LightGroupName 관리 책임을 MasterParam 또는 SlaveParam 중 한 쪽으로 명확히 정하고 그에 맞게 CopyTo 수정

---

## Code Examples

### ParamBase.Save() 반사 루프 (현재 코드)
```csharp
// Source: WPF_Example/Sequence/Param/ParamBase.cs lines 325-370
public virtual bool Save(IniFile saveFile, string group) {
    PropertyInfo[] props = this.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
    foreach (var prop in props) {
        string name = prop.Name;
        string type = prop.PropertyType.Name;
        switch (type) {
            case "Int32":
                saveFile[group][name] = (int)prop.GetValue(this);
                break;
            // ... String, Boolean, Rect, Circle 등
        }
    }
    return true;
}
```

Browsable 필터를 추가한다면 루프 시작 부분에:
```csharp
// 추가 가능한 Browsable 필터
var browsable = prop.GetCustomAttribute<System.ComponentModel.BrowsableAttribute>();
if (browsable != null && !browsable.Browsable) continue;
```

그러나 이 변경은 다른 `Browsable(false)` 필드(SimulImagePath, LastBlobArea 등 InspectionParam의 많은 필드)도 저장에서 제외하게 된다. **의도치 않은 부작용을 막으려면 필드 제거(방법 C)가 더 안전하다.**

### CameraMasterParam LightGroupName 전파 (현재 코드)
```csharp
// Source: WPF_Example/Sequence/Param/CameraMasterParam.cs lines 25-35
public string LightGroupName {
    get { return _LightGroupName; }
    set {
        _LightGroupName = value;
        foreach (CameraSlaveParam camParam in ChildList) {
            camParam.LightGroupName = _LightGroupName;
        }
    }
}
```

---

## Open Questions

1. **LightGroupName이 레시피마다 달라야 하는가?**
   - What we know: `SystemHandler.cs`에 `if (camParam.LightGroupName == "WAFER")` 분기가 있다. OnCreate/OnLoad에서 `DefaultLight` 상수로 재설정한다. 모든 Action의 InspectionParam은 동일한 LightGroupName을 가져야 한다(MasterParam이 전파하므로).
   - What's unclear: "WAFER" LightGroupName을 가진 레시피가 실제로 사용되는지, Phase 14 버그 리포트에서 구체적으로 어떤 증상이 발생했는지
   - Recommendation: 플래너가 사용자에게 버그 증상을 재확인하거나, OnLoad 재설정 제거 후 테스트로 검증

2. **CopyTo에서 LightGroupName 주석 처리는 Bug 3가 맞는가?**
   - What we know: Phase 13의 Reset은 CopyTo를 사용하고, LightGroupName 줄은 주석 처리됨
   - What's unclear: Reset 시 LightGroupName이 복원되지 않는 것이 실제 문제를 일으키는지
   - Recommendation: Bug 2 해결 방향 결정 후 연동해서 처리

3. **기존 레시피 파일(INI) 마이그레이션 필요성**
   - What we know: `Recipe/Site1/Seoul_LED_MIL/main.ini`에 `FrameWidth=0`, `FrameHeight=0`, `LightGroupName=DEFAULT`가 기록돼 있다
   - What's unclear: 이 값들이 로드 시 문제를 일으키는지 (현재 OnLoad 재설정이 LightGroupName을 덮어쓰므로 Load 단계에서는 문제없을 수 있음)
   - Recommendation: 코드 수정 후 기존 INI 파일은 다음 Save 시 자동으로 정리됨 — 별도 마이그레이션 불필요

---

## Environment Availability

Step 2.6: SKIPPED (순수 코드 수정 — 외부 툴/서비스 의존성 없음)

---

## Validation Architecture

### Test Framework
| Property | Value |
|----------|-------|
| Framework | MSBuild (WPF 프로젝트, 자동화 테스트 없음) |
| Config file | WPF_Example/WPF_Example.csproj |
| Quick run command | MSBuild WPF_Example.sln /p:Configuration=Debug 0 errors 확인 |
| Full suite command | 앱 실행 후 수동 레시피 저장/로드 검증 |

### Phase Requirements → Test Map

| ID | Behavior | Test Type | Automated Command | File Exists? |
|----|----------|-----------|-------------------|-------------|
| FIX-01 | INI 저장 시 FrameWidth/FrameHeight 항목 미포함 | manual | Build 후 레시피 저장 → INI 파일 확인 | N/A |
| FIX-02 | 레시피 로드 후 LightGroupName이 INI 저장값과 일치 | manual | LoadRecipe → Param 값 확인 | N/A |
| FIX-03 | Reset 후 LightGroupName이 백업값으로 복원 | manual | 앱 실행 → Reset 클릭 → PropertyGrid 확인 | N/A |

### Wave 0 Gaps
None — 자동화 테스트 인프라가 없는 WPF 프로젝트이며, 빌드 성공(0 errors)이 컴파일 검증 프록시다.

---

## Standard Stack

| 기존 자산 | 목적 | 비고 |
|-----------|------|------|
| `ParamBase.Save()` / `ParamBase.Load()` | INI 직렬화/역직렬화 | 수정 대상 가능 |
| `CameraSlaveParam` | InspectionParam 기반 클래스, FrameWidth/Height/LightGroupName 보유 | 수정 대상 |
| `CameraParam` | FrameWidth/Height 보유 | 수정 대상 |
| `Sequence_Inspection.OnLoad()` | 레시피 로드 후 기본값 재설정 | 수정 대상 |
| `CameraSlaveParam.CopyTo()` / `CameraParam.CopyTo()` | Reset 복원 경로 | LightGroupName 주석 해제 여부 결정 필요 |

**신규 패키지:** 없음. 기존 코드베이스 수정만으로 충분.

---

## State of the Art

| Old Behavior | Correct Behavior | Bug Source |
|-------------|-----------------|------------|
| FrameWidth=0, FrameHeight=0가 INI에 기록됨 | 해당 항목 미포함 | CameraSlaveParam/CameraParam에 레거시 public 필드 존재 |
| 로드 후 LightGroupName이 DefaultLight로 덮어써짐 | INI 저장값 유지 | Sequence_Inspection.OnLoad() 재설정 라인 |
| Reset 후 LightGroupName 미복원 | Reset 시 LightGroupName 복원 | CopyTo()의 LightGroupName 주석 처리 |

---

## Sources

### Primary (HIGH confidence)
- `WPF_Example/Sequence/Param/ParamBase.cs` — Save/Load 반사 루프 직접 분석
- `WPF_Example/Sequence/Param/CameraSlaveParam.cs` — FrameWidth/Height 선언, LightGroupName 프로퍼티, CopyTo
- `WPF_Example/Sequence/Param/CameraParam.cs` — 동일 구조
- `WPF_Example/Sequence/Param/CameraMasterParam.cs` — LightGroupName 전파 구조
- `WPF_Example/Custom/Sequence/Inspection/Sequence_Inspection.cs` — OnLoad 재설정 라인
- `WPF_Example/Sequence/SequenceHandler.cs` — LoadFromIni/ExecOnLoad 순서
- `Recipe/Site1/Seoul_LED_MIL/main.ini` — 실제 INI 파일로 FrameWidth=0 확인

---

## Metadata

**Confidence breakdown:**
- Bug 1 (FrameWidth/Height): HIGH — INI 파일에서 직접 확인됨
- Bug 2 (LightGroupName 덮어쓰기): HIGH — 코드 흐름 직접 추적
- Bug 3 (CopyTo 주석): HIGH — 코드 직접 확인 / 단 수정 방향은 Bug 2에 종속

**Research date:** 2026-04-07
**Valid until:** 코드베이스 변경 전까지 (stable)
