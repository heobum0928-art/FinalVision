# FinalVision 작업 일지

---

## 2026-04-21 (화)

### 완료
- **Phase 17 빌드 검증** — MSBuild (VS2022 Community) 통과, `FinalVision.exe` 정상 생성
  - 빌드 중 `WPF_Example/Utility/Logging.cs:288` 오타 `}1` 발견 후 수정
  - 주의: `dotnet build`는 WPF XAML 생성 실패하므로 사용 금지, 반드시 MSBuild 사용
- **`CLAUDE.md` 코드 컨벤션 문서화** — 헝가리언 표기/조건식/주석/함수/상수 규칙 + AI 리팩토링 기본 프롬프트
- **Btn_start_Click 리팩토링 커밋** (이전 작업 정리)
  - `InspectionListView.xaml.cs`: 중첩 분기를 4개 헬퍼로 분리 (`TryRunSimulInspection` / `RunLiveSequence` / `FindActionIndex` / `HasSimulImage`) — early return + 역할 분리
  - `Action_Inspection.cs`: 속성 주석 마커 정리
- **RESET OK 뱃지 UI 구현** (Phase 17 연장)
  - `Custom/SystemHandler.cs`: `ResetReceived` 이벤트 추가, `ProcessReset()`에서 OK일 때만 Invoke (NG는 에러 로그로 충분)
  - `UI/MenuBar.xaml`: `ResetBadgeStoryboard` 추가(0→1 즉시 노출, 2초 유지, 0.3초 페이드아웃) + 파란 `reset_Badge` Border를 ALIVE 인디케이터 옆에 배치
  - `UI/MenuBar.xaml.cs`: Loaded에서 구독/리소스 캐시, `OnResetReceived()` 핸들러로 storyboard 트리거, Unloaded에서 구독 해제
- **origin/master 푸시** — 5커밋 (`23cbc0b` / `bba2314` / `01c29d0` + 이전 2커밋)

### 다음 작업 후보
- 실제 운용 시 RESET 뱃지 동작 확인 (Handler 측 `$RESET:4@` 송신)
- Handler S/W측 RESET 송신 구현 연동 테스트

---

## 2026-04-20 (월)

### 완료
- **RESET 프로토콜 추가** — 시퀀스 꼬임 강제 복구용 TCP 명령
  - Request: `$RESET:site@`
  - Response: `$RESET:site,OK@` / `$RESET:site,NG@`
  - 동작: **시퀀스 중단 + 상태 READY 복귀 + 조명 OFF** 묶음
  - BUSY 중에도 허용 (강제 복구가 목적이므로 RECIPE와 달리 상태 제약 없음)
- 수정 파일
  - `WPF_Example/TcpServer/VisionRequestPacket.cs` — `VisionRequestType.Reset`, `CMD_RECV_RESET`, 파싱 로직, `ResetPacket` 클래스, `AsReset()` 추가
  - `WPF_Example/TcpServer/VisionResponsePacket.cs` — `EVisionResponseType.Reset`, `CMD_SEND_RESET`, Convert 로직, `ResetResultPacket` 클래스, `AsResetResult()` 추가
  - `WPF_Example/Sequence/SequenceHandler.cs` — `StopAll()` 메서드 추가 (Idle 제외 전체 Stop)
  - `WPF_Example/Device/LightController/LightHandler.cs` — `SetAllOff()` 메서드 추가 (Groups 전체 OFF)
  - `WPF_Example/Custom/SystemHandler.cs` — MainRun switch에 `Reset` case 추가, `ProcessReset()` 메서드 구현
  - `VisionProtocol_ECi_Moving_V1_0.md` — 프로토콜 문서에 3-5 RESET 섹션 추가
- 미빌드/미커밋

### 다음 작업 후보
- 빌드 검증
- Handler S/W측 RESET 송신 구현 연동 테스트

---

## 2026-04-14 (화)

