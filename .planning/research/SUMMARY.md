# Project Research Summary

**Project:** FinalVision v2.0
**Domain:** WPF 산업용 비전 검사 소프트웨어 (자재 유무 판정)
**Researched:** 2026-04-02
**Confidence:** HIGH

## Executive Summary

FinalVision v2.0은 기존 v1.0의 완성된 5-Shot/5-Site 검사 구조 위에 운영 편의성을 더하는 마일스톤이다. 신규 NuGet 패키지나 외부 라이선스는 불필요하며, 이미 프로젝트에 포함된 PropertyTools.Wpf, Ookii.Dialogs.Wpf, OpenCvSharp, 기존 ActionContext/SequenceContext.Timer 구조만으로 모든 목표 기능을 구현할 수 있다. 변경 범위는 하드코딩 경로 교체, 버튼 역할 분리, 신규 Window 1개 추가로 한정된다.

핵심 접근법은 의존성 순서를 엄격히 지키는 것이다. 레시피 복사 버그 수정이 RecipeEditorWindow보다 반드시 선행해야 하고, `D:\Log` 하드코딩 교체가 이미지 폴더 구조 변경의 전제조건이며, 이미지 저장 구조 변경이 일괄 로드 기능의 전제조건이다. 이 순서를 지키면 각 단계를 독립적으로 테스트하고 커밋할 수 있다.

주요 리스크는 두 가지다. 첫째, RecipeEditorWindow의 `GrabAndDisplay` async void 콜백이 창 닫힘 후 실행되면 크래시가 발생할 수 있다 — 취소 플래그로 방지한다. 둘째, 이미지 시간폴더 이름이 같은 초에 충돌할 수 있다 — `HHmmss_fff` 밀리초 포함으로 방지한다. 나머지 변경은 낮은 리스크의 외과적 수정이다.

## Key Findings

### Recommended Stack

기존 스택만으로 충분하다. v1.0에서 이미 포함된 패키지들이 v2.0 전체 기능을 커버한다. 추가 의존성을 도입하면 기존 코드와의 불일치가 발생하므로, 신규 NuGet 패키지는 절대 추가하지 않는다.

**Core technologies:**
- `PropertyTools.Wpf 3.1.0`: RecipeEditorWindow의 Shot별 파라미터 PropertyGrid — 이미 InspectionParam 바인딩 패턴으로 검증됨
- `System.IO` (BCL): 날짜>시간 폴더 계층 관리 — 별도 라이브러리 불필요
- `Ookii.Dialogs.Wpf 5.0.1`: 이미지 디렉터리 선택 FolderBrowserDialog — 이미 참조됨
- `ActionContext.Timer` / `SequenceContext.Timer`: 택타임 로그용 Stopwatch — 이미 존재, Start() + 로그 출력만 추가
- `Logging.PrintLog`: 기존 로그 출력 유틸리티 — 일관성 유지

### Expected Features

**Must have (table stakes):**
- 레시피 복사 버그 수정 — Site 하위 디렉터리 미생성으로 복사된 레시피가 CollectRecipe()에서 미검색
- Run/Grab 역할 명확화 — Grab=카메라 촬상+검사, Run=로드 이미지로 테스트 (현재 혼재)
- 택타임 로그 — Action별 ElapsedMilliseconds 출력 (Timer는 있으나 로그 없음)
- 이미지 저장 구조 개선 — `날짜>시간 하위폴더` 계층 + NG만 기본 저장
- 이미지 디렉터리 일괄 로드 — FolderBrowserDialog로 Shot1~5 일괄 매핑
- 이미지 삭제 기능 — 날짜폴더 단위 삭제 UI
- RecipeEditorWindow — Shot별 파라미터 편집 팝업, Grab 미리보기, 초기화

**Should have (differentiators):**
- 레시피 비교 기능 (두 레시피 파라미터 차이 표시)
- 이미지 뷰어 OK/NG 필터링
- 택타임 트렌드 그래프

**Defer (v2+):**
- 통계 대시보드 — 현장 불필요 확인됨
- 딥러닝 검사 — 과도한 복잡도
- 레시피 버전관리 — 불필요한 복잡도
- FAI/에지 측정 — 절대 금지 (Blob 유무 검사 프로젝트)

### Architecture Approach

기존 컴포넌트 경계를 유지하면서 3개의 신규 컴포넌트를 추가하고 5개의 기존 컴포넌트를 외과적으로 수정한다. 모든 수정은 단일 책임 원칙에 따라 범위가 명확히 한정된다. `InspectionParam.CopyTo()`가 이미 구현되어 있어 RecipeEditorWindow 백업/복원 로직을 즉시 활용할 수 있다.

