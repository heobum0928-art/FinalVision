# Phase 2: HIK 카메라 단일화 및 5-Shot 시퀀스 구조 — Context

**Gathered:** 2026-03-26
**Status:** Ready for planning

<domain>
## Phase Boundary

HIK 카메라 1대로 5개 포지션을 순차 촬상하는 시퀀스 구조를 구현한다.
자재가 이동하면서 각 포지션에서 Software Trigger로 1Shot씩 촬상.
각 Shot은 TCP TEST 커맨드 1개에 대응 — Vision=Server(7701), Handler=Client.
이 Phase는 촬상 + Blob 검출 + TCP 응답까지 담당.

</domain>

<decisions>
## Implementation Decisions

### 주석 규칙
- 신규 추가 / 변경된 코드 라인에는 반드시 `//260326 hbk` 주석 추가
- 파일 상단 또는 메서드 단위로 변경 블록에 표시
- 기존 `//260325 hbk` 주석이 있는 라인 제거 시 그냥 삭제 (새 주석 불필요)

### 코드 정리 (최우선)
- `CameraSlaveParam.cs`의 `_isLoading` 플래그 및 관련 Load() 오버라이드 제거 (260325 hbk)
- `RecipeFileHelper.cs`의 Copy() 내 ini 경로 치환 코드 제거 (260325 hbk)
- 오늘 날짜(260326) 주석 추가

### ID 재정의 (`Custom/Define/ID.cs`)
```csharp
ESequence:
  Inspection = 1   // 기존 Corner_Align 대체

EAction:
  Bolt_One_Inspection   = 1   // 기존 LT_Inspection
  Bolt_Two_Inspection   = 2   // 기존 RT_Inspection
  Bolt_Three_Inspection = 3   // 기존 LB_Inspection
  Assy_Rail_One_Inspection = 4   // 기존 RB_Inspection
  Assy_Rail_Two_Inspection = 5   // 신규
  Unknown = Int32.MaxValue
```

### 시퀀스 클래스 구조
- 기존 `CornerAlignSequence` → `Sequence_Inspection`으로 rename
- 기존 `SequenceHandler.RegisterSequences()` / `RegisterActions()` / `InitializeSequences()` 구조 유지
- 새 파일 위치: `Custom/Sequence/Inspection/`
  - `Sequence_Inspection.cs`
  - `Action_Inspection.cs` (5개 Action 공통 클래스 또는 개별)

### TCP/IP 구조
- Vision = **Server** (TcpListener), Handler = **Client**
- Port: **7701**, IP: 192.168.0.1
- 메시지 포맷: `$<CMD>:<args>@` (시작=$, 종료=@)
- '@' delimiter까지 버퍼 누적 후 파싱 (부분 수신 대비)
- 커맨드: `SITE_STATUS`, `GET_RECIPE`, `RECIPE`, `TEST`, `RESULT`, `LIGHT`
- Site=1 (Final_Inspection), Type 1~5 = Shot 1~5
- `TEST:1,type,null@` 수신 → 해당 Shot 실행 → `RESULT:1,type,OK@` 또는 `NG@` 응답
- BUSY 중 TEST 요청 시 BUSY 응답 또는 무시
- 프로토콜 레퍼런스: `VisionProtocol_ECi_Moving_V1_0.md`

### Shot 파라미터 (각 Action별 독립)
- 5개 Action 각각 독립 파라미터 인스턴스
- `InspectionParam` (CameraSlaveParam 상속):
  - `ROI` (Rect) — 각 Shot별 다른 위치
  - `BlobMinArea` (double) — 자재 최소 면적
  - `BlobMaxArea` (double) — 자재 최대 면적
  - `DelayMs` (int) — Shot 전 안정화 대기 (기본값 0)
  - Exposure/Gain — CameraSlaveParam에 기존 있음
- Blob 판정: Area 범위만 사용 (자재 유무, 원형 판정 불필요)
- 볼트 1개: BlobCount == 1 → OK, 아니면 NG

### 이미지 저장
- OK/NG 각각 저장 여부 UI 토글 (bool SaveOK, bool SaveNG)
- 저장 경로: `D:\Log\{날짜}\{Shot명}_{OK|NG}_{시간}.jpg`
- InspectionParam 또는 별도 SystemSetting에 저장

