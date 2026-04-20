# Vision Inspector Protocol — ECi Moving Display V1.0
> C# / C++ 코드 구현 참조용 문서  
> 통신 방식: **TCP/IP** | Handler S/W: **Client** | Vision S/W: **Server**  
> 기본 접속: `IP: 192.168.0.1` / `Port: 7701`

---

## 1. 메시지 포맷 규칙

```
$<COMMAND>:<param1>,<param2>,...@
```

- **시작**: `$`
- **명령**: 대문자 ASCII
- **구분자**: `:` (명령/파라미터 사이), `,` (파라미터 사이)
- **종료**: `@`
- **target_id**: 현재 예약 필드 → 반드시 `null` 문자열로 전송

---

## 2. Site / Type / Light 정의

### 2-A. Site Definition (모듈 1 — Anti Peeling / FPCB)

| Site No | Site Name               |
|---------|-------------------------|
| 1       | ANTI_PEELING            |
| 2       | ASSY_ROTATE_PLATE_1     |
| 3       | ASSY_ROTATE_PLATE_2     |
| 4       | CHECK_FPCB              |

### 2-B. Type Definition (모듈 1)

| Type No | Type Name    |
|---------|--------------|
| 1       | High         |
| 2       | Low          |
| 3       | LEFT_FPCB    |
| 4       | RIGHT_FPCB   |

### 2-C. Site Definition (모듈 2 — Final Inspection)

| Site No | Site Name         |
|---------|-------------------|
| 1       | Final_Inspection  |

### 2-D. Type Definition (모듈 2)

| Type No | Type Name |
|---------|-----------|
| 1       | Grab1     |
| 2       | Grab2     |
| 3       | Grab3     |
| 4       | Grab4     |
| 5       | Grab5     |

### 2-E. Light OP Definition

| OP No | OP Name |
|-------|---------|
| 0     | OFF     |
| 1     | ON      |

### 2-F. ID Definition

| ID   | 설명                            |
|------|---------------------------------|
| NULL | Reserved — 반드시 `null` 전송   |

---

## 3. 커맨드 레퍼런스

### 3-1. SITE_STATUS — 사이트 상태 조회

```
[Request]   $SITE_STATUS:site@
[Response]  $SITE_STATUS:site,READY@   → 준비완료 (검사 가능)
            $SITE_STATUS:site,BUSY@    → 검사중
            $SITE_STATUS:site,ERROR@   → 오류
```

**예시**
```
Request:  $SITE_STATUS:1@
Response: $SITE_STATUS:1,READY@
```

---

### 3-2. GET_RECIPE — 레시피 목록 요청

```
[Request]   $GET_RECIPE:site,maxnum,option@
[Response]  $RECIPE_LIST:site,num,recipe1,recipe2,...@
```

| 파라미터 | 설명 |
|----------|------|
| site     | 항상 `1` (사이트 구분 없음) |
| maxnum   | 요청할 최대 레시피 개수 |
| option   | `1` = 생성순, `2` = 최근 사용순 |
| num      | (응답) 실제 레시피 개수 |

**예시**
```
Request:  $GET_RECIPE:1,10,1@
Response: $RECIPE_LIST:1,3,RECIPE1,RECIPE2,RECIPE3@
```

---

### 3-3. RECIPE — 레시피 변경

```
[Request]   $RECIPE:site,recipe@
[Response]  $SETTING:OK@   → 변경 완료
            $SETTING:NG@   → 변경 실패
```

> ⚠️ **제약 조건**
> - `site`는 항상 `1` (1회만 수행)
> - **모든 사이트가 READY 상태일 때만** 변경 가능
> - 검사 중(BUSY) 변경 불가 — 명령 무시됨

**예시**
```
Request:  $RECIPE:1,RECIPE1@
Response: $SETTING:OK@
```

---

### 3-4. TEST — Vision 검사 요청

```
[Request]   $TEST:site,type,target_id@
[Response]  Anti Peeling / Rotate Plate → $RESULT:site,type,P,angle,x,y@
            FPCB                        → $RESULT:site,type,OK@  또는  $RESULT:site,type,NG@
            Final Inspection (Grab)     → $RESULT:site,type,OK@  또는  $RESULT:site,type,NG@
```

> `target_id` = 항상 `null` 전송 (예약 필드)

**모듈 1 예시**
```
$TEST:1,1,null@   → Anti Peeling / High 검사
$TEST:1,2,null@   → Anti Peeling / Low 검사
$TEST:2,1,null@   → ASSY_ROTATE_PLATE_1 / High (UnLoad Calibration)
$TEST:2,2,null@   → ASSY_ROTATE_PLATE_1 / Low  (UnLoad Inspection)
```

**모듈 2 예시**
```
$TEST:1,1,null@   → Final_Inspection / Grab1
$TEST:1,2,null@   → Final_Inspection / Grab2
$TEST:1,3,null@   → Final_Inspection / Grab3
$TEST:1,4,null@   → Final_Inspection / Grab4
$TEST:1,5,null@   → Final_Inspection / Grab5
```

**결과 응답 해석**

