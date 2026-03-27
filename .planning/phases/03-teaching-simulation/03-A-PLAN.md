---
phase: 03-teaching-simulation
plan: A
type: execute
wave: 1
depends_on: []
files_modified:
  - WPF_Example/Custom/Sequence/Inspection/Action_Inspection.cs
autonomous: true
requirements:
  - REQ-004
  - REQ-007

must_haves:
  truths:
    - "InspectionParam에 LastOriginalImage/LastAnnotatedImage 프로퍼티가 존재한다"
    - "Grab 후 LastOriginalImage가 저장된다"
    - "BlobDetect 완료 후 LastAnnotatedImage(오버레이 포함 Mat)가 저장된다"
    - "SIMUL 재검사(파라미터 튜닝) 시 LastAnnotatedImage는 갱신되지 않는다"
    - "SIMUL 재검사 시 새로 계산된 오버레이 Mat이 캔버스에 표시된다"
    - "RunBlobDetection이 keypoints를 반환하고 오버레이 Mat을 생성한다"
  artifacts:
    - path: "WPF_Example/Custom/Sequence/Inspection/Action_Inspection.cs"
      provides: "InspectionParam 이미지 버퍼 + Blob 오버레이 생성"
      contains: "LastOriginalImage"
  key_links:
    - from: "Action_Inspection.Run() EStep.Grab"
      to: "InspectionParam.SetOriginalImage()"
      via: "GrabImage() 반환 후 즉시 호출"
      pattern: "SetOriginalImage"
    - from: "Action_Inspection.Run() EStep.BlobDetect (비SIMUL)"
      to: "InspectionParam.SetAnnotatedImage()"
      via: "RunBlobDetection 반환 Mat으로 호출"
      pattern: "SetAnnotatedImage"
    - from: "Action_Inspection.Run() EStep.BlobDetect (SIMUL)"
      to: "DisplayToBackground(annotated)"
      via: "SetAnnotatedImageTemp 경유 또는 직접 전달 — LastAnnotatedImage 변경 없이 캔버스만 갱신"
      pattern: "SetAnnotatedImageTemp\|DisplayToBackground"
---

<objective>
`InspectionParam`에 Shot별 이미지 버퍼(원본/오버레이)를 추가하고, `RunBlobDetection`이 Blob 위치에 원을 그린 오버레이 Mat을 반환하도록 수정한다.

Purpose: Shot 뷰어 UI(Plan C)가 각 Action의 원본/측정 이미지를 조회할 수 있는 데이터 계층을 구축한다.
Output: `InspectionParam`에 `LastOriginalImage`, `LastAnnotatedImage` 추가. `Action_Inspection.Run()`에서 두 이미지를 각 Step에서 저장. SIMUL 재검사 시에는 `LastAnnotatedImage` 잠금을 유지하면서 새 오버레이를 캔버스에 표시하는 임시 경로 제공.
</objective>

<execution_context>
@$HOME/.claude/get-shit-done/workflows/execute-plan.md
@$HOME/.claude/get-shit-done/templates/summary.md
</execution_context>

<context>
@.planning/PROJECT.md
@.planning/ROADMAP.md
@.planning/phases/03-teaching-simulation/03-CONTEXT.md

<interfaces>
<!-- Action_Inspection.cs 현재 구조 (수정 대상) -->

namespace FinalVisionProject.Sequence

public class InspectionParam : CameraSlaveParam
{
    public readonly int ShotIndex;
    public string ProcessName { get; set; }
    public System.Windows.Rect ROI { get; set; }
    public double BlobMinArea { get; set; } = 100;
    public double BlobMaxArea { get; set; } = 50000;
    public int DelayMs { get; set; } = 0;

    // 추가 대상:
    // public Mat LastOriginalImage { get; private set; }
    // public Mat LastAnnotatedImage { get; private set; }
    // public void SetOriginalImage(Mat img)
    // public void SetAnnotatedImage(Mat img)        ← 비SIMUL 전용 (잠금)
    // public void SetAnnotatedImageTemp(Mat img)    ← SIMUL 재검사용 (LastAnnotatedImage 변경 없음)
}

public class Action_Inspection : ActionBase
{
    public enum EStep { Grab=0, BlobDetect=1, SaveImage=2, End=3 }

    // Run() — EStep.Grab에서 _GrabbedImage 획득
    // RunBlobDetection(Mat image, InspectionParam param) → bool (현재)
    // 수정 후: RunBlobDetection → (bool isOk, Mat annotated) 또는
    //           RunBlobDetection 내부에서 Cv2.Circle로 원 그린 Mat 반환
}

