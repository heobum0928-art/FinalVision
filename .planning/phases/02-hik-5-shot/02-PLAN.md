---
phase: 02-hik-5-shot
plan: 01
type: execute
wave: 1
depends_on: []
files_modified:
  - WPF_Example/Sequence/Param/CameraSlaveParam.cs
  - WPF_Example/Utility/RecipeFileHelper.cs
  - WPF_Example/Custom/Define/ID.cs
  - WPF_Example/Custom/Device/DeviceHandler.cs
  - WPF_Example/Custom/TcpServer/ResourceMap.cs
  - WPF_Example/Setting/SystemSetting.cs
  - WPF_Example/TcpServer/VisionResponsePacket.cs
  - WPF_Example/Custom/Sequence/SequenceHandler.cs
  - WPF_Example/Custom/Sequence/Inspection/Sequence_Inspection.cs
  - WPF_Example/Custom/Sequence/Inspection/Action_Inspection.cs
  - WPF_Example/FinalVision.csproj
autonomous: true
requirements: [REQ-002, REQ-003, REQ-006]

must_haves:
  truths:
    - "260325 hbk 임시 코드가 CameraSlaveParam.cs와 RecipeFileHelper.cs에서 완전히 제거됨"
    - "ESequence.Inspection, EAction.Bolt_One~Assy_Rail_Two_Inspection, ETestType 매핑이 모두 일관되게 정의됨"
    - "TCP TEST:1,type,null@ 수신 시 해당 Shot의 Action이 실행되고 RESULT:1,type,OK@ 또는 NG@ 형식으로 응답"
    - "각 Action이 독립 InspectionParam(ROI, BlobMinArea, BlobMaxArea, DelayMs)을 소유함"
    - "SimpleBlobDetector로 ROI 영역 Blob 검출 후 BlobCount==1이면 OK 판정"
    - "OK/NG 이미지가 D:\\Log\\{날짜}\\{Shot명}_{OK|NG}_{시간}.jpg 에 저장됨"
    - "SIMUL_MODE에서 BackgroundImagePath로 테스트 이미지를 사용하여 동일 플로우 동작"
    - "프로젝트가 에러 없이 빌드됨"
  artifacts:
    - path: "WPF_Example/Custom/Define/ID.cs"
      provides: "ESequence.Inspection, EAction 5종 enum"
      contains: "Bolt_One_Inspection"
    - path: "WPF_Example/Custom/Sequence/Inspection/Sequence_Inspection.cs"
      provides: "Inspection 시퀀스 클래스"
      contains: "class Sequence_Inspection"
    - path: "WPF_Example/Custom/Sequence/Inspection/Action_Inspection.cs"
      provides: "5-Shot 공통 Action 클래스 + InspectionParam + SimpleBlobDetector"
      contains: "class Action_Inspection"
    - path: "WPF_Example/Custom/TcpServer/ResourceMap.cs"
      provides: "ETestType 5종 + Initialize() 매핑"
      contains: "Bolt_One_Inspection"
    - path: "WPF_Example/Custom/Sequence/SequenceHandler.cs"
      provides: "RegisterSequences/Actions/InitializeSequences 교체"
      contains: "SEQ_INSPECTION"
  key_links:
    - from: "Custom/TcpServer/ResourceMap.cs"
      to: "Custom/Sequence/SequenceHandler.cs"
      via: "ETestType -> ACT_BOLT_ONE 등 const 문자열 매핑"
      pattern: "SequenceHandler\\.ACT_BOLT"
    - from: "Custom/Sequence/SequenceHandler.cs"
      to: "Custom/Sequence/Inspection/Action_Inspection.cs"
      via: "RegisterAction(new Action_Inspection(...))"
      pattern: "new Action_Inspection"
    - from: "Action_Inspection.Run()"
      to: "SimpleBlobDetector"
      via: "EStep.BlobDetect에서 OpenCvSharp SimpleBlobDetector 호출"
      pattern: "SimpleBlobDetector\\.Create"
    - from: "TcpServer/VisionResponsePacket.cs Convert()"
      to: "프로토콜 RESULT 포맷"
      via: "TestResultPacket → RESULT:site,type,OK@ 단순 포맷"
      pattern: "GetResultString"
---

<objective>
Phase 2: HIK 카메라 단일화 및 5-Shot 시퀀스 구조 — 전체 구현

기존 CornerAlign 시퀀스를 Inspection 시퀀스로 교체하고, 5개 독립 Action(Bolt 1~3, Assy Rail 1~2)을 구현한다. 각 Action은 TCP TEST 커맨드 1개에 대응하여 Grab → Blob 검출 → OK/NG 판정 → 이미지 저장 → TCP 응답까지 처리한다.

