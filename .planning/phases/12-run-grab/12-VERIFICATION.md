---
phase: 12-run-grab
verified: 2026-04-06T07:00:00Z
status: passed
score: 4/4 must-haves verified
re_verification: false
human_verification:
  - test: "RUN 버튼 — BackgroundImagePath 분기 실행 확인"
    expected: "InspectionListView RUN 클릭 시 VirtualCamera.BackgroundImagePath 설정된 상태에서 시퀀스 Start가 실행되고 5-Shot 순차 파일 Grab이 이루어짐"
    why_human: "실제 Plan 12-01에서 BackgroundImagePath 분기는 코드 레벨에서 구현 제거됨 — 대신 InspectionListView 폴더 로드가 SimulImagePath를 사용. 앱 실행 시 동작 확인 필요."
---

# Phase 12: run-grab Verification Report

**Phase Goal:** Grab과 Run 버튼 역할이 명확히 분리되고 저장된 이미지 폴더를 로드하거나 삭제할 수 있다
**Verified:** 2026-04-06T07:00:00Z
**Status:** passed
**Re-verification:** No — initial verification

---

## Goal Achievement

### Observable Truths (Success Criteria)

| # | Truth | Status | Evidence |
|---|-------|--------|---------|
| 1 | Grab 버튼 클릭 시 카메라 촬상 후 검사가 실행된다 | VERIFIED | `InspectionListView.xaml.cs` `button_grab_Click` (line 305~322) 보존됨. `ShotTabView`의 btn_grab은 제거됨 — 역할이 `button_grab`(InspectionListView)으로 통합. 카메라 촬상 전용 동작 유지. |
| 2 | Run 버튼 클릭 시 이전에 로드된 이미지로 카메라 없이 검사 테스트가 실행된다 | VERIFIED | `Btn_start_Click` (line 107~159)에 SimulImagePath 분기 존재. SimulImagePath 설정 시 `act.RunBlobOnLastGrab()` 직접 호출하여 카메라 없이 검사 실행됨. |
| 3 | 시간 폴더를 선택하면 Shot1~5 이미지가 UI에 일괄 로드된다 | VERIFIED | `InspectionListView.xaml.cs` `Btn_LoadFolder_Click` (line 162~219)에 폴더 선택 → BMP 파일 Action 이름 매칭 → SimulImagePath 설정 → `RefreshAllShotImages()` 호출 존재. UAT Test 4 pass. |
| 4 | 날짜 또는 시간 폴더 단위로 저장된 이미지를 삭제할 수 있다 | VERIFIED | `ImageManageWindow.xaml.cs` `Btn_DeleteFolders_Click` (line 39~70)에 `Directory.Delete(item.FullPath, true)` + `CustomMessageBox.ShowConfirmation` 가드 존재. SettingWindow 버튼으로 진입. UAT Test 7, 8 pass. |

**Score:** 4/4 truths verified

---

### Required Artifacts

| Artifact | Expected | Status | Details |
|----------|----------|--------|---------|
| `WPF_Example/UI/ContentItem/ShotTabView.xaml` | btn_grab 제거된 XAML | VERIFIED | Line 20~33 StackPanel에 `btn_grab` 없음. 주석 `260406 hbk D-01` 존재. `btn_editRoi`가 첫 요소. |
| `WPF_Example/UI/ContentItem/ShotTabView.xaml.cs` | Btn_Grab_Click 핸들러 제거 | VERIFIED | `Btn_Grab_Click` 없음. `_grabTask` 없음. Line 137에 D-01 제거 주석. |
| `WPF_Example/UI/ControlItem/InspectionListView.xaml.cs` | Btn_start_Click 분기 로직 | VERIFIED | Line 107~159에 SimulImagePath 분기 + RunBlobOnLastGrab 호출. Line 162~219에 Btn_LoadFolder_Click. |
| `WPF_Example/UI/ContentItem/MainView.xaml.cs` | RefreshShotImage + RefreshAllShotImages | VERIFIED | Line 538~554에 두 메서드 모두 존재. 실제 ShotTabView 배열 참조하여 갱신. |
| `WPF_Example/Setting/SystemSetting.cs` | GetImageDateFolders() 메서드 | VERIFIED | Line 137~146에 메서드 존재. yyyyMMdd 8자리 숫자 폴더 필터링 + OrderByDescending. |
| `WPF_Example/UI/Setting/ImageManageWindow.xaml` | lb_dateFolders ListBox + 삭제 버튼 | VERIFIED | lb_dateFolders ListBox + CheckBox DataTemplate + btn_deleteFolders 모두 존재. |
| `WPF_Example/UI/Setting/ImageManageWindow.xaml.cs` | Btn_DeleteFolders_Click + Directory.Delete | VERIFIED | ShowConfirmation 가드 후 Directory.Delete(path, true). 목록 새로고침 포함. |
| `WPF_Example/UI/Setting/SettingWindow.xaml` | 이미지 관리 버튼 | VERIFIED | Line 80~81에 btn_imageManage 버튼 존재. |
| `WPF_Example/UI/Setting/SettingWindow.xaml.cs` | Btn_imageManage_Click | VERIFIED | Line 44~48에 ImageManageWindow ShowDialog() 호출. |
| `WPF_Example/UI/Device/DeviceSelector.xaml` | Load Image in directory 메뉴 항목 제거 | VERIFIED | Line 72 주석 `260406 hbk Load Image in directory, Next, Prev 삭제`. menu_streaming만 존재. |

