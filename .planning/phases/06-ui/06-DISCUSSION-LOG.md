# Phase 6: 레시피 관리 UI — Discussion Log

> **Audit trail only.** Do not use as input to planning, research, or execution agents.
> Decisions are captured in CONTEXT.md — this log preserves the alternatives considered.

**Date:** 2026-03-27
**Phase:** 06-레시피 관리 UI
**Areas discussed:** 레거시 필드 정리, 에디터 진입점, 5-Shot 레이아웃, ROI 편집, 카메라 파라미터, 미리보기
**Mode:** --auto (모든 항목 자동 선택, 권장 옵션 적용)

---

## 레거시 필드 정리 (CameraSlaveParam)

| Option | Description | Selected |
|--------|-------------|----------|
| 제거 (MotorXPos, MotorYPos, PixelToUM_Offset, PartNo) | ECi_Dispenser 잔재, FinalVision 미사용 | ✓ |
| 유지 | 하위 호환 고려 | |

**Auto-selected:** 제거
**Notes:** 사용자가 "이게 왜 있는건지?" 질문 → 완전 레거시, 제거 결정

---

## 에디터 진입점/형태

| Option | Description | Selected |
|--------|-------------|----------|
| 신규 RecipeEditorWindow | OpenRecipeWindow에서 "편집" 버튼 → 팝업 | ✓ |
| InspectionListView 확장 | 기존 UI에 파라미터 편집 강화 | |

**Auto-selected:** 신규 RecipeEditorWindow
**Notes:** 전용 편집 UI가 사용성에 명확히 유리

---

## 5-Shot 레이아웃

| Option | Description | Selected |
|--------|-------------|----------|
| TabControl (Shot1~5) | 각 탭에 PropertyGrid + Canvas | ✓ |
| TreeView + PropertyGrid | 기존 InspectionListView 패턴 유지 | |
| 스크롤 리스트 | 5개 Shot 한 화면에 세로 스크롤 | |

**Auto-selected:** TabControl
**Notes:** Phase 3 ComboBox 패턴 참고, 탭이 Shot 전환에 직관적

---

## ROI 편집

| Option | Description | Selected |
|--------|-------------|----------|
| PropertyGrid + RuntimeResizer 드래그 (둘 다) | Phase 3 구현 재사용 | ✓ |
| PropertyGrid 숫자 입력만 | 구현 단순 | |
| Canvas 드래그만 | UI 직관적이나 정밀 입력 어려움 | |

**Auto-selected:** 둘 다 (PropertyGrid + RuntimeResizer 드래그)

---

## 카메라 파라미터 편집

| Option | Description | Selected |
|--------|-------------|----------|
| Exposure, Gain만 표시 (레거시 제거 후) | D-01과 연동 | ✓ |
| 전체 CameraSlaveParam 표시 | 레거시 포함 | |

**Auto-selected:** 레거시 제거 후 필요한 파라미터만

---

## 미리보기 검사

| Option | Description | Selected |
|--------|-------------|----------|
| Grab 버튼 수동 클릭 (Phase 3 방식) | 기존과 일관성 | ✓ |
| 파라미터 변경 시 자동 재검사 | 편의성 높지만 성능 부담 | |

**Auto-selected:** Grab 수동 클릭
**Notes:** Phase 3와 동일한 UX 유지

---

## Claude's Discretion

- RecipeEditorWindow와 MainView GrabAndDisplay 연결 방식
- Tab 전환 시 Canvas 이미지 처리
- Window 크기/최소화 옵션

## Deferred Ideas

- Shot별 다른 시뮬레이션 이미지
- 파라미터 변경 시 자동 재검사
- 레시피 버전 히스토리
