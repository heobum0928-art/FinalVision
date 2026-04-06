---
phase: 12
slug: run-grab
status: draft
nyquist_compliant: false
wave_0_complete: false
created: 2026-04-06
---

# Phase 12 — Validation Strategy

> Per-phase validation contract for feedback sampling during execution.

---

## Test Infrastructure

| Property | Value |
|----------|-------|
| **Framework** | 없음 (WPF 수동 검증, 단위 테스트 인프라 없음) |
| **Config file** | none |
| **Quick run command** | `dotnet build WPF_Example.sln` |
| **Full suite command** | 빌드 후 전체 시나리오 수동 실행 |
| **Estimated runtime** | ~30 seconds (빌드) + 수동 검증 |

---

## Sampling Rate

- **After every task commit:** Run `dotnet build WPF_Example.sln`
- **After every plan wave:** 빌드 + 해당 기능 수동 클릭 검증
- **Before `/gsd:verify-work`:** 4가지 Success Criteria 모두 수동 확인
- **Max feedback latency:** 30 seconds (빌드)

---

## Per-Task Verification Map

| Task ID | Plan | Wave | Requirement | Test Type | Automated Command | File Exists | Status |
|---------|------|------|-------------|-----------|-------------------|-------------|--------|
| 12-01-01 | 01 | 1 | OPS-02 | manual | `빌드 + Grab 클릭 → 카메라 촬상 확인` | ❌ 수동 | ⬜ pending |
| 12-01-02 | 01 | 1 | OPS-02 | manual | `빌드 + Run 클릭(이미지 있음) → 검사 결과 확인` | ❌ 수동 | ⬜ pending |
| 12-01-03 | 01 | 1 | OPS-02 | manual | `빌드 + Run 클릭(이미지 없음) → 경고 표시 확인` | ❌ 수동 | ⬜ pending |
| 12-02-01 | 02 | 1 | IMG-03 | manual | `LoadFolder 클릭 → Shot1~5 매핑 확인` | ❌ 수동 | ⬜ pending |
| 12-03-01 | 03 | 2 | IMG-04 | manual | `DeleteFolder 클릭 → 확인 → 삭제 확인` | ❌ 수동 | ⬜ pending |
| 12-03-02 | 03 | 2 | IMG-04 | manual | `DeleteFolder 클릭 → 취소 → 삭제 안 됨 확인` | ❌ 수동 | ⬜ pending |

*Status: ⬜ pending · ✅ green · ❌ red · ⚠️ flaky*

---

## Wave 0 Requirements

Existing infrastructure covers all phase requirements. 수동 검증 전용 — 테스트 파일 불필요.

---

## Manual-Only Verifications

| Behavior | Requirement | Why Manual | Test Instructions |
|----------|-------------|------------|-------------------|
| Grab 클릭 → 카메라 촬상+검사 | OPS-02 | 카메라 하드웨어 필요 | 1. Grab 클릭 2. 카메라 촬상 확인 3. 검사 결과 표시 확인 |
| Run 클릭 → 로드 이미지 검사 | OPS-02 | UI 동작 확인 | 1. 이미지 로드 2. Run 클릭 3. 검사 결과 표시 확인 |
| 시간폴더 선택 → 5-Shot 일괄 로드 | IMG-03 | UI+파일시스템 | 1. LoadFolder 클릭 2. 시간폴더 선택 3. 5개 탭 이미지 확인 |
| 폴더 삭제 + 확인 다이얼로그 | IMG-04 | UI+파일시스템 | 1. DeleteFolder 클릭 2. 확인 3. 폴더 삭제 확인 |

---

## Validation Sign-Off

- [ ] All tasks have `<automated>` verify or Wave 0 dependencies
- [ ] Sampling continuity: no 3 consecutive tasks without automated verify
- [ ] Wave 0 covers all MISSING references
- [ ] No watch-mode flags
- [ ] Feedback latency < 30s
- [ ] `nyquist_compliant: true` set in frontmatter

**Approval:** pending
