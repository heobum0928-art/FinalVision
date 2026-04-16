# Phase 16: ALIVE 상태 UI 인디케이터 - Context

**Gathered:** 2026-04-13
**Status:** Ready for planning (`/gsd-plan-phase 16`)

<domain>
## Phase Boundary

Phase 15에서 구현된 ALIVE 하트비트(`WPF_Example/Custom/SystemHandler.cs` — `AliveProcess`, `_aliveResponseReceived`, `ALIVE_TIMEOUT_MS=3000`, `ALIVE_RETRY_COUNT=3`)의 **연결 상태를 사용자에게 시각적으로 보여주는 WPF UI 인디케이터** 추가.

3-state 표시:
- **회색** — Client 미연결 상태 (Accept 대기)
- **녹색 (깜빡임)** — Client 연결 유지 중, ALIVE 응답 수신할 때마다 1회 flash
- **빨강** — 3회 재시도 모두 무응답으로 타임아웃 (Phase 15 `PerformAliveTimeout()` 발동 시점)

범위 외(OUT):
- ALIVE 프로토콜/재시도 로직 자체 수정 (Phase 15에서 완료)
- 타임아웃 팝업 알람 / 사운드 알림
- 연결 통계·히스토리 로그 UI
- 재시도 카운터(`1/3`, `2/3`) 실시간 노출
- TcpServerWindow 개편 (본 Phase는 MenuBar 인디케이터만 추가)

</domain>

<decisions>
## Implementation Decisions

### 배치 및 비주얼 (D-01 / D-02)
- **D-01 [A2]:** ALIVE 인디케이터는 **MenuBar 우측** `WPF_Example/UI/MenuBar.xaml`의 `Data & Version` Grid(Grid.Column=4, Row 0~3) 영역에 배치한다.
  - Version/DateTime TextBlock과 같은 우측 상단 영역에 추가하여 "시스템 상태 표시" 그룹으로 일관성 확보.
  - StatusBar(A1) 또는 TcpServerWindow(A3)는 채택하지 않음 — MenuBar가 항상 상단 고정이라 운영자가 바로 인지 가능.
- **D-02 [B1]:** 비주얼 형태는 **원형 LED (Ellipse) + 옆에 "ALIVE" 라벨**.
  - WPF `<Ellipse>` 12~16px + TextBlock "ALIVE" 조합.
  - 순수 WPF(`Ellipse`, `SolidColorBrush`, `Storyboard`)만 사용 — v2.0 결정(신규 NuGet 금지) 준수.
  - 아이콘 리소스/이미지 파일 추가 없음.

### 상태 색상 및 깜빡임 (D-03 / D-04)
- **D-03:** 3-state 색상 정의 (정확한 값은 planner 판단):
  - 회색(미연결): 중립 Gray 톤
  - 녹색(연결/수신 대기): 연한 녹색 유지 상태(Base)
  - 녹색 Flash(ALIVE 응답 수신 1회): 진한 녹색 → Base 녹색 fade-out (~100~200ms)
  - 빨강(타임아웃): 진한 Red 솔리드 유지
- **D-04 [C1]:** 녹색 깜빡임은 **ALIVE 응답 수신 시마다 1회 짧은 flash** 방식.
  - 고정 주기(1Hz) 펄스 blink(C2)는 채택하지 않음 — "수신 활동"이 시각적으로 드러나지 않음.
  - 구현 힌트: `Ellipse.Fill` `SolidColorBrush.Color`에 `Storyboard` + `ColorAnimation`으로 flash 트리거. `Storyboard.Begin()`을 ALIVE 응답 수신 이벤트마다 Dispatcher로 invoke.

### 타임아웃 → 복귀 정책 (D-05)
- **D-05 [D1]:** 빨강(타임아웃) 상태에서 **Client 재접속이 감지되는 즉시 녹색(Base)으로 자동 복귀**.
  - 사용자 수동 확인(D2)이나 일정 시간 후 회색 전환(D3)은 채택하지 않음.
  - 복귀 트리거: 기존 VisionServer `OnConnected` 이벤트 경로(Phase 15 `AlarmEventType.OnDisconnected`의 대칭 경로) — planner가 정확한 이벤트 지점 확인 필요.
  - 재접속 시 `_aliveResponseReceived` 플래그/ALIVE 스레드 재기동 상태도 함께 녹색 Base로 리셋.

### Tooltip / Label (D-06)
- **D-06 [E3]:** 툴팁 없음, **원형 LED + "ALIVE" 텍스트 라벨만** 표시.
  - 마지막 수신 시각이나 재시도 카운터(`1/3`)는 노출하지 않음 — 본 Phase 범위 외.
  - 운영자는 "색상 = 연결 상태" 한 가지 정보만 보면 됨.

