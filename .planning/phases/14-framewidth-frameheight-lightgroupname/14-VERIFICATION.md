---
phase: 14-framewidth-frameheight-lightgroupname
verified: 2026-04-07T00:00:00Z
status: passed
score: 3/3 must-haves verified
re_verification: false
gaps: []
human_verification:
  - test: "실제 INI 파일 저장 후 FrameWidth/FrameHeight 항목 부재 확인"
    expected: "레시피 저장 후 INI 파일에 FrameWidth/FrameHeight 키가 없어야 함"
    why_human: "MSBuild 빌드 성공과 프로퍼티 삭제 확인만으로는 런타임 직렬화 결과를 검증할 수 없음 — 실제 Save() 실행 후 파일 확인 필요"
  - test: "레시피 로드 후 LightGroupName 유지 확인"
    expected: "INI에 저장된 LightGroupName 값이 OnLoad 후에도 변경되지 않음"
    why_human: "런타임 동작 — 실제 레시피 파일 Load 후 LightGroupName PropertyGrid 값 확인 필요"
  - test: "Reset 후 LightGroupName 복원 확인"
    expected: "Shot 선택 → Reset 클릭 → LightGroupName이 레시피 로드 시점 값으로 복원됨"
    why_human: "RestoreShot → CopyTo 체인의 런타임 동작 — UI 조작으로 확인 필요"
---

# Phase 14: FrameWidth/FrameHeight/LightGroupName 버그 수정 Verification Report

**Phase Goal:** INI 파일에 불필요한 FrameWidth/FrameHeight가 저장되지 않고, LightGroupName이 레시피별로 올바르게 저장/로드/복원된다
**Verified:** 2026-04-07
**Status:** passed (automated checks) — human verification recommended for runtime behavior
**Re-verification:** No — initial verification

---

## Goal Achievement

### Observable Truths

