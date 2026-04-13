# Phase 16: ALIVE 상태 UI 인디케이터 - Research

**Researched:** 2026-04-13
**Domain:** WPF 3-state LED 인디케이터 (MenuBar) + SystemHandler 상태 브리징
**Confidence:** HIGH (모든 코드 라인 실제 확인 완료)

## Summary

Phase 15에서 구현된 ALIVE 하트비트(`WPF_Example/Custom/SystemHandler.cs` 15~379줄)는 이미 세 가지 상태 신호를 내부에서 발생시키고 있다: (1) `_aliveResponseReceived = true` 설정 지점(MainRun 75줄) — "응답 수신", (2) `PerformAliveTimeout()` 진입(351줄) — "타임아웃", (3) `Server.IsConnected()` 의 true/false 전이 — "연결/미연결". Phase 16은 이 세 상태를 MenuBar의 신규 Ellipse UI에 포워딩만 한다. 기존 Phase 15 하트비트 로직은 수정하지 않는다(L-02).

Phase 15에서 `VisionServer`는 이미 `AlarmEventType.OnConnected` / `OnDisconnected` 이벤트를 `OnAlarm` 이벤트로 발행하고 있다(`TcpServer.cs` 31~34, 443, 207줄) — "OnConnected 이벤트 경로가 없다"는 CONTEXT 가정은 **이미 존재**. 별도로 만들 필요 없음. 단, `SystemHandler`는 현재 이 이벤트를 구독하지 않으며 `Logging.PrintLog` 외에는 아무 처리가 없다(`VisionServer.cs` 25~29 `PerformOnAlarm`). 따라서 Phase 16에서 구독 지점을 **새로 연결**해야 한다.

**Primary recommendation:** 기존 `MainWindow.TimerTick`(100ms DispatcherTimer)이 이미 `menuBar.UpdateState()`를 호출하는 폴링 구조를 쓰고 있다 — 복잡한 이벤트/ViewModel 배관을 추가하지 말고 **"폴링 + flash 이벤트"의 하이브리드**를 쓴다: (a) Connected/Timeout/Connected-Base 3-state는 100ms 폴링에서 `SystemHandler`의 공개 상태 플래그를 읽어 Ellipse.Fill 색상 적용, (b) "녹색 flash"만은 push 방식 — `SystemHandler`에 `event Action OnAliveResponseReceived` 추가하고 MainRun 75줄에서 `Invoke` → `MenuBar.xaml.cs`가 구독 → `Dispatcher.BeginInvoke`로 Storyboard.Begin(). 이 방식이 기존 StatusBar/MenuBar 패턴(폴링 기반 `UpdateState()`)과 가장 일관된다.

## User Constraints (from CONTEXT.md)

### Locked Decisions
- **D-01 [A2]:** ALIVE 인디케이터는 `WPF_Example/UI/MenuBar.xaml` 의 `Data & Version` Grid(Grid.Column=4, Row 0~3) 영역에 배치. StatusBar/TcpServerWindow 채택 안 함.
- **D-02 [B1]:** 원형 Ellipse 12~16px + "ALIVE" TextBlock 조합. 순수 WPF만 사용.
- **D-03:** 3-state 색상 — 회색(미연결) / 녹색 Base(연결 유지) / 진한녹색→Base 녹색 fade(응답 수신 flash, 100~200ms) / 빨강(타임아웃).
- **D-04 [C1]:** 고정 주기 blink(C2) 아님 — **응답 수신 시마다 1회 짧은 flash**. Storyboard + ColorAnimation, Storyboard.Begin() 재진입.
- **D-05 [D1]:** 빨강 상태에서 Client 재접속 감지 즉시 녹색 Base로 자동 복귀. 수동 확인 리셋 버튼/시간 기반 회색 전환 채택 안 함.
- **D-06 [E3]:** Tooltip 없음 — 원형 LED + "ALIVE" 텍스트만. 마지막 수신 시각/재시도 카운터 노출 안 함.

### Claude's Discretion
- SystemHandler → UI 브리징 방식 (이벤트 vs INotifyPropertyChanged VM vs MenuBarModel 신설) — 아래 **Architecture Patterns** 에서 권장안 확정.
- MenuBar `Data & Version` Grid 레이아웃 — Row 추가 vs 기존 Row 에 StackPanel 배치 — 아래에서 권장안 확정.
- Storyboard `ColorAnimation` flash 지속시간(권장 100~200ms) 최종 값.