### 상태 이벤트 브리징 구조 (Claude's Discretion)
- SystemHandler → UI 브리징 방식은 planner 판단:
  - (a) `SystemHandler`에 `ConnectionStateChanged` 이벤트 추가 → MenuBar가 구독 → Dispatcher.Invoke로 `Ellipse.Fill` 변경
  - (b) `INotifyPropertyChanged` ViewModel(`AliveStatusViewModel`) 도입 → DataBinding
  - (c) 기존 `StatusBarModel` 패턴을 따라 `MenuBarModel` 같은 VM 신설
- 어떤 패턴을 쓰든 UI 업데이트는 반드시 `Application.Current.Dispatcher.Invoke`로 수행 (ALIVE 스레드는 UI 스레드가 아님).

### 스레드 안전성 요구사항 (잠금 사항)
- **L-01:** ALIVE 응답 수신 처리는 이미 `SystemHandler.MainRun` 또는 `AliveProcess` 스레드에서 일어남 — UI 업데이트는 절대 해당 스레드에서 직접 수행 금지. 반드시 Dispatcher 경유.
- **L-02:** Phase 15 하트비트 로직(`AliveProcess`, 3회 재시도, `PerformAliveTimeout`) 자체는 수정 금지. UI는 **읽기 전용 관찰자**.
- **L-03:** v2.0 결정 — 신규 NuGet 패키지(예: MaterialDesign, FontAwesome 등) 추가 금지. 순수 WPF만 사용.

</decisions>

<canonical_refs>
## Canonical References

downstream agent(researcher, planner, executor)가 반드시 참고해야 할 문서 및 코드 위치:

- `.planning/ROADMAP.md` — Phase 16 항목 (Success Criteria)
- `.planning/phases/15-v8-alive-dryrun-time-trace/15-CONTEXT.md` — Phase 15 ALIVE 결정사항 (D-02~D-04)
- `.planning/phases/15-v8-alive-dryrun-time-trace/15-02-PLAN.md` — ALIVE 하트비트 구현 PLAN
- `.planning/phases/15-v8-alive-dryrun-time-trace/VERIFICATION.md` — Phase 15 검증 결과
- `WPF_Example/Custom/SystemHandler.cs` — ALIVE 수신/재시도/타임아웃 로직 (21~75, 323~370줄). `_aliveResponseReceived`, `ALIVE_TIMEOUT_MS`, `ALIVE_RETRY_COUNT`, `PerformAliveTimeout()` 참조 지점.
- `WPF_Example/SystemHandler.cs` — `mAliveThread` 생성/종료 (44, 104~108, 162줄)
- `WPF_Example/UI/MenuBar.xaml` — 인디케이터 배치 위치 (Grid.Column=4 "Data & Version" 영역, 229~245줄)
- `WPF_Example/UI/MenuBar.xaml.cs` — code-behind, 이벤트 구독/Dispatcher 진입점
- `WPF_Example/UI/StatusBar.xaml` + `StatusBarModel.cs` — 기존 UserControl + INotifyPropertyChanged 패턴 참고 (MenuBar 쪽 VM 도입 시 동일 스타일 유지)
- `WPF_Example/TcpServer/VisionServer.cs` — `OnConnected` / `OnDisconnected` 이벤트 발생 지점 (D-05 재접속 자동 녹색 복귀 트리거)

</canonical_refs>

<deferred>
## Deferred Ideas (OUT OF SCOPE — 백로그)

- 툴팁 "Last ALIVE: N초 전" / 재시도 카운터(`1/3`, `2/3`) 실시간 노출 (E1, E2)
- 타임아웃 시 팝업 알람 / 사운드 알림
- 연결 이력/통계(로그 뷰어) UI
- TcpServerWindow 내부 개편·재디자인
- StatusBar 측 인디케이터 중복 배치 (A1)
- 수동 확인 리셋 버튼(D2) / 시간 기반 자동 회색 전환(D3)
- 다중 Client 동시 연결 UI 표현 (현재 MAX_CONNECTION_COUNT=1)

</deferred>

<next_steps>
## Next Steps

1. `/gsd-plan-phase 16` 실행 → 본 CONTEXT.md 반영한 PLAN 생성
   - planner는 SystemHandler→UI 이벤트 브리징 방식(위 Claude's Discretion) 중 하나를 확정해야 함
   - MenuBar.xaml `Data & Version` Grid 레이아웃 조정 방법(Row 추가 vs 기존 Row에 StackPanel 배치) 결정
   - Storyboard `ColorAnimation` flash 지속시간(권장 100~200ms) 최종 값 결정
2. PLAN 확인 후 `/gsd-execute-phase 16` 실행
3. 실기 구동으로 3-state 전환 검증 (UAT):
   - 검사 PC 미연결 → 회색
   - Client 연결 + $ALIVE 왕복 → 녹색 flash 반복
   - Client 강제 종료 → 9초(3회×3초) 후 빨강
   - Client 재접속 → 즉시 녹색 복귀

</next_steps>
