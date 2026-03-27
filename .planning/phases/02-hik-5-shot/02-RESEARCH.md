# Phase 2: HIK 카메라 단일화 및 5-Shot 시퀀스 구조 — Research

**Researched:** 2026-03-26
**Domain:** C# WPF / OpenCvSharp SimpleBlobDetector / TcpListener / SequenceBase pattern
**Confidence:** HIGH

---

<user_constraints>
## User Constraints (from CONTEXT.md)

### Locked Decisions

- `CameraSlaveParam.cs`의 `_isLoading` 플래그 및 관련 Load() 오버라이드 제거 (260325 hbk)
- `RecipeFileHelper.cs`의 Copy() 내 ini 경로 치환 코드 제거 (260325 hbk)
- 오늘 날짜(260326) 주석 추가
- ID 재정의: `ESequence.Inspection = 1`, `EAction.Bolt_One_Inspection=1` ~ `EAction.Assy_Rail_Two_Inspection=5`
- 기존 `CornerAlignSequence` → `Sequence_Inspection`으로 rename
- 기존 SequenceHandler.RegisterSequences/RegisterActions/InitializeSequences 구조 유지
- 새 파일 위치: `Custom/Sequence/Inspection/`
- TCP: Vision = Server (TcpListener port 7701), Handler = Client
- 메시지 포맷: `$<CMD>:<args>@`, '@' delimiter까지 버퍼 누적 후 파싱
- `TEST:1,type,null@` 수신 → Shot 실행 → `RESULT:1,type,OK@` 또는 `NG@` 응답
- BUSY 중 TEST 요청 시 BUSY 응답 또는 무시
- `InspectionParam` (CameraSlaveParam 상속): ROI, BlobMinArea, BlobMaxArea, DelayMs
- Blob 판정: filterByArea만 사용, BlobCount == 1 → OK
- 이미지 저장: `D:\Log\{날짜}\{Shot명}_{OK|NG}_{시간}.jpg`, SaveOK/SaveNG UI 토글
- 카메라 이름: `INSPECTION_CAMERA`
- 조명: Handler가 LIGHT 커맨드로 제어 — Vision 내부 ON/OFF 불필요
- LightHandler 기존 구조 유지
- `SIMUL_MODE`: VirtualCamera.GrabImage() 사용, 공용 테스트 이미지 1장
- 각 Action에서 Grab 후 Blob 검출까지 자체 처리

### Claude's Discretion

- TCP Server 구현 위치 (별도 TcpServer 클래스 or 기존 Network 폴더 활용)
- SimpleBlobDetector 파라미터 세부 (filterByArea만)
- InspectionParam의 Save/Load ini 그룹명
- SITE_STATUS BUSY 상태 관리 방식

### Deferred Ideas (OUT OF SCOPE)

- Shot별 다른 테스트 이미지 (SIMUL_MODE)
- 조명 RS232/485 채널 매핑 상세
- 결과 이미지 UI 표시 (Blob 위치 오버레이)
</user_constraints>

---

## Summary

Phase 2는 기존 CornerAlign 시퀀스 구조를 Inspection 시퀀스로 대체하고, 5개의 독립 Action(Shot 1~5)을 추가하는 작업이다. 핵심은 세 가지: (1) 260325 hbk 임시 코드 제거, (2) ID 재정의 및 시퀀스 클래스 교체, (3) TCP TEST 커맨드 수신 시 Shot별 Grab+Blob+응답을 처리하는 구조 연결.

기존 `TcpServer/` 폴더에 이미 완전한 TCP 서버 인프라(`TcpServer.cs`, `VisionServer.cs`, `VisionRequestPacket.cs`, `VisionResponsePacket.cs`)가 구현되어 있다. TCP Server를 새로 구현할 필요가 없고, 기존 구조를 활용하되 `Custom/TcpServer/ResourceMap.cs`의 매핑만 교체하면 된다.

`SimpleBlobDetector`는 OpenCvSharp에 이미 존재하며, `filterByArea = true` 단독 사용으로 충분하다. `VirtualCamera.BackgroundImagePath`를 사용하면 SIMUL_MODE는 파일 경로 설정만으로 자동 동작한다.

**Primary recommendation:** 신규 파일 작성보다 기존 패턴 클론 후 rename이 핵심이다. CornerAlignInspectionAction 패턴을 그대로 복사하여 InspectionAction으로 대체하고, ResourceMap.cs 매핑만 업데이트하면 TCP 플로우가 그대로 연결된다.

---

## Standard Stack

### Core (이미 프로젝트에 존재 — 신규 설치 불필요)

| Library | 위치 | Purpose | 비고 |
|---------|------|---------|------|
| OpenCvSharp (net48) | `ThirdParty/OpenCVSharp/ManagedLib/net48/` | Mat, SimpleBlobDetector, Cv2 | 로컬 DLL 참조 |
| MvCamCtrl.NET | `WPF_Example/` 참조 | HIK 카메라 SDK | 기존 사용 중 |
| System.Net.Sockets | BCL | TcpListener/TcpClient | BCL — 설치 불필요 |
| Newtonsoft.Json | packages.config | 레시피 직렬화 | 기존 사용 중 |
| PropertyTools | NuGet | UI Property Grid | 기존 사용 중 |

### 환경 확인

