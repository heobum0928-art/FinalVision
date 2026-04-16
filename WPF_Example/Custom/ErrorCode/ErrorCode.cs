using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalVisionProject {
    public enum EErrorType {
        CameraDisconnected,


        PropertyChangeFail,
        GrabFail,

        LibraryFail,
        ModelLoadFail,
        ModelSaveFail,

        PatternLoadFail,
        PatternSaveFail,

        ImageSaveFail,

        PatternNotFound,
        ModelNotFound,
        ScoreFail,
        AngleFail,

    }

    //260416 hbk — TCP ERROR 패킷 에러코드 ($ERROR:Site,Code@)
    public enum EVisionErrorCode : int {
        CameraDisconnected = 1,   //260416 hbk — 카메라 연결 끊김
        GrabFail           = 2,   //260416 hbk — 카메라 Grab 실패
        LightError         = 3,   //260416 hbk — 조명 에러
        AliveTimeout       = 4,   //260416 hbk — ALIVE 타임아웃
    }
}
