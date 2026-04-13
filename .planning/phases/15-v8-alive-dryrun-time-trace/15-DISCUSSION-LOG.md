# Phase 15 Discussion Log

**Date:** 2026-04-13
**Mode:** Interactive discuss (post-plan retrofit — 사용자 컨텍스트 없이 작성된 기존 PLAN에 사용자 결정 반영 목적)

---

## Area 1: $LIGHT 응답 포맷 정리

### Q1-1
**Question:** 현재 $LIGHT 응답 코드에 버그가 있습니다 — TestType 필드가 주석 처리되어 `$LIGHT:Site,On`만 나갑니다. v8 규격에 맞게 어떤 포맷으로 통일할까요?

**Options:** `$LIGHT:Site,OK` 통일 / `$LIGHT:Site,Type,On` 유지 / 현장 확인 후 결정

**User response:** "$LIGHT:1,2,1@ 형식을 무시한다는건가" — 수신/응답 구분 혼동.

### Clarification 1-1
수신(Client→Vision) `$LIGHT:1,2,1@` 파싱은 정상이며 변경 없음. 질문은 Vision→Client **응답** 포맷에 한정됨을 설명.

### Q1-2
**User response:** "무슨 말인지 이해가 안가 On/off라는 프로토콜이 있던가?"

### Clarification 1-2
`GetOnString()`은 `1`/`0` 반환. 프로토콜은 숫자 `1=켜짐`, `0=꺼짐`. "ON/OFF"는 용어 실수였음을 정정.

### Q1-3
**User response:** "Site은 장비 위치 파악 용도, type은 Grab1~5 구분. 일괄 조정은 버그가 아니자나 왜 그렇게 판단한거지?"

### Clarification 1-3
"버그" 단정 철회. 주석 처리는 의도적 결정일 수 있음. 정리 방향 2가지(A=Type 제거 / B=Type 포함) 제시.

### Q1-4
**User response:** "Type 포함하는 방향" (1차)

### Q1-5 (최종 재결정)
**User response:** "조명은 어차피 1개라 기존대로 Site + Operation 상태값만 전달하자. 수신 `$LIGHT:1,2,1@`, 응답 `$LIGHT:site,OP`. 조명쪽 수정 없는거지?"

**✅ Decision D-01 (최종):** $LIGHT 응답 = **`$LIGHT:Site,OP`** (현 코드 그대로 유지). Phase 15에서 $LIGHT 관련 코드 수정 **없음**. 조명은 1개뿐이므로 Type 필드 불필요.

---

## Area 2: ALIVE 타임아웃 및 재연결 정책

### Q2-1
**Options:** 소켓 강제 종료 + Accept 복귀 / 알람만 / 둘 다

**User response:** "끊어지면 재접속 try는 어때 3번정도 해보고 NG 띄우는거지"

### Clarification 2-1
Vision은 TCP Server이므로 능동 재접속 불가. "3번 try" 해석 2가지 제시 (A: ping 재시도 / B: 리스너 재시작).

### Q2-2 (최종)
**User response:** "둘조합이 괜찮나"

**✅ Decision D-02, D-03:** ping 재시도 3회 → 모두 무응답이면 기존 소켓 종료 + Accept 복귀 + NG 알람. 상수 3종(`ALIVE_SEND_INTERVAL_MS=1000`, `ALIVE_TIMEOUT_MS=3000`, `ALIVE_RETRY_COUNT=3`).

---

## Area 3: ALIVE 하트비트 스레드 구현 방식

### Q3-1 (최종)
**Options:** Thread + Stopwatch / System.Threading.Timer

**User response:** "Thread + Stopwatch (기존 패턴)"

**✅ Decision D-04:** mSystemThread와 동일 스타일 mAliveThread. Release() 에서 Join(1000).

---

## Scope Creep Handling

본 세션에서 발생 없음.

## Deferred Ideas Captured

- $LIGHT Option B (Vision 내부 자동 조명 제어)
- PLC 연동 / 미들웨어 인터페이스 변경
- 다중 Client 동시 연결

## Canonical Refs Added During Discussion

- `WPF_Example/TcpServer/VisionResponsePacket.cs` 236~252줄 (Light 응답 직렬화 — 현 상태 유지 확정)
- `WPF_Example/TcpServer/VisionResponsePacket.cs` 497~500줄 (`GetOnString()` — 1/0 반환 확인)
