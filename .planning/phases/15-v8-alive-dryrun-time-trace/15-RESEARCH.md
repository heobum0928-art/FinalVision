# Phase 15: 터미널 모드 v8 프로토콜 확장 — Research

**Researched:** 2026-04-10
**Domain:** TCP 명령 처리 레이어 (C# WPF — FinalVisionProject.Network / SystemHandler)
**Confidence:** HIGH — 전체 구현 코드 직접 확인

---

## Summary

Phase 15는 기존 TCP 터미널 모드에 4개의 신규 명령(ALIVE, DRYRUN, TIME, TRACE)을 추가하고 $LIGHT 응답 포맷을 정리하는 작업이다. 코드베이스를 분석한 결과, 명령 추가 패턴이 완전히 규격화되어 있으며 정확히 3개 레이어에 걸쳐 변경이 발생한다.

**명령 처리 흐름:** `VisionRequestPacket.Convert(string)` (파싱) → `ResourceMap.SetIdentifier()` (리소스 매핑) → `Custom/SystemHandler.cs MainRun()` switch 분기 (처리 + 응답). ALIVE는 여기에 더해 백그라운드 스레드(1개)를 `SystemHandler`에 추가해야 한다.

DRYRUN은 신규 명령 중 가장 영향 범위가 넓다. `bool _dryRunMode` 플래그를 SystemHandler에 두고, `ProcessTest()` 분기에서 플래그가 true일 때 `Sequences.Start(packet)` 대신 즉시 `TestResultPacket(OK)`를 Enqueue하면 된다 — Sequence 스레드를 전혀 건드리지 않아도 된다.

**Primary recommendation:** `VisionRequestPacket`/`VisionResponsePacket` 상속 구조를 그대로 따라 4개 패킷 쌍을 추가하고, `MainRun()`의 switch에 case를 추가하는 단일 패턴으로 구현한다. ALIVE 하트비트 스레드만 별도로 설계한다.

---

<user_constraints>
## User Constraints (from CONTEXT.md)

CONTEXT.md 없음 — 아래는 Request_v8_TerminalMode.md 및 ROADMAP.md에서 확인된 잠긴 결정 사항이다.

### Locked Decisions
- **$LIGHT Option A**: 기존 `$LIGHT` 명령 유지. Client가 $TEST 전후로 ON/OFF 송신. 시퀀스 변경 없음.
- **$TRACE 값 유지 정책**: 다음 `$TRACE` 수신 전까지 값 유지 (검사 1회 후 클리어 안 함).
- **Windows 시계 변경 금지**: `$TIME` 수신 시 내부 변수(`DateTime _syncedTime`)에만 저장.
- **신규 NuGet 패키지 추가 금지** (v2.0 결정): 기존 라이브러리만 사용.

### Claude's Discretion
- ALIVE 하트비트 스레드 구현 방식 (Thread vs System.Threading.Timer)
- `_dryRunMode`, `_syncedTime`, `_palletId`, `_materialId` 필드를 SystemHandler에 직접 두는 방식 vs 별도 클래스 분리

### Deferred Ideas (OUT OF SCOPE)
- $LIGHT Option B (Vision 내부 자동 조명 제어)
- PLC 연동 / 미들웨어 인터페이스 변경
- TCP Client 측 콤마 금지 유효성 검증 (Client 책임)
</user_constraints>

<phase_requirements>
## Phase Requirements

| ID | Description | Research Support |
|----|-------------|------------------|
| TERM-01 | `$ALIVE:1@` 수신 시 `$ALIVE:1,OK@` 응답 | VisionRequestPacket 파싱 패턴 확인, AliveRequestPacket/AliveResultPacket 추가 위치 파악 완료 |
| TERM-02 | Vision이 1초 주기로 `$ALIVE:1@` 자체 송신, 3초 내 응답 없으면 연결 끊김 알람 + 재연결 | SystemHandler 백그라운드 스레드 패턴 확인, Server.SendMessage/OnAlarm 활용 |
| TERM-03 | `$DRYRUN:1,1@`/`$DRYRUN:1,0@` 수신 시 내부 플래그 저장, OK 응답. 플래그 ON 시 $TEST → 즉시 OK 응답 | ProcessTest() 분기점 확인 완료 (Sequences.Start 직전) |
| TERM-04 | `$TIME:1,YYYY,M,D,h,m,s@` 수신 시 내부 DateTime 저장, `$TIME:1,OK@` 응답 (Windows 시계 미변경) | 파싱 패턴 확인 완료 |
| TERM-05 | `$TRACE:1,palletId,materialId@` 수신 시 내부 string 변수 저장, `$TRACE:1,OK@` 응답 (값 유지) | 파싱 패턴 확인 완료, 다음 $TRACE까지 클리어 안 함 |
</phase_requirements>

---

## Standard Stack

### Core (변경 없음 — 기존 코드 확장)

| 요소 | 현재 값 | 역할 |
|------|---------|------|
| `System.Threading.Thread` | .NET 4.8 | 기존 백그라운드 스레드 패턴 (mSystemThread, ConnectedClient.mCommunicationThread) |
| `System.Threading.Stopwatch` | .NET 4.8 | ALIVE 타임아웃 측정용 (TcpServer에 이미 mReceiveTimer 있음) |
| `FinalVisionProject.Network.VisionRequestPacket` | 현재 코드 | 수신 패킷 파싱 + 타입 dispatch |
| `FinalVisionProject.Network.VisionResponsePacket` | 현재 코드 | 송신 패킷 직렬화 |
| `FinalVisionProject.SystemHandler` (Custom/SystemHandler.cs) | 현재 코드 | MainRun() 에서 모든 명령 처리 |

**Installation:** 신규 NuGet 없음.

---

## Architecture Patterns

### 기존 명령 추가 패턴 (VERIFIED: 직접 코드 확인)

신규 명령 1건당 정확히 4개 위치가 변경된다:

```
1. VisionRequestPacket.cs
   - enum VisionRequestType 에 값 추가
   - CMD_RECV_XXX 상수 추가
   - Convert(string) 의 switch 에 파싱 case 추가
   - XxxPacket 서브클래스 추가

2. VisionResponsePacket.cs
   - enum EVisionResponseType 에 값 추가
   - CMD_SEND_XXX 상수 추가 (필요 시)
   - Convert(VisionResponsePacket) 의 switch 에 직렬화 case 추가
   - XxxResultPacket 서브클래스 추가

3. Custom/TcpServer/ResourceMap.cs
   - SetIdentifier() 의 switch 에 case 추가 (리소스 매핑이 없으면 Unknown 처리면 충분)

4. Custom/SystemHandler.cs (MainRun 의 switch)
   - ProcessXxx() 메서드 추가
   - MainRun() switch 에 case VisionRequestType.Xxx: 추가
```

### $TEST 처리 패턴 (DryRun 인터셉트 위치)

```csharp
// Custom/SystemHandler.cs — MainRun() 내부
case VisionRequestType.Test:
    if (Setting.AutoLogoutWhenRecvTest && Login.IsLogin) { Login.LogOut(); }
    if (!ProcessTest(packet.AsTest())) {           // ← DryRun은 여기서 인터셉트
        responsePacket = SendTestError(packet.AsTest());
    }
    break;

// ProcessTest() 현재 구현
private bool ProcessTest(TestPacket packet) {
    return Sequences.Start(packet);                // ← DryRun ON이면 Start 대신 즉시 OK Enqueue
}
```

DryRun 인터셉트 방식 (권장):

```csharp
private bool ProcessTest(TestPacket packet) {
    if (_dryRunMode) {
        // 즉시 OK 응답 — Sequence 실행 없음
        TestResultPacket dryResult = new TestResultPacket();
        dryResult.Target = packet.Sender;
        dryResult.Site   = packet.Site;
        dryResult.InspectionType = packet.TestType;
        dryResult.Result = EVisionResultType.OK;
        // PopResponse() 경로를 타도록 Enqueue
        Sequences[packet.Identifier]?.ResponseQueue.Enqueue(dryResult); //260410 hbk
        return true;
    }
    return Sequences.Start(packet);
}
```

> 주의: `ResponseQueue`는 `SequenceBase.ResponseQueue`(ConcurrentQueue)로 public이므로 직접 Enqueue 가능. [VERIFIED: SequenceBase.cs line 69]

### ALIVE 하트비트 패턴

SystemHandler에 별도 스레드 1개 추가. 기존 mSystemThread(SystemProcess)와 분리.

```csharp
// SystemHandler.cs 에 추가할 필드
private Thread mAliveThread;
private Stopwatch _aliveTimer = new Stopwatch();
private volatile bool _aliveResponseReceived = false;
private const int ALIVE_SEND_INTERVAL_MS  = 1000;  //260410 hbk
private const int ALIVE_TIMEOUT_MS        = 3000;  //260410 hbk

// AliveProcess 스레드 루프 (개략)
private void AliveProcess() {
    while (!IsTerminated) {
        if (Server.IsConnected()) {
            // 1초마다 송신
            SendAlive();
            _aliveResponseReceived = false;
            _aliveTimer.Restart();

            // 3초 대기하면서 응답 확인
            while (_aliveTimer.ElapsedMilliseconds < ALIVE_TIMEOUT_MS) {
                if (_aliveResponseReceived) break;
                Thread.Sleep(50);
            }
            if (!_aliveResponseReceived) {
                // 타임아웃 → 알람 + 재연결 처리
                PerformAliveTimeout();
            }
        }
        Thread.Sleep(ALIVE_SEND_INTERVAL_MS - (int)_aliveTimer.ElapsedMilliseconds);
    }
}
```

> Client→V ALIVE(echo) 수신 시: `_aliveResponseReceived = true` 세팅 + `$ALIVE:1,OK@` 즉시 응답.
> V→Client ALIVE 수신 시: `_aliveResponseReceived = true` 세팅.

### 응답 포맷 규칙

모든 신규 명령의 OK 응답 포맷:

| 명령 | 수신 포맷 | 응답 포맷 |
|------|----------|----------|
| `$ALIVE` | `$ALIVE:1@` | `$ALIVE:1,OK@` |
| `$DRYRUN` | `$DRYRUN:1,1@` / `$DRYRUN:1,0@` | `$DRYRUN:1,OK@` |
| `$TIME` | `$TIME:1,YYYY,M,D,h,m,s@` | `$TIME:1,OK@` |
| `$TRACE` | `$TRACE:1,palletId,materialId@` | `$TRACE:1,OK@` |

기존 TestResultPacket 응답 패턴과 다르게, 신규 4개는 모두 `$CMD:Site,OK@` 형태의 단순 응답이다. 즉 `EVisionResponseType` 값 하나씩 추가하면 된다.

### $LIGHT 응답 포맷 버그 (TERM-05 연계)

`VisionResponsePacket.Convert()` Light case [VERIFIED: VisionResponsePacket.cs lines 237-251]:

```csharp
case EVisionResponseType.Light:
    // ...
    if (lightPacket.On == false) {
        msg += lightPacket.GetOnString();          // "LIGHT:Site,0"
    } else {
        // msg += lightPacket.TestType.ToString(); ← 주석 처리됨
        // msg += VisionServer.MSG_CONTENTS_SEPERATOR;
        msg += lightPacket.GetOnString();          // "LIGHT:Site,1" (type 필드 없음)
    }
```

현재 응답: `$LIGHT:1,1@` (ON) / `$LIGHT:1,0@` (OFF)
프로토콜 규격: `$LIGHT:1,type,1@` 형태여야 하는지 확인 필요. [ASSUMED] 규격 문서(Request_v8_TerminalMode.md)에는 "type 필드 없는 예외도 함께 정리"라고만 기술됨. 현재 Option A(Client가 type 포함하여 `$LIGHT:Site,Type,ON/OFF@`로 송신)를 전제로 응답은 `$LIGHT:Site,OK@` 또는 `$LIGHT:Site,ON/OFF@` 중 하나를 결정해야 한다.

---

## Don't Hand-Roll

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| 스레드 안전 큐 | 직접 Lock+Queue 구현 | `ConcurrentQueue<T>` (이미 사용 중) | 기존 ResponseQueue와 동일 패턴 |
| 타임아웃 측정 | Thread.Sleep 루프 카운팅 | `System.Diagnostics.Stopwatch` (이미 사용 중) | 정확도 보장 |
| TCP 메시지 직렬화 | 직접 string 조립 | `VisionResponsePacket.Convert()` 기존 패턴 재사용 | 일관성 |
| 멀티스레드 플래그 | 일반 bool 필드 | `volatile bool` 또는 `Interlocked` | ALIVE 스레드와 MainRun 스레드가 동시 접근 |

---

## Common Pitfalls

### Pitfall 1: ALIVE 응답 처리 경로 혼동
**What goes wrong:** Client→V `$ALIVE:1@`가 수신될 때 MainRun()의 switch에서 처리해야 하는데, ALIVE 스레드가 `_aliveResponseReceived`를 세팅하는 조건과 혼동될 수 있다.
**Why it happens:** 양방향 프로토콜 — Vision이 송신자이기도 하고 수신자이기도 함.
**How to avoid:**
- Client→V ALIVE 수신 시: `_aliveResponseReceived = true` + `$ALIVE:1,OK@` 응답 (MainRun switch case)
- V→Client 응답 수신 시: `_aliveResponseReceived = true` 세팅만 (ALIVE 스레드에서 감시)
- 구분 기준: 현재 코드의 GetRecvPacket()이 수신 큐에서 꺼내므로 MainRun()이 처리하면 됨.

### Pitfall 2: DryRun ResponseQueue Enqueue 대상 시퀀스
**What goes wrong:** `packet.Identifier`로 시퀀스를 찾아야 하는데 null이 될 수 있음. ResourceMap.SetIdentifier()가 먼저 호출되어 Identifier가 채워지지만, DryRun 모드에서 Identifier가 매핑되지 않은 TestType이 들어오면 null.
**How to avoid:** `Sequences[packet.Identifier]` null guard 추가 필수.

### Pitfall 3: $TIME 파싱 — 필드 개수
**What goes wrong:** `$TIME:1,YYYY,M,D,h,m,s@` 는 콜론 분리 후 args가 `1,YYYY,M,D,h,m,s` — Split(',') 하면 7개 필드. M/D/h/m/s는 1~2자리 가변. `Int32.TryParse` 로 각 필드 파싱해야 하며, `DateTime` 생성 시 유효성 검증(월 1~12, 일 1~31 등) 실패 시 NG 응답.
**How to avoid:** dataList.Length < 7 guard 추가.

### Pitfall 4: ALIVE 스레드 생명주기
**What goes wrong:** `SystemHandler.Release()`에서 `IsTerminated = true`로 세팅 후 mSystemThread.Join 하지만 mAliveThread.Join을 빠트리면 프로세스 종료 시 오류.
**How to avoid:** Release()에 `mAliveThread.Join(1000)` 추가.

### Pitfall 5: $LIGHT 응답의 TestType 필드
**What goes wrong:** 현재 VisionResponsePacket Light case에서 TestType을 응답에 포함하지 않음(주석 처리). 요청 패킷에는 TestType이 파싱되어 `LightResultPacket.TestType`에 세팅되지만 응답 직렬화 시 누락됨.
**How to avoid:** 응답 포맷을 `$LIGHT:Site,OK@` 로 통일하거나, `$LIGHT:Site,Type,ON/OFF@`로 통일 — 어느 쪽이든 코드와 일치하도록 수정 필요. [ASSUMED] 현장 요구가 어느 포맷인지 확인 후 결정.

---

## Code Examples

### 기존 패킷 파싱 패턴 (Test case 참조)

```csharp
// VisionRequestPacket.cs — Convert(string) 내 switch case 패턴
case CMD_RECV_TEST: //test
    packet = new TestPacket();
    TestPacket testPacket = packet.AsTest();
    dataList = msgList[1].Split(VisionServer.MSG_CONTENTS_SEPERATOR);
    if (dataList.Length < 3) return null;
    if (Int32.TryParse(dataList[0], out siteNum) == false) return null;
    testPacket.Site = siteNum;
    if (Int32.TryParse(dataList[1], out testKind) == false) return null;
    testPacket.TestType = testKind;
    testID = dataList[2];
    testPacket.TestID = testID;
    break;
```

### 기존 응답 직렬화 패턴 (Test response 참조)

```csharp
// VisionResponsePacket.cs — Convert(VisionResponsePacket) 내 switch case 패턴
case EVisionResponseType.Test:           //260326 hbk
    TestResultPacket testPacket = packet.AsTestResult();
    msg += CMD_SEND_TEST;
    msg += VisionServer.MSG_CMD_SEPERATOR;
    msg += testPacket.Site.ToString();
    msg += VisionServer.MSG_CONTENTS_SEPERATOR;
    msg += testPacket.InspectionType.ToString();
    msg += VisionServer.MSG_CONTENTS_SEPERATOR;
    msg += testPacket.GetResultString();  //260326 hbk — OK/NG만 응답
    break;
```

### MainRun() 처리 패턴 (Light 참조)

```csharp
// Custom/SystemHandler.cs — MainRun() switch 패턴
case VisionRequestType.Light:
    responsePacket = ProcessLightSet(packet.AsLight());
    break;
// → responsePacket이 null이 아니면 Server.SendPacket(i, responsePacket)으로 즉시 응답
```

### DryRun 신규 명령 처리 예시 구조

```csharp
// VisionRequestPacket.cs 에 추가
public const string CMD_RECV_DRYRUN = "DRYRUN";

// VisionRequestType enum 에 추가
DryRun,

// DryRunPacket 클래스 추가
public class DryRunPacket : VisionRequestPacket {
    public bool Enable { get; set; }  //260410 hbk — 1=ON, 0=OFF
    public DryRunPacket() : base(VisionRequestType.DryRun) { }
}

// Custom/SystemHandler.cs 에 추가
private volatile bool _dryRunMode = false;  //260410 hbk

private VisionResponsePacket ProcessDryRun(DryRunPacket packet) {
    _dryRunMode = packet.Enable;  //260410 hbk
    DryRunResultPacket result = new DryRunResultPacket();
    result.Target = packet.Sender;
    result.Site   = packet.Site;
    return result;  //260410 hbk — $DRYRUN:1,OK@
}
```

---

## Assumptions Log

| # | Claim | Section | Risk if Wrong |
|---|-------|---------|---------------|
| A1 | $LIGHT 응답 포맷을 `$LIGHT:Site,OK@`로 통일하면 Client 측 로직에 영향 없다 | Code Examples / Pitfall 5 | Client가 현재 `$LIGHT:Site,0@` / `$LIGHT:Site,1@` 포맷을 파싱하고 있을 경우 프로토콜 불일치 |
| A2 | ALIVE 타임아웃 발생 시 "연결 끊김 알람"이란 기존 `AlarmEventArgs.AlarmEventType.OnDisconnected` 경로를 활용하면 된다 | Architecture Patterns / ALIVE | 별도 UI 알람(팝업 등)이 필요한 경우 추가 구현 필요 |
| A3 | ALIVE 하트비트를 받지 않는 연결 초기 상태(Client 미연결)에서는 AliveProcess 스레드가 `Server.IsConnected() == false` guard로 송신 스킵한다 | Architecture Patterns / ALIVE | 연결 없는 상태에서 스레드가 계속 활성화되어도 송신하지 않으면 OK |
| A4 | `_palletId`, `_materialId`를 SystemHandler 필드에 두면 단일 Client 운영 환경에서 충분하다 | Architecture Patterns | 다중 Client 동시 연결 시 충돌 — 현재 MAX_CONNECTION_COUNT=1이므로 실질적 위험 없음 |

---

## Open Questions

1. **$LIGHT 응답 포맷 최종 결정**
   - What we know: 현재 응답이 `$LIGHT:Site,ON/OFF@`이며 TestType 필드가 누락됨
   - What's unclear: 현장 Client가 기대하는 포맷이 `$LIGHT:Site,OK@`인지, `$LIGHT:Site,Type,ON/OFF@`인지
   - Recommendation: 플랜에서 `$LIGHT:Site,OK@`로 통일하는 방향으로 Task를 구성하되, 실행 전 현장 확인

2. **ALIVE 재연결 루틴 상세**
   - What we know: `PerformAliveTimeout()` 시 "알람 + 재연결"이 요구됨
   - What's unclear: 재연결이란 TcpServer 측에서 기존 Client를 Disconnect() 후 새 연결을 기다리는 것인지, 또는 Vision이 Client에 능동 재접속하는 것인지
   - Recommendation: Vision은 Server 역할이므로 "기존 소켓 강제 종료 → Accept 대기 복귀"로 구현. 재연결 시도(클라이언트 능동 접속)는 Client 책임.

---

## Environment Availability

Step 2.6: SKIPPED — 이 Phase는 기존 TCP 소켓 인프라 위에 C# 코드 변경만 수행하며, 외부 도구/서비스 의존성 없음.

---

## Validation Architecture

nyquist_validation 설정 파일 미확인이나 기본값(enabled)으로 처리.

### Test Framework
| Property | Value |
|----------|-------|
| Framework | 수동 테스트 (기존 TCP 테스트 패턴 — 별도 테스트 프레임워크 없음) |
| Config file | 없음 |
| Quick run | TCP 클라이언트(telnet/Hercules)로 직접 명령 송신 |
| Full suite | ROADMAP Phase 15 Success Criteria 7개 항목 순서대로 확인 |

### Phase Requirements → Test Map

| Req ID | Behavior | Test Type | Verification Method |
|--------|----------|-----------|---------------------|
| TERM-01 | `$ALIVE:1@` 수신 → `$ALIVE:1,OK@` 응답 | smoke | TCP 클라이언트로 `$ALIVE:1@` 송신, 응답 확인 |
| TERM-02 | V가 1초 주기 송신, 3초 타임아웃 알람 | smoke | 연결 후 3초 무응답 → 로그 알람 확인 |
| TERM-03 | `$DRYRUN:1,1@` ON 후 `$TEST` → 즉시 `$RESULT:1,OK@` | smoke | DryRun ON 상태에서 $TEST 송신, 검사 없이 응답 확인 |
| TERM-03 | `$DRYRUN:1,0@` OFF 후 `$TEST` → 정상 검사 | smoke | DryRun OFF 후 $TEST 송신, 시퀀스 실행 확인 |
| TERM-04 | `$TIME:1,2026,4,10,14,0,0@` → `$TIME:1,OK@` | smoke | 응답 확인 + Windows 시계 미변경 확인 |
| TERM-05 | `$TRACE:1,P001,MAT001@` → `$TRACE:1,OK@`, 값 유지 | smoke | 수신 후 로그에 palletId/materialId 출력 확인 |

### Wave 0 Gaps
- 수동 테스트 환경이므로 Wave 0 자동화 Gap 없음. TCP 클라이언트 툴(Hercules 또는 동등한 것)이 개발 PC에 있는지 확인 필요.

---

## Security Domain

security_enforcement 설정 미확인이나 기본값(enabled)으로 처리.

### Applicable ASVS Categories

| ASVS Category | Applies | Standard Control |
|---------------|---------|-----------------|
| V2 Authentication | no | TCP 연결은 인증 없음 (기존 동일) |
| V3 Session Management | no | 단일 Client 연결 관리 (기존 동일) |
| V4 Access Control | no | 해당 없음 |
| V5 Input Validation | yes | `Int32.TryParse` + `dataList.Length` guard (기존 패턴 동일 적용) |
| V6 Cryptography | no | 해당 없음 |

### Known Threat Patterns for {TCP ASCII 프로토콜}

| Pattern | STRIDE | Standard Mitigation |
|---------|--------|---------------------|
| 잘못된 필드 개수 패킷 | Tampering | `dataList.Length < N` guard (기존 패턴 동일) |
| palletId/materialId 에 콤마 포함 | Tampering | Client 책임 (프로토콜 규격 — 콤마 금지), Vision은 수신 그대로 저장 |
| $TIME 범위 초과 값 (월=13 등) | Tampering | `DateTime` 생성 시 예외 catch → NG 응답 또는 무시 |

---

## Sources

### Primary (HIGH confidence)
- `WPF_Example/TcpServer/VisionRequestPacket.cs` — 수신 패킷 파싱 전체 확인
- `WPF_Example/TcpServer/VisionResponsePacket.cs` — 응답 패킷 직렬화 전체 확인
- `WPF_Example/TcpServer/TcpServer.cs` — 스레드 구조, ConcurrentQueue, ConnectedClient 확인
- `WPF_Example/TcpServer/VisionServer.cs` — VisionServer 확장 포인트 확인
- `WPF_Example/Custom/SystemHandler.cs` — MainRun() 명령 dispatch + ProcessXxx 패턴 전체 확인
- `WPF_Example/Custom/TcpServer/ResourceMap.cs` — SetIdentifier() 패턴 확인
- `WPF_Example/Sequence/SequenceHandler.cs` — Start(TestPacket) 확인
- `WPF_Example/Sequence/Sequence/SequenceBase.cs` — ResponseQueue (ConcurrentQueue) 확인
- `WPF_Example/Custom/Sequence/Inspection/Sequence_Inspection.cs` — AddResponse() 패턴 확인
- `.planning/Request_v8_TerminalMode.md` — v8 프로토콜 요구사항 원문
- `.planning/ROADMAP.md` — Phase 15 Success Criteria

---

## Metadata

**Confidence breakdown:**
- Standard stack: HIGH — 기존 코드에서 직접 확인
- Architecture: HIGH — 4개 레이어 변경 위치 코드 직접 확인
- Pitfalls: HIGH — 기존 코드의 주석 처리된 $LIGHT Type 필드 직접 발견

**Research date:** 2026-04-10
**Valid until:** 2026-06-01 (코드베이스 변경이 없으면 유효)
