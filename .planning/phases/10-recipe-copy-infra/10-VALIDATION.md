---
phase: 10
slug: recipe-copy-infra
status: draft
nyquist_compliant: false
wave_0_complete: false
created: 2026-04-02
---

# Phase 10 — Validation Strategy

> Per-phase validation contract for feedback sampling during execution.

---

## Test Infrastructure

| Property | Value |
|----------|-------|
| **Framework** | None — WPF 프로젝트, 자동화 테스트 프레임워크 미설치 |
| **Config file** | none |
| **Quick run command** | `dotnet build FinalVision.sln` |
| **Full suite command** | `dotnet build FinalVision.sln` |
| **Estimated runtime** | ~15 seconds |

---

## Sampling Rate

- **After every task commit:** Run `dotnet build FinalVision.sln`
- **After every plan wave:** Run `dotnet build FinalVision.sln`
- **Before `/gsd:verify-work`:** Build must succeed
- **Max feedback latency:** 15 seconds

---

## Per-Task Verification Map

| Task ID | Plan | Wave | Requirement | Test Type | Automated Command | File Exists | Status |
|---------|------|------|-------------|-----------|-------------------|-------------|--------|
| 10-01-01 | 01 | 1 | RCP-01 | build + manual | `dotnet build FinalVision.sln` | N/A | ⬜ pending |
| 10-02-01 | 02 | 1 | OPS-01 | build + manual | `dotnet build FinalVision.sln` | N/A | ⬜ pending |

*Status: ⬜ pending · ✅ green · ❌ red · ⚠️ flaky*

---

## Wave 0 Requirements

Existing infrastructure covers all phase requirements. No test framework installation needed.

---

## Manual-Only Verifications

| Behavior | Requirement | Why Manual | Test Instructions |
|----------|-------------|------------|-------------------|
| 레시피 복사 시 Site 디렉터리 자동 생성 | RCP-01 | WPF UI 상호작용 필요 | OpenRecipeWindow에서 레시피 선택 → Copy → 새 이름 입력 → Site 디렉터리 생성 확인 |
| 기존 레시피 덮어쓰기 | RCP-01 | WPF UI 상호작용 필요 | 동일 이름 레시피 복사 시 확인 다이얼로그 → Yes → 파일 덮어쓰기 확인 |
| Action별 택타임 로그 출력 | OPS-01 | 시퀀스 실행 필요 | 검사 실행 후 Trace 로그 파일에서 [TAKT] 접두사 라인 확인 |

---

## Validation Sign-Off

- [ ] All tasks have `<automated>` verify or Wave 0 dependencies
- [ ] Sampling continuity: no 3 consecutive tasks without automated verify
- [ ] Wave 0 covers all MISSING references
- [ ] No watch-mode flags
- [ ] Feedback latency < 15s
- [ ] `nyquist_compliant: true` set in frontmatter

**Approval:** pending
