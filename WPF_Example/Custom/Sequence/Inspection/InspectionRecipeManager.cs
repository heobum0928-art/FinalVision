using System;
using System.Collections.Generic;
using System.IO;
using FinalVisionProject.Utility;

//260401 hbk — InspectionRecipeManager: Shot-FAI 2계층 레시피 Save/Load + CRUD
namespace FinalVisionProject.Sequence
{
    /// <summary>
    /// Shot-FAI 2계층 구조 레시피 관리자.
    /// INI 섹션 구조:
    ///   [SHOTS]           → ShotCount
    ///   [SHOT_0]          → ShotConfig 파라미터 (ParamBase 반사 직렬화) + FAICount
    ///   [SHOT_0_FAI_0]    → FAIConfig 파라미터 (ParamBase 반사 직렬화)
    ///   [SHOT_0_FAI_1]    → ...
    ///   [SHOT_1]          → ...
    /// </summary>
    public class InspectionRecipeManager
    {
        #region constants
        private const string SEC_SHOTS = "SHOTS";           //260401 hbk — Shot 개수 섹션
        private const string KEY_SHOT_COUNT = "ShotCount";  //260401 hbk
        private const string KEY_FAI_COUNT = "FAICount";    //260401 hbk
        #endregion

        #region properties
        public List<ShotConfig> Shots { get; private set; } = new List<ShotConfig>();   //260401 hbk
        #endregion

        #region constructors
        public InspectionRecipeManager()   //260401 hbk
        {
        }
        #endregion

        #region methods — Save/Load

        /// <summary>
        /// Shot-FAI 구조를 INI 파일로 저장.
        /// 기존 main.ini에 Shot/FAI 섹션을 추가하는 방식.
        /// </summary>
        public bool Save(IniFile ini)   //260401 hbk
        {
            // Shot 개수 섹션
            ini[SEC_SHOTS][KEY_SHOT_COUNT] = Shots.Count;

            for (int s = 0; s < Shots.Count; s++)
            {
                var shot = Shots[s];
                string shotSection = $"SHOT_{s}";

                // ShotConfig → ParamBase 반사 직렬화
                shot.Save(ini, shotSection);

                // FAI 개수 (ParamBase에 없으므로 수동 추가)
                ini[shotSection][KEY_FAI_COUNT] = shot.FAIs.Count;

                // FAI 저장
                for (int f = 0; f < shot.FAIs.Count; f++)
                {
                    string faiSection = $"SHOT_{s}_FAI_{f}";
                    shot.FAIs[f].Save(ini, faiSection);
                }
            }
            return true;
        }

        /// <summary>
        /// INI 파일에서 Shot-FAI 구조 로드.
        /// owner 파라미터: Action/Sequence 등 ParamBase의 Owner가 될 객체.
        /// </summary>
        public bool Load(IniFile ini, object owner)   //260401 hbk
        {
            Shots.Clear();

            if (!ini.ContainsSection(SEC_SHOTS))
                return false;

            int shotCount = ini[SEC_SHOTS][KEY_SHOT_COUNT].ToInt(0);

            for (int s = 0; s < shotCount; s++)
            {
                string shotSection = $"SHOT_{s}";
                if (!ini.ContainsSection(shotSection))
                    continue;

                var shot = new ShotConfig(owner, s);
                shot.Load(ini, shotSection);

                int faiCount = ini[shotSection][KEY_FAI_COUNT].ToInt(0);

                for (int f = 0; f < faiCount; f++)
                {
                    string faiSection = $"SHOT_{s}_FAI_{f}";
                    if (!ini.ContainsSection(faiSection))
                        continue;

                    var fai = new FAIConfig(owner, f);
                    fai.ShotIndex = s;
                    fai.Load(ini, faiSection);
                    shot.FAIs.Add(fai);
                }

                Shots.Add(shot);
            }
            return true;
        }

