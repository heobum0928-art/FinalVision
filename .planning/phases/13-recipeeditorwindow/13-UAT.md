---
status: testing
phase: 13-recipeeditorwindow
source: [13-01-SUMMARY.md, session-260407]
started: 2026-04-07T15:00:00Z
updated: 2026-04-07T15:00:00Z
---

## Current Test

number: 1
name: Reset 버튼 표시
expected: |
  Edit 모드에서 InspectionListView 우측 툴바에 Reset 버튼(repair 아이콘 + "Reset" 텍스트)이 Copy/Paste 아래에 표시됨
awaiting: user response

## Tests

### 1. Reset 버튼 표시
expected: Edit 모드에서 InspectionListView 우측 툴바에 Reset 버튼(repair 아이콘 + "Reset" 텍스트)이 Copy/Paste 아래에 표시됨
result: [pending]

### 2. Reset 동작 — 파라미터 복원
expected: 레시피 로드 → Shot 선택 → 파라미터 수정(BlobThreshold 등) → Reset 클릭 → 확인 다이얼로그 → PropertyGrid가 로드 시점 값으로 복원됨
result: [pending]

### 3. Reset guard — 백업 없을 때
expected: 레시피 미로드 상태에서 Reset 클릭 → "백업 데이터가 없습니다. 레시피를 먼저 로드하세요." 경고 표시
result: [pending]

### 4. Grab 후 Shot 탭 이미지 갱신
expected: Shot 탭에서 보고 있는 상태에서 Grab 버튼 클릭 → 촬상된 이미지가 Shot 탭 뷰어에 즉시 갱신됨 (측정 라디오탭 누를 필요 없음)
result: [pending]

### 5. Shot 탭 → Action 자동 선택
expected: Shot 1~5 탭 클릭 시 왼쪽 InspectionList에서 해당 Action(Bolt_One_Inspect 등)이 자동 선택됨
result: [pending]

### 6. Action → Shot 탭 자동 전환 (기존 기능 유지)
expected: 왼쪽 InspectionList에서 Action 선택 시 해당 Shot 탭으로 자동 전환됨 (기존 동작 깨지지 않음)
result: [pending]

## Summary

total: 6
passed: 0
issues: 0
pending: 6
skipped: 0
blocked: 0

## Gaps

[none yet]
