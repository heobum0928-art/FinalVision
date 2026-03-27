# FinalVision — Requirements

## REQ-001: 프로젝트 리팩토링
- [ ] 솔루션/프로젝트명 `ECi_Dispenser` → `FinalVision` 으로 변경
- [ ] 네임스페이스 `ReringProject` → `FinalVisionProject` 전체 교체
- [ ] Basler 카메라 관련 코드 제거
- [ ] ATRAS/Project.HWC MX Component(PLC) 관련 코드 제거
- [ ] 불필요한 Corner Align 시퀀스 코드 정리

## REQ-002: HIK 카메라 단일화
- [ ] HIK 카메라 1대 연결 및 초기화
- [ ] 카메라 설정 (노출, 게인, 해상도) — 레시피 연동
- [ ] Grab 이벤트 → OpenCvSharp Mat 변환 유지
- [ ] 가상 카메라(VirtualCamera) 시뮬레이션 모드 유지

## REQ-003: 5-Shot 검사 시퀀스
- [ ] 검사 시작 명령 수신 시 Shot 1→5 순차 실행
- [ ] 각 Shot에서 촬상 → Blob 검사 → 결과 저장
- [ ] Shot 위치 정보 레시피화 (ROI, 딜레이 등)
- [ ] 검사 완료 후 전체 결과 집계 (5개 Shot 중 NG 존재 시 전체 NG)

## REQ-004: OpenCV Blob Detection 검사 알고리즘
- [ ] OpenCvSharp SimpleBlobDetector 구현
- [ ] 자재유무 판정 로직: Blob 면적/수 임계값 기반 OK/NG
- [ ] Blob 파라미터 레시피 저장/로드 (MinArea, MaxArea, MinCircularity 등)
- [ ] 검사 결과 이미지 저장 (OK/NG 폴더 분류)
- [ ] 검사 결과 로그 기록

## REQ-005: 5개 운영 Site 분리
- [ ] Site 1~5 독립 운영 (각 Site별 레시피, 결과)
- [ ] Site별 검사 명령 수신 처리
- [ ] Site별 현재 레시피 표시 및 변경
- [ ] Site별 통계 (검사수, OK수, NG수, 수율)

## REQ-006: TCP/IP 통신
- [ ] 기존 VisionServer 구조 유지 (STX/ETX 프레임)
- [ ] 검사 명령 패킷: `$TEST:Site,TestType,ID@`
- [ ] 검사 결과 응답: Shot별 OK/NG + 전체 판정
- [ ] 레시피 변경/조회 명령 유지
- [ ] 연결 상태 모니터링 (재연결 자동 처리)

## REQ-007: UI 개선
- [ ] 메인 화면: 5개 Shot 이미지 동시 표시
- [ ] Site 선택 탭 또는 패널 (Site 1~5)
- [ ] 실시간 검사 결과 표시 (OK=녹색, NG=빨간색)
- [ ] Blob 결과 오버레이 표시 (검출된 Blob 위치/크기 표시)
- [ ] 통계 대시보드 (Site별 수율)
- [ ] 레시피 편집 UI (Blob 파라미터 조정)

## REQ-008: 레시피 관리
- [ ] Site별 독립 레시피 파일 (JSON 또는 XML)
- [ ] 레시피 항목: 카메라 파라미터 + Blob 검사 파라미터 + ROI
- [ ] 레시피 생성/수정/삭제/복사
- [ ] TCP 명령으로 레시피 전환 지원