        /// <summary>
        /// 파일 경로로 직접 Save.
        /// </summary>
        public bool SaveToFile(string filePath)   //260401 hbk
        {
            try
            {
                var ini = new IniFile();

                // 기존 파일이 있으면 로드하여 병합 (다른 섹션 보존)
                if (File.Exists(filePath))
                    ini.Load(filePath);

                // 기존 SHOT/FAI 섹션 제거 (재작성)
                CleanShotSections(ini);

                Save(ini);

                string dir = Path.GetDirectoryName(filePath);
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                ini.Save(filePath);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// 파일 경로로 직접 Load.
        /// </summary>
        public bool LoadFromFile(string filePath, object owner)   //260401 hbk
        {
            try
            {
                if (!File.Exists(filePath))
                    return false;

                var ini = new IniFile(filePath);
                return Load(ini, owner);
            }
            catch (Exception)
            {
                return false;
            }
        }

        #endregion

        #region methods — CRUD

        public ShotConfig AddShot(object owner, string shotName = null, double zPos = 0)   //260401 hbk
        {
            int index = Shots.Count;
            var shot = new ShotConfig(owner, index)
            {
                ShotName = shotName ?? $"Shot_{index + 1}",
                ZPosition = zPos
            };
            Shots.Add(shot);
            return shot;
        }

        public FAIConfig AddFAI(int shotIndex, object owner, string faiName = null)   //260401 hbk
        {
            if (shotIndex < 0 || shotIndex >= Shots.Count) return null;

            var shot = Shots[shotIndex];
            int faiIdx = shot.FAIs.Count;
            var fai = new FAIConfig(owner, faiIdx)
            {
                FAIName = faiName ?? $"FAI_{faiIdx + 1:D2}",
                ShotIndex = shotIndex
            };
            shot.FAIs.Add(fai);
            return fai;
        }

        public bool RemoveShot(int shotIndex)   //260401 hbk
        {
            if (shotIndex < 0 || shotIndex >= Shots.Count) return false;
            Shots.RemoveAt(shotIndex);
            return true;
        }

        public bool RemoveFAI(int shotIndex, int faiIndex)   //260401 hbk
        {
            if (shotIndex < 0 || shotIndex >= Shots.Count) return false;
            var shot = Shots[shotIndex];
            if (faiIndex < 0 || faiIndex >= shot.FAIs.Count) return false;
            shot.FAIs.RemoveAt(faiIndex);
            return true;
        }

        /// <summary>
        /// 전체 FAI 개수.
        /// </summary>
        public int TotalFAICount   //260401 hbk
        {
            get
            {
                int count = 0;
                foreach (var shot in Shots)
                    count += shot.FAIs.Count;
                return count;
            }
        }

        /// <summary>
        /// 전체 FAI를 플랫 리스트로 반환 (글로벌 인덱스 순).
        /// </summary>
        public List<FAIConfig> GetAllFAIs()   //260401 hbk
        {
            var list = new List<FAIConfig>();
            foreach (var shot in Shots)
                list.AddRange(shot.FAIs);
            return list;
        }

        /// <summary>
        /// 모든 FAI 결과 초기화.
        /// </summary>
        public void ClearAllResults()   //260401 hbk
        {
            foreach (var shot in Shots)
                shot.ClearAllResults();
        }

        #endregion

        #region methods — private

        /// <summary>
        /// INI에서 기존 SHOT/FAI 섹션 제거.
        /// </summary>
        private void CleanShotSections(IniFile ini)   //260401 hbk
        {
            ini.Remove(SEC_SHOTS);
            var keysToRemove = new List<string>();
            foreach (var key in ini.Keys)
            {
                if (key.StartsWith("SHOT_", StringComparison.OrdinalIgnoreCase))
                    keysToRemove.Add(key);
            }
            foreach (var key in keysToRemove)
                ini.Remove(key);
        }

        #endregion
    }
}
