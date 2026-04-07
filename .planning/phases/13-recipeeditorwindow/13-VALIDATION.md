---
phase: 13
slug: recipeeditorwindow
status: draft
nyquist_compliant: false
wave_0_complete: false
created: 2026-04-07
---

# Phase 13 — Validation Strategy

> Per-phase validation contract for feedback sampling during execution.

---

## Test Infrastructure

| Property | Value |
|----------|-------|
| **Framework** | 없음 — WPF 수동 검증 |
| **Config file** | 해당 없음 |
| **Quick run command** | MSBuild 빌드 성공 확인 |
| **Full suite command** | 빌드 성공 + 수동 UI 조작 |
| **Estimated runtime** | ~30 seconds (빌드) |

---

## Sampling Rate

- **After every task commit:** MSBuild 빌드 성공 확인
- **After every plan wave:** 빌드 + 수동 UI 검증
- **Before `/gsd:verify-work`:** Full suite must be green
- **Max feedback latency:** 30 seconds

---

## Per-Task Verification Map

| Task ID | Plan | Wave | Requirement | Test Type | Automated Command | File Exists | Status |
|---------|------|------|-------------|-----------|-------------------|-------------|--------|
| 13-01-01 | 01 | 1 | RCP-05 (D-01) | manual | 빌드 후 레시피 로드 → _backup 존재 확인 | N/A | ⬜ pending |
| 13-01-02 | 01 | 1 | RCP-05 (D-02) | manual | 빌드 + UI에서 Reset 버튼 표시 확인 | N/A | ⬜ pending |
| 13-01-03 | 01 | 1 | RCP-05 (D-03) | manual | Shot 선택 → 파라미터 편집 → Reset → 값 복원 확인 | N/A | ⬜ pending |

*Status: ⬜ pending · ✅ green · ❌ red · ⚠️ flaky*

---

## Wave 0 Requirements

*Existing infrastructure covers all phase requirements — 자동화 테스트 인프라 불필요. 빌드 성공 + 수동 검증으로 충분.*

---

## Manual-Only Verifications

| Behavior | Requirement | Why Manual | Test Instructions |
|----------|-------------|------------|-------------------|
| 레시피 로드 후 백업 생성 | RCP-05 (D-01) | WPF UI 앱, 자동 테스트 프레임워크 없음 | 1. 앱 실행 2. 레시피 로드 3. 디버그 확인 또는 코드 리뷰로 _backup 딕셔너리 생성 확인 |
| Reset 버튼 표시 | RCP-05 (D-02) | UI 시각적 확인 필요 | 1. 앱 실행 2. InspectionListView 툴바에서 Reset 버튼 존재 확인 |
| Shot별 파라미터 복원 | RCP-05 (D-03) | 편집 → Reset → 값 비교 수동 확인 | 1. 레시피 로드 2. Shot 선택 후 파라미터 변경 3. Reset 클릭 4. 변경된 값이 로드 시점 값으로 복원됨 확인 5. 다른 Shot은 변경 없음 확인 |

---

## Validation Sign-Off

- [ ] All tasks have `<automated>` verify or Wave 0 dependencies
- [ ] Sampling continuity: no 3 consecutive tasks without automated verify
- [ ] Wave 0 covers all MISSING references
- [ ] No watch-mode flags
- [ ] Feedback latency < 30s
- [ ] `nyquist_compliant: true` set in frontmatter

**Approval:** pending