### 완료
- ALIVE 하트비트 로직 단순화 (`Custom/SystemHandler.cs`)
  - 기존: V→PLC 송신 후 3초 내 응답 대기, 3회 재시도, 무응답 시 disconnect
  - 변경: V→PLC는 1초마다 그냥 송신(`$ALIVE:1@`), 응답 대기 안 함
  - PLC→V 패킷 마지막 수신시각만 추적(`_lastAliveRecvTimer` Stopwatch)
  - 5초간 PLC ALIVE 미수신 시 down 판정 → 기존 `PerformAliveTimeout()`(disconnect + 빨강 latch) 호출
  - 근거: PLC 메모리 비트(M82000/M83000) 1초 토글 스펙은 메모리 stale 구분용 → TCP는 패킷 도착 자체가 freshness 증거이므로 토글값을 페이로드에 실을 필요 없음
- 제거: `_aliveResponseReceived`, `ALIVE_TIMEOUT_MS`, `ALIVE_RETRY_COUNT`
- 추가: `ALIVE_DOWN_TIMEOUT_MS = 5000`
- 패킷 포맷(`$ALIVE:1@` / `$ALIVE:Site,OK@`) 변경 없음 → `VisionRequestPacket.cs`/`VisionResponsePacket.cs` 미수정
- 미빌드/미커밋

### 다음 작업 후보
- 빌드 검증
- PLC 측 ALIVE 송신 주기/포맷 실측 후 조정

---

## 2026-03-26 (목)

### 완료
- `Action_Inspection.cs` 신규 작성
  - `InspectionParam` : Shot별 ROI/Blob/Delay 파라미터
  - `Action_Inspection` : Grab, BlobDetect, SaveImage, End 상태머신
  - `SimpleBlobDetector` 기반 Blob 검출 (FilterByArea만 사용)
  - Shot별 이미지 버퍼 (`LastOriginalImage`, `LastAnnotatedImage`)
- `MainView.xaml.cs` Grab 흐름 연결
  - `GrabAndDisplay` : 조명 적용, Grab, 이미지 표시 순서로 실행
  - SIMUL_MODE: Grab 후 `RunBlobOnLastGrab` 자동 호출 (B방식)
- `InspectionListView.xaml.cs` SIMUL B방식
  - Grab 완료 후 다음 Action 자동 선택 (`AdvanceToNextAction`)

### 다음 작업 후보
- ROI 드래그 그리기 기능

---

## 2026-03-27 (금)

### 완료
- `ERoiShape { Rectangle, Circle }` enum 추가
- `InspectionParam.ROIShape`, `ROICircle` 프로퍼티 추가
- `RunBlobDetection` ROIShape 분기 구현
  - Circle 모드: 바운딩 박스 추출 + 원형 마스크 적용
  - ROI 미설정(0,0,0,0 또는 Radius=0)이면 즉시 NG 반환
  - GaussianBlur(5x5) + Threshold(Binary) 전처리
- `RuntimeResizer.cs` ROI 드래그 그리기
  - `SetParam`: ROIShape 기반 DrawableRect/Circle 필터링
  - `OnMouseUp`: Circle 모드 - 시작점=중심, 거리=반지름
  - `OnRender`: Circle 모드 - 점선 원 미리보기
- `MainView.xaml.cs` `canvas_main.IsEditable = value` 수정 (ROI 드래그 활성화)
- `ShotTabView.xaml` / `ShotTabView.xaml.cs` 신규 작성
  - Shot별 Grab 버튼, 원본/측정 전환, OK/NG 결과 레이블
  - 줌 슬라이더 + 마우스 휠 줌
- `MainView.xaml` Shot 1~5 탭 추가 (ShotTabView 인스턴스 5개)
- `MainView.xaml.cs` `UpdateShotStrip()` : Shot 탭 헤더 색상 갱신
- `.gitignore` 신규 생성 (bin/obj/.vs/packages 등)
- GitHub 최초 push (master)

### 다음 작업 후보
- 실제 테스트 (ROI 티칭, Grab, 결과 확인)
- Circle 모드 화면 표시 검증

---

## 2026-03-30 (월)

### 완료
- `Action_Inspection.cs` BlobDetect 개선
  - `SimpleBlobDetector`에서 `FindContours` + 면적 필터 방식으로 교체 (Halcon connection/select_shape 대응)
  - GaussianBlur(5x5) 전처리 명시적 추가
  - 디버그용 `ImWrite` 2줄 제거
- `MainView.xaml.cs` Shot 탭 자동 전환
  - InspectionListView에서 Shot 선택 시 해당 Shot 탭(1~5)으로 자동 이동 + 이미지 갱신
