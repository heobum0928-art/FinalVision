# Roadmap: FinalVision

## Milestones

- ✅ **v1.0 기반 구축** - Phases 1-9 (shipped 2026-03-31)
- 🚧 **v2.0 레시피 편집 + 이미지 관리 + 운영 안정화** - Phases 10-13 (in progress)

## Phases

<details>
<summary>✅ v1.0 기반 구축 (Phases 1-9) - SHIPPED 2026-03-31</summary>

### Phase 1: 프로젝트 리팩토링 (이름/네임스페이스 변경)
**Goal**: ECi_Dispenser → FinalVision 으로 프로젝트 완전 리네임 및 불필요 코드 제거
**Plans**: Complete

Plans:
- [x] 솔루션/프로젝트 파일명 변경
- [x] 네임스페이스 전체 교체: ReringProject → FinalVisionProject
- [x] Basler 카메라 파일 제거, ATRAS/MX Component 참조 제거

---

### Phase 2: HIK 카메라 단일화 및 5-Shot 시퀀스 구조
**Goal**: HIK 카메라 1대 기반 5포지션 촬상 시퀀스 구현
**Plans**: Complete

Plans:
- [x] HIK 카메라 단일 인스턴스 구조 정리
- [x] ShotConfig 모델, Sequence_Inspection 클래스, Action_Grab
- [x] 시뮬레이션 모드

---

### Phase 3: 티칭 UI + 시뮬레이션 자동 실행
**Goal**: 작업자 티칭(ROI/Blob 파라미터 설정) + SIMUL B방식 5-Shot 자동 진행 구현
**Plans**: 4 plans

Plans:
- [x] 03-A-PLAN.md — InspectionParam 이미지 버퍼 + Blob 오버레이 (Action_Inspection 수정) ✅ 2026-03-26
- [x] 03-B-PLAN.md — RuntimeResizer ROI 마우스 드래그 신규 생성 ✅ 2026-03-26
- [x] 03-C-PLAN.md — MainView Shot 뷰어 UI (ComboBox + 결과 스트립) ✅ 2026-03-26
- [x] 03-D-PLAN.md — SIMUL B방식 (InspectionListView Grab 자동 진행) ✅ 2026-03-26

---

### Phase 4: 5개 Site 독립 운영 구조
**Goal**: Site 1~5 각각 독립적인 레시피/결과/통계 관리
**Plans**: 3 plans

Plans:
- [x] 04-01-PLAN.md — SiteManager / SiteContext / SiteStatistics 신규 클래스 생성 ✅ 2026-03-26
- [x] 04-02-PLAN.md — Recipe 디렉터리 구조 + RecipeFileHelper Site 오버로드 ✅ 2026-03-26
- [x] 04-03-PLAN.md — SequenceHandler Site 오버로드 + ProcessRecipeChange Site 연결 ✅ 2026-03-26

---

### Phase 5: TCP/IP 통신 ($TEST 프로토콜)
**Goal**: Host(설비)와 TCP 포트 7701로 Shot별 검사 요청/응답 연동
**Plans**: Complete

---

### Phase 6: UI/파라미터 개선
**Goal**: ShotTabView, ROI 드래그, Copy/Paste 기능 완성
**Plans**: Complete

---

### Phase 7: 검사 알고리즘 안정화
**Goal**: Blob Detection 판정 신뢰도 및 파라미터 저장/로드 안정화
**Plans**: Complete

---

### Phase 8: UI/파라미터 개선 (ShotTabView, ROI 드래그, Copy/Paste)
**Goal**: 사용자가 Shot별 파라미터를 편리하게 편집하고 복사/붙여넣기 가능
**Plans**: Complete

---

### Phase 9: 통신 테스트 + 버그 수정
**Goal**: TCP 통신 전체 시나리오 검증 및 발생 버그 수정 완료
**Plans**: Complete

</details>

---

### 🚧 v2.0 레시피 편집 + 이미지 관리 + 운영 안정화 (In Progress)

**Milestone Goal:** 레시피 편집 UI 완성, 이미지 저장/삭제/로드 구조 개선, 택타임 로그 추가, Run/Grab 버튼 역할 명확화

## Phase Details

### Phase 10: 레시피 복사 버그 수정 + 운영 인프라
**Goal**: 레시피 복사가 안정적으로 동작하고 택타임 로그가 기록된다
**Depends on**: Phase 9
**Requirements**: RCP-01, OPS-01
**Success Criteria** (what must be TRUE):
  1. OpenRecipeWindow에서 레시피를 다른 Site로 복사 시 대상 Site 디렉터리가 없어도 복사가 성공하고 CollectRecipe()에서 조회된다
  2. 검사 시퀀스 실행 시 Action별 소요시간(ms)이 로그 파일에 기록된다
  3. 이미 존재하는 대상 경로에 복사해도 기존 파일이 덮어쓰여진다
**Plans**: 2 plans

Plans:
- [x] 10-01-PLAN.md — RecipeFiles.Copy() siteNumber 오버로드 + CopyFilesRecursively 대상 디렉토리 자동 생성
- [x] 10-02-PLAN.md — ActionBase.OnEnd() 택타임 Trace 로그 출력

---

