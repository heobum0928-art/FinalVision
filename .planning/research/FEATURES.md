# Features Research — FinalVision v2.0

## Table Stakes (must build)

| Feature | Complexity | Dependencies | Notes |
|---------|-----------|--------------|-------|
| 레시피 복사 버그 수정 | Low | RecipeFiles.Copy() | Site 디렉터리 미생성 경로 누락 |
| Run/Grab 역할 명확화 | Low | InspectionListView | 버튼 핸들러 분리 (Grab=카메라, Run=로드 이미지) |
| 택타임 로그 | Low | SequenceContext.Timer | ElapsedMilliseconds 로그 1줄 추가 |
| 이미지 저장 구조 개선 | Low | Action_Inspection.SaveResultImage | 날짜>시간 계층 + NG 기본 저장 |
| 이미지 디렉터리 일괄 로드 | Medium | FolderBrowserDialog + 파일명 패턴 매핑 | Shot1~5 일괄 로드 |
| 이미지 삭제 기능 | Medium | 날짜폴더 단위 삭제 UI | 파일시스템 직접 조작 |
| RecipeEditorWindow | High | PropertyGrid + InspectionParam + MainView | 신규 Window, ShotTabView 패턴 재활용 |

## Differentiators (nice to have)

- 레시피 비교 기능 (두 레시피 파라미터 차이 표시)
- 이미지 뷰어에서 OK/NG 필터링
- 택타임 트렌드 그래프

## Anti-features (do NOT build)

- FAI/에지 측정 — Blob 유무 검사 프로젝트, 절대 금지
- 통계 대시보드 — 현장 불필요 확인됨
- 딥러닝 검사 — 과도한 복잡도
- PLC 연동 — TCP/IP 전용
- 레시피 버전관리 — 불필요한 복잡도

## Critical Dependency

레시피 복사 버그 수정 → RecipeEditorWindow보다 반드시 선행

## Confidence: HIGH
기존 코드 분석 기반, 산업용 비전 SW 표준 패턴 참고
