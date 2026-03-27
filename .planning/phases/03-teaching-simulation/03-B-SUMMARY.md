---
phase: 03-teaching-simulation
plan: B
subsystem: RuntimeResizer / ROI 드래그 생성
tags: [wpf, roi, mouse-drag, drawing, runtime-resizer]
one_liner: "RuntimeResizer에 좌클릭 드래그 신규 ROI 생성 기능 추가 — _isDrawingNew 플래그 + 노란 점선 미리보기 + UpdateRect로 Param 반영"
dependency_graph:
  requires: []
  provides: [mouse-drag-roi-creation]
  affects:
    - WPF_Example/UI/ContentItem/RuntimeResizer/RuntimeResizer.cs
    - WPF_Example/UI/ContentItem/RuntimeResizer/DrawableRectangle.cs
tech_stack:
  added: []
  patterns:
    - "_isDrawingNew 플래그 패턴으로 드래그 상태 관리"
    - "ScaleTransform 역변환으로 이미지 좌표 계산"
    - "OnRender에서 DashStyle Pen으로 점선 미리보기 렌더링"
key_files:
  created: []
  modified:
    - WPF_Example/UI/ContentItem/RuntimeResizer/DrawableRectangle.cs
    - WPF_Example/UI/ContentItem/RuntimeResizer/RuntimeResizer.cs
decisions:
  - "DrawableList.Find 대신 foreach로 첫 DrawableRectangle 탐색 (LINQ using 추가 불필요)"
  - "OnRender 내 ScaleTransform PushTransform 블록 안에 미리보기 추가 — 이미지 좌표계 직접 사용"
  - "OnMouseUp의 ReleaseMouseCapture() 직후 _isDrawingNew 분기 처리 — IsEditable 체크 전에 배치"
metrics:
  duration: "15min"
  completed: "2026-03-26"
  tasks_completed: 2
  tasks_total: 2
  files_modified: 2
requirements:
  - REQ-007
  - REQ-008
---

# Phase 03 Plan B: RuntimeResizer 마우스 드래그 신규 ROI 생성 Summary

## What Was Built

`RuntimeResizer`에 마우스 좌클릭 드래그로 새 ROI를 그리는 기능을 구현했다.
기존에는 `IsEditable=true`, `!IsSelected` 상태에서 빈 캔버스 드래그가 스크롤로만 동작했으나,
이제 DrawableList에 ROI가 존재하는 경우 드래그 시 노란 점선 미리보기 사각형이 표시되고,
MouseUp 시 해당 `DrawableRectangle`의 ROI가 드래그 범위로 업데이트되어 파라미터에 반영된다.

## Tasks Completed

| Task | Name | Files Modified |
|------|------|----------------|
| B-1 | DrawableRectangle에 UpdateRect 메서드 추가 | DrawableRectangle.cs |
| B-2 | RuntimeResizer 드래그 신규 ROI 생성 구현 | RuntimeResizer.cs |

## Key Changes

### Task B-1: DrawableRectangle.cs

`ExecResize` 메서드 아래에 `UpdateRect(System.Windows.Rect)` 메서드 추가:
- 최소 크기(`MIN_ROI_WIDTH`, `MIN_ROI_HEIGHT`) 보장
- `OriginalRect = newRect` 로 ROI 전체 교체
- `UpdatePicker()` 호출로 핸들 위치 갱신
- 파라미터 반영은 호출측(`RuntimeResizer.OnMouseUp`)에서 `CheckAvailable()` 호출로 처리

### Task B-2: RuntimeResizer.cs

**필드 추가 (line 61-62):**
- `private bool _isDrawingNew = false`
- `private Point _drawStartPoint`

**OnMouseDown 수정:**
- `!IsSelected` 분기에서 `IsEditable && LeftButton && DrawableList.Count > 0` 조건 시 `_isDrawingNew = true` + 이미지 좌표 저장 + CaptureMouse
- 그 외(우클릭 / IsEditable=false / ROI 없음) → 기존 스크롤 동작 유지

**OnMouseMove 수정:**
- `IsMouseCaptured` 블록 최상단에 `_isDrawingNew` 체크 추가
- `true`이면 `InvalidateVisual()` 후 return — 스크롤 없이 미리보기만 갱신

**OnMouseUp 수정:**
- `ReleaseMouseCapture()` 직후 `_isDrawingNew` 분기 처리
- 드래그 범위 Rect 계산 (음수 방지, 최소 크기 체크)
- `DrawableRectangle.UpdateRect()` → `CheckAvailable(None)` → `SelectedItem` 설정 → `OnSelectionItemChanged` 이벤트
- `return`으로 기존 로직 분리

**OnRender 수정:**
- DrawableList 루프 직후 `_isDrawingNew` 미리보기 블록 추가
- `Pen(Brushes.Yellow, 1)` + `DashStyle([4,4], 0)` 점선으로 사각형 렌더링
- 좌표는 이미지 좌표계 직접 사용 (`PushTransform(_ScaleTransform)` 이후 블록)

## Behavior Summary

| 조건 | 동작 |
|------|------|
| IsEditable=true + 좌클릭 드래그 + ROI 존재 | 노란 점선 미리보기 → MouseUp 시 ROI 업데이트 |
| IsEditable=true + 좌클릭 드래그 + ROI 없음 | 기존 스크롤 |
| IsEditable=true + 우클릭 드래그 | 기존 스크롤 |
| IsEditable=false + 드래그 | 기존 스크롤 |
| 기존 ROI 선택/이동/리사이즈 | 동작 유지 (IsSelected=true 분기는 무변경) |

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 2 - Missing Functionality] DrawableList.Find → foreach 대체**
- **Found during:** Task B-2 구현
- **Issue:** 플랜에서 `DrawableList.Find(d => d is DrawableRectangle)` 사용 제안 — System.Linq using이 이미 없는 경우 컴파일 오류 가능성
- **Fix:** `foreach` 루프로 첫 번째 `DrawableRectangle` 탐색으로 대체 (LINQ 의존 없음)
- **Files modified:** RuntimeResizer.cs

## Known Stubs

없음 — 이번 플랜의 기능(신규 ROI 드래그 생성)은 완전히 구현되었으며 실행 가능한 상태.

## Self-Check: PASSED

- DrawableRectangle.cs `UpdateRect` 메서드 존재 확인
- RuntimeResizer.cs `_isDrawingNew`, `_drawStartPoint` 필드 확인
- OnMouseDown `_isDrawingNew = true` 분기 확인
- OnMouseMove `_isDrawingNew` 체크 + `InvalidateVisual()` 확인
- OnMouseUp `_isDrawingNew = false` + `UpdateRect(` 호출 확인
- OnRender `dashPen` 점선 사각형 렌더링 확인
- 모든 신규 라인 `//260326 hbk` 주석 포함 확인