- OpenCvSharp SimpleBlobDetector: `OpenCvSharp.dll` net48 빌드에 포함 확인됨 (HIGH)
- TcpServer 인프라: `WPF_Example/TcpServer/TcpServer.cs` 이미 완전 구현됨 (HIGH)
- ServerPort 기본값: `SystemSetting.ServerPort = 2505` → **7701로 변경 필요**

---

## Architecture Patterns

### 기존 프로젝트 구조 (확인된 실제 구조)

```
WPF_Example/
├── Custom/
│   ├── Define/ID.cs                   ← ESequence, EAction 정의 (교체 대상)
│   ├── Device/DeviceHandler.cs        ← 카메라 이름 상수 (교체 대상)
│   ├── Sequence/
│   │   ├── Corner/                    ← 기존 (제거 또는 유지)
│   │   │   ├── Sequence_CornerAlign.cs
│   │   │   ├── Action_CornerAlign_Inspection.cs
│   │   │   └── Action_CornerAlign_Calibration.cs
│   │   └── Inspection/               ← 신규 (Phase 2 작업 대상)
│   │       ├── Sequence_Inspection.cs
│   │       └── Action_Inspection.cs
│   ├── SequenceHandler.cs             ← RegisterSequences/Actions 교체 대상
│   └── TcpServer/ResourceMap.cs      ← 매핑 교체 대상
├── TcpServer/                         ← 인프라 (변경 없음)
│   ├── TcpServer.cs                   ← TcpListener 기반 서버 (완전 구현)
│   ├── VisionServer.cs                ← STX/ETX 파싱 래퍼
│   ├── VisionRequestPacket.cs         ← TEST/SITE_STATUS/RECIPE 등 파싱
│   └── VisionResponsePacket.cs        ← RESULT/SITE_STATUS 등 생성
├── Sequence/
│   ├── Action/ActionBase.cs
│   ├── Sequence/SequenceBase.cs
│   └── Param/
│       ├── CameraSlaveParam.cs        ← 260325 제거 대상
│       └── ParamBase.cs
└── Setting/SystemSetting.cs           ← ServerPort 변경 대상
```

---

### Pattern 1: SequenceHandler 등록 패턴 (검증된 실제 코드)

`SequenceHandler.cs`는 3개의 메서드로 구성된다. Phase 2는 이 3곳을 모두 교체한다.

```csharp
// 현재 (교체 전)
private void RegisterSequences() {
    SequenceBuilder.RegisterSequence(
        new CornerAlignSequence(ESequence.Corner_Align, SEQ_CORNER_ALIGN,
            DeviceHandler.CORNER_ALIGN_CAMERA, LightHandler.LIGHT_DEFAULT)
    );
}

private void RegisterActions() {
    SequenceBuilder.RegisterAction(
        new CornerAlignCalibrationAction(EAction.Calibration, ACT_CALIBRATION, 0),
        new CornerAlignInspectionAction(EAction.LT_Inspection, ACT_LT_INSPECT, 1),
        new CornerAlignInspectionAction(EAction.RT_Inspection, ACT_RT_INSPECT, 2),
        new CornerAlignInspectionAction(EAction.LB_Inspection, ACT_LB_INSPECT, 3),
        new CornerAlignInspectionAction(EAction.RB_Inspection, ACT_RB_INSPECT, 4)
    );
}

private void InitializeSequences() {
    SequenceBuilder seq = SequenceBuilder.CreateSequence(ESequence.Corner_Align);
    seq.AddAction(EAction.Calibration, EAction.LT_Inspection, ...);
    RegisterSequence(seq);
}

// Phase 2 교체 후 (목표)
private void RegisterSequences() {
    SequenceBuilder.RegisterSequence(
        new Sequence_Inspection(ESequence.Inspection, SEQ_INSPECTION,
            DeviceHandler.INSPECTION_CAMERA, LightHandler.LIGHT_DEFAULT)
    );
}

private void RegisterActions() {
    SequenceBuilder.RegisterAction(
        new Action_Inspection(EAction.Bolt_One_Inspection,   ACT_BOLT_ONE,   0),
        new Action_Inspection(EAction.Bolt_Two_Inspection,   ACT_BOLT_TWO,   1),
        new Action_Inspection(EAction.Bolt_Three_Inspection, ACT_BOLT_THREE, 2),
        new Action_Inspection(EAction.Assy_Rail_One_Inspection, ACT_ASSY_ONE, 3),
        new Action_Inspection(EAction.Assy_Rail_Two_Inspection, ACT_ASSY_TWO, 4)
    );
}

private void InitializeSequences() {
    SequenceBuilder seq = SequenceBuilder.CreateSequence(ESequence.Inspection);
    seq.AddAction(
        EAction.Bolt_One_Inspection, EAction.Bolt_Two_Inspection,
        EAction.Bolt_Three_Inspection,
        EAction.Assy_Rail_One_Inspection, EAction.Assy_Rail_Two_Inspection
    );
    RegisterSequence(seq);
}
```

---

### Pattern 2: ActionBase의 EStep + Run() 패턴 (검증된 실제 코드)

`Action_CornerAlign_Inspection.cs`에서 확인. Phase 2의 Action_Inspection.cs가 따라야 할 구조:

