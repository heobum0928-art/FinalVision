# FinalVision — Roadmap

## Milestone 1: FinalVision 기반 구축 및 핵심 기능 구현

---

### Phase 1: 프로젝트 리팩토링 (이름/네임스페이스 변경)
**목표**: ECi_Dispenser → FinalVision 으로 프로젝트 완전 리네임 및 불필요 코드 제거

**작업 항목**:
- 솔루션 파일명 변경: `ECi_Dispenser.sln` → `FinalVision.sln`
- 프로젝트 파일명 변경: `ECi_Dispenser.csproj` → `FinalVision.csproj`
- 네임스페이스 전체 교체: `ReringProject` → `FinalVisionProject`
- AssemblyInfo, App.xaml 타이틀 변경
- Basler 카메라 파일 제거 (`Device/Camera/Basler/`)
- ATRAS/Project.HWC MX Component 참조 제거
- 기존 Corner Align 시퀀스 정리 (Blob 검사용으로 재구성 준비)

**완료 기준**: 빌드 성공, 앱 정상 실행, 불필요 코드 없음

---

### Phase 2: HIK 카메라 단일화 및 5-Shot 시퀀스 구조
**목표**: HIK 카메라 1대 기반 5포지션 촬상 시퀀스 구현

**작업 항목**:
- HIK 카메라 단일 인스턴스 구조 정리
- `ShotConfig` 모델: Shot 번호, 딜레이, ROI 정의
- `Sequence_Inspection` 클래스: Shot 1→5 순차 실행
- `Action_Grab`: 각 Shot에서 카메라 트리거 → Mat 획득
- 시뮬레이션 모드: 테스트 이미지로 동작 확인
- Shot별 이미지 임시 저장 (검사 전달용)

**완료 기준**: 5개 Shot 이미지 순차 획득 확인

---

### Phase 3: 티칭 UI + 시뮬레이션 자동 실행
**목표**: 작업자 티칭(ROI/Blob 파라미터 설정) + SIMUL B방식 5-Shot 자동 진행 구현

> 원 Phase 3 로드맵(Blob Detection 알고리즘)은 Phase 2에서 흡수 완료.
> Phase 3는 **티칭 UI + 시뮬레이션 자동 실행**으로 재정의됨.

**Plans: 4 plans**

Plans:
- [x] 03-A-PLAN.md — InspectionParam 이미지 버퍼 + Blob 오버레이 (Action_Inspection 수정) ✅ 2026-03-26
- [x] 03-B-PLAN.md — RuntimeResizer ROI 마우스 드래그 신규 생성 ✅ 2026-03-26
- [x] 03-C-PLAN.md — MainView Shot 뷰어 UI (ComboBox + 결과 스트립) ✅ 2026-03-26
- [x] 03-D-PLAN.md — SIMUL B방식 (InspectionListView Grab 자동 진행) ✅ 2026-03-26

**완료 기준**: SIMUL 모드에서 Grab 반복 클릭으로 Shot 1→5 자동 순환, 각 Shot 원본/측정 이미지 전환 확인, ROI 드래그 생성 동작

**상태**: ✅ Phase 3 완료 (2026-03-26) — 03-A ~ 03-D 전 Plans 완료

---

### Phase 4: 5개 Site 독립 운영 구조
**목표**: Site 1~5 각각 독립적인 레시피/결과/통계 관리

**작업 항목**:
- `SiteManager` 클래스: 5개 Site 인스턴스 관리
- `SiteContext`: Site별 현재 레시피, 최근 결과, 통계
- Site별 레시피 파일 구조 (`Recipe/Site1/`, ... `Recipe/Site5/`)
- `SiteStatistics`: 검사수, OK수, NG수, 수율 계산
- Site 전환 시 레시피 자동 로드
- 결과 이력 관리 (최근 N건)

**Plans: 3 plans**

Plans:
- [x] 04-01-PLAN.md — SiteManager / SiteContext / SiteStatistics 신규 클래스 생성 ✅ 2026-03-26
- [x] 04-02-PLAN.md — Recipe 디렉터리 구조 + RecipeFileHelper Site 오버로드 + SystemSetting.CurrentSiteIndex ✅ 2026-03-26
- [x] 04-03-PLAN.md — SequenceHandler Site 오버로드 + ProcessRecipeChange Site 연결 ✅ 2026-03-26

**완료 기준**: 5개 Site 독립 레시피 로드/저장, 통계 집계 정상 동작

**상태**: ✅ Phase 4 완료 (2026-03-26) — 04-01 ~ 04-03 전 Plans 완료

---

### Phase 5: TCP/IP 통신 재설계
**목표**: 새 검사 플로우(5-Shot + 5-Site)에 맞는 통신 패킷 구조 정의 및 구현

**작업 항목**:
- `VisionRequestPacket` 확장: `INSPECT` 명령 추가 (Site, ID)
- `VisionResponsePacket` 확장: Shot별 결과 + 전체 판정 응답
- 검사 흐름: `$INSPECT:Site,ID@` 수신 → 시퀀스 실행 → `$RESULT:Site,ID,PASS/FAIL,Shot1~5@` 응답
- 기존 RECIPE_CHANGE / RECIPE_GET / SITE_STATUS 명령 유지
- 통신 에러 처리 및 재연결 로직 강화
- TCP 통신 로그 강화

**완료 기준**: 외부 클라이언트(테스트 툴)와 검사 명령/결과 송수신 확인

---

### Phase 6: 레시피 관리 UI
**목표**: Site별 Blob 파라미터 및 카메라 파라미터 레시피 편집 UI

**Plans: 3 plans**

Plans:
- [x] 06-01-PLAN.md — CameraSlaveParam/CameraParam 레거시 필드 제거 (D-01)
- [ ] 06-02-PLAN.md — RecipeEditorWindow 신규 생성 (TabControl + PropertyGrid + RuntimeResizer Canvas)
- [ ] 06-03-PLAN.md — OpenRecipeWindow 편집 진입점 + Save/Load/Copy/Reset 완성

**완료 기준**: 레시피 생성→저장→로드→검사 적용 전 구간 동작 확인

---

### Phase 7: 메인 검사 UI 개선
**목표**: 5개 Shot 이미지 + Site별 실시간 결과 표시 UI

**작업 항목**:
- 메인 화면 레이아웃: 5개 이미지 패널 (Shot 1~5)
- 각 이미지 패널: Blob 오버레이 + OK/NG 배지
- Site 탭/셀렉터: Site 1~5 전환
- 실시간 통계 패널: 수율, 검사수, NG수
- 검사 진행 상태 표시 (현재 Shot 번호, 진행률)
- 결과 이력 리스트 (최근 검사 결과 테이블)
- NG 이미지 팝업 뷰어

**완료 기준**: 실제 검사 시나리오 UI 전 구간 동작 확인

---

## 개발 우선순위

```
Phase 1 → Phase 2 → Phase 3 → Phase 4 → Phase 5 → Phase 6 → Phase 7
(리팩)   (촬상)    (티칭/시뮬)  (운영구조) (통신)    (레시피UI) (메인UI)
```

## 예상 일정
| Phase | 내용 | 예상 소요 |
|-------|------|----------|
| 1 | 프로젝트 리팩토링 | 1일 |
| 2 | HIK 5-Shot 시퀀스 | 2일 |
| 3 | 티칭 UI + 시뮬레이션 | 1일 |
| 4 | 5-Site 운영 구조 | 1일 |
| 5 | TCP/IP 통신 재설계 | 1일 |
| 6 | 레시피 UI | 2일 |
| 7 | 메인 검사 UI | 2일 |
| **합계** | | **약 10일** |
