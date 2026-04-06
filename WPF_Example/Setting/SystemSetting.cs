
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PropertyTools.DataAnnotations;
using FinalVisionProject.UI;
using FinalVisionProject.Utility;
using Newtonsoft.Json;
using System.Reflection;
using FinalVisionProject.Device;
using FinalVisionProject.Properties;

namespace FinalVisionProject.Setting {
    
    public enum ELogType : int {
        Trace = 0,
        Camera = 1,
        LightController = 2,
        TcpConnection = 3,
        Result = 4,
        Image = 5,
        Error = 6,
    }
    
    public partial class SystemSetting {
        public static SystemSetting Handle { get; } = new SystemSetting();
        //setting
        private string SettingIniFile { get; set; } = AppDomain.CurrentDomain.BaseDirectory + @"Setting.ini";
        private string SettingJsonFile { get; set; } = AppDomain.CurrentDomain.BaseDirectory + @"Setting.json";

        //server
        [Category("Connection|Server")]
        public int ServerPort { get; set; } = 7701;   //260326 hbk — 프로토콜 기본 포트

        //recipe
        [Category("Path|Recipe")]
        [DirectoryPath]
        [AutoUpdateText]
        public string RecipeSavePath { get; set; } = AppDomain.CurrentDomain.BaseDirectory + @"Recipe";

        [AutoUpdateText]
        public string CurrentRecipeName { get; set; } = "Default";

        //calibration
        [Category("Path|Calibration")]
        [DirectoryPath]
        [AutoUpdateText]
        public string CalibrationSavePath { get; set; } = AppDomain.CurrentDomain.BaseDirectory + @"Calibration";

        //log
        [Category("Path|Log")]
        [DirectoryPath]
        [AutoUpdateText]
        public string TraceLogSavePath { get; set; } = AppDomain.CurrentDomain.BaseDirectory + @"Trace";
        [DirectoryPath]
        [AutoUpdateText]
        public string ImageSavePath { get; set; } = @"D:\Data\Image";   //260403 hbk -- default image save path
        [DirectoryPath]
        [AutoUpdateText]
        public string ResultSavePath { get; set; } = AppDomain.CurrentDomain.BaseDirectory + @"Result";

        [DirectoryPath]
        [AutoUpdateText]
        public string ErrorSavePath { get; set; } = AppDomain.CurrentDomain.BaseDirectory + @"Error";

        [DirectoryPath]
        [AutoUpdateText]
        public string CameraLogSavePath { get; set; } = AppDomain.CurrentDomain.BaseDirectory + @"Camera";

        [DirectoryPath]
        [AutoUpdateText]
        public string LightControllerPath { get; set; } = AppDomain.CurrentDomain.BaseDirectory + @"LightController";

        [DirectoryPath]
        [AutoUpdateText]
        public string TcpConnectionPath { get; set; } = AppDomain.CurrentDomain.BaseDirectory + @"TcpConnection";

        public int LogDeleteDay { get; set; } = 30;

        //data path
        [Category("Path|MapData")]
        [DirectoryPath]
        [AutoUpdateText]
        public string MapDataLoadPath { get; set; } = AppDomain.CurrentDomain.BaseDirectory + @"Load";
        [DirectoryPath]
        [AutoUpdateText]
        public string MapDataSavePath { get; set; } = AppDomain.CurrentDomain.BaseDirectory + @"Save";


        //inspection image save                                              //260326 hbk
        [Category("Inspection|Image Save")]                                  //260403 hbk
        public bool SaveOriginImage { get; set; } = true;                    //260403 hbk -- 원본 이미지 BMP 저장
        public bool SaveGoodImage { get; set; } = false;                     //260403 hbk -- OK 캡처 이미지 JPG 저장
        public bool SaveNGImage { get; set; } = true;                        //260403 hbk -- NG 캡처 이미지 JPG 저장

        //config

        [Category("System|Enviroment")]
        public int TestTimeOut { get; set; } = 2000;

        public bool AutoLogoutWhenRecvTest { get; set; } = true;

        //260403 hbk -- SaveFailImage 삭제 (원본/캡처 저장으로 대체)


