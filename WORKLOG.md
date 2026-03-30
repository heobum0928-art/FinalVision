# FinalVision 작업 일지

---

## 2026-03-26 (목)

### 완료
- `Action_Inspection.cs` 신규 작성
  - `InspectionParam` : Shot별 ROI/Blob/Delay 파라미터
  - `Action_Inspection` : Grab → BlobDetect → SaveImage → End 상태머신
  - `SimpleBlobDetector` 기반 Blob 검출 (FilterByArea만 사용)
  - Shot별 이미지 버퍼 (`LastOriginalImage`, `LastAnnotatedImage`)
- `MainView.xaml.cs` Grab 흐름 연결
  - `GrabAndDisplay` : 조명 적용 → Grab → 이미지 표시
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
  - ROI 미설정(0,0,0,0 또는 Radius=0) → 즉시 NG 반환
  - GaussianBlur(5x5) + Threshold(Binary) 전처리
- `RuntimeResizer.cs` ROI 드래그 그리기
  - `SetParam`: ROIShape 기반 DrawableRect/Circle 필터링
  - `OnMouseUp`: Circle 모드 — 시작점=중심, 거리=반지름
  - `OnRender`: Circle 모드 — 점선 원 미리보기
- `MainView.xaml.cs` `canvas_main.IsEditable = value` 수정 (ROI 드래그 활성화)
- `ShotTabView.xaml` / `ShotTabView.xaml.cs` 신규 작성
  - Shot별 Grab 버튼, 원본/측정 전환, OK/NG 결과 레이블
  - 줌 슬라이더 + 마우스 휠 줌
- `MainView.xaml` Shot 1~5 탭 추가 (ShotTabView 인스턴스 5개)
- `MainView.xaml.cs` `UpdateShotStrip()` : Shot 탭 헤더 색상 갱신
- `.gitignore` 신규 생성 (bin/obj/.vs/packages 등)
- GitHub 최초 push (master)

### 다음 작업 후보
- 실제 테스트 (ROI 티칭 → Grab → 결과 확인)
- Circle 모드 화면 표시 검증

---

## 2026-03-30 (월)

### 완료
- `Action_Inspection.cs` BlobDetect 개선
  - `SimpleBlobDetector` → `FindContours` + 면적 필터 (Halcon connection/select_shape 대응)
  - GaussianBlur(5x5) 전처리 명시적 추가
  - 디버그용 `ImWrite` 2줄 제거
- `MainView.xaml.cs` Shot 탭 자동 전환
  - InspectionListView에서 Shot 선택 시 해당 Shot 탭(1~5)으로 자동 이동 + 이미지 갱신
- `.gitignore` 정리
  - `ImageBoxEx_Test/`, `tcpip server&client sw/`, `Recipe/`, `결국` 추가

### 다음 작업 후보
- 실제 카메라 연결 테스트 (Grab → Blob 검출 → OK/NG 판정)
- Circle ROI 화면 표시 검증
- TCP 수신 시 검사 결과 Shot 탭 반영 확인

---

## 2026-03-30 (월) — Phase 8

### 완료
- `Action_Inspection.cs` BlobDetect 개선
  - `SimpleBlobDetector` → `FindContours` + 면적 필터 방식으로 교체
  - GaussianBlur(5x5) 전처리 추가
  - 디버그 ImWrite 제거
- `MainView.xaml.cs` Shot 선택 시 해당 탭 자동 전환
- `.gitignore` 정리 (테스트 프로젝트/레시피/임시파일)
- `WORKLOG.md` 작업 일지 신규 작성 (소급 정리)
- **Phase 8 UI/파라미터 개선**
  - `BlobMinArea` 100→100000, `BlobMaxArea` 50000→9999999, `BlobThreshold` 128→100
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
  - 원인: Width(bgW×scale) + RenderTransform(scale) 이중 적용 → 52%가 27%처럼 보임
  - 수정: Width 조정만 사용, OnRender의 dc.PushTransform으로만 스케일링

### 미해결 (추후 확인 필요)
- PropertyGrid에서 ROIShape에 따라 ROI/ROICircle 중 하나만 표시 (ICustomTypeDescriptor 필요)
- ROI Circle 드래그 후 파라미터 적용 확인 (빌드 후 실 테스트 필요)

### 다음 작업 후보
- 빌드 + 실행 테스트 (줌, ROI 표시, Copy/Paste, Edit 버튼)
- 카메라 연결 테스트 (Grab → Circle/Rectangle Blob 검출)

---

## 2026-03-30 (월) — 버그 수정 3건 (슬라이더/ROIShape/Paste)

### 완료
- **슬라이더 52% 이미지 불일치**: `ShotTabView.xaml` ImageBrush `Stretch="None"` → `"Fill"`
  - `Stretch="None"`은 이미지 원본 크기 고정 → 캔버스 Width(_bgW×scale)와 이미지 크기 불일치 원인
  - `Fill`로 변경 시 캔버스 크기에 맞게 이미지 표시 → 슬라이더 값과 일치
- **ROIShape 즉시 반영 안됨**: PropertyGrid에서 Rectangle↔Circle 변경 시 Grab 전까지 이전 도형 유지되던 문제
  - `InspectionParam.ROIShape` auto-property → full property + `ROIShapeChanged` 이벤트 추가
  - `ShotTabView.RefreshImage()`에서 이벤트 구독 → 변경 즉시 `canvas_shot.SetParam` 재호출
- **Copy/Paste 후 강제 갱신**: Paste 후 `SelectionChanged`가 미발생할 경우를 대비해 `SetParam` 직접 호출 추가

### 다음 작업 후보
- 실행 테스트 (슬라이더 이미지 맞춤 확인, ROIShape 즉시 전환 확인, Paste 후 ROI 갱신 확인)
- 카메라 연결 테스트

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
