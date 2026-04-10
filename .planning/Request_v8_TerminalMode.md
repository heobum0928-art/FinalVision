# Vision PC 터미널 모드 — 추가 작업 요청서 (v8)

## 배경

Vision PC의 터미널 모드는 TCP **Server**로 동작하며, 외부 Client와 ASCII 명령 프로토콜로 통신함. 형식 `$CMD:args@`, 구분자 `,`, 종결자 `@`, ID 필드 콤마 금지.

> **참고**: 운영 환경에서는 Vision과 PLC 사이에 미들웨어가 존재해 PLC 신호를 본 프로토콜로 변환·중계하지만, **터미널 모드 코드 자체는 미들웨어의 존재를 알 필요 없음**. 터미널 모드는 단지 "TCP Client가 보내는 명령을 받아 처리하고 응답할 뿐"이며, Client가 미들웨어든 테스트 툴이든 무관하게 동일하게 동작해야 함. 본 문서의 명령 송신 주체는 모두 "Client"로 표기.

기존 ECi Moving Display V1.0 프로토콜에 v8 시퀀스 신규 기능 5건 추가.

---

## 작업 항목

### 1. PC Alive 기능 (양방향)
- **신규 명령**: `$ALIVE`
- Client→V: `$ALIVE:1@` → V 응답 `$ALIVE:1,OK@`
- V→Client: `$ALIVE:1@` (1초 주기 자체 송신) → Client 응답 대기
- **타임아웃**: 3초 내 응답 없으면 연결 끊김 판정 → 알람 + 재연결 루틴
- 백그라운드 스레드 1개 추가 (송신 + 타임아웃 감시)

### 2. $LIGHT 처리 방식 검토 ⚠️ 시퀀스 변경 검토 필요
- **현 스펙**: Client가 `$LIGHT` 명령으로 조명 ON/OFF 직접 제어
- **실제 운영 요구**: Vision이 `$TEST` 수신 시 **자체적으로 조명 ON → Grab → 검사 → OFF** 처리해야 함
- **검토 옵션**:
  - (A) 기존 `$LIGHT` 명령 유지 + Client가 $TEST 전후로 ON/OFF 송신
  - (B) `$LIGHT` 명령 폐기, Vision이 `$TEST` 수신 시 내부에서 조명 자동 제어
- **결정 필요**: 비전 담당자 합의 후 결정
- ※ $LIGHT 응답에 `type` 필드 없는 예외도 함께 정리

### 3. DryRun 모드 추가
- **신규 명령**: `$DRYRUN`
- `$DRYRUN:1,1@` (ON) / `$DRYRUN:1,0@` (OFF) → 응답 `$DRYRUN:1,OK@`
- **동작**: ON 상태에서 `$TEST` 수신 시 카메라 Grab/조명/검사 알고리즘 **모두 스킵**, 즉시 `$RESULT:1,OK@` 응답
- **상태 보존**: 내부 `bool _dryRunMode` 플래그
- ※ 재연결 시 초기 상태는 Client가 명시적으로 한 번 송신해주는 것을 전제 (Vision은 기본 OFF로 시작)

### 4. 시간 동기화 기능
- **신규 명령**: `$TIME`
- `$TIME:1,YYYY,M,D,h,m,s@` → 응답 `$TIME:1,OK@`
- **동작**: 수신값을 내부 변수(`DateTime _syncedTime`)에만 저장, **Windows 시계 변경 금지** (권한 문제 방지)
- **용도**: 검사 로그/이력 타임스탬프용
- Vision은 받기만 하면 됨 (송신 주기는 Client 책임)

### 5. Pallet / Material ID 수신
- **신규 명령**: `$TRACE`
- `$TRACE:1,palletId,materialId@` → 응답 `$TRACE:1,OK@`
- **필드**:
  - Pallet ID: 최대 10자, 콤마 금지
  - Material ID: 최대 50자, 콤마 금지
- **동작**: 내부 변수(`string _palletId`, `string _materialId`)에 저장만. 검증/판정 X
- **수신 시점**: `$TEST` 직전에 도착 → 다음 검사 사이클 로그/이력에 사용
- **값 유지 정책 결정 필요**: 다음 $TRACE 수신 전까지 이전 값 유지? 검사 1회 후 클리어?

---

## 합의·확인 필요 (작업 착수 전)
1. **2번 항목**: $LIGHT 처리 방식 A vs B (시퀀스 변경 여부)
2. **5번 항목**: $TRACE 값 유지/클리어 정책
3. **공통**: 모든 ID 필드 콤마 금지 규칙 — Client 측에서 보장 필요