        [Category("System|Localize")]
        [ItemsSourceProperty("LanguageList")]
        public string Language {
            get {
                return _Language;
            }
            set {
                if (value != _Language) {
                    _Language = value;
                    LocalizationResource localRes = App.Current.Resources["DR"] as LocalizationResource;
                    localRes.ChangeLanguage(_Language);
                }
                
            }
        }
        private string _Language = Properties.LocalizationResource.LanguageCodes[0];

        [Browsable(false)]
        public string[] LanguageList { get; } = Properties.LocalizationResource.LanguageCodes;
        
        
        private SystemSetting() {
            Load();
        }

        //260406 hbk -- IMG-04: 이미지 저장 경로의 날짜 폴더 목록 반환 (D-10)
        public string[] GetImageDateFolders() {
            if (!Directory.Exists(ImageSavePath)) return new string[0];
            return Directory.GetDirectories(ImageSavePath)
                .Where(d => {
                    string name = Path.GetFileName(d);
                    return name.Length == 8 && name.All(char.IsDigit);
                })
                .OrderByDescending(d => d)
                .ToArray();
        }

        public string GetCameraImageSavePath(string camName) {
            string filePath = GetLogSavePath(ELogType.Image, DateTime.Now.ToShortDateString());
            if (Directory.Exists(filePath) == false) {
                Directory.CreateDirectory(filePath);
            }
            filePath += string.Format(@"\{0}_{1}{2}", camName, DateTime.Now.ToString("hhmmssff"), DeviceHandler.EXTENSION_SAVE_IMAGE);
            return filePath;
        }

        public string GetResultImageSavePath(string seqName, string actionName) {
            string filePath = SystemHandler.Handle.Setting.GetLogSavePath(ELogType.Image, DateTime.Now.ToShortDateString());
            if (Directory.Exists(filePath) == false) {
                Directory.CreateDirectory(filePath);
            }
            filePath += string.Format(@"\{0}_{1}_{2}{3}", seqName, actionName, DateTime.Now.ToString("hhmmssff"), DeviceHandler.EXTENSION_SAVE_IMAGE);
            return filePath;
        }

        public string GetLogSavePath(ELogType type, params string [] subDirs) {
            string basePath = AppDomain.CurrentDomain.BaseDirectory;
            switch (type) {
                case ELogType.Trace:
                    basePath = TraceLogSavePath;
                    break;
                case ELogType.Camera:
                    basePath = CameraLogSavePath;
                    break;
                case ELogType.Result:
                    basePath = ResultSavePath;
                    break;
                case ELogType.Image:
                    basePath = ImageSavePath;
                    break;
                case ELogType.Error:
                    basePath = ErrorSavePath;
                    break;
                case ELogType.LightController:
                    basePath = LightControllerPath;
                    break;
                case ELogType.TcpConnection:
                    basePath = TcpConnectionPath;
                    break;
            }

            string finalPath = basePath;
            if (subDirs != null) {
                finalPath = Path.Combine(basePath, Path.Combine(subDirs));
            }
            return finalPath;
        }
        
