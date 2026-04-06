# Requirements: FinalVision

**Defined:** 2026-04-02
**Core Value:** 카메라 1대 + 5-Shot 순차 촬상으로 자재 유무를 정확히 판정하고, TCP 통신으로 설비와 연동하여 자동 검사를 수행한다.

## v2.0 Requirements

### 레시피 관리

- [x] **RCP-01**: 레시피 복사 시 Site 대상 디렉터리가 없으면 자동 생성하여 복사 성공
- [ ] **RCP-02**: RecipeEditorWindow에서 Shot1~5 탭 전환 시 해당 Shot의 ROI/Blob/Delay 파라미터를 PropertyGrid로 편집 가능
- [ ] **RCP-03**: RecipeEditorWindow에서 Grab 버튼으로 현재 Shot 미리보기 검사 실행 가능
- [ ] **RCP-04**: RecipeEditorWindow에서 Save 버튼으로 현재 레시피 저장
- [ ] **RCP-05**: RecipeEditorWindow에서 Reset 버튼으로 파라미터 기본값 초기화
- [ ] **RCP-06**: OpenRecipeWindow에서 Edit 버튼 클릭 시 RecipeEditorWindow 팝업 열림

### 이미지 관리

- [x] **IMG-01**: 검사 이미지를 날짜>시간 하위폴더 구조로 저장 (`D:\Log\{yyyyMMdd}\{HHmmss}\{ShotName}_{OK|NG}.jpg`)
- [x] **IMG-02**: OK 이미지 기본 미저장, NG 이미지만 기본 저장 (설정에서 변경 가능)
- [x] **IMG-03**: 시간 폴더 선택 시 Shot1~5 이미지를 일괄 로드하여 UI에 표시
- [ ] **IMG-04**: 날짜/시간 폴더 단위로 저장된 검사 이미지 삭제 가능

### 운영/로그

- [x] **OPS-01**: Action별 소요시간(ms)을 로그에 기록 (기존 Stopwatch 활용)
- [x] **OPS-02**: Grab 버튼은 카메라 촬상+검사, Run 버튼은 로드된 이미지로 검사 테스트로 역할 분리

## Future Requirements

(없음 — v2.0에서 전부 처리)

## Out of Scope

| Feature | Reason |
|---------|--------|
| 통계 대시보드 (Site별 수율) | 현장 불필요 확인 |
| FAI/Halcon 에지 측정 | Blob 유무 검사 프로젝트, 절대 추가 금지 |
| 딥러닝 검사 | 과도한 복잡도 |
| PLC 연동 | TCP/IP 전용 |
| 레시피 버전관리 | 불필요한 복잡도 |
| 택타임 트렌드 그래프 | v2.0 범위 초과 |

## Traceability

| Requirement | Phase | Status |
|-------------|-------|--------|
| RCP-01 | Phase 10 | Complete |
| OPS-01 | Phase 10 | Complete |
| IMG-01 | Phase 11 | Complete |
| IMG-02 | Phase 11 | Complete |
| OPS-02 | Phase 12 | Complete |
| IMG-03 | Phase 12 | Complete |
| IMG-04 | Phase 12 | Pending |
| RCP-02 | Phase 13 | Pending |
| RCP-03 | Phase 13 | Pending |
| RCP-04 | Phase 13 | Pending |
| RCP-05 | Phase 13 | Pending |
| RCP-06 | Phase 13 | Pending |

**Coverage:**
- v2.0 requirements: 12 total
- Mapped to phases: 12
- Unmapped: 0

---
*Requirements defined: 2026-04-02*
*Last updated: 2026-04-02 after roadmap v2.0 creation (Phases 10-13)*