<!-- VirtualCamera (참조용) -->
// _Camera.GrabImage() → Mat
// _Camera.BackgroundImagePath (string) — SIMUL 이미지 경로

<!-- OpenCvSharp Blob 오버레이 (사용할 API) -->
// KeyPoint[] keypoints = detector.Detect(roiMat);
// keypoints[i].Pt  → Point2f (ROI 내 좌표)
// keypoints[i].Size → float (직경)
// Cv2.Circle(annotated, center, radius, Scalar(0,255,0), 2)
// ROI 좌표 → 전체 이미지 좌표: center.X += roi.X, center.Y += roi.Y
</interfaces>
</context>

<tasks>

<task type="auto">
  <name>Task A-1: InspectionParam 이미지 버퍼 추가</name>
  <files>WPF_Example/Custom/Sequence/Inspection/Action_Inspection.cs</files>
  <read_first>
    - WPF_Example/Custom/Sequence/Inspection/Action_Inspection.cs (전체 — 현재 InspectionParam 구조 파악)
  </read_first>
  <action>
`InspectionParam` 클래스의 `#region properties` 블록 끝에 다음을 추가한다.
모든 신규 라인에 `//260326 hbk // 설명` 형식 주석 필수.

```csharp
// Shot 이미지 버퍼   //260326 hbk // Shot별 원본/오버레이 이미지 보관
public Mat LastOriginalImage { get; private set; }    //260326 hbk // Grab 시 저장 (항상 최신 원본)
public Mat LastAnnotatedImage { get; private set; }   //260326 hbk // 실검사 완료 시 1회 저장, 이후 잠금

// 잠금 플래그: 실운영(SIMUL_MODE 미정의) BlobDetect 완료 후 true   //260326 hbk
private bool _AnnotatedImageLocked = false;           //260326 hbk

public void SetOriginalImage(Mat img)   //260326 hbk // Grab 완료 후 호출
{
    LastOriginalImage?.Dispose();        //260326 hbk // 이전 이미지 해제
    LastOriginalImage = img?.Clone();    //260326 hbk // 클론 저장 (원본 생명주기 독립)
}

public void SetAnnotatedImage(Mat img)   //260326 hbk // 실검사(비SIMUL) BlobDetect 완료 후 1회만 호출
{
    if (_AnnotatedImageLocked) return;   //260326 hbk // SIMUL 재검사 시 덮어쓰기 방지
    LastAnnotatedImage?.Dispose();       //260326 hbk
    LastAnnotatedImage = img?.Clone();   //260326 hbk
    _AnnotatedImageLocked = true;        //260326 hbk // 한 번 저장 후 잠금
}

// SIMUL 재검사용 임시 이미지 전달 — LastAnnotatedImage 변경 없음   //260326 hbk
// 호출자(Action_Inspection)가 이 Mat을 직접 DisplayToBackground로 전달한다   //260326 hbk
private Mat _AnnotatedImageTemp = null;   //260326 hbk

public void SetAnnotatedImageTemp(Mat img)   //260326 hbk // SIMUL 재검사 시 캔버스용 임시 저장
{
    _AnnotatedImageTemp?.Dispose();       //260326 hbk
    _AnnotatedImageTemp = img?.Clone();   //260326 hbk // LastAnnotatedImage 변경 없음
}

public Mat GetAnnotatedImageTemp()   //260326 hbk // 캔버스 표시 후 호출자가 가져감
{
    return _AnnotatedImageTemp;   //260326 hbk
}

public void ResetAnnotatedImageLock()   //260326 hbk // 실운영 새 사이클 시작 시 잠금 해제
{
    _AnnotatedImageLocked = false;       //260326 hbk
}
```

