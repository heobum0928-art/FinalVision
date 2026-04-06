//260403 hbk -- ImageFolderManager: inspection image folder path generation (D-10, D-11)
using System;
using System.IO;
using FinalVisionProject.Setting;

namespace FinalVisionProject.Utility {

    /// <summary>
    /// 검사 이미지 저장 폴더 경로 생성 유틸리티. (D-10: static, D-11: 경로 생성만, 디스크 정리 없음)
    /// </summary>
    public static class ImageFolderManager {

        private static readonly object _lock = new object();   //260403 hbk -- 동시 호출 시 폴더명 충돌 방지

        /// <summary>
        /// 검사 시작 시 날짜>시간 폴더를 생성하고 그 경로를 반환한다. (D-08, D-09)
        /// </summary>
        public static string BeginInspection() {
            lock (_lock) {
                //260403 hbk -- SystemSetting에서 기준 경로 읽기
                string basePath = SystemSetting.Handle.ImageSavePath;

                string dateDir = DateTime.Now.ToString("yyyyMMdd");   //260403 hbk
                string minuteDir = DateTime.Now.ToString("HHmm");    //260403 hbk -- 분 단위 폴더

                string folderPath = Path.Combine(basePath, dateDir, minuteDir);   //260403 hbk
                Directory.CreateDirectory(folderPath);   //260403 hbk -- 이미 존재하면 no-op

                return folderPath;
            }
        }

        /// <summary>
        /// 원본 이미지 저장 경로를 반환한다 (BMP).
        /// 형식: {folderPath}\{shotName}_{OK|NG}.bmp
        /// </summary>
        public static string GetOriginSavePath(string folderPath, string shotName, bool isOk) {
            //260403 hbk -- 원본 이미지 BMP 경로 (초+밀리초 타임스탬프로 같은 분 내 구분)
            string resultStr = isOk ? "OK" : "NG";
            string timeStamp = DateTime.Now.ToString("ss_fff");   //260403 hbk
            return Path.Combine(folderPath, string.Format("{0}_{1}_{2}.bmp", shotName, resultStr, timeStamp));
        }

        /// <summary>
        /// 캡처(어노테이션) 이미지 저장 경로를 반환한다 (JPG).
        /// 형식: {folderPath}\{shotName}_{OK|NG}_capture_{ss_fff}.jpg
        /// </summary>
        public static string GetCaptureSavePath(string folderPath, string shotName, bool isOk) {
            //260403 hbk -- 캡처 이미지 JPG 경로 (초+밀리초 타임스탬프로 같은 분 내 구분)
            string resultStr = isOk ? "OK" : "NG";
            string timeStamp = DateTime.Now.ToString("ss_fff");   //260403 hbk
            return Path.Combine(folderPath, string.Format("{0}_{1}_capture_{2}.jpg", shotName, resultStr, timeStamp));
        }
    }
}