Purpose: FinalVision의 핵심 검사 루프 완성 — 외부 Handler에서 TEST 명령을 보내면 Vision이 촬상+검사+응답하는 end-to-end 플로우 구축.
Output: 빌드 가능한 프로젝트, 5-Shot 시퀀스 동작, TCP TEST/RESULT 연결.
</objective>

<execution_context>
@$HOME/.claude/get-shit-done/workflows/execute-plan.md
@$HOME/.claude/get-shit-done/templates/summary.md
</execution_context>

<context>
@.planning/ROADMAP.md
@.planning/REQUIREMENTS.md
@.planning/STATE.md
@.planning/phases/02-hik-5-shot/02-CONTEXT.md
@.planning/phases/02-hik-5-shot/02-RESEARCH.md
@VisionProtocol_ECi_Moving_V1_0.md

@WPF_Example/Custom/Define/ID.cs
@WPF_Example/Custom/Device/DeviceHandler.cs
@WPF_Example/Custom/TcpServer/ResourceMap.cs
@WPF_Example/Custom/Sequence/SequenceHandler.cs
@WPF_Example/Custom/Sequence/Corner/Sequence_CornerAlign.cs
@WPF_Example/Custom/Sequence/Corner/Action_CornerAlign_Inspection.cs
@WPF_Example/Sequence/Param/CameraSlaveParam.cs
@WPF_Example/Utility/RecipeFileHelper.cs
@WPF_Example/Setting/SystemSetting.cs
@WPF_Example/TcpServer/VisionResponsePacket.cs

<interfaces>
<!-- Key types and contracts the executor needs. Extracted from codebase. -->

From WPF_Example/Custom/Define/ID.cs:
```csharp
namespace FinalVisionProject.Define {
    public enum ESequence : int { Corner_Align = 1 }
    public enum EAction : int { Calibration=1, LT_Inspection=2, RT_Inspection=3, LB_Inspection=4, RB_Inspection=5, Unknown=Int32.MaxValue }
}
```

From WPF_Example/Custom/TcpServer/ResourceMap.cs:
```csharp
namespace FinalVisionProject.Network {
    public enum ESite : int { DEFAULT = 1 }
    public enum ETestType : int { Calibration=1, LT_Inspection=2, RT_Inspection=3, LB_Inspection=4, RB_Inspection=5, Unknown=999 }
    public partial class ResourceMap { void Initialize(); bool SetIdentifier(ref VisionRequestPacket packet); }
}
```

From WPF_Example/Custom/Sequence/SequenceHandler.cs:
```csharp
namespace FinalVisionProject.Sequence {
    public sealed partial class SequenceHandler {
        public const string SEQ_CORNER_ALIGN = "SEQ_CORNER_ALIGN";
        public const string ACT_CALIBRATION = "Calibration";
        public const string ACT_LT_INSPECT = "LT_Inspect";
        // ... ACT_RT_INSPECT, ACT_LB_INSPECT, ACT_RB_INSPECT
        void RegisterSequences(); void RegisterActions(); void InitializeSequences();
    }
}
```

From WPF_Example/Custom/Device/DeviceHandler.cs:
```csharp
namespace FinalVisionProject.Device {
    public sealed partial class DeviceHandler {
        public const string CORNER_ALIGN_CAMERA = "CORNER_ALIGN_CAMERA";
        // RegisterRequiredDevices() uses ECameraType.HIK, ETriggerSource.Software
    }
}
```

From WPF_Example/TcpServer/VisionResponsePacket.cs (TestResultPacket):
```csharp
public class TestResultPacket : VisionResponsePacket {
    public int InspectionType { get; set; }
    public EVisionResultType Result { get; set; }
    public int DieCount { get; set; }
    public int ROICount { get; set; }
    public string Data { get; set; }
    // Convert() 출력: RESULT:site,type,OK/NG,DieCount,ROICount,Data
    // 프로토콜 요구: RESULT:site,type,OK 또는 RESULT:site,type,NG (단순화 필요)
}
```

From WPF_Example/Sequence/Param/CameraSlaveParam.cs:
```csharp
public class CameraSlaveParam : ParamBase, ICameraParam {
    public string DeviceName { get; set; }
    public PropertyItem[] PropertyArray { get; set; }
    public void PasteFromCamera(VirtualCamera camera);
    public virtual void PutImage(Mat image);
    // 260325 hbk 코드: _isLoading 필드, Load() 오버라이드, DeviceName setter guard — 제거 대상
}
```

From WPF_Example/Setting/SystemSetting.cs:
```csharp
public partial class SystemSetting {
    public int ServerPort { get; set; } = 2505;  // → 7701로 변경
    public string ImageSavePath { get; set; }
}
```
</interfaces>
</context>

<tasks>

