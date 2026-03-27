---
phase: 04-5-site
plan: "01"
subsystem: Site
tags: [site-management, singleton, thread-safe, INotifyPropertyChanged]
dependency_graph:
  requires: []
  provides: [SiteManager, SiteContext, SiteStatistics]
  affects: [SequenceHandler, Plan-04-03]
tech_stack:
  added: []
  patterns: [Singleton, Lock-based thread safety, INotifyPropertyChanged, Queue<T> FIFO history]
key_files:
  created:
    - WPF_Example/Custom/Site/SiteStatistics.cs
    - WPF_Example/Custom/Site/SiteContext.cs
    - WPF_Example/Custom/Site/SiteManager.cs
  modified:
    - WPF_Example/FinalVision.csproj
    - WPF_Example/Custom/SystemSetting.cs
decisions:
  - "SiteManager.Handle[siteIndex] uses 0-based index; SwitchSite(siteNumber) uses 1-based number"
  - "SiteStatistics lock(_lock) guards Add/Reset internally; RaisePropertyChanged called outside lock to avoid deadlock"
  - "SiteContext._resultHistory is not thread-safe by design (UI-thread-only access in Phase 4 scope)"
metrics:
  duration: "~3 min"
  completed: "2026-03-26"
  tasks_completed: 3
  files_created: 3
  files_modified: 2
---

# Phase 04 Plan 01: SiteManager / SiteContext / SiteStatistics 생성 Summary

**One-liner:** lock(_lock) 스레드 안전 통계 + Queue<bool> 이력을 갖는 5개 SiteContext 배열을 SiteManager 싱글톤으로 관리하는 Site 데이터 계층 구축

## What Was Built

`WPF_Example/Custom/Site/` 하위에 3개 C# 파일을 신규 생성하여 5개 Site의 레시피명·결과 이력·통계를 메모리에서 독립 관리하는 데이터 계층을 구현했다.

### SiteStatistics.cs
- `INotifyPropertyChanged` 구현으로 WPF 데이터 바인딩 지원
- `_lock` 객체로 `Add()` / `Reset()` 카운터 변경을 원자적 보호 (스레드 안전)
- `RaisePropertyChanged`는 lock 외부에서 호출 (데드락 방지)
- `Yield`: `TotalCount == 0` 시 `0.0` 반환; OK/Total × 100.0 (퍼센트)

### SiteContext.cs
- Site 1~5를 1-based `SiteNumber`로 구분, `SiteName` = "Site1"~"Site5"
- `CurrentRecipeName`: 기본값 "Default", 변경 시 PropertyChanged 알림
- `Statistics`: `new SiteStatistics()` 소유
- `Queue<bool> _resultHistory`: MAX_HISTORY(100) 초과 시 Dequeue → FIFO 이력

### SiteManager.cs
- 싱글톤: `public static SiteManager Handle { get; } = new SiteManager()`
- `SITE_COUNT = 5`, `_sites` 배열(0-based), 생성자에서 `SiteContext(1~5)` 초기화
- `SwitchSite(siteNumber)`: 1-based 입력, 범위(1~5) 외 `false` 반환
- `this[siteIndex]`: 0-based 인덱서

## Tasks Completed

| Task | Name | Files | Status |
|------|------|-------|--------|
| 1 | SiteStatistics.cs 생성 | Custom/Site/SiteStatistics.cs | Done |
| 2 | SiteContext.cs 생성 | Custom/Site/SiteContext.cs | Done |
| 3 | SiteManager.cs 생성 | Custom/Site/SiteManager.cs | Done |

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 3 - Blocking] FinalVision.csproj에 Compile 항목 추가**
- **Found during:** Task 1 (첫 빌드 시 CS2001 오류)
- **Issue:** .csproj가 wildcard가 아닌 explicit `<Compile Include>` 방식이므로 신규 파일이 빌드 목록에 없으면 컴파일 제외됨
- **Fix:** `Custom\SystemSetting.cs` 항목 바로 아래에 3개 Site 파일 `<Compile>` 항목 추가
- **Files modified:** WPF_Example/FinalVision.csproj

**2. [Rule 1 - Bug] Custom/SystemSetting.cs Category 모호 참조 수정**
- **Found during:** Task 1 빌드 (CS0104 에러)
- **Issue:** `using System.ComponentModel` + `using PropertyTools.DataAnnotations` 동시 임포트 상태에서 `[Category("Inspection|Site")]` 어트리뷰트가 두 어셈블리에서 모두 정의되어 모호한 참조 에러 발생
- **Fix:** `[Category(...)]` → `[PropertyTools.DataAnnotations.Category(...)]` 완전 한정명으로 수정
- **Files modified:** WPF_Example/Custom/SystemSetting.cs
- **Note:** 이 에러는 Custom/SystemSetting.cs가 이전 Phase에서 작성된 파일이며, 해당 파일의 빌드 에러가 이번 변경 전에 이미 존재했음. Site 파일 추가 시 해당 프로젝트를 빌드하면서 함께 발견됨.

## Decisions Made

- **SiteManager indexer는 0-based, SwitchSite는 1-based:** Plan 03에서 `SiteManager.Handle[siteNumber - 1]` 패턴으로 접근하는 점을 고려하여 Plan 명세대로 유지
- **SiteStatistics lock 범위:** Add/Reset 내부의 카운터 변경만 lock 보호, `RaisePropertyChanged`는 lock 외부 호출 (이벤트 핸들러에서 재진입 시 데드락 방지)
- **SiteContext._resultHistory 스레드 안전:** Phase 4 범위에서 AddResult는 UI 스레드 직접 호출이므로 Queue 자체는 lock 불필요 (SiteStatistics만 멀티스레드 접근 대상)

## Known Stubs

없음 — 3개 파일 모두 완전 구현됨.

## Build Verification

```
FinalVision -> D:\Project\FinalVision\WPF_Example\bin\x64\Debug\FinalVision.exe
```

- 에러: 0건
- 경고: 기존 pre-existing 경고만 (MSB3270 아키텍처 불일치, CS7035 버전 형식 — 이번 변경과 무관)

## Self-Check: PASSED

- [x] WPF_Example/Custom/Site/SiteStatistics.cs — FOUND
- [x] WPF_Example/Custom/Site/SiteContext.cs — FOUND
- [x] WPF_Example/Custom/Site/SiteManager.cs — FOUND
- [x] namespace FinalVisionProject.Site — 3개 파일 모두 포함
- [x] SiteManager.SITE_COUNT == 5 — 포함
- [x] SiteContext.MAX_HISTORY == 100 — 포함
- [x] SiteStatistics._lock 객체 — 포함
- [x] Build succeeded (exit code 0, error 0) — 확인