- `.gitignore` 정리
  - `ImageBoxEx_Test/`, `tcpip server&client sw/`, `Recipe/`, `결국` 추가

### 다음 작업 후보
- 실제 카메라 연결 테스트 (Grab, Blob 검출, OK/NG 판정)
- Circle ROI 화면 표시 검증
- TCP 수신 시 검사 결과 Shot 탭 반영 확인

---

## 2026-03-30 (월) — Phase 8

### 완료
- `Action_Inspection.cs` BlobDetect 개선
  - `SimpleBlobDetector`에서 `FindContours` + 면적 필터 방식으로 교체
  - GaussianBlur(5x5) 전처리 추가
  - 디버그 ImWrite 제거
- `MainView.xaml.cs` Shot 선택 시 해당 탭 자동 전환
- `.gitignore` 정리 (테스트 프로젝트/레시피/임시파일)
- `WORKLOG.md` 작업 일지 신규 작성 (소급 정리)
- **Phase 8 UI/파라미터 개선**
  - `BlobMinArea` 100에서 100000으로, `BlobMaxArea` 50000에서 9999999로, `BlobThreshold` 128에서 100으로 변경
  - `LastOriginalImage` / `LastAnnotatedImage` PropertyGrid 숨김 (`[Browsable(false)]`)
  - `ShotTabView` 슬라이더: MainView와 동일 레이아웃, 기본값 52%
  - ROI 편집: 로그인/Edit 모드에서만 활성화 (기본 비활성)
  - Shot 탭 선택 시 헤더 DodgerBlue 강조
  - InspectionListView Grab/Light/Copy 버튼 ToolTip 추가

### Phase 8 요구사항 정리 (Grab/Light/Copy 기능 설명)
- **Grab**: 선택된 Shot 카메라로 이미지 촬상 후 MainView 캔버스에 표시
- **Light**: 선택된 Shot의 조명 그룹 ON (레벨 적용)
- **Copy/Paste**: Shot 파라미터(ROI/Blob설정/딜레이 등)를 다른 Shot에 복사

### 다음 작업 후보
- 빌드 후 실제 동작 확인 (슬라이더 위치, 탭 색상, Edit 모드 ROI 보호)
- 실제 카메라 연결 테스트

---

## 2026-03-30 (월) — 버그 수정 5건

### 완료
- **Slider 크기**: 3컬럼 레이아웃으로 가로 절반 크기 축소
- **ROI Edit 버튼**: ToggleButton "Edit ROI" 추가
  - 로그인 시 버튼 활성, 눌러야 ROI 드래그 가능 (이중 잠금)
  - `IsEditable` 프로퍼티 = 전역 권한(로그인), `canvas_shot.IsEditable` = 실제 편집 상태 분리
- **Copy/Paste 미작동**: `InspectionParam.CopyTo` 오버라이드 추가
  - ROI, ROIShape, ROICircle, BlobMinArea/MaxArea/Threshold, DelayMs 모두 복사
- **ROI 테두리 비표시**: RuntimeResizer.OnRender에서 `IsEditable==false`일 때 조기 return 제거
  - ROI 테두리는 항상 표시, Picker 핸들만 Edit 모드 시 표시
- **줌 이중 적용**: ShotTabView에서 `canvas_shot.RenderTransform = _scale` 제거
  - 원인: Width(bgW×scale) + RenderTransform(scale) 이중 적용으로 52%가 27%처럼 보임
  - 수정: Width 조정만 사용, OnRender의 dc.PushTransform으로만 스케일링

### 미해결 (추후 확인 필요)
- PropertyGrid에서 ROIShape에 따라 ROI/ROICircle 중 하나만 표시 (ICustomTypeDescriptor 필요)
- ROI Circle 드래그 후 파라미터 적용 확인 (빌드 후 실 테스트 필요)

### 다음 작업 후보
- 빌드 + 실행 테스트 (줌, ROI 표시, Copy/Paste, Edit 버튼)
- 카메라 연결 테스트 (Grab, Circle/Rectangle Blob 검출)

---

## 2026-03-30 (월) — 버그 수정 3건 (슬라이더/ROIShape/Paste)

