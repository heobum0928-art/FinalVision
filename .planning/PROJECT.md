# FinalVision

## 프로젝트 개요
자재유무 비전 검사 시스템. HIK 카메라 1대로 5개 포지션을 이동하며 촬상, OpenCV Blob Detection으로 자재 유무를 판정한다.
기존 ECi_Dispenser (ReringProject) 코드를 기반으로 FinalVision 프로젝트로 리팩토링 및 기능 확장한다.

## 기술 스택
- **언어/프레임워크**: C# WPF (.NET Framework), Visual Studio 2022
- **카메라**: HIK Vision (MvCamCtrl.NET SDK) — 1대
- **비전 라이브러리**: OpenCvSharp (Blob Detection)
- **통신**: TCP/IP (VisionServer — 기존 구조 유지 및 확장)
- **PLC**: 없음 (TCP/IP 전용)
- **OS**: Windows 10/11

## 핵심 요구사항
- 카메라 1대로 5개 포지션 이동 촬상 (Shot 1~5)
- 운영 파트 5개 분리 (Site 1~5 독립 운영)
- OpenCV Blob Detection 기반 자재유무 판정 (OK / NG)
- Site별 독립 레시피 (Blob 파라미터)
- TCP/IP 기반 Host 통신 (검사 명령 수신 → 결과 전송)
- UI: 5개 검사 이미지 표시 + Site별 실시간 결과 모니터링

## 기반 코드 (변경 사항)
| 항목 | 기존 | 변경 |
|------|------|------|
| 프로젝트명 | ECi_Dispenser | FinalVision |
| 네임스페이스 | ReringProject | FinalVisionProject |
| 카메라 | Basler + HIK | HIK 전용 |
| PLC | MX Component | 제거 |
| 검사 알고리즘 | Corner Align | Blob Detection |
| 운영 구조 | Corner 단일 | 5개 Site 분리 |

## 개발 환경
- 프로젝트 경로: `D:\Project\FinalVision`
- 솔루션 파일: `ECi_Dispenser.sln` → `FinalVision.sln` (리네임 예정)
- 주요 참조: MvCamCtrl.NET, OpenCvSharp4, OpenCvSharp4.runtime.win

## 작업 시작일
2026-03-25
