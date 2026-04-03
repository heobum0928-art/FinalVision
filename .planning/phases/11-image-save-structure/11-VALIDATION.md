---
phase: 11
slug: image-save-structure
status: draft
nyquist_compliant: false
wave_0_complete: false
created: 2026-04-03
---

# Phase 11 — Validation Strategy

> Per-phase validation contract for feedback sampling during execution.

---

## Test Infrastructure

| Property | Value |
|----------|-------|
| **Framework** | 없음 — 프로젝트에 자동화 테스트 인프라 없음 (WPF 앱, .NET 4.8) |
| **Config file** | none |
| **Quick run command** | `MSBuild /t:Build /p:Configuration=Release` (빌드 성공 확인) |
| **Full suite command** | 수동: 앱 빌드 + 시뮬 검사 1회 실행 + 파일 경로 확인 |
| **Estimated runtime** | ~30 seconds (빌드) |

---

## Sampling Rate

- **After every task commit:** Run `MSBuild /t:Build /p:Configuration=Release` (컴파일 에러 없음 확인)
- **After every plan wave:** 수동 smoke test — 앱 실행, 검사 1회, 파일 경로 확인
- **Before `/gsd:verify-work`:** IMG-01/IMG-02 success criteria 전체 수동 통과
- **Max feedback latency:** ~30 seconds (빌드)

---

## Per-Task Verification Map

| Task ID | Plan | Wave | Requirement | Test Type | Automated Command | File Exists | Status |
|---------|------|------|-------------|-----------|-------------------|-------------|--------|
| 11-01-01 | 01 | 1 | IMG-01 | manual-smoke | 빌드 성공 + 수동: 검사 후 `D:\Log\{yyyyMMdd}\{HHmmss_fff}` 폴더 확인 | ❌ W0 | ⬜ pending |
| 11-01-02 | 01 | 1 | IMG-01 | manual-smoke | 수동: 동일 검사 Shot1~5가 같은 시간폴더에 저장 확인 | ❌ W0 | ⬜ pending |
| 11-02-01 | 02 | 2 | IMG-01, IMG-02 | manual-smoke | 수동: NG 검사 후 원본+Annotated 파일 쌍 확인 | ❌ W0 | ⬜ pending |
| 11-02-02 | 02 | 2 | IMG-02 | manual-smoke | 수동: SaveOkImage=false 상태에서 OK 이미지 미저장 확인 | ❌ W0 | ⬜ pending |

*Status: ⬜ pending · ✅ green · ❌ red · ⚠️ flaky*

---

## Wave 0 Requirements

- [ ] 자동화 테스트 없음 — 수동 검증으로 대체 (기존 프로젝트 관행과 동일)
- [ ] 빌드 스크립트 확인 — MSBuild 컴파일 성공 여부로 기본 검증

*Existing infrastructure covers compilation verification. All behavioral tests are manual-only.*

---

## Manual-Only Verifications

| Behavior | Requirement | Why Manual | Test Instructions |
|----------|-------------|------------|-------------------|
| 검사 후 이미지가 날짜>시간 폴더에 저장 | IMG-01 | WPF + 카메라 하드웨어 의존, 테스트 프레임워크 없음 | 1. 앱 실행 2. 시뮬 검사 1회 3. `D:\Log\{yyyyMMdd}\{HHmmss_fff}\` 폴더 존재 확인 |
| Shot1~5 동일 시간폴더 저장 | IMG-01 | 시퀀스 실행 필요 | 1. 검사 1회 실행 2. 시간폴더 내 파일 목록 확인 (Shot별 파일 존재) |
| 밀리초 충돌 시 `_2` 접미사 | IMG-01 | 빠른 연속 검사 필요 | 1. 시뮬에서 빠른 연속 검사 2회 2. 동일 밀리초 시 `_2` 폴더 확인 |
| OK 이미지 기본 미저장 | IMG-02 | 설정 상태 의존 | 1. SaveOkImage=false 확인 2. OK 결과 검사 3. 폴더에 NG 파일만 존재 확인 |
| OK 이미지 저장 옵션 활성화 | IMG-02 | 설정 변경 + 검사 필요 | 1. SaveOkImage=true 설정 2. OK 검사 3. 원본+Annotated 파일 쌍 확인 |

---

## Validation Sign-Off

- [ ] All tasks have `<automated>` verify or Wave 0 dependencies
- [ ] Sampling continuity: no 3 consecutive tasks without automated verify
- [ ] Wave 0 covers all MISSING references
- [ ] No watch-mode flags
- [ ] Feedback latency < 30s
- [ ] `nyquist_compliant: true` set in frontmatter

**Approval:** pending
