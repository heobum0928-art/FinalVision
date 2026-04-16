---
date: "2026-04-16 17:30"
promoted: false
---

260416 작업 내역:
1. ALIVE 인디케이터 버그 수정 — TCP 미연결 시 회색 우선 판정, WPF 애니메이션 HoldEnd 해제 (_flashStoryboard.Stop)
2. TCP ERROR 패킷 시스템 신규 구현 — $ERROR:Site,ErrorCode@ 포맷
   - EVisionErrorCode: 1=카메라끊김, 2=Grab실패, 3=조명에러, 4=ALIVE타임아웃
   - 3회 반복 송신 (100ms 간격)
   - TCP 연결 시 초기화 실패 에러 자동 전송 (SendStartupErrors)
   - 시뮬모드 제외 (#if !SIMUL_MODE)
   - 감지 지점: PerformAliveTimeout, Action_Inspection Grab실패, Lights.OnError