| 검사 종류              | 결과 포맷                              | 설명 |
|------------------------|----------------------------------------|------|
| Anti Peeling / Rotate  | `$RESULT:site,type,P,angle,x,y@`       | P=Pass/Fail, angle/x/y = offset 값 |
| CHECK_FPCB             | `$RESULT:site,type,OK@` or `NG@`       | 합/불 판정 |
| Final Inspection       | `$RESULT:site,type,OK@` or `NG@`       | 합/불 판정 |

---

### 3-5. RESET — 시퀀스 강제 리셋 (260420 추가)

```
[Request]   $RESET:site@
[Response]  $RESET:site,OK@   → 리셋 완료
            $RESET:site,NG@   → 리셋 실패 (조명 OFF 중 일부 실패 등)
```

**동작:** 시퀀스 중단 + 상태 READY 복귀 + 조명 OFF (묶음 실행)

> ⚠️ **제약 없음**
> - BUSY(검사 중)에도 허용 — 시퀀스 꼬임 강제 복구가 목적
> - site는 Request/Response 모두에 포함

**예시**
```
Request:  $RESET:1@
Response: $RESET:1,OK@
```

**사용 시나리오**
- TEST 요청 후 응답이 오지 않는 멈춤 상태
- 조명이 켜진 채 시퀀스가 Error 상태로 고착
- 레시피 변경 전 강제 정리 필요

---

### 3-6. LIGHT — 조명 제어

```
[Request]   $LIGHT:site,type,OP@
[Response]  $LIGHT:site,OP@    ← OP만 반환 (type 없음 주의)
```

> ⚠️ Response는 `type`을 포함하지 않고 **현재 조명 Operating 상태(OP)만** 반환

**예시**
```
$LIGHT:1,2,1@   → Site1, Type2(Low) 조명 ON
$LIGHT:1,2,0@   → Site1, Type2(Low) 조명 OFF

Response: $LIGHT:1,1@   (현재 ON 상태)
Response: $LIGHT:1,0@   (현재 OFF 상태)
```

---

## 4. 정상 시퀀스 플로우

```
[Connection]
  Client ──TCP Connect──► Server (192.168.0.1:7701)

[레시피 목록 조회 (선택)]
  Client → $GET_RECIPE:1,10,1@
  Server → $RECIPE_LIST:1,N,recipe1,...@

[레시피 설정 (필요 시 1회)]
  Client → $SITE_STATUS:1@  (모든 사이트 READY 확인)
  Server → $SITE_STATUS:1,READY@
  Client → $RECIPE:1,RECIPE_NAME@
  Server → $SETTING:OK@

[검사 루프]
  Client → $SITE_STATUS:site@
  Server → $SITE_STATUS:site,READY@

  Client → $LIGHT:site,type,1@        ← 조명 ON
  Server → $LIGHT:site,1@

  Client → $TEST:site,type,null@      ← 검사 요청
  Server → $RESULT:site,type,...@     ← 결과 수신

  Client → $LIGHT:site,type,0@        ← 조명 OFF
  Server → $LIGHT:site,0@
```

---

## 5. C# 구현 힌트

```csharp
// 메시지 빌더
public static string Build(string cmd, params object[] args)
    => $"${cmd}:{string.Join(",", args)}@";

// 파서
public static (string cmd, string[] args) Parse(string msg)
{
    // msg: "$CMD:arg1,arg2@"
    var inner = msg.TrimStart('$').TrimEnd('@');
    var split = inner.Split(':', 2);
    return (split[0], split[1].Split(','));
}

// 상태 판별
public enum SiteStatus { READY, BUSY, ERROR }
public enum TestResult { OK, NG, UNKNOWN }

// RESULT 파싱 예시 (Anti Peeling)
// $RESULT:1,1,P,0.5,10.2,-3.1@
//  → site=1, type=1, judge=P, angle=0.5, x=10.2, y=-3.1
```

---

## 6. C++ 구현 힌트

```cpp
// 메시지 빌드
std::string BuildMsg(const std::string& cmd, 
                     const std::vector<std::string>& args) {
    return "$" + cmd + ":" + JoinStr(args, ",") + "@";
}

// 파싱
struct VisionMsg {
    std::string cmd;
    std::vector<std::string> args;
};
VisionMsg ParseMsg(const std::string& raw);
// raw: "$RESULT:1,1,P,0.5,10.2,-3.1@"

// TCP 수신 시 '@' 까지 버퍼 누적 후 파싱 권장
// → 부분 수신 대비: delimiter = '@'
```

---

## 7. 주의사항 체크리스트

- [ ] 레시피 변경 전 **모든 사이트 READY** 확인 필수
- [ ] `target_id`는 항상 문자열 `"null"` 전송
- [ ] LIGHT Response에서 `type` 필드 없음 — 파서 주의
- [ ] RESULT 포맷이 검사 종류마다 다름 — 분기 처리 필요
- [ ] TCP 수신 시 `@`를 메시지 종료 delimiter로 사용
- [ ] BUSY 상태에서 RECIPE 변경 시 명령 무시됨 (재시도 로직 필요)
- [ ] 모듈 1(4-site)과 모듈 2(1-site/5-type)는 Site/Type 정의가 다름