<task type="auto">
  <name>Task 1: 260325 hbk 제거 + 기반 설정 변경 (ID, DeviceHandler, SystemSetting, ResourceMap, TestResultPacket)</name>
  <files>
    WPF_Example/Sequence/Param/CameraSlaveParam.cs,
    WPF_Example/Utility/RecipeFileHelper.cs,
    WPF_Example/Custom/Define/ID.cs,
    WPF_Example/Custom/Device/DeviceHandler.cs,
    WPF_Example/Setting/SystemSetting.cs,
    WPF_Example/Custom/TcpServer/ResourceMap.cs,
    WPF_Example/TcpServer/VisionResponsePacket.cs
  </files>
  <action>
**모든 변경 라인에 `//260326 hbk` 주석을 반드시 추가한다.**

### 1-A. CameraSlaveParam.cs — 260325 hbk 제거

라인 57~62의 주석 블록과 `private bool _isLoading = false;` 필드를 삭제한다.
라인 75~77의 DeviceName setter 내부 `if (_isLoading) return;` guard 2줄(주석 포함)을 삭제한다.
라인 183~190의 `Load()` 오버라이드 메서드 전체를 삭제한다.

제거 후 DeviceName setter는 다음과 같아야 한다:
```csharp
public string DeviceName {
    get { return _DeviceName; }
    set {
        if (value == null) return;
        _DeviceName = value;
        //선택한 장치의 현재 property 를 가져온다.
        if (pDev == null) return;
        var selectedDev = pDev[value];
        if (selectedDev == null) return;
        this.PasteFromCamera(selectedDev);
    }
}
```

기존 `base.Load()` 호출은 부모 클래스(ParamBase)에서 이미 처리하므로 별도 Load() 오버라이드가 불필요해진다.

### 1-B. RecipeFileHelper.cs — 260325 hbk 제거

라인 139~149의 블록을 삭제한다 (주석 + `string newIniFile = GetRecipeFilePath(newName);` ~ `File.WriteAllText(newIniFile, iniContent);` + 닫는 중괄호).

제거 후 Copy() 메서드는 `CopyFilesRecursively(prevDirPath, newDirPath);` 뒤에 바로 `return true;`로 끝나야 한다.

### 1-C. ID.cs — ESequence, EAction 재정의

기존 `ESequence` 내용을 전부 교체:
```csharp
public enum ESequence : int {
    Inspection = 1,   //260326 hbk
}
```

기존 `EAction` 내용을 전부 교체:
```csharp
public enum EAction : int {
    Bolt_One_Inspection       = 1,   //260326 hbk
    Bolt_Two_Inspection       = 2,   //260326 hbk
    Bolt_Three_Inspection     = 3,   //260326 hbk
    Assy_Rail_One_Inspection  = 4,   //260326 hbk
    Assy_Rail_Two_Inspection  = 5,   //260326 hbk
    Unknown = Int32.MaxValue
}
```

### 1-D. DeviceHandler.cs — 카메라 이름 변경

`CORNER_ALIGN_CAMERA` → `INSPECTION_CAMERA`로 변경:
```csharp
public const string INSPECTION_CAMERA = "INSPECTION_CAMERA";   //260326 hbk
```
`CORNER_ALIGN_CAMERA_WIDTH`, `CORNER_ALIGN_CAMERA_HEIGHT` 상수명도 `INSPECTION_CAMERA_WIDTH`, `INSPECTION_CAMERA_HEIGHT`로 변경한다. 값은 동일(2058, 2456).
`MAX_WIDTH`, `MAX_HEIGHT` 참조도 새 상수명으로 변경한다.
`RegisterRequiredDevices()`의 `SetRequiredDevice()` 호출에서 새 상수명을 사용한다.

### 1-E. SystemSetting.cs — ServerPort 기본값 변경

라인 36: `public int ServerPort { get; set; } = 2505;` → `= 7701;`
```csharp
public int ServerPort { get; set; } = 7701;   //260326 hbk — 프로토콜 기본 포트
```

SystemSetting에 이미지 저장 및 시뮬레이션 관련 설정 추가:
```csharp
[Category("Inspection|Image Save")]                    //260326 hbk
public bool SaveOkImage { get; set; } = false;         //260326 hbk
public bool SaveNgImage { get; set; } = true;          //260326 hbk

[Category("Inspection|Simulation")]                    //260326 hbk
public string SimulImagePath { get; set; } = "";       //260326 hbk — SIMUL_MODE 테스트 이미지 경로
```

### 1-F. ResourceMap.cs (Custom/TcpServer/) — ETestType + Initialize() 교체

