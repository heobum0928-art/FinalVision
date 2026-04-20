---
phase: 17-tcp-reset-command
plan: 01
type: execute
status: complete
completed: 2026-04-20
files_modified:
  - WPF_Example/TcpServer/VisionRequestPacket.cs
  - WPF_Example/TcpServer/VisionResponsePacket.cs
  - WPF_Example/Sequence/SequenceHandler.cs
  - WPF_Example/Device/LightController/LightHandler.cs
  - WPF_Example/Custom/SystemHandler.cs
  - VisionProtocol_ECi_Moving_V1_0.md
  - WORKLOG.md
---

# Phase 17 — TCP RESET 명령 추가

## Goal
시퀀스 꼬임 / BUSY 고착 / 조명 켜진 채 Error 상태 등 운영 중 발생하는 비정상 상태를 TCP 명령 한 번으로 강제 복구할 수 있도록 RESET 프로토콜을 추가한다.

## Protocol
```
[Request]   $RESET:site@
[Response]  $RESET:site,OK@   → 리셋 완료
            $RESET:site,NG@   → 리셋 실패 (조명 OFF 중 일부 실패 등)
```

## 동작 (묶음 실행)
1. **시퀀스 중단** — `SequenceHandler.StopAll()`로 실행 중인 모든 시퀀스 Stop
2. **상태 READY 복귀** — Stop()이 내부적으로 Idle 상태 = READY로 복귀시킴 (별도 로직 불필요)
3. **조명 OFF** — `LightHandler.SetAllOff()`로 등록된 모든 LightGroup OFF

## 제약
- BUSY 중에도 허용 (강제 복구가 목적이므로 RECIPE와 달리 상태 체크 없음)
- site는 Request 값을 Response에 그대로 echo

## 구현
| 파일 | 변경 |
|------|------|
| VisionRequestPacket.cs | `VisionRequestType.Reset`, `CMD_RECV_RESET`, 파싱 case, `ResetPacket` 클래스, `AsReset()` 추가 |
| VisionResponsePacket.cs | `EVisionResponseType.Reset`, `CMD_SEND_RESET`, Convert case, `ResetResultPacket` 클래스, `AsResetResult()` 추가 |
| SequenceHandler.cs | `StopAll()` 메서드 — Idle 제외 전체 Stop() |
| LightHandler.cs | `SetAllOff()` 메서드 — Groups 전체 OFF, 한 그룹이라도 실패 시 false 반환 |
| Custom/SystemHandler.cs | MainRun switch에 Reset case, `ProcessReset()` 메서드 구현 |
| VisionProtocol_ECi_Moving_V1_0.md | 3-5 RESET 섹션 추가 |

## Verification
- [ ] 빌드 검증
- [ ] Handler 측 연동 테스트: `$RESET:1@` → `$RESET:1,OK@` 수신 확인
- [ ] BUSY 중 RESET → 시퀀스 중단 + READY 복귀 확인
- [ ] 조명 ON 상태에서 RESET → 전 그룹 OFF 확인
