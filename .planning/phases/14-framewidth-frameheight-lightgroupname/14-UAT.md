---
status: complete
phase: 14-framewidth-frameheight-lightgroupname
source: [14-01-SUMMARY.md]
started: 2026-04-07T12:00:00Z
updated: 2026-04-07T12:05:00Z
---

## Current Test

[testing complete]

## Tests

### 1. FrameWidth/FrameHeight INI 오염 제거 확인
expected: 레시피(Shot) 저장 후 INI 파일에 FrameWidth/FrameHeight 항목이 존재하지 않아야 합니다. 이전에는 FrameWidth=0, FrameHeight=0이 기록되었으나 수정 후에는 완전히 사라져야 합니다.
result: pass

### 2. LightGroupName OnLoad 값 유지
expected: 레시피/Shot을 INI에서 로드할 때, LightGroupName이 INI에 저장된 값 그대로 유지되어야 합니다. 이전에는 OnLoad 시 "DefaultLight"로 강제 덮어쓰기되었으나, 수정 후에는 INI에 저장된 조명 그룹명이 그대로 로드됩니다.
result: pass

### 3. LightGroupName Reset(RestoreShot) 복원
expected: RestoreShot(Reset) 실행 시, 백업 시점에 저장된 LightGroupName 값으로 복원되어야 합니다. 이전에는 CopyTo에서 LightGroupName 복사가 비활성화되어 Reset 후에도 변경된 값이 유지되었으나, 수정 후에는 백업 시점 값으로 정상 복원됩니다.
result: pass

### 4. DeviceName OnLoad 기본값 유지
expected: OnLoad 시 DeviceName은 기존과 동일하게 "DefaultCamera"로 설정되어야 합니다. LightGroupName 수정이 DeviceName 동작에 영향을 주지 않아야 합니다.
result: pass

## Summary

total: 4
passed: 4
issues: 0
pending: 0
skipped: 0
blocked: 0

## Gaps

[none]
