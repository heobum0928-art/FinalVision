# Phase 1 Plan: 프로젝트 리팩토링

## 목표
ECi_Dispenser → FinalVision 완전 리네임 + 불필요 코드(Basler 카메라) 제거

## 영향 범위
- .cs 파일 287개 (namespace ReringProject → FinalVisionProject)
- .xaml 파일 24개 (xmlns ReringProject → FinalVisionProject)
- 솔루션/프로젝트 파일 2개
- AssemblyInfo.cs, App.xaml 타이틀

---

## Task 1: 솔루션 파일 리네임 및 프로젝트명 변경

### 1-1. ECi_Dispenser.sln → FinalVision.sln (파일 복사 후 내용 수정)
- 솔루션 내 프로젝트명 `ECi_Dispenser` → `FinalVision`
- 프로젝트 경로 참조 `WPF_Example\ECi_Dispenser.csproj` → `WPF_Example\FinalVision.csproj`

### 1-2. ECi_Dispenser.csproj → FinalVision.csproj (파일 리네임)
- `<RootNamespace>ReringProject</RootNamespace>` → `<RootNamespace>FinalVisionProject</RootNamespace>`
- `<AssemblyName>TFE_OCR</AssemblyName>` → `<AssemblyName>FinalVision</AssemblyName>`

---

## Task 2: Basler 카메라 제거

### 2-1. .csproj에서 Basler 참조 제거
- `<Reference Include="Basler.Pylon ...">` 블록 제거
- `<Compile Include="Device\Camera\Basler\BaslerCamera.cs" />` 제거
- `<Compile Include="Device\Camera\Basler\BaslerCameraProperty.cs" />` 제거

### 2-2. BaslerCamera 사용 코드 제거
- `DeviceHandler.cs` 에서 BaslerCamera 관련 코드 제거
- `DeviceSelector.xaml` / `DeviceSelector.xaml.cs` 에서 Basler 옵션 제거

---

## Task 3: 네임스페이스 전체 교체

### 3-1. .cs 파일 전체 (287개)
- `namespace ReringProject` → `namespace FinalVisionProject`
- `using ReringProject` → `using FinalVisionProject`

### 3-2. .xaml 파일 전체 (24개)
- `xmlns:local="clr-namespace:ReringProject` → `xmlns:local="clr-namespace:FinalVisionProject`
- `clr-namespace:ReringProject` → `clr-namespace:FinalVisionProject` (모든 변형)

---

## Task 4: 앱 정보 업데이트

### 4-1. AssemblyInfo.cs
- `AssemblyTitle` → `"FinalVision"`
- `AssemblyProduct` → `"FinalVision"`

### 4-2. SystemHandler.cs
- `ProjectName = "ECi Vision"` → `ProjectName = "FinalVision"`

---

## Task 5: 빌드 확인
- Visual Studio에서 솔루션 열기
- 빌드 오류 확인 및 수정
- 앱 실행 → 타이틀 "FinalVision" 확인

---

## 완료 기준
- [ ] FinalVision.sln, FinalVision.csproj 존재
- [ ] namespace FinalVisionProject 적용 완료
- [ ] Basler 관련 코드/참조 없음
- [ ] AssemblyTitle = "FinalVision"
- [ ] ProjectName = "FinalVision"
- [ ] 빌드 성공