**New components:**
1. `RecipeEditorWindow` — Shot별 파라미터 편집 팝업 (TabControl + PropertyGrid + Grab 미리보기)
2. `ImageFolderManager` — 이미지 저장 경로 관리 (`날짜>시간` 계층, 삭제, 일괄 로드)
3. `TactTimeLogger` — Action/Sequence별 타임 로그 출력

**Modified components:**
1. `Action_Inspection.SaveResultImage` — `@"D:\Log"` 하드코딩 → ImageFolderManager.GetSavePath()
2. `ShotTabView` — Grab/Run 버튼 역할 분리
3. `OpenRecipeWindow` — "Edit" 버튼 추가 → RecipeEditorWindow 팝업 진입
4. `RecipeFiles.Copy` — Site 경로 오버로드 추가 + Directory.CreateDirectory() 선행
5. `SystemSetting` — SaveOkImage 기본값 false

### Critical Pitfalls

1. **RecipeFiles.Copy 버그** — Site 경로 무시로 복사 레시피가 잘못된 위치 생성. `Directory.CreateDirectory()` 선행 + Site 오버로드 추가로 방지.
2. **`D:\Log` 하드코딩** — Action_Inspection.SaveResultImage line 453에 하드코딩, SystemSetting.ImageSavePath 완전 무시. 경로 구조 변경 전 하드코딩부터 교체 필수.
3. **ActionContext.Timer 미활성화** — Timer 객체는 있지만 Start()가 호출되지 않음. LogActionTime 추가 전에 Start() 호출 확인/추가 필수.
4. **GrabAndDisplay async 콜백 크래시** — RecipeEditorWindow 닫힌 후 콜백 실행 시 disposed control 접근. `Window.Closed`에서 `_cancelled = true` 플래그 설정 필수.
5. **시간폴더 이름 충돌** — 같은 초에 여러 검사 시 `HHmmss` 폴더 충돌. `HHmmss_fff` (밀리초 포함) 사용.

## Implications for Roadmap

Based on research, suggested phase structure:

### Phase 1: 버그 수정 및 기반 인프라

**Rationale:** 레시피 복사 버그는 RecipeEditorWindow의 전제조건이므로 가장 먼저 처리. 인프라 유틸리티(ImageFolderManager, TactTimeLogger)는 이후 3개 기능이 공통 의존하므로 함께 구축.
**Delivers:** 안정적인 레시피 복사 + 이미지/로그 인프라 클래스
**Addresses:** 레시피 복사 버그 수정, ImageFolderManager 신규 생성, TactTimeLogger 신규 생성
**Avoids:** Pitfall #1 (RecipeFiles.Copy 버그), Pitfall #2 (`D:\Log` 하드코딩) 교체 준비

### Phase 2: 이미지 저장 구조 + 택타임 로그

**Rationale:** Phase 1의 인프라에 의존하는 기능 중 UI가 없어 독립 테스트 가능한 것들. 이미지 저장 구조 변경은 일괄 로드의 전제조건.
**Delivers:** 날짜>시간 폴더 계층 저장 + Action별 택타임 콘솔 로그
**Uses:** ImageFolderManager.GetSavePath(), TactTimeLogger.LogActionTime(), ActionContext.Timer
**Implements:** Action_Inspection.SaveResultImage 하드코딩 교체, ActionContext.Timer Start() 활성화
**Avoids:** Pitfall #3 (Timer 미활성화), Pitfall #5 (시간폴더 충돌 → HHmmss_fff)

### Phase 3: Run/Grab 분리 + 이미지 로드/삭제 UI

**Rationale:** Phase 2 완료 후 이미지 폴더 구조가 확정되어야 일괄 로드가 의미 있음. ShotTabView 수정과 이미지 UI를 같은 파일에서 처리하므로 함께 묶음.
**Delivers:** 명확한 Grab/Run 역할 분리 + 이미지 폴더 로드/삭제 기능
**Addresses:** Run/Grab 버튼 역할 정리, 이미지 디렉터리 일괄 로드, 이미지 삭제 기능
**Avoids:** Pitfall 이미지 로드 순서 의존성 (Phase 2 완료 후 진행), Pitfall #7 (Step 카운터 리셋)

### Phase 4: RecipeEditorWindow

