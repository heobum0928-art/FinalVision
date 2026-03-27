---
gsd_state_version: 1.0
milestone: v1.0
milestone_name: milestone
status: Executing Phase 06
last_updated: "2026-03-27T07:17:00.000Z"
progress:
  total_phases: 7
  completed_phases: 3
  total_plans: 11
  completed_plans: 9
---

# FinalVision — Project State

## 현재 상태

- **단계**: Phase 06 진행 중 (06-01 완료)
- **마지막 업데이트**: 2026-03-27
- **현재 위치**: Phase 06, Plan 01 — 완료

## 완료된 Phase

- ✅ Phase 1: 프로젝트 리팩토링 (2026-03-25)
  - FinalVision.sln / FinalVision.csproj 생성
  - namespace ReringProject → FinalVisionProject 전체 교체
  - Basler 카메라 코드/참조 완전 제거
  - AssemblyTitle, ProjectName → "FinalVision"
  - 빌드 성공 확인

## 완료된 Plans (Phase 3)

- ✅ 03-A: InspectionParam 이미지 버퍼 + Blob 오버레이 (2026-03-26)
  - InspectionParam에 LastOriginalImage/LastAnnotatedImage/이미지 버퍼 메서드 추가
  - RunBlobDetection → (bool isOk, Mat annotated) tuple 반환으로 변경
  - EStep.Grab에서 SetOriginalImage 호출
  - EStep.BlobDetect에서 #if SIMUL_MODE 분기 (SetAnnotatedImageTemp vs SetAnnotatedImage)
  - Cv2.Circle(OK=초록/NG=빨강) + Cv2.Rectangle(ROI=노랑) 오버레이 생성
- ✅ 03-B: RuntimeResizer 마우스 드래그 신규 ROI 생성 (2026-03-26)
  - DrawableRectangle.UpdateRect(Rect) 메서드 추가 (최소 크기 보장, OriginalRect 교체, UpdatePicker)
  - RuntimeResizer: _isDrawingNew 플래그 + _drawStartPoint 필드 추가
  - OnMouseDown: IsEditable+좌클릭+DrawableList 조건 시 드래그 모드 진입
  - OnMouseMove: _isDrawingNew 시 InvalidateVisual만 호출 (스크롤 없음)
  - OnMouseUp: 드래그 범위 Rect 계산 → UpdateRect → CheckAvailable → SelectedItem 설정
  - OnRender: 노란 점선(DashStyle) 미리보기 사각형 렌더링
- ✅ 03-C: Shot 뷰어 UI (ComboBox + RadioButton + 결과 스트립) (2026-03-26)
  - MainView.xaml Grid Row 2(Auto) 추가
  - comboBox_shot(Shot1~5) + radioButton_original/annotated + stripBorder_Shot1~5 UniformGrid 삽입
  - SHOT_ACTION_NAMES / SHOT_DISPLAY_NAMES 정적 배열 추가
  - ComboBox_shot_SelectionChanged, RadioButton_imageMode_Checked, StripBorder_MouseLeftButtonDown 이벤트 핸들러 구현
  - RefreshShotViewer, GetInspectionParam, UpdateShotStrip 헬퍼 메서드 구현
  - GrabAndDisplay 완료 후 해당 Shot 자동 선택 + 결과 스트립 갱신