### 완료
- **슬라이더 52% 이미지 불일치**: `ShotTabView.xaml` ImageBrush `Stretch="None"`을 `"Fill"`로 변경
  - `Stretch="None"`은 이미지 원본 크기 고정으로 캔버스 Width(_bgW×scale)와 이미지 크기 불일치가 원인
  - `Fill`로 변경하면 캔버스 크기에 맞게 이미지 표시되어 슬라이더 값과 일치
- **ROIShape 즉시 반영 안됨**: PropertyGrid에서 Rectangle/Circle 변경 시 Grab 전까지 이전 도형 유지되던 문제
  - `InspectionParam.ROIShape` auto-property를 full property로 변경, `ROIShapeChanged` 이벤트 추가
  - `ShotTabView.RefreshImage()`에서 이벤트 구독하여 변경 즉시 `canvas_shot.SetParam` 재호출
- **Copy/Paste 후 강제 갱신**: Paste 후 `SelectionChanged`가 미발생할 경우를 대비해 `SetParam` 직접 호출 추가

### 다음 작업 후보
- 실행 테스트 (슬라이더 이미지 맞춤 확인, ROIShape 즉시 전환 확인, Paste 후 ROI 갱신 확인)
- 카메라 연결 테스트

---

## 2026-03-30 (월) — 시퀀스/알고리즘/통신 로그 추가

### 완료
- **TCP 통신 로그** (`ELogType.TcpConnection`)
  - `VisionServer.cs` - `PerformOnRecvMessage`, `PerformOnSendMessage`, `PerformOnAlarm` 로그 추가
    - `[TCP][RECV]`, `[TCP][SEND]`, `[TCP][AlarmType]` 포맷
  - `Custom/SystemHandler.cs` - `MainRun()` 패킷 수신/검사 결과 송신 로그 추가
    - 수신: `[TCP][RECV] From:{IP} Type:{RequestType}`
    - 송신(TEST 결과): `[TCP][SEND] TEST Result To:{IP} Site:{n} Type:{n} Result:{OK/NG}`
- **시퀀스 로그** (`ELogType.Trace`)
  - `SequenceBase.cs` - `Start/Stop/Finish/Error/Pause/Resume` 상태 전환마다 로그
    - `[SEQ] {SeqName} Start, Action:{ActionName}`
    - `[SEQ] {SeqName} Finish, Result:{Pass/Fail}`
    - `[SEQ] {SeqName} Error, Action:{ActionName}`
    - `[SEQ] {SeqName} Stop/Paused/Resumed`
- **알고리즘 로그** (`ELogType.Trace`)
  - `Action_Inspection.cs` - `Run()` 각 Step 결과 로그
    - `[ALGO] {ActionName} Grab OK/실패 NG`
    - `[ALGO] {ActionName} BlobDetect Result:OK/NG`
    - `[ALGO] {ActionName} End, Result:Pass/Fail`

### 로그 저장 경로
- Trace: `{AppBase}\Trace\{날짜}_Trace.log` (시퀀스/알고리즘 공용)
- TcpConnection: `{AppBase}\TcpConnection\{날짜}_TcpConnection.log`

---

## 2026-03-30 (월) — 버그 수정 2건 (ROIShape Edit보호 / Grab 후 52%)

### 완료
- **ROIShape 변경 Edit 보호**: PropertyGrid에서 ROIShape 변경 시 Edit ROI 버튼 활성 상태에서만 허용
  - `_lastAppliedROIShape` 필드: 마지막으로 canvas에 적용된 shape 기록
  - `_revertingROIShape` 플래그: 되돌리기 중 재진입(이벤트 루프) 차단
  - Edit 비활성 시 `OnParamROIShapeChanged`에서 `_lastAppliedROIShape`로 자동 복원
- **Grab 후 52% 기본 적용**: `DisplayToBackground`에서 `slider_scale.Value / 100.0`으로 `_scale` 강제 적용
  - 원인: `new ScaleTransform(1,1)` 초기화 후 Loaded 핸들러 없어 scale = 1.0 유지
  - 수정: Grab 시마다 슬라이더 현재값(기본 52%)을 `_scale`에 반영

### 다음 작업 후보
- 실행 테스트 확인
- 카메라 연결 테스트

---

## 2026-03-30 (월) — Phase 9 통신 테스트 + 버그 수정 5건

