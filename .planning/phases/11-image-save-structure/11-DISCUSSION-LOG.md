# Phase 11: 이미지 저장 구조 개선 - Discussion Log

> **Audit trail only.** Do not use as input to planning, research, or execution agents.
> Decisions are captured in CONTEXT.md — this log preserves the alternatives considered.

**Date:** 2026-04-03
**Phase:** 11-이미지 저장 구조 개선
**Areas discussed:** 저장 경로 구조, OK/NG 저장 정책, 동시성/충돌 방지, ImageFolderManager 설계

---

## 저장 경로 구조

### 기본 경로

| Option | Description | Selected |
|--------|-------------|----------|
| D:\Log 하드코딩 유지 | 현재처럼 D:\Log 고정 | |
| SystemSetting.ImageSavePath 사용 | 기존 ImageSavePath 설정 활용, 기본값 D:\Log | ✓ |
| 새 설정 추가 | InspectionImageSavePath 별도 프로퍼티 | |

**User's choice:** SystemSetting.ImageSavePath 사용

### 폴더 단위

| Option | Description | Selected |
|--------|-------------|----------|
| 검사 1회 = 1폴더 | TCP $TEST 수신 시 시간폴더 1개 생성, Shot1~5 같은 폴더 | ✓ |
| Shot별 폴더 | 각 Shot마다 별도 시간폴더 | |
| Shot 구분 없이 플랫 | 현재와 동일 | |

**User's choice:** 검사 1회 = 1폴더

---

## OK/NG 저장 정책

### 이미지 종류

| Option | Description | Selected |
|--------|-------------|----------|
| 원본만 저장 | Annotated 미저장, 디스크 절약 | |
| 둘 다 저장 | 원본 + Annotated 모두 저장 | ✓ |
| Annotated만 저장 | ROI/Blob 결과 이미지만 저장 | |

**User's choice:** 둘 다 저장

### 현재 동작 유지

| Option | Description | Selected |
|--------|-------------|----------|
| 유지 | 현재 _GrabbedImage(원본) 저장 유지 | ✓ |
| 변경 필요 | 다른 이미지 조합으로 변경 | |

**User's choice:** 유지 (Annotated 저장을 추가)

---

## 동시성/충돌 방지

| Option | Description | Selected |
|--------|-------------|----------|
| HHmmss_fff 밀리초 | 밀리초 포함, 충돌 시 접미사 추가 | ✓ |
| HHmmss + 카운터 | 초 단위 + 내부 카운터 | |
| GUID/랜덤 | 고유 ID, 정렬/검색 어려움 | |

**User's choice:** HHmmss_fff 밀리초

---

## ImageFolderManager 설계

### 클래스 범위

| Option | Description | Selected |
|--------|-------------|----------|
| 경로 생성만 | GetSavePath() 메서드만 제공 | ✓ |
| 경로 + 디스크 정리 | GetSavePath() + CleanOldImages(days) | |
| 경로 + 정리 + 조회 | GetSavePath() + Clean + ListFolders(date) | |

**User's choice:** 경로 생성만 (Phase 12에서 확장)

### 파일 위치

| Option | Description | Selected |
|--------|-------------|----------|
| WPF_Example/Utility/ | RecipeFileHelper와 같은 폴더 | ✓ |
| WPF_Example/Custom/ | Custom 폴더 하위 | |
| ATRAS/Project.BaseLib/ | 기반 라이브러리 | |

**User's choice:** WPF_Example/Utility/

---

## Claude's Discretion

- ImageFolderManager 내부 메서드 시그니처 설계
- Annotated 이미지 저장 추가 구현 방식
- 검사 시작 시점 폴더명 전달 메커니즘

## Deferred Ideas

- 디스크 정리 (오래된 이미지 자동 삭제) — Phase 12
- 이미지 폴더 조회/로드 UI — Phase 12
- 이미지 삭제 UI — Phase 12