---

### Key Link Verification

| From | To | Via | Status | Details |
|------|----|-----|--------|---------|
| InspectionListView Btn_start_Click | act.RunBlobOnLastGrab() | SimulImagePath 존재 시 직접 호출 | WIRED | Line 126 `act.RunBlobOnLastGrab()` 확인 |
| InspectionListView Btn_start_Click | mParentWindow.StartSequence() | BackgroundImagePath 분기 (Plan 01 원안) | PARTIAL | Plan 01 코드에서 BackgroundImagePath 분기가 최종 코드에서 제거됨. 대신 폴더 로드는 SimulImagePath 방식으로 구현됨. 기능 목표(성공 기준 2, 3)는 달성됨. |
| InspectionListView Btn_LoadFolder_Click | RefreshAllShotImages() | 폴더 로드 후 5개 탭 일괄 갱신 | WIRED | Line 214 `mParentWindow.mainView.RefreshAllShotImages()` 확인 |
| SettingWindow Btn_imageManage_Click | ImageManageWindow.ShowDialog() | 이미지 관리 창 진입 | WIRED | SettingWindow.xaml.cs line 45~47 확인 |
| ImageManageWindow Btn_DeleteFolders_Click | Directory.Delete(path, true) | ShowConfirmation 가드 후 삭제 | WIRED | ImageManageWindow.xaml.cs line 59 확인 |
| SystemSetting.GetImageDateFolders() | LoadDateFolders() | ImageManageWindow Loaded 이벤트 | WIRED | Window_Loaded → LoadDateFolders() → GetImageDateFolders() 체인 확인 |

---

### Architectural Deviation Note (Plan 02 vs Actual)

Plan 12-02는 `ShotTabView.xaml.cs`에 `btn_loadFolder`를 배치하고 `VirtualCamera.BackgroundImagePath`를 설정하는 방식을 명시했다. 그러나 실제 구현은 다음과 같이 다르게 작동한다:

- **폴더 로드 위치:** `InspectionListView.xaml.cs` `Btn_LoadFolder_Click` (`button_loadFolder` in `InspectionListView.xaml`)
- **설정 방식:** `BackgroundImagePath` 대신 각 Action의 `InspectionParam.SimulImagePath`를 파일명 매칭으로 개별 설정
- **GrabImage() 파일 기반 전환 없음:** VirtualCamera.BackgroundImagePath 미사용, 대신 RunBlobOnLastGrab()이 SimulImagePath의 이미지를 사용

이 구현 방식은 Plan 12-02 설계와 다르지만 **성공 기준 3** ("시간 폴더를 선택하면 Shot1~5 이미지가 UI에 일괄 로드된다")을 달성한다. UAT 4번, 5번 테스트에서 검증됨.

Plan 12-01의 BackgroundImagePath → StartSequence 분기는 최종 코드(`InspectionListView.xaml.cs` 실제 파일)에 존재하지 않는다. 대신 SimulImagePath 분기만 구현되어 있으며, 이것이 "로드된 이미지로 검사 테스트" 기능을 수행한다.

---

### Data-Flow Trace (Level 4)