`ETestType` enum을 교체:
```csharp
public enum ETestType : int {
    Bolt_One_Inspection       = 1,   //260326 hbk
    Bolt_Two_Inspection       = 2,   //260326 hbk
    Bolt_Three_Inspection     = 3,   //260326 hbk
    Assy_Rail_One_Inspection  = 4,   //260326 hbk
    Assy_Rail_Two_Inspection  = 5,   //260326 hbk
    Unknown = 999
}
```

`Initialize()` 메서드를 교체:
```csharp
public void Initialize() {
    // Camera                                               //260326 hbk
    Add(EResource.Camera,   ESite.DEFAULT, DeviceHandler.INSPECTION_CAMERA);    //260326 hbk
    // Light
    Add(EResource.Light,    ESite.DEFAULT, LightHandler.LIGHT_DEFAULT);
    // Sequence
    Add(EResource.Sequence, ESite.DEFAULT, SequenceHandler.SEQ_INSPECTION);     //260326 hbk

    // Action — 5 Shot mapping                              //260326 hbk
    Add(EResource.Action, ESite.DEFAULT, ETestType.Bolt_One_Inspection,       SequenceHandler.ACT_BOLT_ONE);        //260326 hbk
    Add(EResource.Action, ESite.DEFAULT, ETestType.Bolt_Two_Inspection,       SequenceHandler.ACT_BOLT_TWO);        //260326 hbk
    Add(EResource.Action, ESite.DEFAULT, ETestType.Bolt_Three_Inspection,     SequenceHandler.ACT_BOLT_THREE);      //260326 hbk
    Add(EResource.Action, ESite.DEFAULT, ETestType.Assy_Rail_One_Inspection,  SequenceHandler.ACT_ASSY_ONE);        //260326 hbk
    Add(EResource.Action, ESite.DEFAULT, ETestType.Assy_Rail_Two_Inspection,  SequenceHandler.ACT_ASSY_TWO);        //260326 hbk
}
```

`SetIdentifier()` 메서드는 변경하지 않는다 (기존 로직 그대로 동작).

### 1-G. VisionResponsePacket.cs — TestResultPacket Convert() 단순화

`Convert()` 메서드의 `case EVisionResponseType.Test:` 블록을 단순화한다.
기존: `RESULT:site,type,OK/NG,DieCount,ROICount,Data`
변경 후: `RESULT:site,type,OK` 또는 `RESULT:site,type,NG`

```csharp
case EVisionResponseType.Test:                             //260326 hbk
    TestResultPacket testPacket = packet.AsTestResult();
    msg += CMD_SEND_TEST;
    msg += VisionServer.MSG_CMD_SEPERATOR;
    msg += testPacket.Site.ToString();
    msg += VisionServer.MSG_CONTENTS_SEPERATOR;
    msg += testPacket.InspectionType.ToString();
    msg += VisionServer.MSG_CONTENTS_SEPERATOR;
    msg += testPacket.GetResultString();                   //260326 hbk — OK/NG만 응답
    break;
```

DieCount, ROICount, Data 관련 줄을 삭제한다. TestResultPacket 클래스의 DieCount, ROICount, Data 프로퍼티는 남겨두되 주석으로 사용하지 않음을 표시한다 (다른 곳에서 참조 가능성 있으므로 컴파일 에러 방지).
  </action>
  <verify>
    <automated>cd D:/Project/FinalVision && dotnet build FinalVision.sln --configuration Debug 2>&1 | tail -5</automated>
  </verify>
  <done>
    - CameraSlaveParam.cs에서 _isLoading, Load() 오버라이드, DeviceName guard가 완전 제거됨
    - RecipeFileHelper.cs Copy()에서 ini 경로 치환 블록이 제거됨
    - ID.cs에 ESequence.Inspection=1, EAction 5종(Bolt_One~Assy_Rail_Two) 정의됨
    - DeviceHandler.cs에 INSPECTION_CAMERA 상수 정의됨
    - SystemSetting.cs ServerPort 기본값 7701, SaveOkImage/SaveNgImage 추가됨
    - ResourceMap.cs에 ETestType 5종 + Initialize() 매핑 교체됨
    - VisionResponsePacket.cs TestResult 포맷이 RESULT:site,type,OK/NG로 단순화됨
    - 모든 변경 라인에 //260326 hbk 주석 존재
    - 빌드 성공 (이 시점에서는 SequenceHandler.cs에서 에러 예상 — Task 2에서 해결)
  </done>
</task>

<task type="auto">
  <name>Task 2: Sequence_Inspection + Action_Inspection 신규 생성 및 SequenceHandler 교체</name>
  <files>
    WPF_Example/Custom/Sequence/Inspection/Sequence_Inspection.cs,
    WPF_Example/Custom/Sequence/Inspection/Action_Inspection.cs,
    WPF_Example/Custom/Sequence/SequenceHandler.cs
  </files>
  <action>
**모든 신규/변경 라인에 `//260326 hbk` 주석을 반드시 추가한다.**