**Rationale:** 모든 인프라(ImageFolderManager, RecipeFiles 버그 수정, PropertyGrid 패턴)가 완성된 후 마지막에 구현. 가장 복잡한 신규 Window이므로 독립적으로 집중.
**Delivers:** Shot별 파라미터 편집 팝업 + Grab 미리보기 + 초기화 기능
**Uses:** PropertyTools.Wpf PropertyGrid, InspectionParam.CopyTo(), GrabAndDisplay()
**Implements:** RecipeEditorWindow (신규), OpenRecipeWindow "Edit" 버튼 추가
**Avoids:** Pitfall #4 (GrabAndDisplay 콜백 크래시 → _cancelled 플래그), Pitfall #8 (MainView 참조 생성자 주입)

### Phase Ordering Rationale

- 의존성 체인: RecipeFiles.Copy 버그 수정 → RecipeEditorWindow, `D:\Log` 수정 → 이미지 구조, 이미지 구조 → 일괄 로드. 이 순서를 역전하면 중간 단계가 무효화된다.
- 인프라 우선: ImageFolderManager와 TactTimeLogger를 먼저 만들면 Phase 2~4에서 각각 가져다 쓰기만 하면 된다.
- UI 없는 변경을 UI 있는 변경보다 앞에 배치하여 단계별 빌드/테스트가 가능하다.
- RecipeEditorWindow는 가장 많은 의존성(PropertyGrid, Grab, 레시피 복사 수정, MainView 참조)을 갖기 때문에 마지막 단계가 맞다.

### Research Flags

Phases likely needing deeper research during planning:
- **Phase 4:** RecipeEditorWindow Grab Preview UI 결정 (별도 MiniCanvas vs ShotTabView 팝업 재사용) — 실제 코드 검토 후 결정 필요
- **Phase 3:** Step 카운터 리셋 시점 — `Action_Inspection.Run()` 상태머신 내부 정확한 초기화 위치 확인 필요

Phases with standard patterns (skip research-phase):
- **Phase 1:** RecipeFiles.Copy 수정 + Directory.CreateDirectory — 표준 파일시스템 패턴, 원인 이미 특정됨
- **Phase 2:** Timer 로그 출력 — Stopwatch 읽기 + PrintLog 1줄 패턴, 완전히 문서화됨
- **Phase 3:** FolderBrowserDialog 패턴 — Ookii.Dialogs 표준 사용법, 파일명 매핑 패턴 명확

## Confidence Assessment

| Area | Confidence | Notes |
|------|------------|-------|
| Stack | HIGH | 모든 패키지 packages.config + csproj에서 직접 확인됨. 신규 의존성 불필요 |
| Features | HIGH | 기존 코드 분석 기반. 산업용 비전 SW 표준 패턴 일치 |
| Architecture | HIGH | 실제 코드 전수 분석 기반. 컴포넌트 경계와 수정 범위 명확 |
| Pitfalls | HIGH | 전 항목 실제 코드에서 직접 확인됨 (line 번호 포함) |

**Overall confidence:** HIGH

### Gaps to Address

- `SequenceBase.MainExecute()` 내부 Timer.Start() 호출 여부 — Phase 2 착수 전 실제 코드에서 확인 후 Start() 추가 또는 존재 확인
- RecipeEditorWindow Grab Preview: 별도 MiniCanvas 구현 vs ShotTabView 재사용 — Phase 4 착수 전 UI 설계 결정 필요. 재사용이 코드 일관성 면에서 유리하나 팝업 크기 제약 확인 요구됨

## Sources

### Primary (HIGH confidence)
- 실제 소스 코드 전수 분석 (Action_Inspection.cs, RecipeFiles.cs, ShotTabView.xaml.cs, SystemSetting.cs 등) — 모든 버그 원인 및 수정 위치 직접 확인
- WPF_Example/FinalVision.csproj + packages.config — 의존성 전체 확인

### Secondary (MEDIUM confidence)
- PropertyTools.Wpf 3.1.0 기존 사용 패턴 (ShotTabView, OpenRecipeWindow) — PropertyGrid 재사용 패턴 추론
- Ookii.Dialogs.Wpf 기존 사용 위치 — FolderBrowserDialog 사용 패턴 확인

### Tertiary (LOW confidence)
- 이미지 일괄 로드 파일명 패턴 매핑 — 파일 명명 규칙이 SaveResultImage에서 정해지므로 Phase 2 완료 후 확정

---
*Research completed: 2026-04-02*
*Ready for roadmap: yes*
