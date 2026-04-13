using FinalVisionProject.Device;
using FinalVisionProject.Sequence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalVisionProject.Network {
    public enum ESite : int {

        DEFAULT = 1,
    }

    public enum ETestType : int {
        Bolt_One_Inspection       = 1,   //260326 hbk
        Bolt_Two_Inspection       = 2,   //260326 hbk
        Bolt_Three_Inspection     = 3,   //260326 hbk
        Assy_Rail_One_Inspection  = 4,   //260326 hbk
        Assy_Rail_Two_Inspection  = 5,   //260326 hbk
        Unknown = 999
    }


    /// <summary>
    /// 통신 프로토콜의 zone, site, type 등의 정보를 시스템의 sequence, action, light 이름 등으로 치환하기 위한 map을 구성합니다.
    /// </summary>
    public partial class ResourceMap {
        public void Initialize() {
            // Camera                                                                                          //260326 hbk
            Add(EResource.Camera,   ESite.DEFAULT, DeviceHandler.INSPECTION_CAMERA);                          //260326 hbk
            // Light
            Add(EResource.Light,    ESite.DEFAULT, LightHandler.LIGHT_DEFAULT);
            // Sequence
            Add(EResource.Sequence, ESite.DEFAULT, SequenceHandler.SEQ_INSPECTION);                           //260326 hbk

            // Action — 5 Shot mapping                                                                        //260326 hbk
            Add(EResource.Action, ESite.DEFAULT, ETestType.Bolt_One_Inspection,      SequenceHandler.ACT_BOLT_ONE);    //260326 hbk
            Add(EResource.Action, ESite.DEFAULT, ETestType.Bolt_Two_Inspection,      SequenceHandler.ACT_BOLT_TWO);    //260326 hbk
            Add(EResource.Action, ESite.DEFAULT, ETestType.Bolt_Three_Inspection,    SequenceHandler.ACT_BOLT_THREE);  //260326 hbk
            Add(EResource.Action, ESite.DEFAULT, ETestType.Assy_Rail_One_Inspection, SequenceHandler.ACT_ASSY_ONE);    //260326 hbk
            Add(EResource.Action, ESite.DEFAULT, ETestType.Assy_Rail_Two_Inspection, SequenceHandler.ACT_ASSY_TWO);    //260326 hbk
        }

        public bool SetIdentifier(ref VisionRequestPacket packet) {
            switch (packet.RequestType) {
                case VisionRequestType.Light:
                    LightPacket lightPacket = packet.AsLight();
                    lightPacket.Identifier = Find(EResource.Light, (ESite)lightPacket.Site);
                    //lightPacket.Identifier = Find(EResource.Light, (ESite)lightPacket.Site, (ETestType)lightPacket.TestType);
                    if (lightPacket.On) {
                        lightPacket.Identifier2 = Find(EResource.Action, (ESite)lightPacket.Site, (ETestType)lightPacket.TestType);
                    }
                    // 01.12 else 구문 추가.
                    else
                        lightPacket.Identifier2 = Find(EResource.Action, (ESite)lightPacket.Site, (ETestType)lightPacket.TestType);
                    break;
                case VisionRequestType.RecipeChange:
                case VisionRequestType.RecipeGet:
                    //no identifier
                    break;
                case VisionRequestType.SiteStatus:
                    packet.Identifier = Find(EResource.Sequence, (ESite)packet.Site);
                    break;
                case VisionRequestType.Test:
                    TestPacket testPacket = packet.AsTest();
                    testPacket.Identifier = Find(EResource.Sequence, (ESite)testPacket.Site);
                    testPacket.Identifier2 = Find(EResource.Action, (ESite)testPacket.Site, (ETestType)testPacket.TestType);
                    break;
                case VisionRequestType.DryRun:   //260413 hbk — 리소스 매핑 불필요
                case VisionRequestType.Time:     //260413 hbk
                case VisionRequestType.Trace:    //260413 hbk
                    break;
                case VisionRequestType.Unknown:
                    break;
            }

            return true;
        }
    }

    
}