### Phase 9 통신 시나리오 확정
- TCP 포트 7701, CommunicationTest 프로그램으로 FinalVision 서버에 접속
- 프로토콜: `$TEST:1,1,null@` ~ `$TEST:1,5,null@` (Shot 1~5)
- 응답: `$RESULT:1,1,P@` or `$RESULT:1,1,F@`
- 레시피 경로: `D:\Data\Recipe\Site1\{레시피명}\main.ini`

### 완료
- **RecipeFileHelper cross-thread 예외**: `CollectRecipe()` / `CollectRecipe(int siteNumber)`
  - SystemProcess 스레드에서 ObservableCollection 직접 수정시 NotSupportedException 발생
  - 파일/폴더 탐색은 백그라운드에서, List 수정은 `Dispatcher.Invoke`로 UI 스레드에서 처리
- **UI Save 경로 불일치**: `MainWindow.xaml.cs`
  - `SaveRecipe(name)`을 `SaveRecipe(1, name)`으로 변경
  - Site1 경로(`D:\Data\Recipe\Site1\{name}\main.ini`)로 저장하도록 수정
- **LightHandler 그룹명**: `LIGHT_DEFAULT`를 `"Corner_Align"`에서 `"Final_Inspection"`으로 변경
- **TCP 검사 후 ShotTabView UI 미갱신**: `ShotTabView.Initialize()`
  - `seq.OnFinish` 구독 추가하여 검사 완료 시 `RefreshImage()` + `UpdateResultLabel()` 자동 호출
- **TCP 검사 후 MainView 이미지 미갱신**: `MainView.xaml.cs`
  - OnFinish 핸들러 추가, `param.GetAnnotatedImageTemp()` 우선 사용 (SIMUL_MODE 대응)
  - `canvas_main.SetParam((ParamBase)param)` 명시적 캐스트로 오버로드 모호성 해소

### 다음 작업 후보
- 빌드 + 통신 테스트 재실행 (MainView 이미지 갱신 확인)
- 시퀀스/알고리즘/통신 로그 추가 (기존 로그 폴더 활용)
- ROICircle/ROIRectangle 동시 표시 현상 확인

---

## 2026-03-31 (화) — 미커밋분 정리 + 버그 수정 2건

### 이전 세션(260331) 미커밋 작업 확인
- `SimulImagePath` Shot별 개별 이미지 로드 (InspectionParam 프로퍼티 + ShotTabView 열기/삭제 버튼)
- `LastBlobArea` 최근 Blob 면적 저장, OK/NG 옆 면적 표시
- `RuntimeResizer.SetDisplayMat` — 마우스 위치에서 X/Y/Gray 값 표시
- `SystemHandler.LoadRecipe(siteNumber, name)` 오버로드 추가
- `MilManager.cs`, `AlligatorAlgMil/` 레거시 삭제
- `SimulImagePath` 전역 방식 제거 (Shot별 개별 로드로 대체)
- ShotTabView 슬라이더 고정폭(120px), X/Y/Gray 표시 라벨 추가
- 주석 정리 (화살표 제거 등)

### 버그 수정 2건

#### 1. Auto 검사 시 MainView에서 이전 ROI 표시됨
- **원인**: MainView OnFinish 핸들러에서 `inspSeq.CurrentActionIndex`를 UI 스레드(BeginInvoke)에서 읽음
  - 다음 Shot이 이미 시작되면 인덱스가 변경되어 다른 Shot의 ROI/이미지 참조
  - `DisplaySequenceContext` → `SetContext`가 ROIShape 필터링 없이 Rect+Circle 모두 표시
- **수정**:
  - `MainView.xaml.cs` OnFinish: `ctx.ActionParam` 직접 참조 (sequence thread에서 캡처, 인덱스 조회 제거)
  - `RuntimeResizer.SetContext`: `SetParam`과 동일한 ROIShape 필터링 로직 추가

#### 2. Auto Shot별 측정이미지만 나오고 원본이미지 안나옴
- **원인**: `SequenceContext.Clear()`에서 `ResultImage`를 초기화하지 않음
  - 이전 실행의 stale ResultImage가 남아 `DisplaySequenceContext`에서 잘못된 이미지 표시
  - OnFinish 핸들러에서 `ctx.ResultImage` 우선 사용 → stale 이미지가 원본 대신 표시됨
