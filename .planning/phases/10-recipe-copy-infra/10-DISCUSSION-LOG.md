# Phase 10: 레시피 복사 버그 수정 + 운영 인프라 - Discussion Log

> **Audit trail only.** Do not use as input to planning, research, or execution agents.
> Decisions are captured in CONTEXT.md -- this log preserves the alternatives considered.

**Date:** 2026-04-02
**Phase:** 10-레시피 복사 버그 수정 + 운영 인프라
**Areas discussed:** 택타임 로그 형식, 레시피 복사 경로 처리, 로그 파일 관리

---

## 택타임 로그 형식

### Q1: 택타임 로그 출력 대상 ELogType

| Option | Description | Selected |
|--------|-------------|----------|
| 기존 Trace 로그에 통합 | ELogType.Trace에 [TAKT] 접두사로 출력 | ✓ |
| 새 ELogType 추가 (TaktTime) | 별도 로그 파일 분리 | |
| Result 로그에 포함 | 검사 결과와 함께 기록 | |

**User's choice:** 기존 Trace 로그에 통합
**Notes:** 별도 파일 불필요, 기존 Trace 로그에서 검사 흐름과 함께 확인 가능

### Q2: Action별 로그 형식

| Option | Description | Selected |
|--------|-------------|----------|
| Action별 개별 출력 | 각 Action 완료 시점에 개별 로그 | ✓ |
| 시퀀스 완료 시 요약 출력 | 전체 시퀀스 끝에 한 줄로 요약 | |
| 둘 다 | 개별 + 요약 모두 출력 | |

**User's choice:** Action별 개별 출력
**Notes:** [TAKT] {ActionName}: {elapsed}ms 형식

### Q3: 출력 ON/OFF 설정

| Option | Description | Selected |
|--------|-------------|----------|
| 항상 출력 | 별도 설정 없이 항상 기록 | ✓ |
| 설정으로 ON/OFF | SystemSetting에 EnableTaktLog 추가 | |

**User's choice:** 항상 출력
**Notes:** 단순하게 유지, 디버깅에도 유용

---

## 레시피 복사 경로 처리

### Q1: Site간 복사 지원 범위

| Option | Description | Selected |
|--------|-------------|----------|
| 같은 Site 내 복사만 | Site1 내에서 다른 이름으로 복사 | ✓ |
| Site간 복사 지원 | Site1에서 Site2로 복사 가능 | |
| 두 가지 모두 | 같은 Site + Site간 복사 | |

**User's choice:** 같은 Site 내 복사만
**Notes:** Copy()에 siteNumber 파라미터 추가

### Q2: 덮어쓰기 정책

| Option | Description | Selected |
|--------|-------------|----------|
| 확인 후 forceCopy=true | 기존 UI 흐름 유지 | ✓ |
| Claude가 결정 | 적절한 방식으로 구현 | |

**User's choice:** 확인 후 forceCopy=true
**Notes:** 기존 CustomMessageBox.ShowConfirmation 흐름 그대로 유지

---

## 로그 파일 관리

### Q1: 별도 보관 정책 필요 여부

| Option | Description | Selected |
|--------|-------------|----------|
| 기존 설정 그대로 | Trace 통합이므로 30일 자동 삭제 적용 | ✓ |
| 별도 보관 기간 설정 | 택타임 로그만 다른 보관 정책 | |

**User's choice:** 기존 설정 그대로
**Notes:** Trace 로그에 통합되므로 별도 관리 불필요

---

## Claude's Discretion

- ActionBase Timer.Stop() 이후 ElapsedMilliseconds 로그 출력 위치
- CopyFilesRecursively 내부 대상 디렉터리 생성 보완 방식

## Deferred Ideas

None