```csharp
public class Action_Inspection : ActionBase {
    private VirtualCamera _Camera;
    private InspectionParam _MyParam;

    public enum EStep {
        Grab       = 0,
        BlobDetect = 1,
        SaveImage  = 2,
        End        = 3,
    }

    public override void OnLoad() {
        _Camera = SystemHandler.Handle.Devices[_MyParam.DeviceName];
        if (_Camera != null) {
            _Camera.Properties.ApplyFromParam(_MyParam);
            _Camera.SetSoftwareTriggerMode();
        }
        base.OnLoad();
    }

    public override ActionContext Run() {
        switch ((EStep)Step) {
            case EStep.Grab:
                // Thread.Sleep(_MyParam.DelayMs) if needed
                Mat image = _Camera.GrabImage();   // HIK: SW trigger 발사 후 대기
                // store image in context
                Step++;
                break;

            case EStep.BlobDetect:
                // SimpleBlobDetector → keypoints.Count == 1 → OK
                Step++;
                break;

            case EStep.SaveImage:
                // SaveOK/SaveNG 조건부 저장
                Step++;
                break;

            case EStep.End:
                FinishAction(EContextResult.Pass);  // or Fail
                break;
        }
        return base.Run();
    }

    public Action_Inspection(EAction id, string name, int shotIndex) : base(id, name) {
        Context = new InspectionActionContext(this);
        Param   = new InspectionParam(this, shotIndex);
        _MyParam = Param as InspectionParam;
    }
}
```

---

### Pattern 3: CameraSlaveParam 상속 패턴 (검증된 실제 코드)

`CornerAlignInspectionParam`이 `CameraSlaveParam`을 상속하는 패턴을 그대로 사용.

```csharp
// Source: Action_CornerAlign_Inspection.cs
public class CornerAlignInspectionParam : CameraSlaveParam {
    public readonly int AlgIndex;

    [Category("ROI Setting")]
    [Rectangle, Converter(typeof(UI.RectConverter))]
    public System.Windows.Rect HorzRect { get; set; }

    public CornerAlignInspectionParam(object owner, int algIndex) : base(owner) {
        AlgIndex = algIndex;
    }

    public override bool Load(IniFile loadFile, string groupName) {
        return base.Load(loadFile, groupName);
    }
}

// Phase 2 InspectionParam — 동일 상속 구조 유지, 프로퍼티만 교체
public class InspectionParam : CameraSlaveParam {
    public readonly int ShotIndex;

    [Category("ROI Setting")]
    [Rectangle, Converter(typeof(UI.RectConverter))]
    public System.Windows.Rect ROI { get; set; }

    [Category("Blob")]
    public double BlobMinArea { get; set; } = 100;
    public double BlobMaxArea { get; set; } = 50000;

    [Category("General")]
    public int DelayMs { get; set; } = 0;

    // SaveOK, SaveNG — InspectionParam or SystemSetting (Claude's discretion)
    // 권장: SystemSetting에 두어 전체 공유 (레시피별 아님)

    public InspectionParam(object owner, int shotIndex) : base(owner) {
        ShotIndex = shotIndex;
    }
}
```

---

### Pattern 4: TCP 서버 — 기존 구조 활용 (신규 구현 불필요)

기존 `TcpServer.cs`는 `TcpListener` 기반의 완전한 구현이다. `VisionServer.cs`가 그 위에 STX/ETX 파싱을 추가하고, `VisionRequestPacket.cs`가 `TEST:site,type,null@`를 이미 파싱한다.

**Phase 2에서 변경할 곳: `Custom/TcpServer/ResourceMap.cs` (매핑만 업데이트)**

```csharp
// 현재 (교체 전) — Custom/TcpServer/ResourceMap.cs
public void Initialize() {
    Add(EResource.Camera,   ESite.DEFAULT, DeviceHandler.CORNER_ALIGN_CAMERA);
    Add(EResource.Light,    ESite.DEFAULT, LightHandler.LIGHT_DEFAULT);
    Add(EResource.Sequence, ESite.DEFAULT, SequenceHandler.SEQ_CORNER_ALIGN);

    Add(EResource.Action, ESite.DEFAULT, ETestType.Calibration,    SequenceHandler.ACT_CALIBRATION);
    Add(EResource.Action, ESite.DEFAULT, ETestType.LT_Inspection,  SequenceHandler.ACT_LT_INSPECT);
    Add(EResource.Action, ESite.DEFAULT, ETestType.RT_Inspection,  SequenceHandler.ACT_RT_INSPECT);
    Add(EResource.Action, ESite.DEFAULT, ETestType.LB_Inspection,  SequenceHandler.ACT_LB_INSPECT);
    Add(EResource.Action, ESite.DEFAULT, ETestType.RB_Inspection,  SequenceHandler.ACT_RB_INSPECT);
}

// Phase 2 교체 후
public void Initialize() {
    Add(EResource.Camera,   ESite.DEFAULT, DeviceHandler.INSPECTION_CAMERA);
    Add(EResource.Light,    ESite.DEFAULT, LightHandler.LIGHT_DEFAULT);
    Add(EResource.Sequence, ESite.DEFAULT, SequenceHandler.SEQ_INSPECTION);

    Add(EResource.Action, ESite.DEFAULT, ETestType.Bolt_One_Inspection,       SequenceHandler.ACT_BOLT_ONE);
    Add(EResource.Action, ESite.DEFAULT, ETestType.Bolt_Two_Inspection,       SequenceHandler.ACT_BOLT_TWO);
    Add(EResource.Action, ESite.DEFAULT, ETestType.Bolt_Three_Inspection,     SequenceHandler.ACT_BOLT_THREE);
    Add(EResource.Action, ESite.DEFAULT, ETestType.Assy_Rail_One_Inspection,  SequenceHandler.ACT_ASSY_ONE);
    Add(EResource.Action, ESite.DEFAULT, ETestType.Assy_Rail_Two_Inspection,  SequenceHandler.ACT_ASSY_TWO);
}
```