### Deferred Ideas (OUT OF SCOPE)
- Tooltip "Last ALIVE: N초 전" / 재시도 카운터(`1/3`) 실시간 노출 (E1, E2)
- 타임아웃 시 팝업 알람 / 사운드 알림
- 연결 이력/통계(로그 뷰어) UI
- TcpServerWindow 내부 개편
- StatusBar 측 중복 배치 (A1)
- 수동 확인 리셋 버튼(D2) / 시간 기반 자동 회색 전환(D3)
- 다중 Client 동시 연결 UI 표현 (MAX_CONNECTION_COUNT=1)

## Project Constraints (from memory)

- **주석 규약:** 모든 신규/수정 라인 끝에 `//260413 hbk` 형식 주석 필수 (오늘 날짜 YYMMDD 2자리).
- **FAI/측정 코드 금지:** 본 Phase는 UI만 건드리므로 해당 없음.
- **WORKLOG.md:** 작업 종료 시 일일 기록 (Phase별 실행 흐름은 `/gsd-execute` 체계가 관리).
- **v2.0 NuGet 금지:** MaterialDesign/FontAwesome 등 신규 패키지 절대 추가 금지 — 순수 WPF(`Ellipse`, `SolidColorBrush`, `Storyboard`, `ColorAnimation`)만 사용.

## Phase Requirements

Phase 16 은 ROADMAP.md 에 상세 REQ-ID 가 없고 "ALIVE 상태 UI 인디케이터" 서술만 있다. CONTEXT.md 의 3-state 정의(D-03~D-05)를 플래너가 REQ 로 변환해야 한다. 권장 임시 ID:

| ID | Description | Research Support |
|----|-------------|------------------|
| ALIVE-UI-01 | Client 미연결 상태에서 MenuBar 인디케이터는 회색이다 | `Server.IsConnected()` = false (폴링), `AlarmEventType.OnDisconnected` (이벤트) |
| ALIVE-UI-02 | Client 연결 + ALIVE 응답 수신 시마다 녹색 flash 1회 (100~200ms fade) | `_aliveResponseReceived = true` at `SystemHandler.cs:75` |
| ALIVE-UI-03 | 3회 재시도 모두 무응답으로 `PerformAliveTimeout()` 진입 시 빨강 유지 | `SystemHandler.cs:351` |
| ALIVE-UI-04 | 빨강 상태에서 Client 재접속 즉시 녹색 Base 로 복귀 | `TcpServer.cs:443` `AlarmEventType.OnConnected` raise + `Server.IsConnected()` 폴링 |
| ALIVE-UI-05 | UI 스레드 외에서 Ellipse 색 변경 금지 — `Dispatcher` 경유 | L-01 잠금 |
| ALIVE-UI-06 | Phase 15 `AliveProcess`/재시도/`PerformAliveTimeout` 로직 미수정 | L-02 잠금 (읽기 전용 관찰자) |

## Standard Stack

### Core (모두 기존 .NET Framework / WPF 표준 — 신규 의존성 없음)

| Library | Version | Purpose | Why Standard |
|---------|---------|---------|--------------|
| `System.Windows.Shapes.Ellipse` | WPF (project TFM) | 원형 LED 도형 | D-02 순수 WPF 원칙 |
| `System.Windows.Media.SolidColorBrush` | WPF | Ellipse.Fill 색상 | ColorAnimation 대상 |
| `System.Windows.Media.Animation.Storyboard` | WPF | flash 트리거 컨테이너 | `Begin()` 재진입 지원 |
| `System.Windows.Media.Animation.ColorAnimation` | WPF | 진한녹색→Base 녹색 fade | `Duration` / `From` / `To` / `AutoReverse` |
| `System.Windows.Threading.Dispatcher` | WPF | ALIVE 스레드→UI 스레드 포워딩 | L-01 잠금 요구 |

신규 NuGet 0건. v2.0 금지 규정 준수.

### Alternatives Considered
| Instead of | Could Use | Tradeoff |
|------------|-----------|----------|
| Ellipse + 코드 기반 Storyboard | XAML VisualStateManager | 3-state 는 VSM 으로도 가능하지만 "수신할 때마다 1회 flash"는 VSM 이 아닌 명시적 `Storyboard.Begin()` 호출이 자연스럽다 |
| 이벤트 기반 브리징 | `INotifyPropertyChanged` VM | StatusBarModel 은 VM 패턴이지만 **주기적 값 갱신**(CPU/RAM) 용도. Flash 같은 1회성 트리거는 VM 에 넣기 어색함(bool 플립 후 애니메이션 기동이 복잡해짐) |
| 폴링(100ms) | 순수 이벤트 | 기존 `MainWindow.TimerTick` 100ms 폴링이 이미 `menuBar.UpdateState()` 호출 중 — 동일 경로 재사용이 가장 일관됨 |

