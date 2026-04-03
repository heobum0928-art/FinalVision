---
status: testing
phase: 10-recipe-copy-infra
source: [10-01-SUMMARY.md, 10-02-SUMMARY.md]
started: 2026-04-03T01:30:00Z
updated: 2026-04-03T01:30:00Z
---

## Current Test

number: 1
name: 레시피 복사 — 새 Site 폴더 자동 생성
expected: |
  OpenRecipeWindow에서 레시피 선택 후 새 이름으로 복사(Copy) 실행.
  대상 Site 디렉터리가 존재하지 않아도 복사가 성공하고,
  복사 후 레시피 목록(CollectRecipe)에 새 레시피가 표시된다.
awaiting: user response

## Tests

### 1. 레시피 복사 — 새 Site 폴더 자동 생성
expected: OpenRecipeWindow에서 레시피 선택 후 새 이름으로 복사(Copy) 실행. 대상 Site 디렉터리가 존재하지 않아도 복사가 성공하고, 복사 후 레시피 목록(CollectRecipe)에 새 레시피가 표시된다.
result: [pending]

### 2. 레시피 복사 — 덮어쓰기 확인
expected: 이미 존재하는 레시피 이름으로 복사 시 덮어쓰기 확인 팝업이 표시된다. 확인 클릭 시 기존 파일이 덮어쓰여지고, 취소 시 복사가 중단된다.
result: [pending]

### 3. 택타임 Trace 로그 출력
expected: 검사 시퀀스 실행(Auto 또는 Grab) 후 로그 파일을 확인하면 각 Action별로 [TAKT] {ActionName}: {소요시간}ms 형식의 Trace 로그가 기록되어 있다.
result: [pending]

## Summary

total: 3
passed: 0
issues: 0
pending: 3
skipped: 0
blocked: 0

## Gaps

[none yet]
