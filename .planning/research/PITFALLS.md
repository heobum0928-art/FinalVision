# Pitfalls Research — FinalVision v2.0

## Critical Pitfalls

### 1. Recipe Copy Bug — 정확한 원인
- `RecipeFiles.Copy()`가 flat 경로(Site 없는) 오버로드 사용
- `Site{N}` 대상 디렉터리가 생성되지 않음
- 복사된 레시피가 잘못된 경로에 생성 → `CollectRecipe(siteNumber)`에서 안 보임
- **Prevention:** Site 경로 오버로드 추가, 대상 디렉터리 `Directory.CreateDirectory()` 선행

### 2. `D:\Log` 하드코딩 — SaveResultImage
- `Action_Inspection.SaveResultImage()` line 453에 `@"D:\Log"` 하드코딩
- `SystemSetting.ImageSavePath`가 완전히 무시됨
- **Prevention:** 경로 구조 변경 전에 하드코딩부터 수정 → ImageFolderManager로 교체

### 3. Tact Time Timer 혼동
- `SequenceContext.Timer`는 시퀀스 시작 시 1회 Restart → 누적 시간 측정
- Action별 개별 시간을 원하면 `ActionContext.Timer` 사용해야 함
- `ActionContext.Timer`는 존재하지만 Start()가 호출되지 않는 상태
- **Prevention:** ActionContext.Timer.Start() 호출 추가 후 개별 로그 출력

### 4. RecipeEditorWindow Grab 콜백 안전성
- `GrabAndDisplay`는 `async void` + closure callback 패턴
- 에디터 창이 닫힌 후 콜백이 실행되면 disposed control 접근 → crash
- **Prevention:** Window.Closed 핸들러에서 `_cancelled = true` 플래그 설정, 콜백에서 체크

### 5. 이미지 일괄 로드 순서 의존성
- 현재 flat 날짜 폴더에 여러 Shot 파일 혼재
- Shot1~5 일괄 로드는 날짜>시간 하위폴더 구조가 먼저 적용되어야 의미 있음
- **Prevention:** 이미지 저장 구조 변경 → 삭제 기능 → 일괄 로드 순서 준수

## Medium Pitfalls

### 6. SaveOkImage 기본값 변경
- 기존 레시피/설정에 `SaveOkImage=true`로 저장된 값이 있을 수 있음
- 기본값만 false로 바꾸면 기존 설정은 영향 없음 (IniFile에서 로드)
- **Prevention:** 신규 설치에서만 적용, 기존 설정 파일은 자동 유지

### 7. Run/Grab 분리 시 Step 카운터 리셋
- `Action_Inspection.Run()`은 `Step` 필드로 상태머신 진행
- 로드된 이미지로 Run 시 Step이 올바르게 초기화되는지 확인 필요
- **Prevention:** Run 전 Step = 0 리셋 보장

### 8. RecipeEditorWindow와 MainView 참조
- RecipeEditorWindow가 MainView 참조를 받아야 GrabAndDisplay 호출 가능
- OpenRecipeWindow → RecipeEditorWindow 체인에서 MainView 전달 필요
- **Prevention:** 생성자 주입으로 명시적 전달 (이미 Plan 06-03에서 설계됨)

## Low Pitfalls

### 9. 이미지 삭제 시 파일 잠금
- 다른 프로세스(탐색기 미리보기 등)가 파일 잠금 → 삭제 실패 가능
- **Prevention:** try-catch + 실패 시 사용자 알림

### 10. 시간별 폴더 이름 충돌
- 같은 초에 여러 검사 시 `HHmmss` 폴더 충돌 가능
- **Prevention:** `HHmmss_fff` (밀리초 포함) 사용

## Build Order 제약

1. Recipe Copy 버그 수정 → RecipeEditorWindow보다 선행 필수
2. `D:\Log` 하드코딩 수정 → 이미지 구조 변경보다 선행 필수
3. 이미지 저장 구조 변경 → 일괄 로드보다 선행 필수
4. ActionContext.Timer 활성화 → 택타임 로그보다 선행 필수

## Confidence: HIGH
전 항목 실제 코드 직접 확인 기반