**Installation:** 없음 (기존 `WPF_Example/FinalVision.csproj` 참조만 사용).

## Architecture Patterns

### 기존 패턴 관찰

1. **MenuBar 상태 갱신은 폴링 기반** (`MainWindow.xaml.cs:117`):
   ```
   DispatcherTimer 100ms → menuBar.UpdateState() (MenuBar.xaml.cs:53)
     → label_DateTime.Text / label_status.Content / label_seqName.Content 직접 갱신
   ```
   — INotifyPropertyChanged 쓰지 않음, 단순 code-behind 필드 접근.

2. **StatusBarModel** 은 `INotifyPropertyChanged` 를 쓰지만 `CpuUsage`/`RamUsage` 같은 **주기적 수치** 에만 적용되고, `UpdateResourceInfo()` 도 결국 폴링 호출(`MainWindow.xaml.cs:119`).

3. **TcpServer 이벤트 발행**: `OnAlarm?.Invoke(...)` 으로 `AlarmEventType.OnConnected`/`OnDisconnected`/`OnAcceptFail`/`OnSendFail`/`OnRecvMessageParsingFail` 등 모두 한 이벤트로 발행. `VisionServer.PerformOnAlarm` 은 현재 **로그 출력만** 하고 있음(`VisionServer.cs:25~29`).

### Recommended Pattern (브리징 방식 확정)

**하이브리드: 폴링 (3-state) + 이벤트 (flash)**

| 상태 | 트리거 원천 | 전달 경로 | UI 동작 |
|------|-------------|-----------|---------|
| 회색 (미연결) | `Server.IsConnected()` == false | 폴링 (`MenuBar.UpdateState()`) | `_ellipseBrush.Color = Gray` |
| 녹색 Base (연결 유지) | `Server.IsConnected()` == true && `!_aliveTimeoutLatched` | 폴링 | `_ellipseBrush.Color = BaseGreen` |
| 녹색 Flash (응답 수신) | `_aliveResponseReceived = true` at `SystemHandler.cs:75` | **이벤트** (신규 `Action OnAliveHeartbeatReceived`) | `_flashStoryboard.Begin()` (재진입) |
| 빨강 (타임아웃) | `PerformAliveTimeout()` 진입 at `SystemHandler.cs:351` | **이벤트** (신규 `Action OnAliveTimeout`) + `_aliveTimeoutLatched=true` 래치 | `_ellipseBrush.Color = Red` |
| 복귀 | `AlarmEventType.OnConnected` 수신 | **이벤트** (기존 `Server.OnAlarm` 구독) → `_aliveTimeoutLatched=false` | 폴링이 다음 Tick 에 녹색 Base 로 되돌림 |

**왜 이 패턴인가:**
- CONTEXT.md Claude's Discretion 3가지 옵션 중 **(a) C# event** 를 선택. 이유: StatusBarModel 패턴(VM)은 주기 값 갱신용이며, flash 같은 이산(discrete) 트리거는 event 가 자연스럽다.
- 기존 `menuBar.UpdateState()` 폴링 훅에 3-state 확정을 얹으면 **추가 타이머 없이** 재접속 감지가 자동으로 된다(D-05).
- `AlarmEventType.OnConnected` 이벤트는 이미 존재하므로 구독만 추가 — Phase 15 코드 수정 아님(L-02 위반 아님).

### Implementation Sketch

**`SystemHandler` (partial `WPF_Example/Custom/SystemHandler.cs`) 추가할 것 (Phase 15 로직 수정 없음 — 이벤트 발행만 추가):**

```csharp
// 260413 hbk — Phase 16: ALIVE 상태 UI 브리징 이벤트
public event Action AliveHeartbeatReceived;  //260413 hbk
public event Action AliveTimeout;            //260413 hbk

// MainRun 75줄 바로 다음에 한 줄 추가:
case VisionRequestType.Alive:
    _aliveResponseReceived = true;
    AliveHeartbeatReceived?.Invoke();  //260413 hbk — UI flash 트리거
    responsePacket = ProcessAlive(packet.AsAlive());
    break;

// PerformAliveTimeout() 최하단에 한 줄 추가 (351줄 직후):
private void PerformAliveTimeout() {
    Logging.PrintLog(...);
    if (Server.GetConnectedClientCount() > 0) { ... }
    AliveTimeout?.Invoke();  //260413 hbk — UI 빨강 트리거
}
```

또한 `AliveProcess` 하트비트 스레드 내부에서 V→Client 송신 후 받는 응답은 Client→V 요청이 아닌 **Echo Response**로 처리될 수 있다 — 위 MainRun case 는 이미 `_aliveResponseReceived = true` 를 세팅하므로 event 도 여기서 함께 발행하면 **송신 하트비트 응답**도 flash 된다 (정상 동작).

