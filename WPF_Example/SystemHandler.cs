using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using FinalVisionProject.Utility;
using FinalVisionProject.Setting;
using FinalVisionProject.Sequence;
using FinalVisionProject.Network;
using FinalVisionProject.Device;
using System.Windows;
using FinalVisionProject.UI;
using FinalVisionProject.UserData;
using FinalVisionProject.Login;
using FinalVisionProject.Properties;

namespace FinalVisionProject {

    public sealed partial class SystemHandler {

        public static string ProjectName { get; } = "FinalVision";

        public static SystemHandler Handle { get; } = new SystemHandler();

        public SystemSetting Setting { get; private set; }

        public LoginManager Login { get; private set; }

        public DeviceHandler Devices { get; private set; }

        public LightHandler Lights { get; private set; }

        public RecipeFiles Recipes { get; private set; }

        public SequenceHandler Sequences { get; private set; }

        public VisionServer Server { get; private set; }

        public GlobalUserData UserData { get; private set; }

        public LocalizationResource Localize { get; set; }

        private Thread mSystemThread;
        private Thread mAliveThread;  //260413 hbk — ALIVE 하트비트 스레드
        private bool IsTerminated = false;

        public bool IsInitializeFail { get; private set; } = false;
        public bool IsReleased { get; private set; } = false;

        private SystemHandler() {
            //1. system setting
            Setting = SystemSetting.Handle;

            //2. recipe file info
            Recipes = RecipeFiles.Handle;

            //3. user data
            UserData = new GlobalUserData();

            //4. log
            Logging.SetLog((int)ELogType.Trace, Enum.GetName(typeof(ELogType), ELogType.Trace), Setting.GetLogSavePath(ELogType.Trace));
            Logging.SetLog((int)ELogType.Camera, Enum.GetName(typeof(ELogType), ELogType.Camera), Setting.GetLogSavePath(ELogType.Camera));
            Logging.SetLog((int)ELogType.TcpConnection, Enum.GetName(typeof(ELogType), ELogType.TcpConnection), Setting.GetLogSavePath(ELogType.TcpConnection));
            Logging.SetLog((int)ELogType.Result, Enum.GetName(typeof(ELogType), ELogType.Result), Setting.GetLogSavePath(ELogType.Result));
            Logging.SetLog((int)ELogType.Error, Enum.GetName(typeof(ELogType), ELogType.Error), Setting.GetLogSavePath(ELogType.Error));
            Logging.SetLog((int)ELogType.LightController, Enum.GetName(typeof(ELogType), ELogType.LightController), Setting.GetLogSavePath(ELogType.LightController));
            Logging.Start();

            //5. device
            Devices = DeviceHandler.Handle;
            EInitializeResult result = Devices.Initialize();
            if (result != EInitializeResult.Success) {
                IsInitializeFail = true;
                CustomMessageBox.Show("Error", "Camera Initialize Fail", MessageBoxImage.Error);
            }

            //6. lights
            Lights = LightHandler.Handle;
            
        }
        
        //after constructor
        public void Initialize() {

            //1. light
            if (Lights.Initialize() == false) {
                IsInitializeFail = true;
                CustomMessageBox.Show("Error", "Light Controller Open Fail", MessageBoxImage.Error);
            }

            //2. sequence
            Sequences = SequenceHandler.Handle;
            
            //3. server
            Server = new VisionServer();
            
            //4. Main Thread
            mSystemThread = new Thread(SystemProcess);
            mSystemThread.Priority = ThreadPriority.Highest;
            mSystemThread.Name = "SystemProcess";
            mSystemThread.Start();

            //5. Alive Thread  //260413 hbk
            mAliveThread = new Thread(AliveProcess);
            mAliveThread.Priority = ThreadPriority.BelowNormal;
            mAliveThread.Name = "AliveProcess";
            mAliveThread.IsBackground = true;
            mAliveThread.Start();

            //6. Light Error => TCP ERROR 패킷 송신  //260416 hbk
            Lights.OnError += (args) => {
                SendErrorPacket(1, EVisionErrorCode.LightError);
            };

            //login
            Login = LoginManager.Handle;

            //on sequence create
            Sequences.ExecOnCreate();

            //collect recipe
            Recipes.CollectRecipe();   //260403 hbk — RecipeSavePath 루트 기준 (Site 하위 경로 사용 안 함)

            //localization resource
            Localize = App.Current.Resources["DR"] as LocalizationResource;
            //Localize.LanguageChanged += LanguageChanged;

            Logging.PrintLog((int)ELogType.Trace, "[SYSTEM] Initialized");
        }

        public bool LoadRecipe(string recipeName) {
            bool result = Sequences.LoadRecipe(recipeName, ERecipeFileType.Ini);
            if (result) {
                Logging.PrintLog((int)ELogType.Trace, "[RECIPE] Loaded : {0}", recipeName);
            }
            else {
                Logging.PrintLog((int)ELogType.Trace, "[RECIPE] Load fail : {0}", recipeName);
            }
            return result;
        }

        public bool LoadRecipe(int siteNumber, string recipeName) {   //260331 hbk — Site 경로(Site1)로 로드 (SaveRecipe(1,name)과 경로 일치)
            bool result = Sequences.LoadRecipe(siteNumber, recipeName);
            if (result) {
                Logging.PrintLog((int)ELogType.Trace, "[RECIPE] Loaded Site{0} : {1}", siteNumber, recipeName);
            }
            else {
                Logging.PrintLog((int)ELogType.Trace, "[RECIPE] Load fail Site{0} : {1}", siteNumber, recipeName);
            }
            return result;
        }
        

        public void Release() {
            Setting.Save();

            Devices.Dispose();

            Sequences.Dispose();
            
            Server.Dispose();

            Lights.Release();

            IsTerminated = true;
            mSystemThread.Join(1000);
            if (mAliveThread != null) mAliveThread.Join(1000);  //260413 hbk

            Logging.PrintLog((int)ELogType.Trace, "[SYSTEM] Released");

            Logging.Stop();
            IsReleased = true;
        }

        private void SystemProcess(object param) {
            while (IsTerminated == false) {
                MainRun();
                Thread.Sleep(1);
            }
        }
        
    }
}