### 2-A. 디렉토리 생성

`WPF_Example/Custom/Sequence/Inspection/` 폴더를 생성한다.

### 2-B. Sequence_Inspection.cs — CornerAlignSequence 클론 후 리네임

`Corner/Sequence_CornerAlign.cs`를 복사하여 `Inspection/Sequence_Inspection.cs`로 만든다. 다음을 변경한다:

1. 클래스명: `CornerAlignSequence` → `Sequence_Inspection`, `CornerAlignSequenceContext` → `InspectionSequenceContext`
2. 생성자의 시그니처와 내용은 동일 구조 유지 (ESequence id, string name, string defaultCamera, string defaultLight)
3. Context = new InspectionSequenceContext(this)
4. 나머지 OnCreate(), OnLoad(), OnRelease() 구조 동일
5. namespace는 `FinalVisionProject.Sequence` 유지
6. .csproj에 파일이 자동 포함되는지 확인 (SDK 스타일이면 자동, 구형이면 Include 추가)

```csharp
//260326 hbk — Sequence_Inspection: CornerAlignSequence 대체
namespace FinalVisionProject.Sequence {
    public class InspectionSequenceContext : SequenceContext { ... }
    public class Sequence_Inspection : SequenceBase { ... }
}
```

### 2-C. Action_Inspection.cs — 5-Shot 공통 Action 클래스

`Corner/Action_CornerAlign_Inspection.cs`를 기반으로 신규 작성한다.

**InspectionActionContext** (ActionContext 상속):
```csharp
public class InspectionActionContext : ActionContext {   //260326 hbk
    public InspectionActionContext(ActionBase source) : base(source) { }
}
```

**InspectionParam** (CameraSlaveParam 상속):
```csharp
public class InspectionParam : CameraSlaveParam {   //260326 hbk
    public readonly int ShotIndex;                   //260326 hbk

    [Category("ROI Setting")]                        //260326 hbk
    [Rectangle, Converter(typeof(UI.RectConverter))]
    public System.Windows.Rect ROI { get; set; }     //260326 hbk

    [Category("Blob")]                               //260326 hbk
    public double BlobMinArea { get; set; } = 100;   //260326 hbk
    public double BlobMaxArea { get; set; } = 50000; //260326 hbk

    [Category("General")]                            //260326 hbk
    public int DelayMs { get; set; } = 0;            //260326 hbk

    public InspectionParam(object owner, int shotIndex) : base(owner) {   //260326 hbk
        ShotIndex = shotIndex;
    }

    public override bool Load(IniFile loadFile, string groupName) {       //260326 hbk
        return base.Load(loadFile, groupName);
    }

    public override bool Save(IniFile saveFile, string groupName) {       //260326 hbk
        return base.Save(saveFile, groupName);
    }
}
```