> 주의: `AliveProcess` 326줄 `_aliveResponseReceived = false` 재설정은 **매 송신 시도 전** 이므로 MainRun 쪽 event 발행과 경합하지 않는다 (UI event 는 플래그 와 무관하게 즉시 발행).

**`MenuBar.xaml.cs`** 에서 이벤트 구독 + 폴링 확장:

```csharp
private SolidColorBrush _aliveBrush;
private Storyboard _flashStoryboard;
private bool _aliveTimeoutLatched;  //260413 hbk — 재접속 시 폴링에서 clear

private void MenuBar_Loaded(...) {
    mParentWindow = (MainWindow)Window.GetWindow(this);
    UpdateLoginID(...);

    //260413 hbk — ALIVE 인디케이터 초기화
    _aliveBrush = (SolidColorBrush)alive_Ellipse.Fill;  // x:Name
    _flashStoryboard = (Storyboard)this.Resources["AliveFlashStoryboard"];

    SystemHandler.Handle.AliveHeartbeatReceived += OnAliveHeartbeat;
    SystemHandler.Handle.AliveTimeout += OnAliveTimeoutEvent;
    SystemHandler.Handle.Server.OnAlarm += OnServerAlarm;  //260413 hbk — 재접속 감지
}

private void OnAliveHeartbeat() {
    Dispatcher.BeginInvoke(new Action(() => {
        if (_aliveTimeoutLatched) return;  // 빨강 고정 중엔 flash 안 함
        _flashStoryboard.Begin(this, true);  // isControllable=true → 재진입 안전
    }));
}

private void OnAliveTimeoutEvent() {
    Dispatcher.BeginInvoke(new Action(() => {
        _aliveTimeoutLatched = true;
        _flashStoryboard.Stop(this);
        _aliveBrush.Color = Colors.Red;
    }));
}

private void OnServerAlarm(object s, AlarmEventArgs e) {
    if (e.AlarmType == AlarmEventArgs.AlarmEventType.OnConnected) {
        Dispatcher.BeginInvoke(new Action(() => {
            _aliveTimeoutLatched = false;  // 다음 UpdateState() 폴링에서 녹색 Base 복귀
        }));
    }
}

// 기존 UpdateState() 확장:
public void UpdateState() {
    this.label_DateTime.Text = DateTime.Now.ToString();
    label_status.Content = SystemHandler.Handle.Sequences.StateAll;
    label_seqName.Content = SystemHandler.Handle.Sequences.StateSequenceName;

    //260413 hbk — ALIVE 3-state 폴링
    if (_aliveTimeoutLatched) return;  // 빨강 유지
    var connected = SystemHandler.Handle.Server?.IsConnected() ?? false;
    _aliveBrush.Color = connected ? BaseGreen : Colors.Gray;
}
```

### XAML 레이아웃 결정 (D-01 정밀화)

현재 `Data & Version` Grid (`MenuBar.xaml:230~245`) 구조:

```
Grid.Column=4 Grid.Row=0
  RowDefinitions: [3*, 1(선), 2*, 2*]
    Row 0: label_DateTime (TextBlock, 날짜)
    Row 1: 구분선 (1px)
    Row 2: label_Version (Platform Ver)
    Row 3: label_DLLVersion (DLL Ver)
```

**권장: 기존 4 Row 를 건드리지 말고 `RowDefinitions` 에 Row 4(신규) 를 추가** — Platform/DLL 버전 라벨 아래에 StackPanel(Horizontal) 로 `Ellipse` + `TextBlock "ALIVE"` 배치. 상단 DateTime/버전 라벨의 정렬은 그대로 유지.

**대안:** Row 0 의 DateTime 좌측에 StackPanel 로 병합 — 단 Row 0 은 `TextBlock Grid.Column="0"` 단일 Column 이고 `HorizontalAlignment="Right"` 라 StackPanel 로 감싸야 한다. 복잡도가 더 높아 **신규 Row 4 권장**.

**Storyboard 리소스는 `UserControl.Resources` 에 추가:**

```xml
<Storyboard x:Key="AliveFlashStoryboard">
    <ColorAnimation Storyboard.TargetName="alive_Ellipse"
                    Storyboard.TargetProperty="(Shape.Fill).(SolidColorBrush.Color)"
                    From="#FF00B050" To="#FF7EE08B" Duration="0:0:0.15" />
</Storyboard>
```

