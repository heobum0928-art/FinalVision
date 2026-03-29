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