- **수정**:
  - `SequenceContext.Clear()`: `ResultImage = null` 추가
  - MainView OnFinish: `ctx.ResultImage` 대신 `param.LastAnnotatedImage` 우선 사용

### 수정 파일
- `WPF_Example/UI/ContentItem/MainView.xaml.cs` — OnFinish 핸들러 ctx.ActionParam 직접 참조
- `WPF_Example/UI/ContentItem/RuntimeResizer/RuntimeResizer.cs` — SetContext ROIShape 필터링
- `WPF_Example/Sequence/Sequence/SequenceContext.cs` — Clear() ResultImage null 초기화

### 다음 작업 후보 (260401)
- 빌드 후 Auto 검사 테스트 (ROI 정상 표시, 원본/측정 전환 확인)
- 미커밋 변경사항 커밋 (260330~260331 전체)
- Phase 6 잔여: 06-02 RecipeEditorWindow, 06-03 OpenRecipeWindow
- Phase 7: 메인 검사 UI

---

## 2026-04-03 (목)

### Phase 10 완료 + 버그 수정

#### Phase 10 실행 (레시피 복사 + 택타임 로그)
- Plan 01/02 이전 세션에서 이미 실행 완료, VERIFICATION Passed (7/7)
- ROADMAP/STATE/PROJECT.md 완료 처리 + 커밋

#### 레시피 경로 구조 변경 — Site 하위 경로 제거 (D-05, D-06, D-10, D-11)
- `RecipeFileHelper.Copy()` — siteNumber 파라미터 제거, RecipeSavePath 루트 기준 복사
- `RecipeFileHelper.GetRecipeFilePath(int, string)` — `"Site" + siteNumber` → `siteNumber.ToString()` (TCP용)
- `RecipeFileHelper.CollectRecipe(int)` — `"Site" + siteNumber` → `siteNumber.ToString()` (TCP용)
- `OpenRecipeWindow` — Copy/Delete 후 `CollectRecipe()` 파라미터 없는 버전 호출
- `InspectionListView` — Copy 후 `CollectRecipe()` 파라미터 없는 버전 호출
- `SystemHandler.cs` 초기화 — `CollectRecipe()` 루트 기준
- `MainWindow.xaml.cs` — `LoadRecipe(1, name)` → `LoadRecipe(name)`, `SaveRecipe(1, name)` → `SaveRecipe(name)`

#### 시뮬모드 Grab 크래시 수정
- **원인**: 시뮬모드에서 TCP $TEST 수신 시 `_Camera.GrabImage()` null 반환 → Grab 실패 NG
  - 기존 로드된 이미지(`LastOriginalImage`)를 참조하면 `SetOriginalImage`에서 같은 객체 Dispose 후 Clone 시도 → 크래시
  - TCP/UI 스레드 동시 접근 시 동기화 없음
- **수정**:
  - `InspectionParam._imageLock` 추가 — SetOriginalImage thread safety
  - `InspectionParam.GetOriginalImageClone()` — 시뮬모드용 안전한 Clone 반환
  - `Action_Inspection.Run()` Grab 스텝 `#if SIMUL_MODE` 분기:
    - 시뮬모드: `GetOriginalImageClone()` 로드된 이미지 재사용, SetOriginalImage 미호출 (원본 보존)
    - 실제모드: `_Camera.GrabImage()` + `SetOriginalImage` (기존 동작)

### 수정 파일
- `WPF_Example/Utility/RecipeFileHelper.cs` — Copy() siteNumber 제거, TCP경로 Site→숫자
- `WPF_Example/UI/Recipe/OpenRecipeWindow.xaml.cs` — CollectRecipe() 루트 기준
- `WPF_Example/UI/ControlItem/InspectionListView.xaml.cs` — Copy/CollectRecipe 루트 기준
- `WPF_Example/SystemHandler.cs` — 초기화 CollectRecipe() 루트 기준
- `WPF_Example/MainWindow.xaml.cs` — LoadRecipe/SaveRecipe siteNumber 제거
- `WPF_Example/Custom/SystemHandler.cs` — TCP CollectRecipe 주석 갱신
- `WPF_Example/Custom/Sequence/Inspection/Action_Inspection.cs` — 시뮬모드 Grab + thread safety
