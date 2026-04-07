---
phase: 14
slug: framewidth-frameheight-lightgroupname
status: draft
nyquist_compliant: false
wave_0_complete: false
created: 2026-04-07
---

# Phase 14 — Validation Strategy

> Per-phase validation contract for feedback sampling during execution.

---

## Test Infrastructure

| Property | Value |
|----------|-------|
| **Framework** | Manual INI verification + Visual Studio build |
| **Config file** | none — WPF project, no test framework |
| **Quick run command** | `dotnet build FinalVision.sln` |
| **Full suite command** | `dotnet build FinalVision.sln && grep -c "FrameWidth" Recipe/Site1/*/main.ini` |
| **Estimated runtime** | ~30 seconds |

---

## Sampling Rate

- **After every task commit:** Run `dotnet build FinalVision.sln`
- **After every plan wave:** Run full suite command
- **Before `/gsd:verify-work`:** Full suite must be green
- **Max feedback latency:** 30 seconds

---

## Per-Task Verification Map

| Task ID | Plan | Wave | Requirement | Test Type | Automated Command | File Exists | Status |
|---------|------|------|-------------|-----------|-------------------|-------------|--------|
| 14-01-01 | 01 | 1 | TBD | build | `dotnet build FinalVision.sln` | ✅ | ⬜ pending |

*Status: ⬜ pending · ✅ green · ❌ red · ⚠️ flaky*

---

## Wave 0 Requirements

*Existing infrastructure covers all phase requirements.*

---

## Manual-Only Verifications

| Behavior | Requirement | Why Manual | Test Instructions |
|----------|-------------|------------|-------------------|
| FrameWidth/FrameHeight not in INI after save | TBD | INI file content check | Save recipe, verify main.ini lacks FrameWidth/FrameHeight keys |
| LightGroupName persists after load | TBD | Runtime behavior | Set LightGroupName in recipe, save, reload, verify value retained |
| Reset restores LightGroupName | TBD | Runtime behavior | Reset recipe parameters, verify LightGroupName restored correctly |

---

## Validation Sign-Off

- [ ] All tasks have `<automated>` verify or Wave 0 dependencies
- [ ] Sampling continuity: no 3 consecutive tasks without automated verify
- [ ] Wave 0 covers all MISSING references
- [ ] No watch-mode flags
- [ ] Feedback latency < 30s
- [ ] `nyquist_compliant: true` set in frontmatter

**Approval:** pending