**Action_Inspection** (ActionBase 상속):
```csharp
public class Action_Inspection : ActionBase {   //260326 hbk

    private VirtualCamera _Camera;              //260326 hbk
    private InspectionParam _MyParam;           //260326 hbk
    private Mat _GrabbedImage;                  //260326 hbk
    private bool _IsOK;                         //260326 hbk

    public enum EStep {                         //260326 hbk
        Grab       = 0,
        BlobDetect = 1,
        SaveImage  = 2,
        End        = 3,
    }

    // 생성자
    public Action_Inspection(EAction id, string name, int shotIndex) : base(id, name) {   //260326 hbk
        Context = new InspectionActionContext(this);
        Param   = new InspectionParam(this, shotIndex);
        _MyParam = Param as InspectionParam;
    }

    // OnLoad — 카메라 초기화 + SW Trigger 설정
    // CornerAlignInspectionAction.OnLoad()와 동일 패턴
    public override void OnLoad() {
        _MyParam.ProcessName = Param.OwnerName;   //260326 hbk
        _Camera = SystemHandler.Handle.Devices[_MyParam.DeviceName];   //260326 hbk
        if (_Camera != null) {
            // SIMUL_MODE: SystemSetting.SimulImagePath → BackgroundImagePath 주입  //260326 hbk
            var setting = Setting.SystemSetting.Handle;                             //260326 hbk
            if (!string.IsNullOrEmpty(setting.SimulImagePath))                      //260326 hbk
                _Camera.BackgroundImagePath = setting.SimulImagePath;               //260326 hbk

            if (_Camera.Properties == null) {
                CustomMessageBox.Show(_Camera.Name + " Camera Not Open!", "...", MessageBoxImage.Error);
                return;
            }
            if (!_Camera.Properties.ApplyFromParam(_MyParam)) {
                CustomMessageBox.Show(_Camera.Name + " Camera Property Set Fail!", "...", MessageBoxImage.Error);
            }
            if (!_Camera.SetSoftwareTriggerMode()) {
                CustomMessageBox.Show(_Camera.Name + " SW Trigger Fail!", "...", MessageBoxImage.Error);
            }
        } else {
            CustomMessageBox.Show(_MyParam.DeviceName + " Camera Not Open!", "...", MessageBoxImage.Error);
            return;
        }
        base.OnLoad();
    }

    // Run — EStep 기반 상태머신
    public override ActionContext Run() {
        switch ((EStep)Step) {

            case EStep.Grab:   //260326 hbk
                // 딜레이 적용
                if (_MyParam.DelayMs > 0)
                    System.Threading.Thread.Sleep(_MyParam.DelayMs);
                // SW Trigger로 Grab
                _GrabbedImage = _Camera.GrabImage();
                if (_GrabbedImage == null) {
                    _IsOK = false;
                    Step = (int)EStep.SaveImage;   // Grab 실패 → NG, SaveImage로 건너뜀
                    break;
                }
                Step++;
                break;

            case EStep.BlobDetect:   //260326 hbk
                _IsOK = RunBlobDetection(_GrabbedImage, _MyParam);   //260326 hbk
                Step++;
                break;

            case EStep.SaveImage:   //260326 hbk
                SaveResultImage(_GrabbedImage, _IsOK);   //260326 hbk
                Step++;
                break;

            case EStep.End:   //260326 hbk
                FinishAction(_IsOK ? EContextResult.Pass : EContextResult.Fail);   //260326 hbk
                break;
        }
        return base.Run();
    }

    // Blob 검출 — SimpleBlobDetector, filterByArea만
    private bool RunBlobDetection(Mat image, InspectionParam param) {   //260326 hbk
        if (image == null) return false;

        try {
            // Gray 변환
            Mat gray;
            if (image.Channels() == 1)
                gray = image;
            else {
                gray = new Mat();
                Cv2.CvtColor(image, gray, ColorConversionCodes.BGR2GRAY);
            }

            // ROI 적용 (경계 클램핑)
            var roi = param.ROI;
            int x = Math.Max(0, (int)roi.X);
            int y = Math.Max(0, (int)roi.Y);
            int w = Math.Min((int)roi.Width,  gray.Width  - x);
            int h = Math.Min((int)roi.Height, gray.Height - y);
            if (w <= 0 || h <= 0) return false;   // ROI 무효 → NG

            Mat roiMat = new Mat(gray, new OpenCvSharp.Rect(x, y, w, h));

            // SimpleBlobDetector 설정
            var blobParams = new SimpleBlobDetector.Params();
            blobParams.FilterByArea = true;
            blobParams.MinArea = (float)param.BlobMinArea;
            blobParams.MaxArea = (float)param.BlobMaxArea;
            blobParams.FilterByCircularity = false;
            blobParams.FilterByConvexity   = false;
            blobParams.FilterByInertia     = false;
            blobParams.FilterByColor       = false;

            using (var detector = SimpleBlobDetector.Create(blobParams)) {
                KeyPoint[] keypoints = detector.Detect(roiMat);
                return keypoints.Length == 1;   // BlobCount==1 → OK
            }
        }
        catch (Exception ex) {
            // Blob 검출 실패 → NG
            Logging.PrintLog((int)Setting.ELogType.Error, $"BlobDetect Error: {ex.Message}");   //260326 hbk
            return false;
        }
    }

    // 이미지 저장 — D:\Log\{날짜}\{Shot명}_{OK|NG}_{시간}.jpg
    private void SaveResultImage(Mat image, bool isOK) {   //260326 hbk
        if (image == null) return;

        var setting = Setting.SystemSetting.Handle;
        if (isOK && !setting.SaveOkImage) return;    //260326 hbk
        if (!isOK && !setting.SaveNgImage) return;   //260326 hbk

        try {
            string dateDir  = DateTime.Now.ToString("yyyyMMdd");                    //260326 hbk
            string timeStr  = DateTime.Now.ToString("HHmmss_fff");                  //260326 hbk
            string resultStr = isOK ? "OK" : "NG";                                 //260326 hbk
            string dir = System.IO.Path.Combine(@"D:\Log", dateDir);                //260326 hbk
            System.IO.Directory.CreateDirectory(dir);                               //260326 hbk
            string filePath = System.IO.Path.Combine(dir,
                $"{Name}_{resultStr}_{timeStr}.jpg");                               //260326 hbk
            image.SaveImage(filePath);                                              //260326 hbk
        }
        catch (Exception ex) {
            Logging.PrintLog((int)Setting.ELogType.Error, $"SaveImage Error: {ex.Message}");   //260326 hbk
        }
    }
}
```