### 디바이스
- 카메라: 1대, 이름 = `INSPECTION_CAMERA` (Custom/Device/DeviceHandler.cs)
- 조명: 동축(Coaxial) JL-C-260_230-CLW, 1대 2채널
- 조명 제어: Handler가 TCP LIGHT 커맨드로 제어 → Vision 내부에서 조명 ON/OFF 불필요
- LightHandler 기존 구조 유지 (RS232/485 미확인, 기존 코드 유지)

### Simulation Mode
- `SIMUL_MODE` 컴파일 상수 또는 런타임 플래그
- `VirtualCamera.GrabImage()` 사용
- 5개 Shot 공용 테스트 이미지 1장 사용
- 테스트 이미지 경로: InspectionParam 또는 SystemSetting에 설정

### 이미지 전달
- 각 Action에서 Grab 후 Blob 검출까지 자체 처리
- Shot별 독립 실행 (Context 누적 불필요)
- 결과(OK/NG)만 TCP로 반환

### Claude's Discretion
- TCP Server 구현 위치 (별도 TcpServer 클래스 or 기존 Network 폴더 활용)
- SimpleBlobDetector 파라미터 세부 (filterByArea만)
- InspectionParam의 Save/Load ini 그룹명
- SITE_STATUS BUSY 상태 관리 방식

</decisions>

<canonical_refs>
## Canonical References

**Downstream agents MUST read these before planning or implementing.**

### 기존 시퀀스 구조 (참조 필수)
- `WPF_Example/Custom/Sequence/Corner/Sequence_CornerAlign.cs` — SequenceContext/SequenceBase 패턴
- `WPF_Example/Custom/Sequence/Corner/Action_CornerAlign_Inspection.cs` — ActionBase/EStep 패턴
- `WPF_Example/Custom/Sequence/SequenceHandler.cs` — RegisterSequences/RegisterActions/InitializeSequences 패턴

### ID 정의
- `WPF_Example/Custom/Define/ID.cs` — ESequence, EAction enum (확장 대상)

### 카메라
- `WPF_Example/Device/Camera/Hik/HikCamera.cs` — GrabImage(), ExecuteSoftwareTrigger() 사용법
- `WPF_Example/Device/Camera/VirtualCamera.cs` — SIMUL_MODE 시뮬레이션 구조

### 디바이스 등록
- `WPF_Example/Custom/Device/DeviceHandler.cs` — RegisterRequiredDevices() (카메라 이름 변경 대상)

### 파라미터
- `WPF_Example/Sequence/Param/CameraSlaveParam.cs` — 상속 기반 파라미터 (260325 제거 대상)
- `WPF_Example/Utility/RecipeFileHelper.cs` — Copy() (260325 제거 대상)

### 프로토콜
- `VisionProtocol_ECi_Moving_V1_0.md` — TCP 메시지 포맷, 커맨드 레퍼런스

### 프로젝트 구조
- `.planning/ROADMAP.md` — Phase 2 작업 항목 및 완료 기준
- `.planning/REQUIREMENTS.md` — REQ-002, REQ-003 참조

</canonical_refs>

<specifics>
## Specific Ideas

- Action 명칭: `EAction.Bolt_One_Inspection` ~ `EAction.Assy_Rail_Two_Inspection`
- 시퀀스 명칭: `ESequence.Inspection`, 문자열 `"SEQ_INSPECTION"`
- TCP 파싱: `msg.TrimStart('$').TrimEnd('@')` → Split(':', 2) → Split(',')
- Blob 판정: `keypoints.Count == 1` → OK
- 이미지 저장: NG 시 필수, OK 시 UI 토글

</specifics>

<deferred>
## Deferred Ideas

- Shot별 다른 테스트 이미지 (SIMUL_MODE) — 추후 테스트 강화 시 고려
- 조명 RS232/485 채널 매핑 상세 — 현장 확인 후
- 결과 이미지 UI 표시 (Blob 위치 오버레이) — Phase 7 Main UI에서

</deferred>

---

*Phase: 02-hik-5-shot*
*Context gathered: 2026-03-26 via discuss-phase + conversation refinement*
