using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PropertyTools.DataAnnotations;
using FinalVisionProject.Define;
using FinalVisionProject.Network;
using FinalVisionProject.Setting;
using FinalVisionProject.Site;
using FinalVisionProject.Utility;

namespace FinalVisionProject.Sequence {
    
    public class RecipeChangedEventArgs : EventArgs {
        public string RecipeName { get; private set; }

        public RecipeChangedEventArgs(string name) {
            RecipeName = name;
        }
    }

    public delegate void OnRecipeChangedEvent(object sender, RecipeChangedEventArgs arg);

    public sealed partial class SequenceHandler : IDisposable {
        [Browsable(false)]
        public static SequenceHandler Handle { get; } = new SequenceHandler();

        public string ModelName { get; set; }

        [ReadOnly(true)]
        public string Version {
            get {
                return SystemHandler.Handle.Recipes.GetVersion();
            }
        }
        
        [Browsable(false)]
        private readonly Dictionary<ESequence, SequenceBase> Sequences = new Dictionary<ESequence, SequenceBase>();

        [Browsable(false)]
        private SystemSetting pSetting;

        public event OnRecipeChangedEvent OnRecipeChanged;

        private SequenceHandler() {
            RegisterSequences();
            RegisterActions();
            InitializeSequences();
            SequenceBuilder.Free();

            pSetting = SystemHandler.Handle.Setting;
        }

        public void Dispose() {
            ExecOnRelease();
            for(int i = 0; i < Sequences.Count; i++) {
                SequenceBase seq = Sequences.ElementAt(i).Value;
                seq.Release();
            }
        }

        public void RegisterSequence(SequenceBuilder sb) {
            SequenceBase seq = sb.Publish();
            Sequences.Add(seq.ID, seq);
        }
        
        [Browsable(false)]
        [ReadOnly(true)]
        public int Count {
            get => Sequences.Count;
        }

        [Browsable(false)]
        [ReadOnly(true)]
        public string StateSequenceName {
            get {
                return _StateSeqName;
            }
        }

        private string _StateSeqName;
        [Browsable(false)]
        [ReadOnly(true)]
        public EContextState StateAll {
            get {
                EContextState allState = EContextState.Idle;
                _StateSeqName = "All Sequences";

                for (int i = 0; i < Sequences.Count; i++) {
                    SequenceBase seq = Sequences.ElementAt(i).Value;
                    if (seq.State == EContextState.Running) {
                        allState = EContextState.Running;
                        _StateSeqName = seq.Name;
                        return EContextState.Running;
                    }
                    else if(seq.State != EContextState.Idle) {
                        _StateSeqName = seq.Name;
                        allState = seq.State;
                    }
                }
                return allState;
            }
        }

        public bool IsIdle {
            get {
                return StateAll == EContextState.Idle;
            }
        }

        public Dictionary<string, SequenceContext> GetContextDictionary() {
            Dictionary<string, SequenceContext> dict = new Dictionary<string, SequenceContext>();
            for(int i = 0; i < Sequences.Count; i++) {
                SequenceBase seq = Sequences.ElementAt(i).Value;

                dict.Add(seq.Name, seq.Context);
            }
            return dict;
        }
        [Browsable(false)]
        [ReadOnly(true)]
        public SequenceBase this[int index] {
            get { return Sequences.ElementAtOrDefault(index).Value; }
        }
        [Browsable(false)]
        [ReadOnly(true)]
        public SequenceBase this[ESequence id] {
            get {
                if (Sequences.ContainsKey(id) == false) return null;
                return Sequences[id];
            }
        }
        [Browsable(false)]
        [ReadOnly(true)]
        public SequenceBase this[string name] {
            get {
                for(int i = 0; i < Sequences.Count; i++) {
                    SequenceBase seq = Sequences.ElementAt(i).Value;
                    if (seq.Name == name) {
                        return seq;
                    }
                }
                return null;
                //ESequence seqID = (ESequence)Enum.Parse(typeof(ESequence), name);
                //return this[seqID];
            }
        }