**중요: using 문**
파일 상단에 다음 using을 포함한다:
```csharp
using System;
using System.Collections.Generic;
using PropertyTools.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FinalVisionProject.Define;
using FinalVisionProject.Device;
using FinalVisionProject.UI;
using FinalVisionProject.Utility;
using FinalVisionProject.Setting;
using OpenCvSharp;
using System.Windows;
```

**InspectionParam에 ProcessName 프로퍼티를 명시적으로 추가한다** (OnLoad()에서 `_MyParam.ProcessName = Param.OwnerName;` 사용하므로 컴파일 에러 방지):
```csharp
[Category("Common")]                              //260326 hbk
[ReadOnly(true)]
public string ProcessName { get; set; }           //260326 hbk
```

### 2-D. SequenceHandler.cs — RegisterSequences/Actions/InitializeSequences 교체

기존 const 문자열과 메서드를 전부 교체:

```csharp
public sealed partial class SequenceHandler {
    public const string SEQ_INSPECTION = "SEQ_INSPECTION";             //260326 hbk

    public const string ACT_BOLT_ONE   = "Bolt_One_Inspect";           //260326 hbk
    public const string ACT_BOLT_TWO   = "Bolt_Two_Inspect";           //260326 hbk
    public const string ACT_BOLT_THREE = "Bolt_Three_Inspect";         //260326 hbk
    public const string ACT_ASSY_ONE   = "Assy_Rail_One_Inspect";      //260326 hbk
    public const string ACT_ASSY_TWO   = "Assy_Rail_Two_Inspect";      //260326 hbk

    public const int SHOT_INDEX_BOLT_ONE   = 0;                        //260326 hbk
    public const int SHOT_INDEX_BOLT_TWO   = 1;                        //260326 hbk
    public const int SHOT_INDEX_BOLT_THREE = 2;                        //260326 hbk
    public const int SHOT_INDEX_ASSY_ONE   = 3;                        //260326 hbk
    public const int SHOT_INDEX_ASSY_TWO   = 4;                        //260326 hbk

    private void RegisterSequences() {                                 //260326 hbk
        SequenceBuilder.RegisterSequence(
            new Sequence_Inspection(ESequence.Inspection, SEQ_INSPECTION,
                DeviceHandler.INSPECTION_CAMERA, LightHandler.LIGHT_DEFAULT)
        );
    }

    private void RegisterActions() {                                   //260326 hbk
        SequenceBuilder.RegisterAction(
            new Action_Inspection(EAction.Bolt_One_Inspection,       ACT_BOLT_ONE,   SHOT_INDEX_BOLT_ONE),
            new Action_Inspection(EAction.Bolt_Two_Inspection,       ACT_BOLT_TWO,   SHOT_INDEX_BOLT_TWO),
            new Action_Inspection(EAction.Bolt_Three_Inspection,     ACT_BOLT_THREE, SHOT_INDEX_BOLT_THREE),
            new Action_Inspection(EAction.Assy_Rail_One_Inspection,  ACT_ASSY_ONE,   SHOT_INDEX_ASSY_ONE),
            new Action_Inspection(EAction.Assy_Rail_Two_Inspection,  ACT_ASSY_TWO,   SHOT_INDEX_ASSY_TWO)
        );
    }

    private void InitializeSequences() {                               //260326 hbk
        SequenceBuilder seq;
        seq = SequenceBuilder.CreateSequence(ESequence.Inspection);
        seq.AddAction(
            EAction.Bolt_One_Inspection,
            EAction.Bolt_Two_Inspection,
            EAction.Bolt_Three_Inspection,
            EAction.Assy_Rail_One_Inspection,
            EAction.Assy_Rail_Two_Inspection
        );
        RegisterSequence(seq);
    }
}
```

기존 `SEQ_CORNER_ALIGN`, `ACT_CALIBRATION`, `ACT_LT_INSPECT` 등의 const, `DEFAULT_Alg_index`, `Calibration_Alg_Index` 등은 모두 삭제한다.

### 2-E. FinalVision.csproj 업데이트 (필수)

`WPF_Example/FinalVision.csproj`를 읽어서 SDK 스타일(`<Project Sdk="...">`) 여부를 확인한다.

**SDK 스타일이 아닌 경우 (구형 csproj — ToolsVersion 방식):**
기존 Corner 파일의 `<Compile Include="...">` 항목 3개를 찾아 제거한다:
```xml
<!-- 제거 대상 -->
<Compile Include="Custom\Sequence\Corner\Sequence_CornerAlign.cs" />
<Compile Include="Custom\Sequence\Corner\Action_CornerAlign_Inspection.cs" />
<Compile Include="Custom\Sequence\Corner\Action_CornerAlign_Calibration.cs" />
```