| Artifact | Data Variable | Source | Produces Real Data | Status |
|----------|--------------|--------|-------------------|--------|
| InspectionListView (folder load) | `param.SimulImagePath` | `Directory.GetFiles(dlg.SelectedPath, "*.bmp")` + 파일 매칭 | 실제 파일시스템 읽기 | FLOWING |
| InspectionListView (RUN → RunBlobOnLastGrab) | `act.Param.SimulImagePath` | 위 폴더 로드 또는 Btn_OpenImage_Click에서 설정 | 실제 파일경로 | FLOWING |
| ImageManageWindow lb_dateFolders | `DateFolderItem` 컬렉션 | `SystemSetting.GetImageDateFolders()` → `Directory.GetDirectories` | 실제 디렉터리 목록 | FLOWING |
| ShotTabView RefreshImage | `param.LastOriginalImage` | `param.SetOriginalImage(mat)` ← `Cv2.ImRead(SimulImagePath)` | 실제 BMP 파일 | FLOWING |

---

### Behavioral Spot-Checks

Step 7b: SKIPPED — WPF 앱은 UI 스레드가 필요하므로 CLI에서 직접 실행 불가. UAT 10/10 pass로 대체 확인됨.

---

### Requirements Coverage

| Requirement | Source Plan | Description | Status | Evidence |
|-------------|------------|-------------|--------|---------|
| OPS-02 | 12-01 | Grab은 카메라 촬상 전용, Run은 로드된 이미지로 검사 테스트로 역할 분리 | SATISFIED | ShotTabView btn_grab 제거; InspectionListView button_grab=촬상, btn_start=시뮬 검사 |
| IMG-03 | 12-02 | 시간 폴더 선택 시 Shot1~5 이미지를 일괄 로드하여 UI에 표시 | SATISFIED | Btn_LoadFolder_Click → SimulImagePath 매칭 → RefreshAllShotImages() |
| IMG-04 | 12-03 | 날짜/시간 폴더 단위로 저장된 검사 이미지 삭제 가능 | SATISFIED | ImageManageWindow + ShowConfirmation + Directory.Delete(path, true) |

---

### Anti-Patterns Found

| File | Line | Pattern | Severity | Impact |
|------|------|---------|----------|--------|
| InspectionListView.xaml.cs | 259 | 주석 `ShotTabView 폴더 버튼 제거 → DeviceSelector "Load Image in Directory" 메뉴로 통합` | Info | Plan 12-02에서 ShotTabView에 배치 예정이었으나 InspectionListView로 이동됨. 주석 내용이 오해를 줄 수 있으나 기능 동작에 영향 없음. |

실질적인 stub, TODO, 미완성 코드는 발견되지 않았다.

---

### Human Verification Required

#### 1. RUN 버튼 BackgroundImagePath 분기 동작 확인

**Test:** 폴더 로드 없이 RUN 클릭 — BackgroundImagePath가 설정된 카메라에서 시퀀스 Start가 실행되는지 확인  
**Expected:** Plan 12-01 설계에서는 BackgroundImagePath 분기가 있었으나, 실제 코드에는 존재하지 않음. 대신 SimulImagePath 없는 상태에서 RUN 클릭 시 기존 카메라 시퀀스가 실행되는지 확인  
**Why human:** BackgroundImagePath 분기가 코드에서 제거되었으므로, "VirtualCamera.BackgroundImagePath를 직접 설정 후 RUN 클릭" 경로가 실제 런타임에서 어떻게 동작하는지 코드 정적 분석으로는 확인 불가. UAT에서 이 경로는 직접 테스트되지 않았음.

---

### Gaps Summary

None — 4가지 성공 기준 모두 코드 레벨에서 검증됨.

**구현 방식 차이 요약:**  
Plan 12-02는 `ShotTabView`에 `btn_loadFolder`를 두고 `BackgroundImagePath`를 설정하는 방식을 명시했다. 실제 구현은 `InspectionListView`에 `button_loadFolder`를 두고 각 Action의 `SimulImagePath`를 파일명 매칭으로 설정하는 방식을 채택했다. 이 차이는 **기능 목표를 그대로 달성**하며, UAT 10/10 통과로 실제 동작이 검증되었다. 설계 의도(IMG-03: 폴더 선택 → 5-Shot 이미지 일괄 로드 표시)가 충족된다.

---

_Verified: 2026-04-06T07:00:00Z_  
_Verifier: Claude (gsd-verifier)_
