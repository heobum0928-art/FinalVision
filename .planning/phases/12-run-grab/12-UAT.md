---
status: complete
phase: 12-run-grab
source: [12-01-SUMMARY.md, 12-02-SUMMARY.md, 12-03-SUMMARY.md]
started: 2026-04-06T06:00:00Z
updated: 2026-04-06T06:30:00Z
---

## Current Test

[testing complete]

## Tests

### 1. ShotTabView Grab 버튼 제거 확인
expected: ShotTabView 탭에 "Grab" 버튼이 없음. Edit ROI, 원본/측정, 슬라이더, 열기/삭제만 존재.
result: pass

### 2. 이미지 열기 = 로드만 (검사 X)
expected: Shot탭 "열기" 버튼으로 이미지 파일 선택 시 이미지만 표시되고, OK/NG 결과 판정이 실행되지 않음. 결과 레이블이 "---" 유지.
result: pass

### 3. RUN 버튼 = 선택된 Shot 검사 실행
expected: Shot탭에서 이미지를 열고(열기), InspectionListView에서 해당 Action 선택 후 RUN 클릭 시 RunBlobOnLastGrab이 실행되어 OK/NG 결과가 표시됨.
result: pass

### 4. 폴더 일괄 로드 (Action 이름 매칭)
expected: InspectionListView 툴바 "폴더" 버튼 클릭 → 시간폴더 선택 → Action 이름(Bolt_One_Inspect 등)으로 파일 자동 매칭 → 5개 ShotTabView에 이미지 표시. 매칭 안 되면 알림 메시지.
result: pass

### 5. 폴더 로드 후 RUN = 개별 Shot 검사
expected: 폴더 일괄 로드 후 InspectionListView에서 특정 Action 선택 → RUN 클릭 → 해당 Shot만 검사 실행 및 OK/NG 결과 표시.
result: pass

### 6. InspectionListView Grab 버튼 = 카메라 촬상 유지
expected: InspectionListView 에디터 툴바의 Grab(카메라) 버튼 클릭 시 기존 카메라 촬상 동작 유지 (시뮬모드: VirtualCamera Grab).
result: pass

### 7. SystemSetting 이미지 관리 창
expected: SystemSetting 창에 "이미지 관리" 버튼 클릭 → ImageManageWindow 열림 → yyyyMMdd 날짜 폴더 목록 표시 (체크박스).
result: pass

### 8. 이미지 폴더 삭제 (확인 다이얼로그)
expected: 날짜 폴더 체크 후 "선택 삭제" 클릭 → 확인 다이얼로그 표시 → "예" 클릭 시 삭제 완료 및 목록 새로고침. "아니오" 시 삭제 안 됨.
result: pass

### 9. MapData 설정 숨김
expected: SystemSetting 창에서 MapData 항목(MapDataLoadPath, MapDataSavePath)이 보이지 않음.
result: pass

### 10. DeviceSelector 메뉴 정리
expected: 카메라 창(DeviceSelector) "..." 메뉴에서 "Load Image in directory", "Next Image", "Prev Image" 항목이 없음. Save Image, Open Image Dir, Streaming만 존재.
result: pass

## Summary

total: 10
passed: 10
issues: 0
pending: 0
skipped: 0
blocked: 0

## Gaps

[none]
