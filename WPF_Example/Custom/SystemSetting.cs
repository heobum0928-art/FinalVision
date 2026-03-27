using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PropertyTools.DataAnnotations;

namespace FinalVisionProject.Setting {
    //project 별 설정 항목 추가.
    public partial class SystemSetting {
        [PropertyTools.DataAnnotations.Category("Inspection|Site")]
        public int CurrentSiteIndex { get; set; } = 1;  // 1~5 (Site1=1 ... Site5=5)
    }
}