**ETestType enum도 함께 교체 필요** (현재 `TcpServer/ResourceMap.cs`에 정의):
```csharp
// 현재
public enum ETestType : int {
    Calibration   = 1,
    LT_Inspection = 2,
    RT_Inspection = 3,
    LB_Inspection = 4,
    RB_Inspection = 5,
    Unknown = 999
}

// Phase 2 교체 후 (프로토콜 type 1~5에 대응)
public enum ETestType : int {
    Bolt_One_Inspection       = 1,   // $TEST:1,1,null@
    Bolt_Two_Inspection       = 2,   // $TEST:1,2,null@
    Bolt_Three_Inspection     = 3,   // $TEST:1,3,null@
    Assy_Rail_One_Inspection  = 4,   // $TEST:1,4,null@
    Assy_Rail_Two_Inspection  = 5,   // $TEST:1,5,null@
    Unknown = 999
}
```

---

### Pattern 5: HikCamera.GrabImage() 동작 방식 (검증된 실제 코드)

```csharp
// Source: HikCamera.cs 라인 519-545
public override Mat GrabImage() {
    if (CaptureMode == ECaptureModeType.Streaming) return null;

    GrabState = EGrabStateType.Grabbing;
    mStopwatch.Restart();

    if (!SetSoftwareTriggerMode()) return null;

    prevImageCount = imageCount;
    ExecuteSoftwareTrigger();    // CameraHandle.SetCommandValue("TriggerSoftware")

    while (true) {
        if (GrabState == EGrabStateType.Done)   break;       // OnGrabResult 콜백에서 Done 세팅
        if (GrabState == EGrabStateType.Fail)   break;
        if (mStopwatch.ElapsedMilliseconds >= pConfig.GrabTimeOut) return null;
        Thread.Sleep(1);
    }
    return LastImage;   // LastImage는 BackgroundImagePath가 있으면 파일 반환 (SIMUL_MODE)
}
```

**핵심 관찰:**
- `GrabImage()`는 SW Trigger를 발사하고 콜백(`OnGrabResult`) 완료까지 폴링 대기한다.
- `LastImage` getter는 `BackgroundImagePath`가 설정되어 있으면 파일에서 이미지를 읽어 반환한다 — SIMUL_MODE는 이 경로를 활용하면 된다.
- `SetSoftwareTriggerMode()` 내부에서 `StopStream()` → `StartGrabbing()`을 호출하므로, 연속 GrabImage() 호출 전 스트리밍 상태 주의.

---

### Pattern 6: VirtualCamera SIMUL_MODE 동작

```csharp
// Source: VirtualCamera.cs 라인 219-268 (LastImage getter)
public virtual Mat LastImage {
    get {
        lock (Interlock) {
            if (BackgroundImagePath == null) {
                IsGrabFromFile = false;
                return LastGrabImage;     // 실제 카메라 이미지
            }
            // BackgroundImagePath가 설정되어 있으면 해당 폴더의 이미지 파일을 순차 반환
            string selectedImageFile = BackgroundImageFileList[BackgroundImageIndex];
            // ... 파일 로드 후 Properties.Width/Height로 resize
            return resizedImage;
        }
    }
}
```

**SIMUL_MODE 구현 방법**: `InspectionParam` 또는 `SystemSetting`에 `SimulImagePath` 문자열 추가.
`OnLoad()` 시점에 `_Camera.BackgroundImagePath = _MyParam.SimulImagePath` 설정.
실제 카메라가 없을 때 파일에서 자동으로 이미지를 반환하므로 코드 분기 최소화 가능.

---

### Pattern 7: 이미지 저장 패턴

`VirtualCamera.SaveImage(fileName)` 이미 존재 (라인 332~354):
```csharp
public virtual bool SaveImage(string fileName) {
    lock (Interlock) {
        Mat grabbedImage = LastImage;
        if (grabbedImage == null) return false;
        if (Info.ImageType == ECaptureImageType.Gray8) {
            Mat grayImage = new Mat(...);
            Cv2.CvtColor(grabbedImage, grayImage, ColorConversionCodes.BGR2GRAY);
            result = grayImage.SaveImage(fileName);
        }
        else result = grabbedImage.SaveImage(fileName);
    }
}
```

Phase 2에서는 Action 내부에서 Mat을 직접 저장하는 방식도 가능:
```csharp
// Action.Run() 내부 — SaveImage 스텝
string dateDir  = DateTime.Now.ToString("yyyyMMdd");
string timeStr  = DateTime.Now.ToString("HHmmss_fff");
string resultStr = isOK ? "OK" : "NG";
string dir = Path.Combine(@"D:\Log", dateDir);
Directory.CreateDirectory(dir);
string filePath = Path.Combine(dir, $"{actionName}_{resultStr}_{timeStr}.jpg");
grabMat.SaveImage(filePath);   // OpenCvSharp Mat.SaveImage()
```

