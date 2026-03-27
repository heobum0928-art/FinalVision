using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalVisionProject.Device {
    /// <summary>
    /// 시스템에서 사용되는 조명 장치를 설정합니다.
    /// Sequece name 과 동일하게 맞춰줘야 한다.
    /// </summary>
    public sealed partial class LightHandler {


        // DEFAULT
        public const string LIGHT_DEFAULT = "Corner_Align";

        /// <summary>
        /// 사용되는 조명 컨트롤러 및 조명 그룹 (제어 단위) 을 설정합니다.
        /// </summary>
        public void RegisterLightController() {

            Controllers.Add(new JPFLightController(0).SetChannelNames(LIGHT_DEFAULT));
            Groups.Add(new LightGroup(LIGHT_DEFAULT).AddChannel(LIGHT_DEFAULT));
        }
    }
}
