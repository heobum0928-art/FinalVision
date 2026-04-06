---
status: complete
phase: 11-image-save-structure
source: [11-01-SUMMARY.md, 11-02-SUMMARY.md, uncommitted changes]
started: 2026-04-06T00:00:00Z
updated: 2026-04-06T00:15:00Z
---

## Current Test

[testing complete]

## Tests

### 1. 이미지 저장 폴더 구조 확인
expected: Auto 검사 또는 수동 Grab 실행 후, D:\Data\Image\{yyyyMMdd}\{HHmm}\ 폴더가 생성된다. 날짜(yyyyMMdd) > 분(HHmm) 2단계 계층 구조. 같은 분 안의 여러 검사는 동일 폴더에 저장된다.
result: pass

### 2. 원본 이미지 BMP 저장 (SaveOriginImage)
expected: SaveOriginImage=true(기본값) 상태에서 검사 실행 시, 폴더 안에 {ShotName}_{OK|NG}_{ss_fff}.bmp 파일이 생성된다. 확장자가 .bmp이고 파일명에 초+밀리초 타임스탬프가 포함된다.
result: pass

### 3. 캡처(어노테이션) 이미지 JPG 저장 — NG
expected: SaveNGImage=true(기본값) 상태에서 NG 판정 시, 같은 폴더에 {ShotName}_{NG}_capture_{ss_fff}.jpg 파일이 함께 생성된다. ROI/Blob 결과가 그려진 캡처 이미지이다.
result: pass

### 4. OK 캡처 이미지 기본 미저장
expected: SaveGoodImage=false(기본값) 상태에서 OK 판정 시, 캡처(_capture) JPG 파일이 저장되지 않는다. 원본 BMP만 저장된다(SaveOriginImage=true인 경우).
result: pass

### 5. OK 캡처 저장 옵션 활성화
expected: SystemSetting에서 SaveGoodImage를 true로 변경 후 OK 판정 검사하면, {ShotName}_{OK}_capture_{ss_fff}.jpg 파일도 저장된다.
result: pass

### 6. 5-Shot 동일 분 폴더
expected: 한 번의 Auto 검사(5-Shot 순차)에서 Shot1~Shot5 이미지가 모두 같은 {HHmm} 폴더 안에 저장된다. 각 Shot마다 별도 폴더가 생기지 않는다.
result: pass

### 7. SystemSetting.ImageSavePath 기본값
expected: SystemSetting 화면에서 ImageSavePath 항목이 D:\Data\Image로 표시된다.
result: pass

### 8. SIMUL 모드 이미지 저장
expected: SIMUL_MODE에서 Grab+BlobDetect 후에도 캡처 이미지가 정상 저장된다. (SetAnnotatedImage 호출로 LastAnnotatedImage가 갱신되어 null이 아님)
result: pass

## Summary

total: 8
passed: 8
issues: 0
pending: 0
skipped: 0
blocked: 0

## Gaps

[none]