- ✅ 03-D: SIMUL 모드 B방식 자동 진행 (Grab → 다음 Action 자동 포커스) (2026-03-26)
  - GrabAndDisplay 시그니처: bool eventCall → Action onComplete = null (콜백 파라미터)
  - onComplete?.Invoke() 삽입: UpdateShotStrip() 이후, canvas_main.InvalidateVisual() 직후
  - button_grab_Click에 #if SIMUL_MODE / #else / #endif 분기 추가
  - AdvanceToNextAction() 메서드 추가 (#if SIMUL_MODE 블록, UI 스레드 직접 실행)
  - treeListBox_sequence.SelectedItem 레퍼런스 비교로 현재 노드 탐색
  - Shot 5 이후 nextIndex < 0 시 자동 진행 중단

## 완료된 Plans (Phase 4)

- ✅ 04-01: SiteManager / SiteContext / SiteStatistics 신규 생성 (2026-03-26)
  - Custom/Site/ 디렉터리 + SiteStatistics.cs / SiteContext.cs / SiteManager.cs 생성
  - SiteStatistics: lock(_lock) 스레드 안전 TotalCount/OkCount/NgCount/Yield, INotifyPropertyChanged
  - SiteContext: Queue<bool>(MAX_HISTORY=100) FIFO 이력, SiteStatistics 소유, INotifyPropertyChanged
  - SiteManager: 싱글톤 Handle, 5개 SiteContext 배열, SwitchSite(1-based 1~5 범위 검증)
  - FinalVision.csproj Compile 항목 3개 추가, SystemSetting.cs Category 모호 참조 수정
- ✅ 04-02: Recipe/Site1~5/ 디렉터리 구조, RecipeFileHelper Site 오버로드, SystemSetting.CurrentSiteIndex (2026-03-26)
  - Recipe/Site1~5/ 폴더 생성, Seoul_LED_MIL → Recipe/Site1/Seoul_LED_MIL/ 마이그레이션
  - Site2~5 Default/main.ini 생성
  - RecipeFiles.GetRecipeFilePath(int siteNumber, string name) 오버로드 추가
  - RecipeFiles.CollectRecipe(int siteNumber) 오버로드 추가 (Site별 독립 스캔)
  - SystemSetting.CurrentSiteIndex (int, 기본값 1) 추가 — Setting.ini 자동 저장/로드
  - 빌드 성공 (오류 0건)
- ✅ 04-03: SequenceHandler Site 오버로드 + ProcessRecipeChange Site 연동 (2026-03-26)
  - SequenceHandler에 using FinalVisionProject.Site 추가
  - LoadRecipe(int, string) + SaveRecipe(int, string) public 오버로드 추가 (기존 메서드 유지)
  - LoadFromIni(int, string) + SaveToIni(int, string) private 오버로드 추가
  - LoadFromIni(int, string): pSetting.CurrentRecipeName 전역 갱신 + SiteManager.Handle[siteNumber-1].CurrentRecipeName Site별 갱신
  - ProcessRecipeChange: CollectRecipe(siteNumber) → HasRecipe → Sequences.LoadRecipe(siteNumber, recipeName) 흐름으로 교체
  - 빌드 성공 (오류 0건)

## 완료된 Plans (Phase 6)

- ✅ 06-01: CameraSlaveParam/CameraParam 레거시 필드 제거 (2026-03-27)
  - PixelToUM_Offset, MotorXPos, MotorYPos, PartNo 프로퍼티 제거 (CameraSlaveParam, CameraParam)
  - ConvertPixelToMM 메서드 제거 (CameraSlaveParam)
  - CopyTo() 내 레거시 필드 대입 제거 (양쪽 파일 대칭 정리)
  - FrameWidth, FrameHeight, LightGroupName, LightLevel 유지
  - 빌드 성공 (오류 0건)

## 현재 진행 Phase

Phase 6 진행 중: UI 레거시 정리 및 RecipeEditorWindow PropertyGrid 개선

## 주요 결정 사항

- 카메라: HIK 전용 (Basler 제거)
- PLC: 사용 안 함 (TCP/IP 전용)
- 비전: OpenCvSharp SimpleBlobDetector
- 운영: 5개 Site × 5-Shot 구조
- 네임스페이스: ReringProject → FinalVisionProject
- [03-A] RunBlobDetection: bool → (bool isOk, Mat annotated) Value Tuple 반환
- [03-A] LastAnnotatedImage 잠금 플래그(_AnnotatedImageLocked)로 SIMUL 재검사 시 덮어쓰기 방지
- [03-A] SIMUL/비SIMUL 분기: #if SIMUL_MODE 컴파일 심볼 사용
- [03-B] ROI 드래그: _isDrawingNew 플래그 패턴, DrawableList.Find → foreach (LINQ 무의존)
- [03-B] 미리보기: DashStyle([4,4]) 노란 점선, OnRender 이미지 좌표계 직접 사용
- [03-C] Shot ComboBox 이벤트 억제: _suppressShotComboEvent 플래그 패턴
- [03-C] GrabAndDisplay Shot 자동 선택: Array.IndexOf(SHOT_ACTION_NAMES, param.ActionName)
- [03-C] UpdateShotStrip public 선언 — Plan D SIMUL 자동 실행 완료 시 외부 호출 대비
- [03-D] GrabAndDisplay onComplete 콜백: bool eventCall → Action onComplete = null (기존 호출부 호환)
- [03-D] AdvanceToNextAction: SelectedItem 레퍼런스 비교 + UI 스레드 직접 실행 (내부 Dispatcher 불필요)
- [03-D] Shot 5 자동 진행 중단: nextIndex < 0 early-return (마지막 Action에서 멈춤)
- [04-01] SiteManager.Handle[siteIndex] 0-based 인덱서, SwitchSite(siteNumber) 1-based 입력
- [04-01] SiteStatistics lock 범위: Add/Reset 카운터 변경만 보호, RaisePropertyChanged는 lock 외부
- [04-01] SiteContext._resultHistory: Phase 4 범위 UI-thread-only 접근 → lock 불필요
- [04-02] Site 오버로드 패턴: 기존 메서드 삭제 없이 int siteNumber 첫 파라미터 오버로드 추가
- [04-02] CollectRecipe(int) — List.Clear() 후 해당 Site 폴더만 스캔 (Site 간 데이터 혼합 방지)
- [04-02] CurrentSiteIndex: partial class CustomSetting.cs에 [Category("Inspection|Site")] 어노테이션
- [04-03] LoadFromIni(int, string): 전역 CurrentRecipeName + SiteManager Site별 CurrentRecipeName 동시 갱신 (기존 코드 호환)
- [04-03] ProcessRecipeChange: CollectRecipe(siteNumber) 먼저 호출하여 Site별 레시피 목록 최신화 후 HasRecipe 체크
- [04-03] ProcessRecipeChange: 기존 Setting.CurrentRecipeName 중복 조건 체크 제거 (로직 단순화)
- [06-01] 레거시 필드 제거: 주석 처리가 아닌 완전 삭제 — PropertyGrid 노출 방지
- [06-01] 삭제 위치에 //260327 hbk 코멘트 블록으로 변경 이유 기록

## 알려진 이슈

- 없음 (Phase 06-01 기준)

## 다음 액션

- Phase 06: 06-02 이후 플랜 계속 진행
- 마지막 세션: 2026-03-27 — 06-01-PLAN.md 완료