| # | Truth | Status | Evidence |
|---|-------|--------|----------|
| 1 | INI 파일에 FrameWidth, FrameHeight 항목이 저장되지 않는다 | VERIFIED | CameraSlaveParam.cs, CameraParam.cs에서 FrameWidth/FrameHeight public 프로퍼티 완전 삭제 확인. 소스 .cs 파일 내 FrameWidth/FrameHeight는 삭제 이유 주석(//260407 hbk)만 남음. ParamBase.Save()가 리플렉션으로 public property만 직렬화하므로 프로퍼티 삭제로 INI 오염 차단됨. |
| 2 | 레시피 로드 후 LightGroupName이 INI에 저장된 값을 유지한다 (DefaultLight로 덮어쓰이지 않음) | VERIFIED | Sequence_Inspection.OnLoad() (line 125)에 `_MyParam.LightGroupName = DefaultLight` 없음. 해당 대입은 OnCreate() (line 93)에만 존재. OnLoad()는 DeviceName만 재설정하고 주석으로 명시됨. |
| 3 | Reset 후 LightGroupName이 백업 시점 값으로 복원된다 | VERIFIED | CameraSlaveParam.CopyTo() — CameraSlaveParam 분기(line 176)와 CameraParam 분기(line 190) 모두 `LightGroupName = this.LightGroupName` 활성 코드로 존재. CameraParam.CopyTo() — CameraParam 분기(line 243) `camParam.LightGroupName = this.LightGroupName` 활성 코드 확인. RestoreShot() → CopyTo() 체인이 LightGroupName을 복원하는 경로 완성. |

**Score:** 3/3 truths verified

---

### Required Artifacts

| Artifact | Expected | Status | Details |
|----------|----------|--------|---------|
| `WPF_Example/Sequence/Param/CameraSlaveParam.cs` | FrameWidth/FrameHeight 필드 제거, LightGroupName CopyTo 주석 해제 | VERIFIED | Line 21: `//260407 hbk — FrameWidth/FrameHeight 레거시 필드 제거` 주석만 잔존. Line 176, 190: LightGroupName CopyTo 활성 코드. |
| `WPF_Example/Sequence/Param/CameraParam.cs` | FrameWidth/FrameHeight 필드 제거 | VERIFIED | Line 73: `//260407 hbk — FrameWidth/FrameHeight 레거시 필드 제거` 주석만 잔존. Line 243: LightGroupName CopyTo 활성 코드. |
| `WPF_Example/Custom/Sequence/Inspection/Sequence_Inspection.cs` | OnLoad에서 LightGroupName 강제 재설정 제거 | VERIFIED | OnLoad() (line 125-134): LightGroupName 대입 없음. DeviceName만 DefaultCamera로 재설정. 주석 //260407 hbk 존재. |

---

### Key Link Verification

| From | To | Via | Status | Details |
|------|----|-----|--------|---------|
| `Sequence_Inspection.OnLoad()` | `CameraMasterParam.LightGroupName setter` | OnLoad가 더 이상 DefaultLight를 강제 세팅하지 않으므로 INI 로드 값이 유지됨 | VERIFIED | OnLoad() 내 `_MyParam.LightGroupName` 대입 없음 확인 (line 125-134 전체 검토) |
| `CameraSlaveParam.CopyTo()` | `InspectionParam.LightGroupName` | Reset(RestoreShot)이 CopyTo를 호출하면 LightGroupName도 복원됨 | VERIFIED | `slaveParam.LightGroupName = this.LightGroupName` (line 176), `camParam.LightGroupName = this.LightGroupName` (line 190) — 모두 활성 코드 |

---

### Data-Flow Trace (Level 4)

Not applicable. Phase 14는 UI 렌더링 컴포넌트가 아닌 파라미터 직렬화/역직렬화 버그 수정이다. 동적 데이터 렌더링 경로 없음. 런타임 동작은 Human Verification으로 이관.

---

### Behavioral Spot-Checks

Step 7b: SKIPPED — WPF 애플리케이션으로 서버/서비스 없이는 런타임 동작 테스트 불가. 코드 레벨 검증 완료.

---

### Requirements Coverage

| Requirement ID | Source Plan | Description | Status | Evidence |
|---------------|-------------|-------------|--------|----------|
| FIX-01 | 14-01-PLAN.md | FrameWidth/FrameHeight 레거시 필드가 INI에 저장되는 버그 제거 | SATISFIED | CameraSlaveParam.cs, CameraParam.cs에서 public int FrameWidth/FrameHeight 프로퍼티 완전 삭제 + CopyTo 내 레거시 대입 제거. 커밋 59f73a5 확인. |
| FIX-02 | 14-01-PLAN.md | LightGroupName이 OnLoad에서 덮어써지는 버그 수정 | SATISFIED | Sequence_Inspection.OnLoad()에서 `_MyParam.LightGroupName = DefaultLight` 제거 확인. 커밋 f421f04 확인. |
| FIX-03 | 14-01-PLAN.md | CopyTo에서 LightGroupName 복사 활성화 | SATISFIED | CameraSlaveParam.CopyTo()와 CameraParam.CopyTo() 내 LightGroupName 대입 주석 해제 활성화 확인. 커밋 f421f04 확인. |

**Note:** FIX-01, FIX-02, FIX-03은 REQUIREMENTS.md의 공식 요구사항 표에 정의되지 않은 Phase 14 전용 버그픽스 ID다. ROADMAP.md (line 167)에서 Phase 14 Requirements로 선언되어 있으나 REQUIREMENTS.md Traceability 표에는 미등록 상태 — 이는 REQUIREMENTS.md가 v2.0 기능 요구사항 중심으로 작성되어 버그픽스 ID를 별도 관리하지 않는 구조에서 발생한 것으로, 버그픽스 자체는 모두 구현 완료됨.

---

### Anti-Patterns Found

| File | Line | Pattern | Severity | Impact |
|------|------|---------|----------|--------|
| — | — | — | — | 발견 없음 |

스캔 항목:
- FrameWidth/FrameHeight 프로퍼티: 소스 .cs 파일 내 0건 (binary/XML은 서드파티)
- TODO/FIXME/PLACEHOLDER: 해당 3개 파일에서 없음
- `return null` / `return {}`: 해당 없음 (CopyTo는 bool 반환, 정상)
- `_MyParam.LightGroupName = DefaultLight`: OnLoad 내 0건, OnCreate 내 1건 (정상 — 신규 레시피 생성 시 기본값 세팅)

---

### Human Verification Required

#### 1. INI 저장 시 FrameWidth/FrameHeight 항목 부재 확인

**Test:** 레시피 Save 후 해당 .ini 파일을 텍스트 에디터로 열어 FrameWidth, FrameHeight 키 검색
**Expected:** 해당 키가 존재하지 않아야 함
**Why human:** MSBuild 빌드 성공 + 프로퍼티 삭제는 정적 분석 완료. 런타임에서 ParamBase.Save() 리플렉션이 실제로 해당 프로퍼티를 제외하는지는 실행 후 파일 확인 필요.

#### 2. 레시피 로드 후 LightGroupName 유지 확인

**Test:** LightGroupName이 "Group_A"로 저장된 레시피를 Load → PropertyGrid에서 LightGroupName 확인
**Expected:** "DefaultLight"가 아닌 "Group_A" (INI 저장값)가 표시되어야 함
**Why human:** OnLoad 코드 경로는 정적 확인 완료. 실제 INI 파싱 후 값이 올바르게 바인딩되는지는 런타임 동작.

#### 3. Reset 후 LightGroupName 복원 확인

**Test:** 레시피 로드 후 LightGroupName을 임의로 변경 → Shot 선택 후 Reset 클릭 → LightGroupName 값 확인
**Expected:** 레시피 로드 시점 LightGroupName으로 복원됨
**Why human:** RestoreShot() → CopyTo() → LightGroupName 복원 체인이 코드 레벨에서 연결되었으나, UI 동작과 PropertyGrid 갱신은 런타임 확인 필요.

---

### Gaps Summary

없음. 3개 must-have truth 모두 코드 레벨에서 VERIFIED. 런타임 행동 확인 3건은 자동화 불가능한 WPF 애플리케이션 동작으로 Human Verification 항목으로 분류함.

---

### Commit Verification

| Commit | Files | Status |
|--------|-------|--------|
| 59f73a5 | CameraSlaveParam.cs, CameraParam.cs | FOUND — FrameWidth/FrameHeight 제거 (2 files, 11+11 deletions) |
| f421f04 | Sequence_Inspection.cs, CameraSlaveParam.cs, CameraParam.cs | FOUND — LightGroupName OnLoad 제거 + CopyTo 활성화 (3 files) |

---

_Verified: 2026-04-07_
_Verifier: Claude (gsd-verifier)_