        /// <summary>
        /// 지정한 이름의 레시피를 로드합니다.
        /// 로드 성공 시 OnRecipeChanged 이벤트를 발생시키고, 각 시퀀스/액션의 OnLoad를 호출합니다.
        /// </summary>
        public bool LoadRecipe(string name, ERecipeFileType fileType = ERecipeFileType.Ini) {
            bool result = true;
            switch (fileType) {
                case ERecipeFileType.Ini:
                    result = LoadFromIni(name);
                    break;
                case ERecipeFileType.Json:
                    result = LoadFromJson(name);
                    break;
            }
            // 로드 결과와 무관하게 레시피 변경 이벤트 발생 (UI 갱신 목적)
            OnRecipeChanged?.Invoke(this, new RecipeChangedEventArgs(name));

            // 레시피 파일 존재 여부와 무관하게 항상 OnLoad 호출
            // 파일 없을 때도 카메라 참조(_Camera) 초기화가 보장됨
            ExecOnLoad(name);

            //260407 hbk — 레시피 로드 완료 후 Reset용 백업
            if (Sequences.ContainsKey(ESequence.Inspection) && Sequences[ESequence.Inspection] is Sequence_Inspection inspSeq) {
                inspSeq.TakeBackup();
            }

            return result;
        }

        /// <summary>
        /// Site 지정 레시피 로드. INI 전용 (Phase 4 범위).
        /// </summary>
        public bool LoadRecipe(int siteNumber, string name) {
            bool result = LoadFromIni(siteNumber, name);
            OnRecipeChanged?.Invoke(this, new RecipeChangedEventArgs(name));
            ExecOnLoad(name);

            //260407 hbk — 레시피 로드 완료 후 Reset용 백업
            if (Sequences.ContainsKey(ESequence.Inspection) && Sequences[ESequence.Inspection] is Sequence_Inspection inspSeq) {
                inspSeq.TakeBackup();
            }

            return result;
        }

        /// <summary>
        /// 현재 메모리의 파라미터를 지정한 이름으로 저장합니다.
        /// name이 null이면 현재 ModelName(로드된 레시피 이름)으로 덮어씁니다.
        /// </summary>
        public bool SaveRecipe(string name, ERecipeFileType fileType = ERecipeFileType.Ini) {
            bool result = true;
            switch (fileType) {
                case ERecipeFileType.Ini:
                    result = SaveToIni(name);
                    break;
                case ERecipeFileType.Json:
                    result = SaveToJson(name);
                    break;
            }
            return result;
        }

        /// <summary>
        /// Site 지정 레시피 저장. INI 전용 (Phase 4 범위).
        /// </summary>
        public bool SaveRecipe(int siteNumber, string name) {
            return SaveToIni(siteNumber, name);
        }

        /// <summary>
        /// INI 파일에서 레시피를 로드합니다.
        /// 각 시퀀스/액션의 Param을 순서대로 "Param0", "Param1", ... 그룹으로 읽어옵니다.
        /// 로드 완료 후 CurrentRecipeName을 업데이트하여 외부 파일 경로 기준을 맞춥니다.
        /// </summary>
        private bool LoadFromIni(string name) {
            if (string.IsNullOrEmpty(name)) return false;
            string recipeFile = SystemHandler.Handle.Recipes.GetRecipeFilePath(name);
            if (File.Exists(recipeFile) == false) return false;

            IniFile loadFile = new IniFile(recipeFile);
            ModelName = loadFile["Info"]["ModelName"].ToString();
            string Version = loadFile["Info"]["Version"].ToString();

            // 버전 불일치 시 처리 (현재 미구현 - 필요 시 마이그레이션 로직 추가)
            if(Version != SystemHandler.Handle.Recipes.GetVersion()) {
                //not matched version

            }

            // 시퀀스, 액션 순서로 파라미터를 순차 로드 (저장 순서와 반드시 일치해야 함)
            int m = 0;
            for (int i = 0; i < Sequences.Count; i++) {
                for (int j = 0; j < this[i].ActionCount; j++) {
                    ParamBase param = this[i][j].Param;
                    param.Load(loadFile, "Param" + m.ToString());
                    m++;
                }
            }

            // 로드한 레시피 이름을 CurrentRecipeName에 반영
            // ParamBase.GetExternalFilePath()가 이 값을 기준으로 모델/이미지 파일 경로를 결정함
            pSetting.CurrentRecipeName = name;

            return true;
        }