주의: `SetOriginalImage`는 `Mat.Clone()`으로 저장해야 한다. `GrabImage()`가 반환한 Mat은 카메라 내부 버퍼일 수 있으므로 복사 필수.
  </action>
  <verify>
    <automated>grep -n "LastOriginalImage\|LastAnnotatedImage\|SetOriginalImage\|SetAnnotatedImage\|SetAnnotatedImageTemp\|_AnnotatedImageLocked" "WPF_Example/Custom/Sequence/Inspection/Action_Inspection.cs"</automated>
  </verify>
  <acceptance_criteria>
    - `LastOriginalImage { get; private set; }` 존재
    - `LastAnnotatedImage { get; private set; }` 존재
    - `SetOriginalImage(Mat img)` 메서드 존재
    - `SetAnnotatedImage(Mat img)` 메서드 존재 (잠금 포함)
    - `SetAnnotatedImageTemp(Mat img)` 메서드 존재 (잠금 없음, SIMUL 전용)
    - `GetAnnotatedImageTemp()` 메서드 존재
    - `_AnnotatedImageLocked` 필드 존재
    - `ResetAnnotatedImageLock()` 메서드 존재
    - 모든 신규 라인에 `//260326 hbk` 포함
  </acceptance_criteria>
  <done>InspectionParam에 이미지 버퍼 프로퍼티 + 메서드가 컴파일 가능하게 추가됨. SIMUL/비SIMUL 두 경로 모두 지원.</done>
</task>

<task type="auto">
  <name>Task A-2: RunBlobDetection 오버레이 반환 + Run() 이미지 버퍼 저장</name>
  <files>WPF_Example/Custom/Sequence/Inspection/Action_Inspection.cs</files>
  <read_first>
    - WPF_Example/Custom/Sequence/Inspection/Action_Inspection.cs (Task A-1 완료 후 현재 상태)
  </read_first>
  <action>
**1. RunBlobDetection 시그니처 변경**

현재: `private bool RunBlobDetection(Mat image, InspectionParam param)`
변경: `private (bool isOk, Mat annotated) RunBlobDetection(Mat image, InspectionParam param)`

메서드 내부 로직 변경:
- `detector.Detect(roiMat)` 후 `keypoints` 획득 (기존 동일)
- 오버레이 Mat 생성:
```csharp
// 오버레이용 컬러 Mat 생성 (전체 이미지 크기)   //260326 hbk
Mat annotated = new Mat();                                          //260326 hbk
if (image.Channels() == 1)                                          //260326 hbk
    Cv2.CvtColor(image, annotated, ColorConversionCodes.GRAY2BGR);  //260326 hbk // Grayscale → BGR 변환
else                                                                //260326 hbk
    annotated = image.Clone();                                      //260326 hbk

bool isOk = (keypoints.Length == 1);   //260326 hbk
Scalar circleColor = isOk ? new Scalar(0, 255, 0) : new Scalar(0, 0, 255);   //260326 hbk // OK=초록, NG=빨강

foreach (var kp in keypoints)          //260326 hbk // Blob 위치에 원 그리기
{
    // ROI 내 좌표 → 전체 이미지 좌표로 변환   //260326 hbk
    int cx = (int)kp.Pt.X + x;        //260326 hbk // x = ROI X 오프셋
    int cy = (int)kp.Pt.Y + y;        //260326 hbk // y = ROI Y 오프셋
    int radius = Math.Max(5, (int)(kp.Size / 2));   //260326 hbk
    Cv2.Circle(annotated, new OpenCvSharp.Point(cx, cy), radius, circleColor, 2);   //260326 hbk
}

// ROI 사각형 표시   //260326 hbk
Cv2.Rectangle(annotated,
    new OpenCvSharp.Point(x, y),
    new OpenCvSharp.Point(x + w, y + h),
    new Scalar(255, 255, 0), 1);   //260326 hbk // ROI 경계 = 노랑

return (isOk, annotated);   //260326 hbk
```

- 오류 발생 시 `return (false, null)` 반환
- ROI 무효 시 `return (false, null)` 반환 (기존 early-return 유지)

**2. Run() EStep.Grab 수정**

`_GrabbedImage = _Camera.GrabImage();` 직후 (null 체크 전):
```csharp
_MyParam.SetOriginalImage(_GrabbedImage);   //260326 hbk // 원본 이미지 버퍼 저장
```

**3. Run() EStep.BlobDetect 수정**

현재: `_IsOK = RunBlobDetection(_GrabbedImage, _MyParam);`
변경 — SIMUL/비SIMUL 경로 분기:

```csharp
var (isOk, annotated) = RunBlobDetection(_GrabbedImage, _MyParam);   //260326 hbk
_IsOK = isOk;                                                          //260326 hbk
#if SIMUL_MODE
// SIMUL 재검사: LastAnnotatedImage 변경 없이 캔버스용 임시 저장   //260326 hbk
_MyParam.SetAnnotatedImageTemp(annotated);   //260326 hbk // LastAnnotatedImage 잠금 유지
// 캔버스 표시는 GrabAndDisplay(MainView)에서 처리 — DisplayToBackground(annotated) 호출
#else
// 실운영: 최초 1회만 LastAnnotatedImage 갱신 (이후 잠금)   //260326 hbk
_MyParam.SetAnnotatedImage(annotated);   //260326 hbk
#endif
annotated?.Dispose();   //260326 hbk // Clone됐으므로 로컬 해제
Step++;
```