### Phase 11: 이미지 저장 구조 개선
**Goal**: 검사 이미지가 날짜>시간 하위폴더 계층으로 저장되고 NG만 기본 저장된다
**Depends on**: Phase 10
**Requirements**: IMG-01, IMG-02
**Success Criteria** (what must be TRUE):
  1. 검사 완료 후 이미지가 `D:\Log\{yyyyMMdd}\{HHmmss_fff}\{ShotName}_{OK|NG}.jpg` 경로에 저장된다
  2. 기본 설정에서 OK 이미지는 저장되지 않고 NG 이미지만 저장된다
  3. SystemSetting에서 OK 이미지 저장 옵션을 활성화하면 OK 이미지도 저장된다
  4. 같은 초에 여러 검사가 실행되어도 시간폴더 이름이 충돌하지 않는다
**Plans**: 2 plans
**UI hint**: yes

Plans:
- [x] 11-01-PLAN.md — ImageFolderManager + InspectionSequenceContext + SystemSetting.ImageSavePath
- [x] 11-02-PLAN.md — Action_Inspection.SaveResultImage with ImageFolderManager paths + annotated image

---

### Phase 12: Run/Grab 역할 분리 + 이미지 로드/삭제
**Goal**: Grab과 Run 버튼 역할이 명확히 분리되고 저장된 이미지 폴더를 로드하거나 삭제할 수 있다
**Depends on**: Phase 11
**Requirements**: OPS-02, IMG-03, IMG-04
**Success Criteria** (what must be TRUE):
  1. Grab 버튼 클릭 시 카메라 촬상 후 검사가 실행된다
  2. Run 버튼 클릭 시 이전에 로드된 이미지로 카메라 없이 검사 테스트가 실행된다
  3. 시간 폴더를 선택하면 Shot1~5 이미지가 UI에 일괄 로드된다
  4. 날짜 또는 시간 폴더 단위로 저장된 이미지를 삭제할 수 있다
**Plans**: 3 plans
**UI hint**: yes

Plans:
- [x] 12-01-PLAN.md — ShotTabView Grab 버튼 제거 + InspectionListView RUN 버튼 SimulImagePath 분기
- [x] 12-02-PLAN.md — ShotTabView 폴더 로드 버튼 + Ookii FolderBrowserDialog Shot1~5 매핑
- [x] 12-03-PLAN.md — SystemSetting 이미지 관리 탭 + 날짜 폴더 체크 삭제

---

### Phase 13: RecipeEditorWindow
**Goal**: 작업자가 Shot별 파라미터를 편집 창에서 직접 수정하고 저장할 수 있다
**Depends on**: Phase 10, Phase 11, Phase 12
**Requirements**: RCP-02, RCP-03, RCP-04, RCP-05, RCP-06
**Success Criteria** (what must be TRUE):
  1. OpenRecipeWindow에서 Edit 버튼을 클릭하면 RecipeEditorWindow 팝업이 열린다
  2. RecipeEditorWindow에서 Shot1~5 탭 전환 시 해당 Shot의 ROI/Blob/Delay 파라미터가 PropertyGrid에 표시되어 편집 가능하다
  3. Grab 버튼으로 현재 Shot의 카메라 촬상 + 검사 미리보기가 실행된다
  4. Save 버튼으로 편집된 파라미터가 레시피 파일에 저장된다
  5. Reset 버튼으로 모든 파라미터가 기본값으로 초기화된다
**Plans**: 2 plans
**UI hint**: yes

Plans:
- [ ] 13-01: RecipeEditorWindow 신규 생성 — TabControl(Shot1~5) + PropertyGrid 바인딩 + 백업/복원 로직
- [ ] 13-02: Grab 미리보기 구현 — GrabAndDisplay async + _cancelled 플래그 (창 닫힘 크래시 방지)
- [ ] 13-03: Save/Reset 버튼 구현 + OpenRecipeWindow Edit 버튼 진입점 추가

---

## Progress

| Phase | Milestone | Plans Complete | Status | Completed |
|-------|-----------|----------------|--------|-----------|
| 1. 프로젝트 리팩토링 | v1.0 | — | Complete | 2026-03-26 |
| 2. HIK 5-Shot 시퀀스 | v1.0 | — | Complete | 2026-03-26 |
| 3. 티칭 UI + 시뮬레이션 | v1.0 | 4/4 | Complete | 2026-03-26 |
| 4. 5-Site 운영 구조 | v1.0 | 3/3 | Complete | 2026-03-26 |
| 5. TCP/IP 통신 | v1.0 | — | Complete | 2026-03-30 |
| 6. UI/파라미터 개선 | v1.0 | — | Complete | 2026-03-30 |
| 7. 검사 알고리즘 안정화 | v1.0 | — | Complete | 2026-03-30 |
| 8. ShotTabView/ROI/Copy | v1.0 | — | Complete | 2026-03-31 |
| 9. 통신 테스트 + 버그 수정 | v1.0 | — | Complete | 2026-03-31 |
| 10. 레시피 복사 버그 + 인프라 | v2.0 | 2/2 | Complete    | 2026-04-03 |
| 11. 이미지 저장 구조 개선 | v2.0 | 2/2 | Complete    | 2026-04-03 |
| 12. Run/Grab 분리 + 이미지 UI | v2.0 | 3/3 | Complete    | 2026-04-06 |
| 13. RecipeEditorWindow | v2.0 | 0/3 | Not started | - |
