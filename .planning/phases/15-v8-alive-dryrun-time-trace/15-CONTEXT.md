# Phase 15: v8 프로토콜 확장 (ALIVE / DRYRUN / TIME / TRACE) - Context

**Gathered:** 2026-04-13
**Status:** Ready for re-planning (기존 PLAN 2개는 사용자 컨텍스트 없이 작성됨 — 본 CONTEXT 반영을 위해 /gsd-plan-phase 15 재실행 필요)

<domain>
## Phase Boundary

기존 TCP 터미널 모드에 4개 신규 명령($ALIVE, $DRYRUN, $TIME, $TRACE) 추가. $LIGHT는 현 응답 포맷(`$LIGHT:Site,OP`)이 정상 스펙으로 확정되어 **Phase 15에서 코드 수정 없음**. 명령 처리 레이어(VisionRequestPacket / VisionResponsePacket / ResourceMap / SystemHandler) 위에서 C# 코드 변경만 수행하며 외부 의존성(NuGet, 미들웨어, PLC 연동) 변경 없음.

범위 외(OUT):
- $LIGHT Option B (Vision 내부 자동 조명 제어)
- PLC / 미들웨어 인터페이스 변경
- Client 측 콤마 금지 유효성 검증 (Client 책임)
- 본 Phase 범위를 벗어난 다른 프로토콜 명령 수정

</domain>

<decisions>
## Implementation Decisions

### $LIGHT 응답 포맷 정리
- **D-01:** $LIGHT 응답 포맷은 **`$LIGHT:Site,OP`** 로 유지 (현재 코드 그대로, **Phase 15에서 $LIGHT 관련 코드 수정 없음**).
  - 수신(Client→Vision): `$LIGHT:Site,Type,On` (예: `$LIGHT:1,2,1@`) — 파싱 로직 변경 없음.
  - 응답(Vision→Client): `$LIGHT:Site,OP` (예: `$LIGHT:1,1@` / `$LIGHT:1,0@`) — 현재 `VisionResponsePacket.cs` 236~252줄 상태 유지. Type 필드 주석 처리된 그대로 둠.
  - 근거: 설비의 조명은 1개뿐이므로 응답에 Type을 실을 필요 없음. Site + Operation(ON/OFF) 상태값만으로 충분.
  - v8 요청서의 *"type 필드 없는 예외 정리"* 는 **현 상태를 정상 스펙으로 확정**하는 방향으로 해석.

### ALIVE 타임아웃 및 재연결 정책
- **D-02:** V→Client $ALIVE 송신 후 3초 내 응답이 없으면 **ping 재시도 최대 3회**.
  - 1차 타임아웃 → 즉시 $ALIVE 재송신 후 다시 3초 대기 (최대 3회까지)
  - 3회 모두 무응답 시 기존 Client 소켓을 강제 종료하고 Accept 대기 상태로 복귀
  - 동시에 `AlarmEventType.OnDisconnected` 경로로 **NG 알람** 발생
  - Vision은 TCP Server이므로 Client로 능동 재접속 불가 — 재접속은 Client 책임
- **D-03:** 재시도 카운트, 타임아웃 값은 상수로 관리 (`ALIVE_SEND_INTERVAL_MS=1000`, `ALIVE_TIMEOUT_MS=3000`, `ALIVE_RETRY_COUNT=3`).

### ALIVE 하트비트 구현 방식
- **D-04:** `System.Threading.Thread + Stopwatch` 패턴 사용.
  - 기존 `mSystemThread`(SystemProcess) 와 동일 스타일로 `mAliveThread` 추가.
  - `System.Threading.Timer`는 사용하지 않음 — 코드베이스 일관성을 우선.
  - `volatile bool _aliveResponseReceived` 로 MainRun 스레드와의 동기화.
  - `SystemHandler.Release()` 에서 `mAliveThread.Join(1000)` 호출하여 정상 종료 보장.

### Claude's Discretion (downstream이 결정)
- `_dryRunMode`, `_syncedTime`, `_palletId`, `_materialId` 필드를 `SystemHandler` 에 직접 두는 방식 vs 별도 상태 클래스 분리 — planner 판단.
- `PerformAliveTimeout()` 내부 상세 로직(알람 이벤트 payload 포맷 등) — planner 판단.
- $TIME 파싱 시 유효하지 않은 날짜값(월=13 등) NG 처리 방식 — planner 판단.

### Locked from prior research (재확인)
- **$LIGHT 처리 방식 = Option A** (Client가 $TEST 전후로 ON/OFF 송신, 시퀀스 변경 없음)
- **$TRACE 값 유지 정책 = 다음 $TRACE 수신 전까지 값 유지** (검사 1회 후 클리어 안 함)
- **$TIME은 Windows 시계 변경 금지**, 내부 `DateTime _syncedTime` 에만 저장
- **신규 NuGet 패키지 추가 금지** (v2.0 결정)

</decisions>

<canonical_refs>
## Canonical References

downstream agent(researcher, planner, executor)가 반드시 참고해야 할 문서:

- `.planning/Request_v8_TerminalMode.md` — v8 프로토콜 요구사항 원문
- `.planning/ROADMAP.md` (Phase 15 항목) — Success Criteria
- `.planning/phases/15-v8-alive-dryrun-time-trace/15-RESEARCH.md` — 4 레이어 변경 패턴 / Pitfall / Assumption Log
- `WPF_Example/TcpServer/VisionRequestPacket.cs` — 수신 파싱 기존 패턴 (Light case 216~252줄 참조)
- `WPF_Example/TcpServer/VisionResponsePacket.cs` — 응답 직렬화 기존 패턴 (Light case 236~252줄, 현재 Type 필드 주석 처리)
- `WPF_Example/Custom/SystemHandler.cs` — MainRun() switch dispatch 패턴
- `WPF_Example/Custom/TcpServer/ResourceMap.cs` — SetIdentifier() 매핑 패턴
- `WPF_Example/Sequence/Sequence/SequenceBase.cs` — ResponseQueue (ConcurrentQueue) — DryRun 인터셉트 시 직접 Enqueue 대상

</canonical_refs>

<deferred>
## Deferred Ideas (OUT OF SCOPE — 백로그)

- $LIGHT Option B: Vision이 $TEST 수신 시 내부에서 조명 자동 제어 (시퀀스 변경 필요)
- PLC 연동 / 미들웨어 인터페이스 변경
- TCP Client 측 콤마 금지 유효성 검증 (Client 책임)
- 다중 Client 동시 연결 지원 (현재 MAX_CONNECTION_COUNT=1)
- ALIVE 타임아웃 상세 UI (팝업 알람 등) — 현재는 기존 `OnDisconnected` 알람 경로 재사용

</deferred>

<next_steps>
## Next Steps

1. `/gsd-plan-phase 15` 재실행 → 본 CONTEXT.md 반영하여 기존 15-01, 15-02 PLAN 재생성
   - D-01($LIGHT 현 상태 유지 — 수정 작업 없음): 기존 15-01 PLAN의 `$LIGHT 응답 포맷 통일` must_have를 **제거**하고 files_modified에서 LightResultPacket 관련 변경 제외
   - D-02(ping 3회 재시도), D-03(상수 3종), D-04(Thread+Stopwatch) 가 15-02 PLAN must_haves에 명시적으로 들어가야 함
2. 재생성된 PLAN 확인 후 `/gsd-execute-phase 15` 로 실행
</next_steps>
