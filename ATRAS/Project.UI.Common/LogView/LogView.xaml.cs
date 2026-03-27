using Microsoft.WindowsAPICodePack.Taskbar;
using Project.BaseLib.Logger;
using Project.BaseLib.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Shapes;

namespace Project.UI.Common
{
    public partial class LogView : UserControl
    {
        public readonly string vsopenPath = "..\\..\\Tools\\VisualStudioFileOpenTool\\VisualStudioFileOpenTool.exe";
        public readonly string vsopenVer = "12";

        public LogView()
        {
            InitializeComponent();

            DataContext = new LogViewModel();

            (DataContext as LogViewModel).list_view_ex = listViewEx;

            LogManager.InitializeLogger(false);


        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && e.OriginalSource is TextBlock)
                (DataContext as LogViewModel).OnSendLogSnippet(this);

            base.OnMouseMove(e);
        }

     

        void ListViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                LogItem logItem = ((ListViewItem)sender).Content as LogItem;

                Process process = new Process();
                ProcessStartInfo startInfo = new ProcessStartInfo(vsopenPath, vsopenVer + " " + logItem.LogEvent.FilePath + " " + logItem.LogEvent.LineNumber);
                process.StartInfo = startInfo;
                process.Start();
            }
            catch (Exception)
            {                
            }
        }


        private void OnOpenFileLog(object sender, System.Windows.RoutedEventArgs e)
        {

        }

        private void OnOpenFileFolder(object sender, System.Windows.RoutedEventArgs e)
        {

        }

        private void UserControl_GotFocus(object sender, System.Windows.RoutedEventArgs e)
        {
         
       //     TaskbarManager.Instance.TabbedThumbnail.SetThumbnailClip(ff, new Rectangle());

            (DataContext as LogViewModel).ResetLogError();
        }

        private void MenuItem_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            (DataContext as LogViewModel).LogItems.Clear();
        }

    }
}