`annotated`를 캔버스에 표시하는 경로(SIMUL 분기):
- `MainView.GrabAndDisplay`에서 `DisplayToBackground`를 이미 `_GrabbedImage`(또는 오버레이)로 호출하는지 확인한다.
- SIMUL 분기에서 `GetAnnotatedImageTemp()`로 임시 Mat을 가져와 `DisplayToBackground`에 전달하거나, `RunBlobDetection`이 반환한 `annotated`를 그대로 `DisplayToBackground`에 넘기면 된다.
- 구체적 연결 위치: `GrabAndDisplay` 내부 `DisplayToBackground` 호출 직전에 `#if SIMUL_MODE` 분기로 `param.GetAnnotatedImageTemp() ?? mat`을 사용한다. Plan C/D 실행자는 이 흐름을 따른다.

**주의사항:**
- `x`, `y`, `w`, `h` 변수는 ROI 클램핑 후 계산된 값 (기존 코드의 로컬 변수 재사용)
- Tuple 반환 시 C# 7.0 이상 Value Tuple 사용 (`(bool, Mat)` 반환)
- .NET Framework 4.8은 ValueTuple 지원함
  </action>
  <verify>
    <automated>grep -n "SetOriginalImage\|SetAnnotatedImage\|SetAnnotatedImageTemp\|RunBlobDetection\|annotated\|isOk\|circleColor\|SIMUL_MODE" "WPF_Example/Custom/Sequence/Inspection/Action_Inspection.cs"</automated>
  </verify>
  <acceptance_criteria>
    - `RunBlobDetection` 반환 타입이 `(bool isOk, Mat annotated)` 또는 동등한 tuple 형식
    - EStep.Grab에서 `SetOriginalImage` 호출 존재
    - EStep.BlobDetect에서 `#if SIMUL_MODE` / `#else` / `#endif` 분기 존재
    - SIMUL 분기: `SetAnnotatedImageTemp` 호출 존재 (`SetAnnotatedImage` 호출 없음)
    - 비SIMUL 분기: `SetAnnotatedImage` 호출 존재
    - `Cv2.Circle(` 호출 존재 (오버레이 그리기)
    - `Cv2.Rectangle(` 호출 존재 (ROI 표시)
    - `//260326 hbk` 주석이 모든 신규 라인에 존재
  </acceptance_criteria>
  <done>BlobDetect 완료 시 keypoint 원 + ROI 사각형이 그려진 오버레이 Mat이 생성됨. 비SIMUL 시 LastAnnotatedImage에 저장(잠금), SIMUL 재검사 시 임시 경로로 캔버스에만 표시됨. 빌드 성공.</done>
</task>

</tasks>

<verification>
빌드 성공 확인:
- Visual Studio 2022에서 전체 솔루션 빌드 (`Ctrl+Shift+B`)
- 오류 0개 (경고 허용)
- `InspectionParam`의 `LastOriginalImage`, `LastAnnotatedImage`, `SetAnnotatedImageTemp` 프로퍼티/메서드 IntelliSense에서 접근 가능
</verification>

<success_criteria>
- InspectionParam에 `LastOriginalImage`, `LastAnnotatedImage`, `SetOriginalImage()`, `SetAnnotatedImage()`, `SetAnnotatedImageTemp()`, `GetAnnotatedImageTemp()`, `ResetAnnotatedImageLock()` 추가
- Grab 시 `SetOriginalImage()` 호출, BlobDetect 완료 시 비SIMUL=`SetAnnotatedImage()` / SIMUL=`SetAnnotatedImageTemp()` 분기 호출
- 오버레이 Mat에 검출 Blob 원(OK=초록/NG=빨강) + ROI 사각형(노랑) 그려짐
- SIMUL 재검사 시 LastAnnotatedImage 잠금 유지, 캔버스는 새 오버레이로 갱신됨
- 전체 빌드 성공
</success_criteria>

<output>
완료 후 `.planning/phases/03-teaching-simulation/03-A-SUMMARY.md` 생성
</output>
