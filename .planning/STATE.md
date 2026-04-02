---
gsd_state_version: 1.0
milestone: v2.0
milestone_name: 레시피 편집 + 이미지 관리 + 운영 안정화
status: Defining requirements
last_updated: "2026-04-02"
progress:
  total_phases: 0
  completed_phases: 0
  total_plans: 0
  completed_plans: 0
---

# FinalVision — Project State

## Current Position

Phase: Not started (defining requirements)
Plan: —
Status: Defining requirements
Last activity: 2026-04-02 — Milestone v2.0 started

## Project Reference

See: .planning/PROJECT.md (updated 2026-04-02)

**Core value:** 카메라 1대 + 5-Shot 순차 촬상으로 자재 유무를 정확히 판정하고, TCP 통신으로 설비와 연동하여 자동 검사를 수행한다.
**Current focus:** v2.0 requirements 정의 중

## Accumulated Context

- 카메라: HIK 전용 (Basler 제거)
- PLC: 사용 안 함 (TCP/IP 전용)
- 비전: OpenCvSharp FindContours + 면적 필터
- 운영: 5개 Site × 5-Shot 구조
- TCP 포트: 7701
- 레시피 경로: D:\Data\Recipe\Site{N}\{레시피명}\main.ini
- 로그 경로: {AppBase}\Trace\ (시퀀스/알고리즘), {AppBase}\TcpConnection\ (통신)
- 기존 ActionContext.Timer / SequenceContext.Timer (Stopwatch) — 로그 미출력 상태
- OK 이미지 저장 설정: SystemSetting.SaveOkImage / SaveNgImage 존재
