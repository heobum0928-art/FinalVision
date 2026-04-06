# Phase 12: Run/Grab 역할 분리 + 이미지 로드/삭제 - Context

**Gathered:** 2026-04-06
**Status:** Ready for planning

<domain>
## Phase Boundary

Grab과 Run 버튼 역할이 명확히 분리되고 저장된 이미지 폴더를 로드하거나 삭제할 수 있다.

- OPS-02: Grab 버튼은 카메라 촬상+검사, Run 버튼은 로드된 이미지로 검사 테스트로 역할 분리
- IMG-03: 시간 폴더 선택 시 Shot1~5 이미지를 일괄 로드하여 UI에 표시
- IMG-04: 날짜/시간 폴더 단위로 저장된 검사 이미지 삭제 가능

</domain>

<decisions>
## Implementation Decisions

### Grab/Run 버튼 역할 분리 (OPS-02)
- **D-01:** ShotTabView의 Shot별 Grab 버튼(btn_grab) 삭제 — XAML + 코드비하인드(Btn_Grab_Click) 모두 제거
- **D-02:** InspectionListView 우측 툴바의 기존 Grab 버튼(button_grab) 유지 — 카메라 촬상 전용
- **D-03:** InspectionListView 상단의 기존 RUN 버튼(btn_start)의 동작 변경 — SimulImagePath(또는 BackgroundImagePath)가 설정되어 있으면 로드된 이미지로 검사 실행, 없으면 기존처럼 카메라 시퀀스 실행
- **D-04:** RUN 버튼은 선택된 Shot만 RunBlobOnLastGrab 실행 (5-Shot 전체가 아닌 InspectionListView에서 현재 선택된 Shot)

### 이미지 폴더 일괄 로드 (IMG-03)
- **D-05:** VirtualCamera.BackgroundImagePath 재활용 — 시간폴더 선택 시 BackgroundImagePath를 설정하면 GrabImage()가 카메라 대신 파일에서 순차적으로 이미지 반환
- **D-06:** 기존 DeviceSelector.MenuItem_LoadImage_Click 패턴 재사용 — Ookii VistaFolderBrowserDialog로 폴더 선택 후 BackgroundImagePath 설정
- **D-07:** 폴더 로드 후 기존 RUN(시퀀스 Start)으로 5-Shot 순차 Grab 실행 — 카메라 대신 파일에서 읽어서 검사하는 기존 VirtualCamera 메커니즘 활용
- **D-08:** 폴더 로드 버튼 위치는 Claude 재량 (InspectionListView 툴바 또는 DeviceSelector 메뉴 확장)

### 이미지 폴더 삭제 (IMG-04)
- **D-09:** 삭제 UI를 SystemSetting 창에 '이미지 관리' 탭으로 추가
- **D-10:** 날짜 폴더(yyyyMMdd) 목록을 보여주고 체크박스로 선택 삭제 (시간폴더 펼침 없이 날짜 폴더 단위만)
- **D-11:** 삭제 전 CustomMessageBox.ShowConfirmation 확인 다이얼로그 필수
- **D-12:** Directory.Delete(path, recursive: true)로 삭제 — 기존 프로젝트 패턴 준수

### Claude's Discretion
- ShotTabView에서 Grab 제거 후 빈 공간 레이아웃 조정 방식
- 폴더 로드 버튼의 정확한 배치 위치
- 날짜 폴더 목록 UI 세부 디자인 (ListView vs DataGrid vs ListBox)
- BackgroundImageFileList 정렬 순서와 Shot1~5 파일명 매핑 검증 로직

</decisions>

<canonical_refs>
## Canonical References

**Downstream agents MUST read these before planning or implementing.**

### Grab/Run 버튼 (현재 구현)
- `WPF_Example/UI/ContentItem/ShotTabView.xaml` — btn_grab 버튼 정의 (라인 21~22, 제거 대상)
- `WPF_Example/UI/ContentItem/ShotTabView.xaml.cs` — Btn_Grab_Click (라인 136~176, 제거 대상)
- `WPF_Example/UI/ControlItem/InspectionListView.xaml` — button_grab (라인 184~190), btn_start/RUN (라인 153~158)
- `WPF_Example/UI/ControlItem/InspectionListView.xaml.cs` — button_grab_Click (라인 218~233), Btn_start_Click (라인 103~124)

### VirtualCamera 이미지 폴더 로드 (재활용 대상)
- `WPF_Example/Device/Camera/VirtualCamera.cs` — BackgroundImagePath (라인 142~164), BackgroundImageFileList (라인 165), GrabImage() 내 파일 로드 분기 (라인 222~233)
- `WPF_Example/UI/Device/DeviceSelector.xaml.cs` — MenuItem_LoadImage_Click (라인 269~295, Ookii FolderBrowserDialog 패턴)

### 이미지 저장 구조 (Phase 11)
- `WPF_Example/Utility/ImageFolderManager.cs` — GetOriginSavePath/GetCaptureSavePath 파일명 패턴
- `WPF_Example/Setting/SystemSetting.cs` — ImageSavePath (라인 60)

### SystemSetting 창 (삭제 탭 추가 대상)
- `WPF_Example/Setting/SystemSetting.cs` — 기존 설정 구조

### 검사 시퀀스
- `WPF_Example/Custom/Sequence/Inspection/Action_Inspection.cs` — RunBlobOnLastGrab (라인 291~301), Run() 상태머신

</canonical_refs>

<code_context>
## Existing Code Insights

### Reusable Assets
- `VirtualCamera.BackgroundImagePath` + `BackgroundImageFileList`: 폴더 → 이미지 목록 자동 수집 + GrabImage()에서 순차 반환. Phase 12 폴더 로드의 핵심 재활용 대상
- `Ookii.Dialogs.Wpf.VistaFolderBrowserDialog`: DeviceSelector에서 이미 사용 중. 폴더 선택 UI 패턴 그대로 재사용
- `CustomMessageBox.ShowConfirmation`: InspectionListView에서 이미 사용 중. 삭제 확인용 재사용
- `Action_Inspection.RunBlobOnLastGrab()`: 이미지 기반 즉시 Blob 검사. RUN 버튼의 "로드 이미지 검사" 경로에서 활용

### Established Patterns
- 폴더 선택: Ookii VistaFolderBrowserDialog + SelectedPath = SystemSetting.ImageSavePath
- 확인 다이얼로그: CustomMessageBox.ShowConfirmation + MessageBoxButton.YesNo
- 이미지 로드: VirtualCamera.BackgroundImagePath setter 내부에서 자동 파일 목록 수집

### Integration Points
- InspectionListView: RUN(btn_start) 버튼 동작 변경 지점
- ShotTabView: Grab 버튼 제거 지점
- SystemSetting: 이미지 관리 탭 추가 지점
- VirtualCamera: BackgroundImagePath 설정 → GrabImage() 연동

</code_context>

<specifics>
## Specific Ideas

- 사용자가 "기존 Run 버튼 이용"을 명시 — 새 버튼 추가 대신 기존 btn_start(RUN) 동작 변경
- 사용자가 "기존 directory in image 재활용" 명시 — VirtualCamera.BackgroundImagePath 패턴으로 폴더 로드 구현
- ShotTabView Shot별 Grab 버튼 삭제는 사용자가 직접 요청한 사항

</specifics>

<deferred>
## Deferred Ideas

None — discussion stayed within phase scope

</deferred>

---

*Phase: 12-run-grab*
*Context gathered: 2026-04-06*