---

## Don't Hand-Roll

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| TCP Server (TcpListener) | 새 TcpListener 클래스 | 기존 `TcpServer.cs` + `VisionServer.cs` | 이미 완전 구현됨 — Header/Trailer 처리, 멀티 클라이언트, 로깅 포함 |
| Message 파싱 | 직접 Split 로직 | `VisionRequestPacket.Convert(string)` | STX/ETX 제거, CMD 분리, site/type 파싱 이미 구현 |
| Blob 검출 결과 전송 | 수동 string 조립 | `TestResultPacket` + `VisionServer.SendPacket()` | 포맷 오류 방지 |
| Camera Property 동기화 | 수동 카메라 설정 | `_Camera.Properties.ApplyFromParam(_MyParam)` | CameraSlaveParam ↔ VirtualCamera 연결 이미 구현 |
| 파일에서 테스트 이미지 반환 | GrabImage() 오버라이드 | `VirtualCamera.BackgroundImagePath` | LastImage getter가 자동으로 파일 반환 |

---

## 260325 hbk 제거 대상 — 정확한 라인

### 1. `CameraSlaveParam.cs` 제거 대상

**제거할 필드 (라인 62):**
```csharp
private bool _isLoading = false;
```

**제거할 Load() 오버라이드 (라인 183-190):**
```csharp
public override bool Load(IniFile loadFile, string groupName) {
    // 로드 시작 전 플래그 설정 → DeviceName setter에서 PasteFromCamera() 실행 차단 //260325 hbk
    _isLoading = true;
    bool result = base.Load(loadFile, groupName);
    // 로드 완료 후 플래그 해제
    _isLoading = false;
    return result;
}
```

**제거할 DeviceName setter 내부 guard (라인 77-78):**
```csharp
// 로드 중에는 PasteFromCamera() 실행 안 함 //260325 hbk
// UI에서 카메라를 바꿀 때만 카메라 현재값을 가져와야 함
if (_isLoading) return;
```

**제거 후 DeviceName setter는 단순하게:**
```csharp
public string DeviceName {
    get { return _DeviceName; }
    set {
        if (value == null) return;
        _DeviceName = value;
        if (pDev == null) return;
        var selectedDev = pDev[value];
        if (selectedDev == null) return;
        this.PasteFromCamera(selectedDev);
    }
}
```

**제거할 주석 블록 (라인 57-60):**
```csharp
// [BUG FIX] ini 로드 중에는 PasteFromCamera()가 실행되지 않도록 막는 플래그 //260325 hbk
// 문제: rmfoeh
//       카메라 현재값으로 Exposure, Gain 등을 덮어씀.
//       ...
```

### 2. `RecipeFileHelper.cs` 제거 대상

**제거할 블록 (라인 139-149 전체):**
```csharp
//복사된 ini 파일 내 ModelFile 절대 경로를 새 레시피 이름으로 교체 //260325 hbk
// 복사 직후에는 ini 안의 경로가 모두 prevName을 가리키므로 newName으로 치환해야 함
// 미적용 시 두 레시피가 동일한 .mmf 모델 파일을 공유하게 되어 한쪽 수정이 다른 쪽에 영향을 줌
string newIniFile = GetRecipeFilePath(newName);
if (File.Exists(newIniFile))
{
    string iniContent = File.ReadAllText(newIniFile);
    iniContent = iniContent.Replace(prevName, newName);
    File.WriteAllText(newIniFile, iniContent);
}
```

**제거 후 Copy() 메서드는:**
```csharp
public bool Copy(string prevName, string newName, bool forceCopy = false) {
    string prevDirPath = Path.Combine(SystemHandler.Handle.Setting.RecipeSavePath, prevName);
    string newDirPath  = Path.Combine(SystemHandler.Handle.Setting.RecipeSavePath, newName);
    if (Directory.Exists(newDirPath) && (forceCopy == false)) return false;
    CopyFilesRecursively(prevDirPath, newDirPath);
    return true;
}
```

---

## Common Pitfalls

### Pitfall 1: ServerPort 기본값 불일치
**What goes wrong:** `SystemSetting.ServerPort` 기본값이 `2505`이고, 프로토콜은 `7701`을 지정한다. 기본값으로 실행하면 Handler가 연결 불가.
**Why it happens:** Setting.json/ini가 없으면 기본값 2505 사용.
**How to avoid:** `SystemSetting.cs`의 `ServerPort` 기본값을 7701로 변경하거나, Setting.json에 7701을 저장.
**Warning signs:** TcpServerWindow에 "클라이언트 없음" 표시.

### Pitfall 2: ETestType enum이 ResourceMap.cs에 정의되어 있음
**What goes wrong:** `ETestType`은 `Custom/Define/ID.cs`가 아니라 `TcpServer/ResourceMap.cs`에 정의되어 있다. ID.cs만 수정하고 ResourceMap.cs의 ETestType을 빠뜨리면 TCP 라우팅이 깨진다.
**How to avoid:** `TcpServer/ResourceMap.cs`의 `ETestType` enum을 먼저 교체하고, `Custom/TcpServer/ResourceMap.cs`의 `Initialize()` 매핑을 업데이트한다.

