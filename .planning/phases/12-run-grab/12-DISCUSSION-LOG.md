# Phase 12: Run/Grab 역할 분리 + 이미지 로드/삭제 - Discussion Log

> **Audit trail only.** Do not use as input to planning, research, or execution agents.
> Decisions are captured in CONTEXT.md — this log preserves the alternatives considered.

**Date:** 2026-04-06
**Phase:** 12-run-grab
**Areas discussed:** 폴더 삭제 UI, Grab/Run 버튼 배치, 폴더 일괄 로드

---

## 폴더 삭제 UI

| Option | Description | Selected |
|--------|-------------|----------|
| ShotTabView 툴바 오른쪽 | 기존 [열기][삭제] 옆에 [폴더삭제] 버튼 추가 | |
| InspectionListView 툴바 | 우측 패널의 Grab/Light 버튼 옆에 배치 | |
| 별도 메뉴/설정 창 | SystemSetting 창 또는 MainWindow 메뉴에 배치 | ✓ |

**User's choice:** 별도 메뉴/설정 창
**Notes:** 자주 쓰지 않는 기능이므로 숨김

### 삭제 방식

| Option | Description | Selected |
|--------|-------------|----------|
| FolderBrowserDialog | Ookii VistaFolderBrowserDialog로 직접 선택 | |
| 날짜 목록 UI | 이미지 저장 경로에서 날짜 폴더 목록을 보여주고 체크박스로 선택 | ✓ |
| Claude 재량 | 구현 방식은 Claude가 결정 | |

**User's choice:** 날짜 목록 UI

### 삭제 UI 위치

| Option | Description | Selected |
|--------|-------------|----------|
| SystemSetting 창에 탭 추가 | 기존 SystemSetting 창에 '이미지 관리' 탭 추가 | ✓ |
| 별도 팝업 창 | 새 Window로 '이미지 관리' 창 만들기 | |
| Claude 재량 | 구현 위치는 Claude가 결정 | |

**User's choice:** SystemSetting 창에 탭 추가

### 삭제 단위

| Option | Description | Selected |
|--------|-------------|----------|
| 날짜 폴더만 | yyyyMMdd 날짜 폴더 단위로만 삭제 | ✓ |
| 날짜 + 시간 폴더 | 날짜 클릭 시 하위 시간폴더 펼침, 둘 다 선택 삭제 가능 | |
| Claude 재량 | 삭제 단위는 Claude가 결정 | |

**User's choice:** 날짜 폴더만

---

## Grab/Run 버튼 배치

**User's initial input (free text):**
1. 영상 Grab 버튼이 shot 별로 1씩 UI에 있고
2. 메인화면 우측에 Grab버튼이 있다
3. 우측 Grab 버튼으로 Grab하는 기능을 넣고 기존 Shot별로 Grab하는 UI 및 기능은 삭제하자

**Decisions captured:**
- ShotTabView Shot별 Grab 버튼 삭제
- InspectionListView 우측 툴바 Grab 버튼 유지

### RUN 버튼 역할

| Option | Description | Selected |
|--------|-------------|----------|
| RUN = 로드 이미지로 검사 | SimulImagePath 있으면 로드 이미지로 검사, 없으면 기존 시퀀스 | ✓ |
| RUN = 항상 로드 이미지만 | RUN은 오직 로드된 이미지로만 검사 | |
| RUN 동작 유지 | RUN은 기존처럼 시퀀스 실행, 변경 없음 | |

**User's choice:** RUN = 로드 이미지로 검사

### RUN 범위

| Option | Description | Selected |
|--------|-------------|----------|
| 5-Shot 전체 순차 검사 | RUN 클릭 시 Shot1→Shot2→...Shot5 모두 실행 | |
| 선택된 Shot만 | InspectionListView에서 현재 선택된 Shot만 실행 | ✓ |
| Claude 재량 | 구현 범위는 Claude가 결정 | |

**User's choice:** 선택된 Shot만

---

## 폴더 일괄 로드

### 로드 방식

| Option | Description | Selected |
|--------|-------------|----------|
| VirtualCamera.BackgroundImagePath 재활용 | 기존 DeviceSelector 패턴 재사용 — 폴더 선택 → BackgroundImagePath 설정 → GrabImage()가 파일에서 읽음 | ✓ |
| 별도 로드 로직 | Shot별 SimulImagePath에 직접 파일 경로 매핑 | |

**User's choice:** VirtualCamera.BackgroundImagePath 재활용
**Notes:** 사용자가 "기존 directory in image 재활용" 직접 제안

---

## Claude's Discretion

- ShotTabView Grab 제거 후 빈 공간 레이아웃 조정
- 폴더 로드 버튼의 정확한 배치 위치
- 날짜 폴더 목록 UI 세부 디자인
- BackgroundImageFileList 정렬과 Shot 매핑 검증

## Deferred Ideas

None
