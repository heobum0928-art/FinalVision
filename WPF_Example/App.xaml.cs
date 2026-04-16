using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using FinalVisionProject.UI;
using FinalVisionProject.Utility;
using FinalVisionProject.Setting;
using FinalVisionProject.Properties;
using System.Globalization;

namespace FinalVisionProject {
    /// <summary>
    /// App.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class App : Application {
        Mutex mMutex;

        public App() {
            //260409 hbk -- 3종 예외 핸들러 등록 (UI/비UI/Task)
            this.Dispatcher.UnhandledException += this.Dispatcher_UnhandledException;
            AppDomain.CurrentDomain.UnhandledException += this.CurrentDomain_UnhandledException;
            TaskScheduler.UnobservedTaskException += this.TaskScheduler_UnobservedTaskException;
        }

        private void Application_Startup(object sender, StartupEventArgs e) {
            var resource = App.Current.Resources["DR"] as LocalizationResource;
            if (resource != null) {
                resource.ChangeLanguage("ko-KR");
            }

            CultureInfo c = new CultureInfo("ko-KR");
            var lang = System.Windows.Markup.XmlLanguage.GetLanguage(c.IetfLanguageTag);
            FrameworkElement.LanguageProperty.OverrideMetadata(typeof(FrameworkElement), new FrameworkPropertyMetadata(lang));

            var view = new MainWindow();
            view.Show();
        }

        protected override void OnStartup(StartupEventArgs e) {
            string mutexName = AppDomain.CurrentDomain.FriendlyName;
            mMutex = new Mutex(true, mutexName, out bool createNew);

            if (!createNew) {
                CustomMessageBox.Show("Duplicate execution error", "Program is already running.", MessageBoxImage.Error, true);
                Shutdown();
                return;
            }
            base.OnStartup(e);
        }

        private void Dispatcher_UnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e) {
            //260409 hbk -- UI 스레드 예외: 덤프 저장 후 종료
            CrashDumpHelper.WriteDump(e.Exception, "DispatcherUnhandledException");
            CustomMessageBox.Show("Unhandled Exception", e.Exception.ToString(), MessageBoxImage.Error, true, false);
            e.Handled = true;

            if (SystemHandler.Handle.IsReleased == false) {
                if (MainWindow != null) {
                    MainWindow.Close();
                }

            }
        }

        //260409 hbk -- 비UI 스레드 예외 (ThreadPool, Thread 등)
        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e) {
            if (e.ExceptionObject is Exception ex) {
                CrashDumpHelper.WriteDump(ex, "AppDomain.UnhandledException");
            }
        }

        //260409 hbk -- Task 미관찰 예외
        private void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e) {
            CrashDumpHelper.WriteDump(e.Exception, "TaskScheduler.UnobservedTaskException");
            e.SetObserved();
        }
    }
}