### Pitfall 3: SequenceHandler.cs 상단 const 문자열이 ResourceMap의 키
**What goes wrong:** `SequenceHandler.SEQ_CORNER_ALIGN`, `ACT_LT_INSPECT` 등의 const 문자열을 `Custom/TcpServer/ResourceMap.cs`에서 참조한다. SequenceHandler.cs의 const를 변경하면서 ResourceMap.cs 참조도 함께 업데이트해야 한다.
**How to avoid:** SequenceHandler.cs const 변경 시 ResourceMap.cs의 `Add(EResource.Action, ...)` 인자를 동시에 업데이트.

### Pitfall 4: GrabImage() 중 Streaming 모드 충돌
**What goes wrong:** UI Live View가 Streaming 모드이면 `HikCamera.GrabImage()`가 즉시 null 반환 (라인 520).
**Why it happens:** `if (CaptureMode == ECaptureModeType.Streaming) return null;`
**How to avoid:** 검사 시작 전 Live View 스트리밍을 중지하거나, Action.OnLoad()에서 `_Camera.StopStream()` 호출.

### Pitfall 5: SimpleBlobDetector가 밝은 Blob을 검출하지 못함
**What goes wrong:** SimpleBlobDetector는 기본적으로 어두운 영역(dark blobs)을 검출한다.
**Why it happens:** `filterByColor = true`, `blobColor = 0` (dark) 기본값.
**How to avoid:** 자재가 밝으면 `blobColor = 255` (bright)로 설정. 또는 이미지를 반전(Cv2.BitwiseNot)하여 처리.

### Pitfall 6: ROI가 이미지 경계를 벗어나면 예외 발생
**What goes wrong:** `new Mat(mat, new Rect(...))` 에서 Rect가 Mat 크기 초과 시 OpenCv 예외.
**How to avoid:** ROI 적용 전 `Rect.IntersectsWith` 또는 `Cv2.Rect` 클램핑 처리.

### Pitfall 7: 260325 제거 후 ini 로드 순서 버그 재현 가능성
**What goes wrong:** `_isLoading` 플래그를 제거하면 Load() 중 DeviceName setter가 PasteFromCamera()를 호출하여 ini 값을 카메라 현재값으로 덮어쓸 수 있다.
**Why it happens:** ini 파일 내에서 PropertyArray(Exposure 등)보다 DeviceName이 나중에 저장되어 있으면 문제없지만, 순서가 바뀌면 버그 재현.
**How to avoid (Phase 2 범위):** `InspectionParam`의 ini 저장 시 DeviceName을 PropertyArray보다 먼저 저장하도록 그룹명 순서를 제어하거나, ParamBase의 Save 순서를 확인. 이 위험이 낮으면 우선 제거하고 테스트로 검증.

---

## Code Examples

### SimpleBlobDetector — filterByArea만 사용

```csharp
// Source: OpenCvSharp API (Context7 확인 불가 — OpenCvSharp 공식 doc 기반, MEDIUM confidence)
// OpenCvSharp 4.x (net48) SimpleBlobDetector 사용법

using OpenCvSharp;

public static KeyPoint[] DetectBlobs(Mat grayMat, OpenCvSharp.Rect roi,
    double minArea, double maxArea) {

    // ROI 적용 (경계 체크 포함)
    OpenCvSharp.Rect clampedRoi = new OpenCvSharp.Rect(
        Math.Max(0, roi.X),
        Math.Max(0, roi.Y),
        Math.Min(roi.Width,  grayMat.Width  - Math.Max(0, roi.X)),
        Math.Min(roi.Height, grayMat.Height - Math.Max(0, roi.Y))
    );
    Mat roiMat = new Mat(grayMat, clampedRoi);

    var param = SimpleBlobDetector.Params;
    // filterByArea만 사용
    param.FilterByArea      = true;
    param.MinArea           = (float)minArea;
    param.MaxArea           = (float)maxArea;
    param.FilterByCircularity = false;
    param.FilterByConvexity   = false;
    param.FilterByInertia     = false;
    param.FilterByColor       = false;   // 필요 시 true + blobColor=255 for bright

    // Threshold (기본값 사용 또는 조정)
    param.MinThreshold = 10;
    param.MaxThreshold = 200;

    using var detector = SimpleBlobDetector.Create(param);
    KeyPoint[] keypoints = detector.Detect(roiMat);

    roiMat.Dispose();
    return keypoints;
}

// 판정 로직
bool isOK = (keypoints.Length == 1);
```

**주의:** `SimpleBlobDetector.Params`는 static property로, 인스턴스 생성 전에 설정한다. OpenCvSharp 4.x API.

---

### TestResultPacket 응답 빌드 (기존 패턴 활용)

```csharp
// Source: VisionResponsePacket.cs 패턴 참조
// $TEST:1,3,null@ 수신 → $RESULT:1,3,OK@ 응답

var response = new TestResultPacket();
response.Site           = testPacket.Site;        // 1
response.InspectionType = testPacket.TestType;    // 3 (Bolt_Three_Inspection)
response.Result         = isOK ? EVisionResultType.OK : EVisionResultType.NG;
response.DieCount       = keypoints.Length;
response.ROICount       = 1;
response.Data           = "";

// 현재 VisionResponsePacket.Convert(Test) 포맷:
// "RESULT:1,3,P,{DieCount},{ROICount},{Data}" → STX/ETX wrap으로 "$RESULT:1,3,P,1,1,@"

// 단순 OK/NG 응답을 위해 Convert를 그대로 사용
_VisionServer.SendPacket(senderIp, response);
```