신규 Inspection 파일 2개를 추가한다:
```xml
<!-- 추가 — 260326 hbk -->
<Compile Include="Custom\Sequence\Inspection\Sequence_Inspection.cs" />
<Compile Include="Custom\Sequence\Inspection\Action_Inspection.cs" />
```

**SDK 스타일인 경우:** 폴더 내 파일이 자동 포함되므로 삭제된 파일만 확인하면 된다.

`<done>` 확인: csproj에 Inspection/*.cs Compile 항목 존재, Corner/*.cs 항목 없음.

### 2-F. Corner 폴더 처리

기존 `Custom/Sequence/Corner/` 폴더의 3개 파일은 삭제하지 않고 남겨둔다 (참조 제거로 인해 빌드에서 무시되지 않는다면, 기존 파일에서 `CornerAlignSequence`, `CornerAlignInspectionAction`, `CornerAlignCalibrationAction` 클래스가 컴파일되면 ESequence.Corner_Align 등 삭제된 enum 참조로 빌드 에러 발생). **따라서 Corner 폴더의 3개 .cs 파일을 삭제하거나, csproj에서 제외해야 한다.** 삭제가 깔끔하다.

Corner/Action_CornerAlign_Calibration.cs, Corner/Action_CornerAlign_Inspection.cs, Corner/Sequence_CornerAlign.cs를 삭제한다.
  </action>
  <verify>
    <automated>cd D:/Project/FinalVision && dotnet build FinalVision.sln --configuration Debug 2>&1 | tail -10</automated>
  </verify>
  <done>
    - Custom/Sequence/Inspection/ 폴더에 Sequence_Inspection.cs, Action_Inspection.cs 생성됨
    - Sequence_Inspection: SequenceBase 상속, InspectionSequenceContext 포함
    - Action_Inspection: EStep(Grab, BlobDetect, SaveImage, End), Run()에서 SimpleBlobDetector 사용
    - InspectionParam: CameraSlaveParam 상속, ROI/BlobMinArea/BlobMaxArea/DelayMs 프로퍼티
    - SequenceHandler.cs: SEQ_INSPECTION + 5개 ACT const + RegisterSequences/Actions/InitializeSequences 교체
    - Corner 폴더 3개 파일 삭제 완료
    - FinalVision.csproj에 Inspection/*.cs Compile 항목 추가, Corner/*.cs 항목 제거
    - 프로젝트 빌드 성공 (0 errors)
    - 모든 신규/변경 라인에 //260326 hbk 주석 존재
  </done>
</task>

</tasks>

<verification>
1. `dotnet build FinalVision.sln --configuration Debug` — 0 errors
2. 기존 CornerAlign 참조가 코드에 없음 확인: `grep -r "CornerAlign\|Corner_Align\|CORNER_ALIGN" WPF_Example/Custom/ WPF_Example/Setting/ WPF_Example/Sequence/ --include="*.cs"` — 결과 없음 (Corner 폴더 삭제 후)
3. 260326 주석 확인: `grep -c "260326 hbk" WPF_Example/Custom/Define/ID.cs WPF_Example/Custom/TcpServer/ResourceMap.cs WPF_Example/Custom/Sequence/SequenceHandler.cs WPF_Example/Custom/Sequence/Inspection/*.cs WPF_Example/Setting/SystemSetting.cs` — 각 파일에서 1 이상
4. ETestType과 EAction이 1~5로 일치: ResourceMap.cs의 ETestType.Bolt_One_Inspection=1 ~ Assy_Rail_Two_Inspection=5, ID.cs의 EAction도 동일
5. VisionResponsePacket.cs의 TestResult Convert 출력 포맷이 `RESULT:site,type,OK` 또는 `RESULT:site,type,NG`임을 코드 검토로 확인
</verification>

<success_criteria>
- 프로젝트 빌드 성공 (0 errors, 0 warnings 목표, 기존 warning은 허용)
- 260325 hbk 코드 완전 제거 확인
- TCP TEST:1,{1~5},null@ 수신 시 해당 Shot Action이 라우팅되는 구조 완성 (ResourceMap → SequenceHandler → Action_Inspection)
- Action_Inspection.Run()에서 Grab → BlobDetect → SaveImage → End 플로우 구현됨
- SimpleBlobDetector로 ROI 영역 검출, BlobCount==1 → OK 판정
- 이미지 저장: D:\Log\{날짜}\{Shot명}_{OK|NG}_{시간}.jpg
- SystemSetting.SaveOkImage/SaveNgImage로 저장 제어 가능
- ServerPort 기본값 7701
- SIMUL_MODE: VirtualCamera.BackgroundImagePath 경로 설정으로 테스트 가능 (별도 코드 분기 불필요)
</success_criteria>

<output>
After completion, create `.planning/phases/02-hik-5-shot/02-01-SUMMARY.md`
</output>