        /// <summary>
        /// Site 지정 INI 로드. Recipe/SiteN/name/main.ini 경로를 사용한다.
        /// 로드 완료 후 SiteContext.CurrentRecipeName 갱신.
        /// </summary>
        private bool LoadFromIni(int siteNumber, string name) {
            if (string.IsNullOrEmpty(name)) return false;
            string recipeFile = SystemHandler.Handle.Recipes.GetRecipeFilePath(siteNumber, name);
            if (File.Exists(recipeFile) == false) return false;

            IniFile loadFile = new IniFile(recipeFile);
            ModelName = loadFile["Info"]["ModelName"].ToString();
            string version = loadFile["Info"]["Version"].ToString();

            if (version != SystemHandler.Handle.Recipes.GetVersion()) {
                // 버전 불일치 (현재 미구현 — 기존 LoadFromIni(string)와 동일 처리)
            }

            int m = 0;
            for (int i = 0; i < Sequences.Count; i++) {
                for (int j = 0; j < this[i].ActionCount; j++) {
                    ParamBase param = this[i][j].Param;
                    param.Load(loadFile, "Param" + m.ToString());
                    m++;
                }
            }

            // 전역 CurrentRecipeName 갱신 (기존 코드 호환)
            pSetting.CurrentRecipeName = name;
            // Site별 CurrentRecipeName 갱신
            SiteManager.Handle[siteNumber - 1].CurrentRecipeName = name;

            return true;
        }

        /// <summary>
        /// 현재 메모리의 파라미터를 INI 파일로 저장합니다.
        /// name이 null이면 현재 ModelName(로드된 레시피 이름)으로 덮어씁니다.
        /// </summary>
        private bool SaveToIni(string name) {
            // name이 지정된 경우 ModelName 갱신 (새 이름으로 저장 또는 다른 이름으로 저장)
            if (name != null) ModelName = name;
            string recipeFile = SystemHandler.Handle.Recipes.GetRecipeFilePath(ModelName);

            // 레시피 디렉터리가 없으면 생성 (새 레시피인 경우)
            string recipeDir = Path.GetDirectoryName(recipeFile);
            if (Directory.Exists(recipeDir) == false) {
                Directory.CreateDirectory(recipeDir);
            }

            IniFile saveFile = new IniFile();
            saveFile["Info"]["ModelName"] = ModelName;
            saveFile["Info"]["Version"] = Version;

            // 시퀀스, 액션 순서로 파라미터를 순차 저장 (로드 순서와 반드시 일치해야 함)
            int m = 0;
            for (int i = 0; i < Sequences.Count; i++) {
                for (int j = 0; j < this[i].ActionCount; j++) {
                    ParamBase param = this[i][j].Param;
                    param.Save(saveFile, "Param" + m.ToString());
                    m++;
                }
            }

            saveFile.Save(recipeFile);
            return true;
        }

        /// <summary>
        /// Site 지정 INI 저장. Recipe/SiteN/name/main.ini 경로를 사용한다.
        /// </summary>
        private bool SaveToIni(int siteNumber, string name) {
            if (name != null) ModelName = name;
            string recipeFile = SystemHandler.Handle.Recipes.GetRecipeFilePath(siteNumber, ModelName);

            string recipeDir = Path.GetDirectoryName(recipeFile);
            if (Directory.Exists(recipeDir) == false) {
                Directory.CreateDirectory(recipeDir);
            }

            IniFile saveFile = new IniFile();
            saveFile["Info"]["ModelName"] = ModelName;
            saveFile["Info"]["Version"] = Version;

            int m = 0;
            for (int i = 0; i < Sequences.Count; i++) {
                for (int j = 0; j < this[i].ActionCount; j++) {
                    ParamBase param = this[i][j].Param;
                    param.Save(saveFile, "Param" + m.ToString());
                    m++;
                }
            }

            saveFile.Save(recipeFile);
            return true;
        }

