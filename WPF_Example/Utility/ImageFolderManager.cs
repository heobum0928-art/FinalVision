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
                //260403 hbk -- D-01: SystemSetting에서 기준 경로 읽기 (하드코딩 금지)
                string basePath = SystemSetting.Handle.ImageSavePath;

                string dateDir = DateTime.Now.ToString("yyyyMMdd");   //260403 hbk
                string timeStr = DateTime.Now.ToString("HHmmss_fff"); //260403 hbk -- D-08: 밀리초 정밀도

                string baseDir    = Path.Combine(basePath, dateDir);
                string folderPath = Path.Combine(baseDir, timeStr);

                //260403 hbk -- D-08: 밀리초 충돌 시 _2, _3 ... 접미사로 고유 폴더 보장
                if (Directory.Exists(folderPath)) {
                    int suffix = 2;
                    string candidate;
                    do {
                        candidate = Path.Combine(baseDir, string.Format("{0}_{1}", timeStr, suffix));
                        suffix++;
                    } while (Directory.Exists(candidate));
                    folderPath = candidate;
                }

                Directory.CreateDirectory(folderPath);   //260403 hbk

                return folderPath;
            }
        }

        /// <summary>
        /// 원본 이미지 저장 경로를 반환한다. (D-03)
        /// 형식: {folderPath}\{shotName}_{OK|NG}.jpg
        /// </summary>
        public static string GetSavePath(string folderPath, string shotName, bool isOk) {
            //260403 hbk
            string resultStr = isOk ? "OK" : "NG";
            return Path.Combine(folderPath, string.Format("{0}_{1}.jpg", shotName, resultStr));
        }

        /// <summary>
        /// 어노테이션 이미지 저장 경로를 반환한다. (D-04)
        /// 형식: {folderPath}\{shotName}_{OK|NG}_annotated.jpg
        /// </summary>
        public static string GetAnnotatedSavePath(string folderPath, string shotName, bool isOk) {
            //260403 hbk
            string resultStr = isOk ? "OK" : "NG";
            return Path.Combine(folderPath, string.Format("{0}_{1}_annotated.jpg", shotName, resultStr));
        }
    }
}
