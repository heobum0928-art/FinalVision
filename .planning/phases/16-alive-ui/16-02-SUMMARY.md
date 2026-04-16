---
phase: 16-alive-ui
plan: 02
status: complete
date: 2026-04-13
subsystem: WPF UI / MenuBar
tags: [alive, indicator, storyboard, dispatcher, wpf]
dependency_graph:
  requires: [16-01]
  provides: [MenuBar ALIVE 3-state LED indicator]
  affects: [WPF_Example/UI/MenuBar.xaml, WPF_Example/UI/MenuBar.xaml.cs]
tech_stack:
  added: [System.Windows.Media.Animation (Storyboard/ColorAnimation)]
  patterns: [Dispatcher.BeginInvoke, event subscribe/unsubscribe, volatile bool latch, WPF Storyboard]
key_files:
  created: []
  modified:
    - WPF_Example/UI/MenuBar.xaml
    - WPF_Example/UI/MenuBar.xaml.cs
decisions:
  - AlarmEventArgs namespace은 FinalVisionProject.Network — using 추가 필요 (Rule 1 auto-fix)
  - dotnet build CLI XAML codegen 에러 411건은 pre-existing (16-01 SUMMARY 동일) — Visual Studio에서만 정상 빌드
metrics:
  duration: ~25min
  completed_date: 2026-04-13
  tasks_completed: 2
  files_modified: 2
---

# Phase 16 Plan 02: ALIVE UI 인디케이터 (MenuBar frontend) Summary

## One-liner

WPF MenuBar에 Ellipse LED(14px) + "ALIVE" TextBlock + 150ms ColorAnimation Storyboard를 추가하고, 3-state(Gray/BaseGreen/Red) 폴링 + 이벤트 기반 flash/latch 로직을 code-behind에 구현.

## Tasks Completed

| Task | Name | Commit | Files |
|------|------|--------|-------|
| 1 | MenuBar.xaml Row4 ALIVE 인디케이터 + AliveFlashStoryboard | fcd7c10 | WPF_Example/UI/MenuBar.xaml |
| 2 | MenuBar.xaml.cs 이벤트 구독 + 3-state 폴링 + flash/latch | 17707ef | WPF_Example/UI/MenuBar.xaml.cs |
| 3 | 실기 UAT — 육안 검증 | (checkpoint:human-verify) | — |

## Changes

### Task 1: MenuBar.xaml

- `UserControl.Resources`에 `AliveFlashStoryboard` Storyboard 리소스 추가
  - `ColorAnimation`: `alive_Ellipse` 대상, `#FF00B050` → `#FF7EE08B`, `Duration=0:0:0.15`
- `Data & Version` Grid (Grid.Column=4) 에 Row 4 (`Height=2*`) 신규 추가
- Row 4에 `StackPanel` → `Ellipse x:Name="alive_Ellipse"` (14px, Fill=#FF9E9E9E) + `TextBlock Text="ALIVE"` 배치
- 기존 Row 0~3 및 자식 요소(label_DateTime, 구분선, label_Version, label_DLLVersion) 일체 수정 없음

### Task 2: MenuBar.xaml.cs

- `using System.Windows.Media.Animation` + `using FinalVisionProject.Network` 추가
- 필드 추가: `_aliveBrush` (SolidColorBrush 캐시), `_flashStoryboard` (Storyboard 캐시), `_aliveTimeoutLatched` (volatile bool), `AliveGray/AliveBaseGreen/AliveRed` Color 상수
- `MenuBar_Loaded` 확장: 초기화 + `AliveHeartbeatReceived`/`AliveTimeout`/`Server.OnAlarm` 구독 + `Unloaded` 훅
- `UpdateState()` 확장: 3-state 폴링 — null 가드 → 래치 분기(Red) → `IsConnected()` 분기(BaseGreen/Gray)
- `OnAliveHeartbeat()`: `Dispatcher.BeginInvoke` + `_aliveTimeoutLatched` guard + `_flashStoryboard.Begin(this, true)`
- `OnAliveTimeoutEvent()`: `Dispatcher.BeginInvoke` + `_aliveTimeoutLatched=true` + `Stop(this)` + `AliveRed`
- `OnServerAlarm()`: `OnConnected` 분기 시 `Dispatcher.BeginInvoke` + `_aliveTimeoutLatched=false`
- `MenuBar_Unloaded()`: 이벤트 구독 해제 (메모리 누수 방지)

## Acceptance Criteria Results

### Task 1 (XAML)

| 기준 | 결과 |
|------|------|
| `x:Name="alive_Ellipse"` 1건 | PASS |
| `x:Key="AliveFlashStoryboard"` 1건 | PASS |
| `From="#FF00B050"` 1건 | PASS |
| `To="#FF7EE08B"` 1건 | PASS |
| `Duration="0:0:0.15"` 1건 | PASS |
| `#FF9E9E9E` 1건 | PASS |
| `Text="ALIVE"` 1건 | PASS |
| `Grid.Row="4"` 최소 1건 | PASS |
| 기존 label_DateTime/label_Version/label_DLLVersion 유지 | PASS |

### Task 2 (code-behind)

| 기준 | 결과 |
|------|------|
| `AliveHeartbeatReceived += OnAliveHeartbeat` 1건 | PASS |
| `AliveHeartbeatReceived -= OnAliveHeartbeat` 1건 | PASS |
| `AliveTimeout += OnAliveTimeoutEvent` 1건 | PASS |
| `AliveTimeout -= OnAliveTimeoutEvent` 1건 | PASS |
| `Server.OnAlarm += OnServerAlarm` 1건 | PASS |
| `_flashStoryboard.Begin(this, true)` 1건 | PASS |
| `_flashStoryboard.Stop(this)` 1건 | PASS |
| `Dispatcher.BeginInvoke` 최소 3건 | PASS (3건) |
| `_aliveTimeoutLatched = true` 1건 | PASS |
| `_aliveTimeoutLatched = false` 1건 | PASS |
| `IsConnected()` 1건 | PASS |
| `0x9E, 0x9E, 0x9E` (AliveGray) 1건 | PASS |
| `0x7E, 0xE0, 0x8B` (AliveBaseGreen) 1건 | PASS |
| `0xE5, 0x39, 0x35` (AliveRed) 1건 | PASS |
| L-01: 신규 동기 Dispatcher.Invoke 0건 | PASS |
| L-02: SystemHandler 내부 직접 접근 0건 | PASS |
| L-03: FinalVision.csproj 변경 0건 (이 플랜 기여분) | PASS |

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 1 - Bug] AlarmEventArgs 네임스페이스 using 누락**
- **Found during:** Task 2 빌드 검증
- **Issue:** `AlarmEventArgs`가 `FinalVisionProject.Network` 네임스페이스에 있으나 MenuBar.xaml.cs에 해당 using 없음 → CS0246 컴파일 에러
- **Fix:** `using FinalVisionProject.Network;` 추가 (//260413 hbk 주석 포함)
- **Files modified:** WPF_Example/UI/MenuBar.xaml.cs
- **Commit:** 17707ef (Task 2 커밋에 포함)

## Known Stubs

없음. ALIVE 인디케이터는 16-01에서 연결된 실 이벤트 스트림을 구독하며, 하드코딩된 더미 데이터 없음.

## Threat Flags

없음. 신규 네트워크 엔드포인트, 인증 경로, 파일 접근 패턴 추가 없음.

## Self-Check: PASS

Task 3 human-verify 완료 (2026-04-16). ALIVE 인디케이터 실기 육안 검증 승인.
