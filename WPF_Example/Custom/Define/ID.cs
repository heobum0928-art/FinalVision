using PropertyTools.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalVisionProject.Define {

    /// <summary>
    /// 시퀀스의 ID(쓰레드 단위 = 카메라)
    /// </summary>
    public enum ESequence : int {
        Inspection = 1,   //260326 hbk — Final_Inspection 시퀀스 (카메라 1대 5-Shot)
    }

    /// <summary>
    /// 각 시퀀스에 종속되는 action의 ID (쓰레드가 수행할 수 있는 동작 단위)
    /// </summary>
    public enum EAction : int {
        Bolt_One_Inspection       = 1,   //260326 hbk — TCP type:1, 볼트 1번 검사
        Bolt_Two_Inspection       = 2,   //260326 hbk — TCP type:2, 볼트 2번 검사
        Bolt_Three_Inspection     = 3,   //260326 hbk — TCP type:3, 볼트 3번 검사
        Assy_Rail_One_Inspection  = 4,   //260326 hbk — TCP type:4, 어셈블리 레일 1번 검사
        Assy_Rail_Two_Inspection  = 5,   //260326 hbk — TCP type:5, 어셈블리 레일 2번 검사

        Unknown = Int32.MaxValue
    }

}