- `From` = 진한 녹색(수신 순간), `To` = Base 녹색(0.15초 후 복귀).
- `AutoReverse="False"` (기본). `FillBehavior="HoldEnd"` 는 **쓰지 않는다** — 다음 Tick 에 `_aliveBrush.Color = BaseGreen` 폴링이 덮어쓰므로 불필요.
- `Duration` 150ms 권장(100~200ms 범위 중 가독성 우수한 중앙값).

### 재진입 안전성 (Storyboard.Begin)

- `_flashStoryboard.Begin(this, true)` 의 세 번째 인자(`isControllable=true`)는 반드시 필요 — `Stop()` 을 명시적으로 호출하려면 controllable 이어야 함(타임아웃 → 빨강 전환 시).
- ALIVE 송신은 1초 주기이고 flash 는 150ms 이므로 자연스럽게 완료 후 다음 flash 가 시작된다(재진입 경쟁 거의 없음). 하지만 `Begin()` 은 이미 진행 중인 Storyboard 를 **처음부터 재시작**하므로 안전하다 (WPF 공식 동작).
- 반드시 `Dispatcher.BeginInvoke` (비동기) 사용 — `Invoke` (동기) 는 ALIVE 스레드를 UI 스레드와 동기화 대기시켜 Phase 15 의 1초 주기에 영향을 줄 수 있음.

## Don't Hand-Roll

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| 주기적 깜빡임 | `DispatcherTimer` 로 수동 색 토글 | `Storyboard + ColorAnimation` | fade 보간, 재진입 안전, GC 부담 적음 |
| 연결 상태 폴링 | 신규 타이머 | 기존 `MainWindow.TimerTick` 100ms | 이미 돌고 있는 훅 재사용 |
| 상태 프로퍼티 변경 통지 | 수동 event + lock | `INotifyPropertyChanged`… 라고 말하고 싶지만 **본 Phase는 VM 불필요** | 기존 MenuBar 가 VM 미사용, 일관성 위해 폴링 + event 하이브리드 채택 |
| Dispatcher 경유 | 수동 SynchronizationContext.Post | `Dispatcher.BeginInvoke` | WPF 표준, ALIVE 스레드에서 안전 |

## Common Pitfalls

### Pitfall 1: `Storyboard.Begin()` 을 ALIVE 스레드에서 직접 호출
**증상:** `InvalidOperationException: The calling thread cannot access this object because a different thread owns it.`
**원인:** `AliveProcess` / `SystemProcess` 는 UI 스레드 아님.
**방지:** event 핸들러 내부를 `Dispatcher.BeginInvoke` 로 감싸기.

### Pitfall 2: 빨강 상태에서 flash 덮어씌우기
**증상:** 타임아웃 중인데 Client 가 마지막 ALIVE 를 보내면 flash 가 빨강을 덮어씀.
**방지:** `_aliveTimeoutLatched` 래치 후 `OnAliveHeartbeat` 초입에서 `if (_aliveTimeoutLatched) return;`.

### Pitfall 3: Phase 15 `_aliveResponseReceived` 플래그 간섭
**증상:** UI 에서 `_aliveResponseReceived` 를 직접 읽거나 쓰면 Phase 15 재시도 루프가 깨진다.
**방지:** UI 는 **읽기 전용 관찰자**. 플래그는 절대 수정 안 함. event 만 구독(L-02).

