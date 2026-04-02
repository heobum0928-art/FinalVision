# FinalVision

## What This Is

자재유무 비전 검사 시스템. HIK 카메라 1대로 5개 포지션을 이동하며 촬상, OpenCV Blob Detection으로 자재 유무를 판정한다.
Host(설비)가 TCP로 Shot 1개씩 순차 요청하여 검사를 수행하며, 5개 Site 독립 운영을 지원한다.

## Core Value

카메라 1대 + 5-Shot 순차 촬상으로 자재 유무를 정확히 판정하고, TCP 통신으로 설비와 연동하여 자동 검사를 수행한다.

## Current Milestone: v2.0 레시피 편집 + 이미지 관리 + 운영 안정화

**Goal:** 레시피 편집 UI 완성, 이미지 저장/삭제/로드 구조 개선, 택타임 로그 추가, Run/Grab 버튼 역할 명확화

**Target features:**
- 레시피 복사 버그 수정 (Site 하위 디렉터리 미생성)
- 레시피 편집 UI (RecipeEditorWindow) — Shot별 파라미터 편집, Grab 미리보기, 초기화
- 택타임 로그 — Action별 소요시간 기록 (기존 Timer/Stopwatch 활용)
- 이미지 저장 구조 개선 — 날짜 > 시간별 하위폴더, NG만 기본 저장
- 이미지 삭제 기능
- 이미지 디렉터리 로드 — 폴더 선택 시 Shot1~5 일괄 로드
- Run/Grab 버튼 역할 정리 — Grab=카메라 촬상+검사, Run=로드된 이미지로 테스트

## Requirements

### Validated

- ✓ 프로젝트 리팩토링 (ECi_Dispenser → FinalVision) — v1.0 Phase 1
- ✓ HIK 카메라 단일화 + 5-Shot 시퀀스 — v1.0 Phase 2
- ✓ 티칭 UI + 시뮬레이션 자동 실행 — v1.0 Phase 3
- ✓ 5개 Site 독립 운영 구조 — v1.0 Phase 4
- ✓ TCP/IP 통신 ($TEST 프로토콜) — v1.0 Phase 5
- ✓ UI/파라미터 개선 (ShotTabView, ROI 드래그, Copy/Paste) — v1.0 Phase 8
- ✓ 통신 테스트 + 버그 수정 — v1.0 Phase 9

### Active

- [ ] 레시피 복사 버그 수정
- [ ] 레시피 편집 UI (RecipeEditorWindow)
- [ ] 택타임 로그
- [ ] 이미지 저장 구조 개선
- [ ] 이미지 삭제 기능
- [ ] 이미지 디렉터리 로드
- [ ] Run/Grab 버튼 역할 정리

### Out of Scope

- 통계 대시보드 (Site별 수율) — 현장 불필요 확인
- FAI/Halcon 에지 측정 — Blob 유무 검사 프로젝트, 절대 추가 금지
- PLC 연동 — TCP/IP 전용

## Context

- 기존 ECi_Dispenser (ReringProject) 코드 기반 리팩토링
- 카메라: HIK Vision (MvCamCtrl.NET SDK) 1대
- 비전: OpenCvSharp FindContours + 면적 필터 (Halcon 사용 안 함)
- 통신: TCP/IP 포트 7701, $TEST:Site,TestType,ID@ 프로토콜
- 레시피 경로: D:\Data\Recipe\Site{N}\{레시피명}\main.ini
- 이미지 저장 경로: D:\Log\{날짜}\{Shot명}_{OK|NG}_{시간}.jpg (v2.0에서 구조 변경 예정)
- 기존 ActionContext.Timer / SequenceContext.Timer (Stopwatch) 구조 있음 — 로그 출력만 추가 필요

## Constraints

- **Tech stack**: C# WPF (.NET Framework), Visual Studio 2022
- **Camera**: HIK Vision 1대 전용
- **OS**: Windows 10/11
- **주석 규칙**: //YYMMDD hbk 형식 유지

## Key Decisions

| Decision | Rationale | Outcome |
|----------|-----------|---------|
| HIK 전용 (Basler 제거) | 프로젝트 단순화 | ✓ Good |
| PLC 미사용 (TCP/IP 전용) | 설비 구조 | ✓ Good |
| OpenCvSharp FindContours + 면적 필터 | Halcon 라이선스 불필요, Blob 유무 검사에 충분 | ✓ Good |
| 5-Site × 5-Shot 구조 | 독립 운영 요구 | ✓ Good |
| OK 이미지 기본 미저장 | 디스크 절약, NG만 관리 | — Pending (v2.0) |

## Evolution

This document evolves at phase transitions and milestone boundaries.

**After each phase transition** (via `/gsd:transition`):
1. Requirements invalidated? → Move to Out of Scope with reason
2. Requirements validated? → Move to Validated with phase reference
3. New requirements emerged? → Add to Active
4. Decisions to log? → Add to Key Decisions
5. "What This Is" still accurate? → Update if drifted

**After each milestone** (via `/gsd:complete-milestone`):
1. Full review of all sections
2. Core Value check — still the right priority?
3. Audit Out of Scope — reasons still valid?
4. Update Context with current state

---
*Last updated: 2026-04-02 after milestone v2.0 start*
