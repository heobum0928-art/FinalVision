---
phase: 4
slug: 5-site
status: draft
nyquist_compliant: false
wave_0_complete: false
created: 2026-03-26
---

# Phase 4 — Validation Strategy

> Per-phase validation contract for feedback sampling during execution.

---

## Test Infrastructure

| Property | Value |
|----------|-------|
| **Framework** | Manual / compile-time (C# WinForms — no automated test runner configured) |
| **Config file** | none |
| **Quick run command** | `msbuild FinalVision.sln /p:Configuration=Debug /t:Build` |
| **Full suite command** | `msbuild FinalVision.sln /p:Configuration=Debug /t:Build` |
| **Estimated runtime** | ~10 seconds |

---

## Sampling Rate

- **After every task commit:** Run `msbuild FinalVision.sln /p:Configuration=Debug /t:Build`
- **After every plan wave:** Run `msbuild FinalVision.sln /p:Configuration=Debug /t:Build`
- **Before `/gsd:verify-work`:** Full build must be green + manual verification steps completed
- **Max feedback latency:** ~10 seconds

---

## Per-Task Verification Map

| Task ID | Plan | Wave | Requirement | Test Type | Automated Command | File Exists | Status |
|---------|------|------|-------------|-----------|-------------------|-------------|--------|
| 4-01-01 | 01 | 1 | SiteManager class | compile | `msbuild ... /t:Build` | ❌ W0 | ⬜ pending |
| 4-01-02 | 01 | 1 | SiteContext class | compile | `msbuild ... /t:Build` | ❌ W0 | ⬜ pending |
| 4-01-03 | 01 | 1 | SiteStatistics class | compile | `msbuild ... /t:Build` | ❌ W0 | ⬜ pending |
| 4-02-01 | 02 | 2 | Recipe path structure | manual | verify folder layout | ✅ | ⬜ pending |
| 4-02-02 | 02 | 2 | RecipeFiles.CollectRecipe | compile+manual | build + UI test | ✅ | ⬜ pending |
| 4-03-01 | 03 | 3 | Site switch auto-load | manual | UI walkthrough | ✅ | ⬜ pending |
| 4-03-02 | 03 | 3 | Result history (최근 N건) | compile+manual | build + UI check | ✅ | ⬜ pending |

*Status: ⬜ pending · ✅ green · ❌ red · ⚠️ flaky*

---

## Wave 0 Requirements

- [ ] `Custom/Site/SiteManager.cs` — new file (SiteManager class stub)
- [ ] `Custom/Site/SiteContext.cs` — new file (SiteContext class stub)
- [ ] `Custom/Site/SiteStatistics.cs` — new file (SiteStatistics class stub)
- [ ] `Recipe/Site1/` through `Recipe/Site5/` — directory structure created

---

## Manual-Only Verifications

| Behavior | Requirement | Why Manual | Test Instructions |
|----------|-------------|------------|-------------------|
| Site 전환 시 레시피 자동 로드 | SiteManager auto-load | Requires running app + UI interaction | Switch site in UI, verify correct recipe loads without manual refresh |
| 결과 이력 최근 N건 표시 | SiteContext history queue | Requires runtime inspection | Run 10+ inspections, verify only last N results shown per site |
| 5개 Site 독립 통계 | SiteStatistics per-site | Requires multi-site runtime | Run inspections across multiple sites, verify stats don't bleed across sites |
| Recipe 폴더 구조 마이그레이션 | RecipeFiles path change | Filesystem layout | Verify `Recipe/Site1/Seoul_LED_MIL/main.ini` exists and loads correctly |

---

## Validation Sign-Off

- [ ] All tasks have `<automated>` verify or Wave 0 dependencies
- [ ] Sampling continuity: no 3 consecutive tasks without automated verify
- [ ] Wave 0 covers all MISSING references
- [ ] No watch-mode flags
- [ ] Feedback latency < 10s
- [ ] `nyquist_compliant: true` set in frontmatter

**Approval:** pending