### Pitfall 4: `Server` null (생성자 시점)
**증상:** `MenuBar_Loaded` 가 `SystemHandler.Handle.Server.OnAlarm += ...` 시점에 `Server` 가 null 이면 NRE.
**원인:** `SystemHandler` 생성자(Custom/...`) 는 `Server` 초기화 전에 실행되며, `Initialize()` 메서드 95줄에서 `Server = new VisionServer()` 생성. `MenuBar.Loaded` 는 일반적으로 `Initialize()` 이후 호출되지만 방어 코드 필수.
**방지:** `if (SystemHandler.Handle.Server != null) Server.OnAlarm += ...;`. 또는 `MenuBar_Loaded` 가 항상 `Initialize()` 이후인지 `App.xaml.cs` 순서 확인(현재 검증 못함 — planner 확인 필요).

### Pitfall 5: `Dispatcher.Invoke` (동기) 사용
**증상:** Phase 15 의 1초 주기 정밀도 저하, 극단적 경우 hAliveThread 지연으로 오탐지 타임아웃.
**방지:** `BeginInvoke` (비동기) 를 강제. L-01 잠금 준수.

## Code Examples

### 정확한 수정 지점 (라인번호 확인 완료)

**파일 1:** `WPF_Example/Custom/SystemHandler.cs`
- **라인 21:** `_aliveResponseReceived` 필드 선언 (수정 금지 — 읽지도 않음)
- **라인 74~77:** `case VisionRequestType.Alive:` — **여기에 `AliveHeartbeatReceived?.Invoke()` 한 줄 추가**
- **라인 315~361:** `AliveProcess()` 메서드 (수정 금지 — L-02)
- **라인 369~379:** `PerformAliveTimeout()` — **라인 378~379 사이(메서드 끝 직전)에 `AliveTimeout?.Invoke()` 한 줄 추가**
- **신규 필드 추가 위치:** 25번 라인(상수 아래) 근처에 `public event Action AliveHeartbeatReceived; public event Action AliveTimeout;` 두 줄.

**파일 2:** `WPF_Example/SystemHandler.cs`
- 수정 없음 — `mAliveThread` 생성/시작/종료는 그대로(44, 104~108, 162줄).

**파일 3:** `WPF_Example/UI/MenuBar.xaml`
- **라인 234~239 `Grid.RowDefinitions`:** `<RowDefinition Height="2*"/>` 한 줄 추가(Row 4).
- **라인 245 직전:** Row 4 에 StackPanel + Ellipse + TextBlock 3개 신규 요소 추가.
- **라인 9 `UserControl.Resources` 내부 혹은 라인 40 Style 아래:** `<Storyboard x:Key="AliveFlashStoryboard">` 신규 리소스.

**파일 4:** `WPF_Example/UI/MenuBar.xaml.cs`
- **라인 22~30 필드/생성자:** `_aliveBrush`, `_flashStoryboard`, `_aliveTimeoutLatched` 필드 추가.
- **라인 48~51 `MenuBar_Loaded`:** 이벤트 구독 + Storyboard/Brush 캐싱 추가.
- **라인 53~57 `UpdateState`:** 3-state 폴링 로직 추가 (위 Implementation Sketch 참조).
- **신규 메서드 3개 추가:** `OnAliveHeartbeat`, `OnAliveTimeoutEvent`, `OnServerAlarm`.
- **Release/Unload 시 이벤트 구독 해제:** `MainWindow` 또는 `UserControl.Unloaded` 에서 `SystemHandler.Handle.AliveHeartbeatReceived -= OnAliveHeartbeat;` 등 (planner 가 위치 결정 — `MenuBar_Unloaded` 추가 권장).

### 전체 XAML 조각 (Row 4 추가)

```xml
<!-- Data & Version — 260413 hbk: Row 4 ALIVE 인디케이터 추가 -->
<Grid Grid.Column="4" Grid.Row="0" Margin="5,0,0,0">
    <Grid.ColumnDefinitions>
        <ColumnDefinition/>
    </Grid.ColumnDefinitions>
    <Grid.RowDefinitions>
        <RowDefinition Height="3*"/>
        <RowDefinition Height="1"/>
        <RowDefinition Height="2*"/>
        <RowDefinition Height="2*"/>
        <RowDefinition Height="2*"/>  <!--260413 hbk-->
    </Grid.RowDefinitions>

    <TextBlock x:Name="label_DateTime" Grid.Row="0" .../>
    <Label Grid.Row="1" .../>
    <TextBlock x:Name="label_Version" Grid.Row="2" .../>
    <TextBlock x:Name="label_DLLVersion" Grid.Row="3" .../>

    <!--260413 hbk — ALIVE 인디케이터-->
    <StackPanel Grid.Row="4" Orientation="Horizontal"
                HorizontalAlignment="Right" VerticalAlignment="Center"
                Margin="0,0,6,2">
        <Ellipse x:Name="alive_Ellipse" Width="14" Height="14"
                 Stroke="Gray" StrokeThickness="1">
            <Ellipse.Fill>
                <SolidColorBrush Color="#FF9E9E9E"/>  <!--Gray Base-->
            </Ellipse.Fill>
        </Ellipse>
        <TextBlock Text="ALIVE" FontSize="12" Margin="4,0,0,0"
                   VerticalAlignment="Center"/>
    </StackPanel>
</Grid>
```

## State of the Art

| Old Approach | Current Approach | When Changed | Impact |
|--------------|------------------|--------------|--------|
| 수동 `DispatcherTimer` 깜빡임 | `Storyboard + ColorAnimation` | WPF .NET 3.0 (2006) | 부드러운 fade, 재진입 안전, 선언적 |

없음 (순수 WPF 표준만 사용).

## Environment Availability

본 Phase 는 기존 WPF 프로젝트 내부 수정만이며 외부 도구/서비스 의존성 없음.

| Dependency | Required By | Available | Version | Fallback |
|------------|------------|-----------|---------|----------|
| WPF (PresentationFramework) | Ellipse/Storyboard | ✓ | .NET Framework (기존 project TFM) | — |

## Validation Architecture

> `.planning/config.json` 확인 불가 — 보수적으로 섹션 포함.

### Test Framework
| Property | Value |
|----------|-------|
| Framework | **없음** — FinalVision 은 단위 테스트 인프라 없이 수동 UAT 기반 검증 |
| Config file | 없음 |
| Quick run command | `dotnet build WPF_Example/FinalVision.csproj` |
| Full suite command | 실기 구동(Visual Studio Run) + Client 시뮬레이터 연결 |

### Phase Requirements → Test Map

| Req ID | Behavior | Test Type | Method | 비고 |
|--------|----------|-----------|--------|-----|
| ALIVE-UI-01 | 미연결 시 회색 | 수동 UAT | Client 미접속 상태에서 MenuBar Ellipse 색상 Gray 확인 | 자동화 불가(WPF 화면) |
| ALIVE-UI-02 | 수신 시 녹색 flash | 수동 UAT | Client 연결 후 1초 주기 flash 육안 확인 | — |
| ALIVE-UI-03 | 타임아웃 시 빨강 | 수동 UAT | Client 강제 종료 → 약 9초(3회×3초) 후 빨강 확인 | — |
| ALIVE-UI-04 | 재접속 시 녹색 복귀 | 수동 UAT | 빨강 상태에서 Client 재연결 → 즉시 녹색 복귀 확인 | — |
| ALIVE-UI-05 | UI 스레드 안전성 | 수동 + 빌드 | `InvalidOperationException` 미발생, 디버거 중단점 | — |
| ALIVE-UI-06 | Phase 15 로직 미수정 | 코드 Diff | `git diff --stat SystemHandler.cs` 에서 `AliveProcess`/`PerformAliveTimeout` 본문 변경 없음 확인 | L-02 회귀 방지 |

### Sampling Rate
- **Per task commit:** `dotnet build` 또는 VS 빌드 그린.
- **Per wave merge:** 위 ALIVE-UI-01~04 UAT 4건 실기 수행.
- **Phase gate:** 위 UAT 4건 통과 + Phase 15 회귀 재측정(Client 연결 유지 1분+ 관측).

### Wave 0 Gaps
- None — FinalVision 은 테스트 프레임워크를 도입하지 않는 정책(v2.0 NuGet 금지 연장). 본 Phase 에서 신규 테스트 프로젝트 추가 금지.

## Assumptions Log

| # | Claim | Section | Risk if Wrong |
|---|-------|---------|---------------|
| A1 | `MenuBar_Loaded` 시점에 `SystemHandler.Handle.Server` 가 이미 non-null (`SystemHandler.Initialize()` 먼저 실행됨) | Common Pitfalls #4 | NullReferenceException — 방어 코드로 안전하게 처리 가능 |
| A2 | `MainWindow.TimerTick` 이 항상 돌고 있다 (`mTimer.Start()` 호출됨) — 코드 확인은 partial, `IsVisible` 분기에서 `mTimer.Stop()` 경로 존재 | Architecture Patterns | MainWindow 가 숨겨지면 3-state 폴링이 멈춘다. 하지만 이때 UI 가 안 보이므로 사실상 무관 |
| A3 | `_aliveResponseReceived = true` 설정이 MainRun 75줄에서 일어나지만, **V→Client 하트비트 송신의 응답**도 같은 case 로 들어오는지는 Client 구현에 따라 다름. Phase 15 설계는 "Client echo 가 같은 `$ALIVE:1@` 형식으로 돌아옴" — 이 가정 하에 flash 가 송수신 양방향 모두 1회 발생 | Code Examples | Client 가 echo 를 안 보내면 flash 가 안 뜸 — 하지만 Phase 15 요구사항 자체가 그 경우 타임아웃 → 빨강 전환이라 일관됨 |
| A4 | `Storyboard.Begin(this, true)` 의 재진입 시 이전 Storyboard 를 처음부터 재시작하는 WPF 동작 | Architecture Patterns | 기지식(Claude training) — 실기 검증 권장. 만약 이전 애니메이션이 끝나지 않아 쌓인다면 Begin 전에 `Stop(this)` 호출 추가 |
| A5 | CLAUDE.md 가 본 repo 에 존재하지 않음 — 주석 규약은 `memory/MEMORY.md` 의 `feedback_comments.md` 인덱스 항목 기반 | Project Constraints | 규약 불일치 위험 낮음 (memory 항목이 명시적) |

**이 표가 비어있지 않음:** 위 5건은 사용자/플래너가 실기 또는 최종 설계 시 확인 권장. 특히 A3 는 Phase 15 의 "V→Client 하트비트 Echo" 프로토콜 재확인 필요.

## Open Questions (RESOLVED)

1. **Phase 15 Echo 프로토콜의 정확한 방향성**
   - 알려진 것: `AliveProcess` 가 1초마다 `$ALIVE:1@` 를 Client 에 송신, Client 응답을 3초 대기, `_aliveResponseReceived` 로 수신 판정.
   - 불명: 수신 이벤트가 `VisionRequestType.Alive` case (MainRun 74줄)로 들어오는지, 아니면 별도 echo 파싱 경로가 있는지. 코드에서는 MainRun case 만 플래그를 세팅한다 — 즉 V→Client 송신의 응답도 `VisionRequestType.Alive` 로 파싱되어 동일 case 를 탄다고 **가정**.
   - 권장: planner 가 `VisionRequestPacket.Convert` 의 ALIVE 파싱 경로를 재확인 후 flash event 발행 지점 확정.
   - **RESOLVED:** SystemHandler.cs:74 already contains 'case VisionRequestType.Alive:' with comment 'Client→V echo 응답'. Event invoke point confirmed correct.

2. **`Server.OnAlarm` 구독 해제 시점**
   - 알려진 것: `Server.OnAlarm` 은 `TcpServer` 에서 발행.
   - 불명: `MenuBar` 가 교체되거나 App 이 shutdown 될 때 구독 해제 훅이 없다면 GC 가 `SystemHandler`(Singleton) 루트로부터 `MenuBar` 를 잡고 있어 메모리 누수. 일반적으로 App 종료 시점이므로 치명적이진 않음.
   - 권장: `MenuBar_Unloaded` 에서 구독 해제.
   - **RESOLVED:** 16-02 Task 2 handles via MenuBar_Unloaded event (Server.OnAlarm -= OnServerAlarm).

3. **Color 상수 정의 위치**
   - CONTEXT.md 가 "정확한 값은 planner 판단" 이라 함. 권장 HEX(`#FF9E9E9E` Gray, `#FF7EE08B` BaseGreen, `#FF00B050` FlashGreen, `#FFE53935` Red) — planner 가 XAML 리소스 / `MenuBar.xaml.cs` 상수 중 선택.
   - **RESOLVED:** 16-02 Task 1 defines as static readonly Color fields in MenuBar.xaml.cs.

## Sources

### Primary (HIGH confidence)
- `WPF_Example/Custom/SystemHandler.cs` lines 1-383 (실제 코드 읽음)
- `WPF_Example/SystemHandler.cs` lines 1-179 (실제 코드 읽음)
- `WPF_Example/UI/MenuBar.xaml` lines 1-251 (실제 코드 읽음)
- `WPF_Example/UI/MenuBar.xaml.cs` lines 1-116 (실제 코드 읽음)
- `WPF_Example/UI/StatusBarModel.cs` lines 1-127 (실제 코드 읽음 — 경로 수정: `WPF_Example/UI/StatusBarModel.cs`, CONTEXT.md `Custom/StatusBarModel.cs` 는 오기)
- `WPF_Example/TcpServer/VisionServer.cs` lines 1-87 (실제 코드 읽음)
- `WPF_Example/TcpServer/TcpServer.cs` lines 25-480 발췌 (AlarmEventType enum + OnAlarmProcess 확인)
- `WPF_Example/MainWindow.xaml.cs` lines 100-132 (TimerTick/UpdateState 호출 경로)
- `.planning/phases/15-v8-alive-dryrun-time-trace/15-02-PLAN.md` (Phase 15 하트비트 계약)
- `.planning/phases/16-alive-ui/16-CONTEXT.md` (사용자 결정)

### Secondary (MEDIUM confidence)
- WPF `Storyboard.Begin(FrameworkElement, bool isControllable)` 동작 — Claude training 기반(실기 검증 권장)

### Tertiary (LOW confidence)
- 없음

## Metadata

**Confidence breakdown:**
- Standard stack: HIGH — 순수 WPF 기본형, 프로젝트 기존 자산만 사용
- Architecture: HIGH — 기존 폴링 훅 `TimerTick`/`UpdateState()` 및 `AlarmEventType` enum 모두 실제 코드 확인
- Pitfalls: HIGH — 스레드 안전성/래치 논리는 Phase 15 구현과의 실제 상호작용에서 도출
- Assumptions (A3): MEDIUM — Phase 15 ALIVE echo 파싱 경로 최종 확인 필요

**Research date:** 2026-04-13
**Valid until:** 2026-05-13 (WPF 자체 변화 거의 없음, 프로젝트 내부 구조는 Phase 17 까지 안정 예상)
