# Phase 13: RecipeEditorWindow - Discussion Log

> **Audit trail only.** Do not use as input to planning, research, or execution agents.
> Decisions are captured in CONTEXT.md — this log preserves the alternatives considered.

**Date:** 2026-04-06
**Phase:** 13-recipeeditorwindow
**Areas discussed:** 백업 시점, Reset 버튼 위치, Reset 범위

---

## Scope Reduction

사용자가 Phase 13 범위를 RecipeEditorWindow 전체에서 Reset 기능만으로 축소 결정.

**원래 범위:** RecipeEditorWindow 신규 창 (RCP-02~RCP-06) — TabControl + PropertyGrid + Grab 미리보기 + Save/Reset
**축소된 범위:** 기존 InspectionListView에서 파라미터 편집 후 Save 전 Reset 기능만

---

## 백업 시점

| Option | Description | Selected |
|--------|-------------|----------|
| 레시피 로드 시 (추천) | LoadRecipe() 시점에 자동 백업 — 편집 시작 전 상태로 복원 가능 | ✓ |
| Save 직전 | Save 버튼 클릭 시 백업 — 저장 후에도 직전 상태로 복원 가능 | |
| 둘 다 | 로드 시 + Save 시 각각 백업 — 복원 시점 선택 가능 | |

**User's choice:** 레시피 로드 시 (추천)
**Notes:** 없음

---

## Reset 버튼 위치

| Option | Description | Selected |
|--------|-------------|----------|
| InspectionListView 툴바 (추천) | Save/Copy/Paste 버튼이 있는 툴바에 추가 — 기존 패턴과 일관성 | ✓ |
| PropertyGrid 아래 | PropertyGrid 하단에 배치 — 편집 영역과 가까움 | |
| 너에게 맡김 | Claude 재량으로 배치 | |

**User's choice:** InspectionListView 툴바 (추천)
**Notes:** 없음

---

## Reset 범위

| Option | Description | Selected |
|--------|-------------|----------|
| 전체 Shot 일괄 (추천) | Shot1~5 모든 파라미터를 로드 시점으로 복원 — 단순하고 실수 방지 | |
| 선택된 Shot만 | 현재 트리에서 선택된 Shot의 파라미터만 복원 | ✓ |
| 사용자 선택 | Reset 클릭 시 전체/선택 Shot 선택 다이얼로그 표시 | |

**User's choice:** 선택된 Shot만
**Notes:** 없음

---

## Claude's Discretion

- 백업 데이터 구조 구현 방식
- Reset 버튼 아이콘/텍스트
- PropertyGrid 갱신 방식
- 확인 다이얼로그 필요 여부

## Deferred Ideas

- RecipeEditorWindow 신규 팝업 창 (RCP-02, RCP-06)
- Grab 미리보기 (RCP-03)
- Save 버튼 별도 구현 (RCP-04)