        private bool LoadFromJson(string name) {
            try {
                string recipeFile = SystemHandler.Handle.Recipes.GetRecipeFilePath(name);
                if (File.Exists(recipeFile) == false) return false;

                using (StreamReader loadFile = File.OpenText(recipeFile)) {
                    string json = loadFile.ReadLine();
                    JsonConvert.PopulateObject(json, this);

                    for (int i = 0; i < Sequences.Count; i++) {
                        for (int j = 0; j < this[i].ActionCount; j++) {
                            ParamBase param = this[i][j].Param;
                            json = loadFile.ReadLine();
                            JsonConvert.PopulateObject(json, param);
                        }
                    }
                }

                pSetting.CurrentRecipeName = name;
            }
            catch (Exception e) {
                Logging.PrintErrLog((int)ELogType.Error, string.Format("Exception ReturnCode:{0}", "LoadFromJson Exception", e.ToString()));
                return false;
            }
            return true;
        }

        private bool SaveToJson(string name = null) {
            if (name != null) ModelName = name;
            try {
                string recipeFile = SystemHandler.Handle.Recipes.GetRecipeFilePath(ModelName);
                string recipeDir = Path.GetDirectoryName(recipeFile);
                if (Directory.Exists(recipeDir) == false) {
                    Directory.CreateDirectory(recipeDir);
                }

                string json = JsonConvert.SerializeObject(this);
                StreamWriter saveFile = File.CreateText(recipeFile);
                saveFile.Write(json);
                saveFile.WriteLine();

                //mmf or ect files
                for (int i = 0; i < Sequences.Count; i++) {
                    for (int j = 0; j < this[i].ActionCount; j++) {
                        ParamBase param = this[i][j].Param;
                        json = JsonConvert.SerializeObject(param);
                        saveFile.Write(json);
                        saveFile.WriteLine();
                    }
                }
                saveFile.Flush();
                saveFile.Close();
            }
            catch (Exception e) {
                Logging.PrintErrLog((int)ELogType.Error, string.Format("Exception ReturnCode:{0}", "SaveToJson Exception", e.ToString()));
                return false;
            }
            return true;
        }

        public void ExecOnRelease() {
            for (int i = 0; i < Sequences.Count; i++) {
                SequenceBase seq = this[i];
                seq.OnRelease();
            }
        }

        public void ExecOnCreate() {
            for(int i = 0; i < Sequences.Count; i++) {
                SequenceBase seq = this[i];
                seq.OnCreate();
            }
        }

        public void ExecOnLoad(string name) {
            for(int i = 0; i < Sequences.Count; i++) {
                SequenceBase seq = this[i];
                seq.OnLoad();
            }
        }
        

        public bool Start(TestPacket packet) {
            string seqName = packet.Identifier;
            SequenceBase seq = this[seqName];
            if (seq == null) return false;
            /*
            if(seq.ID == ESequence.Wafer)
            {
                seq.TargetID = packet.TestID;
            }
            */
            return seq.Start(packet);
        }

        public bool Start(ESequence seqID, EAction beginActionID) {
            if (Sequences.ContainsKey(seqID) == false) return false;
            return Sequences[seqID].Start(beginActionID);
        }

        public bool Stop(ESequence id) {
            if (Sequences.ContainsKey(id) == false) return false;
            return Sequences[id].Stop();
        }

        public bool Pause(ESequence id) {
            if (Sequences.ContainsKey(id) == false) return false;
            return Sequences[id].Pause();
        }

        public EContextState GetSequenceState(ESequence id) {
            if (Sequences.ContainsKey(id) == false) return EContextState.Idle;
            return Sequences[id].State;
        }

        public EContextState GetSequenceState(string name) {
            SequenceBase seq = this[name];
            if (seq == null) return EContextState.Idle;
            return seq.State;
        }
        
    }
}