**주의:** 현재 `TestResultPacket`의 Convert는 DieCount, ROICount, Data까지 포함하여 전송한다. 프로토콜 문서의 `$RESULT:site,type,OK@`와 포맷이 다를 수 있다. Phase 2에서 `VisionResponsePacket.Convert(Test)` 부분을 단순화하거나, 별도 응답 메서드를 추가하는 방식을 결정해야 한다 (Claude's Discretion).

---

### TCP TEST 커맨드 처리 흐름

```csharp
// TcpServerWindow 또는 SystemHandler의 처리 루프에서
// (기존 GetRecvPacket → switch 구조 재사용)

if (_VisionServer.GetRecvPacket(clientIndex, out VisionRequestPacket packet)) {
    switch (packet.RequestType) {
        case VisionRequestType.Test:
            TestPacket testPkt = packet.AsTest();
            // testPkt.Site = 1, testPkt.TestType = 1~5
            // testPkt.Identifier = SEQ_INSPECTION (ResourceMap이 매핑)
            // testPkt.Identifier2 = ACT_BOLT_ONE 등 (ResourceMap이 매핑)

            // BUSY 체크
            var siteStatusPkt = new SiteStatusResultPacket();
            siteStatusPkt.Site = testPkt.Site;
            if (_IsBusy) {
                siteStatusPkt.Result = EVisionSiteStatusType.Busy;
                _VisionServer.SendPacket(packet.Sender, siteStatusPkt);
                break;
            }

            // 시퀀스 실행 (기존 SequenceHandler.Run() 패턴)
            _IsBusy = true;
            // ... SequenceHandler.Handle.Run(testPkt.Identifier, testPkt.Identifier2)
            // 결과 수신 후 TestResultPacket 응답
            _IsBusy = false;
            break;
    }
}
```

---

## State of the Art

| Old Approach | Current Approach | Impact |
|--------------|------------------|--------|
| CornerAlign 4-Shot | Inspection 5-Shot (Blob) | Action 5개, ResourceMap 5개 |
| ECameraType.Basler (제거됨) | ECameraType.HIK 전용 | HikCamera만 사용 |
| MX Component PLC | 제거됨 | HWC 참조 없음 |
| `_isLoading` 플래그 (260325) | 제거 (260326) | Load() 단순화 |
| Copy() ini 치환 (260325) | 제거 (260326) | Copy() 단순화 |

---

## File List — Phase 2 변경 대상

### 수정 (기존 파일)

| 파일 | 변경 내용 |
|------|---------|
| `Custom/Define/ID.cs` | ESequence, EAction enum 교체 |
| `Custom/Device/DeviceHandler.cs` | CORNER_ALIGN_CAMERA → INSPECTION_CAMERA, 해상도 확인 |
| `Custom/Sequence/SequenceHandler.cs` | const 문자열, RegisterSequences/Actions/Initialize 교체 |
| `Custom/TcpServer/ResourceMap.cs` | ETestType enum + Initialize() 매핑 교체 |
| `TcpServer/ResourceMap.cs` | ETestType enum 교체 (두 파일 모두 확인 필요) |
| `Sequence/Param/CameraSlaveParam.cs` | 260325 hbk 코드 제거 |
| `Utility/RecipeFileHelper.cs` | 260325 hbk 코드 제거 |
| `Setting/SystemSetting.cs` | ServerPort 기본값 2505 → 7701 |

### 신규 생성

| 파일 | 내용 |
|------|------|
| `Custom/Sequence/Inspection/Sequence_Inspection.cs` | CornerAlignSequence 클론 후 rename |
| `Custom/Sequence/Inspection/Action_Inspection.cs` | CornerAlignInspectionAction 기반 Blob 검사 포함 |

### 확인 필요 (변경 가능)

| 파일 | 이슈 |
|------|------|
| `UI/TcpServer/TcpServerWindow.xaml.cs` | TEST 커맨드 처리 로직이 여기 있는지 확인 |
| `Custom/SystemSetting.cs` | 별도 Custom SystemSetting 존재 — SimulImagePath 추가 위치 결정 |
| `Custom/Sequence/Corner/*.cs` | Phase 2에서 제거 vs 유지 결정 필요 (빌드 의존성 확인) |

---

## Environment Availability

| Dependency | Required By | Available | Fallback |
|------------|------------|-----------|---------|
| OpenCvSharp net48 | SimpleBlobDetector | 로컬 DLL (`ThirdParty/OpenCVSharp/ManagedLib/net48/`) | 없음 — 필수 |
| MvCamCtrl.NET (HIK SDK) | HikCamera.Open() | 기존 프로젝트 참조 | VirtualCamera (SIMUL_MODE) |
| Port 7701 | TcpServer | 미확인 — 방화벽/OS 설정 의존 | 다른 포트로 변경 가능 |

---

## Validation Architecture

> .planning/config.json 없음 — nyquist_validation 기본 활성화로 처리

이 Phase는 WPF 애플리케이션으로 자동 단위 테스트 인프라가 없다. 검증은 통합 테스트(수동) 방식으로 수행한다.

### Phase Requirements → Test Map

| Req ID | Behavior | Test Type | 검증 방법 |
|--------|----------|-----------|---------|
| REQ-002 | HIK 카메라 1대 연결 | Manual smoke | 앱 실행 후 카메라 열기, SIMUL_MODE는 이미지 파일 로드 확인 |
| REQ-003 | 5-Shot 순차 실행 | Manual integration | TCP 클라이언트로 TEST:1,1,null@ ~ TEST:1,5,null@ 전송, RESULT 응답 확인 |
| REQ-003 | Blob 검출 → OK/NG 판정 | Manual | SIMUL 이미지 변경하여 OK/NG 전환 확인 |
| REQ-003 | 이미지 저장 | Manual | D:\Log\ 폴더 파일 생성 확인 |

### Wave 0 Gaps
- 자동화 테스트 프레임워크 없음 — 수동 검증으로 대체
- TCP 통신 수동 테스트 도구: `telnet 127.0.0.1 7701` 또는 별도 테스트 클라이언트 사용

---

## Open Questions

1. **`TestResultPacket` 응답 포맷 불일치**
   - 현재 `VisionResponsePacket.Convert(Test)`는 `RESULT:site,type,P/F,DieCount,ROICount,Data` 포맷을 생성한다.
   - 프로토콜 문서는 `RESULT:site,type,OK@` 또는 `RESULT:site,type,NG@`를 요구한다.
   - 해결: `TestResultPacket`의 Convert 로직을 단순화하거나 새 응답 패킷 타입 추가.
   - 권장: Phase 2에서 Convert를 직접 수정하고 `OK/NG` 문자열 반환으로 단순화.

2. **TcpServerWindow.xaml.cs에서 TEST 커맨드를 처리하는지 확인 필요**
   - 현재 코드를 읽지 않았다. ResourceMap이 시퀀스 이름을 식별하지만, 실제 `SequenceHandler.Run()`을 호출하는 위치를 확인해야 한다.
   - 만약 TcpServerWindow.xaml.cs에 처리 로직이 있다면 해당 파일도 수정 대상.

3. **Corner 시퀀스 파일 제거 시 참조 확인**
   - `Corner/Action_CornerAlign_Calibration.cs`가 `SequenceHandler.RegisterActions()`에서 `CornerAlignCalibrationAction`으로 참조된다. Phase 2에서 Calibration Action은 제거되므로 해당 파일도 제거 가능.
   - Corner 폴더 전체 삭제 전 빌드 의존성(using, 참조) 확인 필수.

---

## Sources

### Primary (HIGH confidence)
- `WPF_Example/TcpServer/TcpServer.cs` — TcpListener 구현 전체 확인
- `WPF_Example/TcpServer/VisionServer.cs` — STX/ETX 파싱, SendPacket/GetRecvPacket 확인
- `WPF_Example/TcpServer/VisionRequestPacket.cs` — TEST/LIGHT/RECIPE/SITE_STATUS 파싱 확인
- `WPF_Example/TcpServer/VisionResponsePacket.cs` — TestResultPacket, SiteStatusResultPacket 확인
- `WPF_Example/TcpServer/ResourceMap.cs` — ETestType enum, SiteMap/TestMap 구조 확인
- `WPF_Example/Custom/TcpServer/ResourceMap.cs` — Initialize() 매핑 확인
- `WPF_Example/Custom/Sequence/SequenceHandler.cs` — RegisterSequences/Actions/Initialize 패턴 확인
- `WPF_Example/Custom/Sequence/Corner/Action_CornerAlign_Inspection.cs` — EStep/Run()/OnLoad() 패턴 확인
- `WPF_Example/Device/Camera/Hik/HikCamera.cs` — GrabImage(), ExecuteSoftwareTrigger(), OnGrabResult() 확인
- `WPF_Example/Device/Camera/VirtualCamera.cs` — BackgroundImagePath, LastImage getter, SaveImage() 확인
- `WPF_Example/Sequence/Param/CameraSlaveParam.cs` — 260325 코드 정확한 라인 확인
- `WPF_Example/Utility/RecipeFileHelper.cs` — 260325 Copy() 코드 정확한 라인 확인
- `WPF_Example/Setting/SystemSetting.cs` — ServerPort 기본값 2505 확인
- `VisionProtocol_ECi_Moving_V1_0.md` — TEST/RESULT/SITE_STATUS 포맷 확인

### Secondary (MEDIUM confidence)
- OpenCvSharp SimpleBlobDetector.Params 설정 방식 — 공식 OpenCvSharp wiki 기반 (네트워크 검색 없이 훈련 데이터 기반)

---

## Metadata

**Confidence breakdown:**
- Standard Stack: HIGH — 프로젝트 내 실제 DLL 및 참조 확인
- Architecture patterns: HIGH — 실제 코드 파일 전체 확인
- TCP 인프라: HIGH — TcpServer.cs 완전 구현 확인
- 260325 제거 대상: HIGH — 정확한 라인 확인
- SimpleBlobDetector API: MEDIUM — 코드 파일 확인, Context7 검색 불필요 (OpenCvSharp는 CV2 랩핑으로 API 안정적)
- Pitfalls: HIGH — 실제 코드에서 직접 발견

**Research date:** 2026-03-26
**Valid until:** 2026-04-26 (30일 — OpenCvSharp/HIK SDK는 안정적)