        public void Load() {
            IniFile loadFile = new IniFile();
            if (File.Exists(SettingIniFile)) {
                loadFile.Load(SettingIniFile);
            }
            string group = "Default";
            bool isDirectory = false;
            bool isFile = false;
            PropertyInfo[] props = GetType().GetProperties();
            foreach (var prop in props) {
                string name = prop.Name;
                string type = prop.PropertyType.Name;

                //set group Name 
                Attribute attr = prop.GetCustomAttribute(typeof(CategoryAttribute));
                if (attr != null) {
                    CategoryAttribute catAttr = attr as CategoryAttribute;
                    group = catAttr.Category;
                }
                //directory check
                attr = prop.GetCustomAttribute(typeof(DirectoryPathAttribute));
                if (attr != null) {
                    isDirectory = true;
                }
                else {
                    isDirectory = false;
                }
                /*
                //file check
                attr = prop.GetCustomAttribute(typeof(InputFilePathAttribute));
                if (attr != null) {
                    isFile = true;
                }
                attr = prop.GetCustomAttribute(typeof(OutputFilePathAttribute));
                if (attr != null) {
                    isFile = true;
                }
                */
                try {
                    switch (type) {
                        case "Int32":
                            int iValue = loadFile[group][name].ToInt();
                            prop.SetValue(this, iValue);
                            break;
                        case "Double":
                            double dValue = loadFile[group][name].ToDouble();
                            prop.SetValue(this, dValue);
                            break;
                        case "String":
                            string sValue = loadFile[group][name].ToString();
                            if(isDirectory) {
                                if (Directory.Exists(sValue) == false) Directory.CreateDirectory(sValue);
                            }
                            prop.SetValue(this, sValue);
                            break;
                        case "Boolean":
                            bool bValue = loadFile[group][name].ToBool();
                            prop.SetValue(this, bValue);
                            break;
                        case "Rect":
                            System.Windows.Rect rectVal = loadFile[group][name].ToRect();
                            prop.SetValue(this, rectVal);
                            break;
                        case "Line":
                            Line lineVal = loadFile[group][name].ToLine();
                            prop.SetValue(this, lineVal);
                            break;
                        case "Circle":
                            Circle circleVal = loadFile[group][name].ToCircle();
                            prop.SetValue(this, circleVal);
                            break;
                        case "PropertyItem[]":
                            Sequence.PropertyItem[] propItems = (Sequence.PropertyItem[])prop.GetValue(this);
                            for (int i = 0; i < propItems.Length; i++) {
                                propItems[i].Value = loadFile[group][propItems[i].Name].ToDouble();
                            }
                            break;
                        case "ModelFinderViewModel":
                            ModelFinderViewModel modelView = (ModelFinderViewModel)prop.GetValue(this);
                            modelView.Load(loadFile, group, prop.Name);
                            break;
                    }
                }
                catch (Exception e) {
                    Logging.PrintErrLog((int)ELogType.Error, e.Message);
                }
            }
        }

        public void Save() {
            IniFile saveFile = new IniFile();
            string group = "Default";
            PropertyInfo[] props = GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
            foreach (var prop in props) {
                string name = prop.Name;
                string type = prop.PropertyType.Name;
                
                //set group Name 
                Attribute attr = prop.GetCustomAttribute(typeof(CategoryAttribute));
                if(attr != null) {
                    CategoryAttribute catAttr = attr as CategoryAttribute;
                    group = catAttr.Category;
                }

                switch (type) {
                    case "Int32":
                        saveFile[group][name] = (int)prop.GetValue(this);
                        break;
                    case "Double":
                        saveFile[group][name] = (double)prop.GetValue(this);
                        break;
                    case "String":
                        saveFile[group][name] = (string)prop.GetValue(this);
                        break;
                    case "Boolean":
                        saveFile[group][name] = (bool)prop.GetValue(this);
                        break;
                    case "Rect":
                        saveFile[group][name] = (System.Windows.Rect)prop.GetValue(this);
                        break;
                    case "Line":
                        saveFile[group][name] = (Line)prop.GetValue(this);
                        break;
                    case "Circle":
                        saveFile[group][name] = (Circle)prop.GetValue(this);
                        break;
                    case "PropertyItem[]":
                        Sequence.PropertyItem[] propItems = (Sequence.PropertyItem[])prop.GetValue(this);
                        for (int i = 0; i < propItems.Length; i++) {
                            saveFile[group][propItems[i].Name] = propItems[i].Value;
                        }
                        break;
                    case "ModelFinderViewModel":
                        ModelFinderViewModel modelView = (ModelFinderViewModel)prop.GetValue(this);
                        modelView.Save(saveFile, group, prop.Name);
                        break;
                    default:
                        break;
                }
            }
            saveFile.Save(SettingIniFile);
        }
        /*
        public void SaveToJson() {
            string json = JsonConvert.SerializeObject(this);
            StreamWriter saveFile = File.CreateText(SettingJsonFile);
            saveFile.Write(json);
            saveFile.Flush();
            saveFile.Close();
        }

        public void LoadFromJson() {
            if (File.Exists(SettingJsonFile) == false) return;
            
            StreamReader loadFile = File.OpenText(SettingJsonFile);
            string json = loadFile.ReadToEnd();
            JsonConvert.PopulateObject(json, this);
            loadFile.Close();
        }
        */
    }
}
