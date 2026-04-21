# Code Conventions

## 1. 명명 규칙 (Hungarian Notation)

| Prefix | Type        | Example      |
|--------|-------------|--------------|
| b      | bool        | bIsValid     |
| n      | int         | nCount       |
| f      | float       | fThreshold   |
| d      | double      | dScore       |
| sz     | string      | szName       |
| p      | pointer     | pBuffer      |
| v      | vector/list | vResults     |
| m_     | 멤버변수     | m_nWidth     |
| g_     | 전역변수     | g_szAppName  |

- 변수명은 최대 3단어 이내


## 2. 조건식 규칙

- `??` 단독 사용 → 허용 (건드리지 말 것)
- `?` 삼항 단독 1depth → 허용
- `??` + `?` 혼용 → if-else로 분리
- 삼항 중첩 2depth 이상 → if-else로 분리


## 3. 주석 규칙

- 자명한 코드엔 주석 금지
- why(왜 이렇게 했는지)만 주석 허용

```
❌ // 카운트를 1 증가시킵니다
✅ // 헤더 포함이라 +1
```


## 4. 함수 규칙

- 함수 1개 = 역할 1개
- early return 우선, 불필요한 else 금지
- try-catch는 IO/외부호출에만 사용

```csharp
❌
if (bIsValid)
    return true;
else
    return false;

✅
if (!bIsValid) return false;
return true;
```


## 5. 상수 규칙

- 매직넘버 금지 → const 상수로 선언

```csharp
❌ if (nScore > 85)
✅ const int PASS_THRESHOLD = 85;
   if (nScore > PASS_THRESHOLD)
```


## 6. AI 리팩토링 요청 시 기본 프롬프트

```
아래 코드를 리팩토링해줘.

[최우선 조건]
- 기능, 입출력, 사이드이펙트 100% 동일하게 유지
- 확신 없으면 수정하지 말고 질문 먼저 할 것

[리팩토링 규칙]
- CONVENTIONS.md 기준 적용
- ?? 단독 사용은 유지
- ?? + ? 혼용 / 삼항 중첩 → if-else 분리
- 자명한 주석 제거, why만 유지
- 매직넘버 → const 상수
- 불필요한 else 제거, early return 적용
- 함수가 2가지 이상 역할이면 분리

[출력 형식]
1. 변경 항목 목록 (before → after)
2. 리팩토링된 전체 코드
3. 기능 동일성 체크 코멘트

[검증 요청]
1. 기능이 바뀔 수 있는 위험한 변경 표시
2. 헝가리언 표기법 누락 변수 확인
3. 조건식 규칙 위반 잔존 여부 확인
```
