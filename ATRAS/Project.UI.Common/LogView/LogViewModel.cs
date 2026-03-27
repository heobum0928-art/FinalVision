using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Timers;
using Project.BaseLib.Utils;

using Microsoft.WindowsAPICodePack.Taskbar;
using Project.BaseLib.Logger;
using System.Windows.Controls;

namespace Project.UI.Common
{
    public class LogViewModel : ViewModelBase
    {
        private const int MAX_LOG_SIZE = 5000;
        private object logLock = new object();
        public ObservableCollection<LogItem> LogItems { get; set; }
        public DelegateCommand OpenLogsFolderCommand { get; set; }
        public DelegateCommand OpenLogFileCommand { get; set; }
        public TaskbarProgressBarState latestTaskbarState = TaskbarProgressBarState.NoProgress;
        private readonly Timer processTimer;
        System.Timers.Timer timeTimer;

        public ListViewEx list_view_ex;

        protected int _SelectedIndex = 0;

        public int SelectedIndex
        {
            get
            {
                return _SelectedIndex;
            }

            set
            {
                _SelectedIndex = value;
                OnPropertyChanged();
            }
        }


        public LogViewModel() : base("LogViewModel")

        {
            LogItems = new ObservableCollection<LogItem>();
            OpenLogsFolderCommand = new DelegateCommand(OnOpenLogsFolder);
            OpenLogFileCommand = new DelegateCommand(OnOpenLogFile);
            timeTimer = new System.Timers.Timer();
            timeTimer.Interval = 1000*60;   
            timeTimer.Elapsed += timeTimer_Elapsed;
            BindingOperations.EnableCollectionSynchronization(LogItems, logLock);
            LogManager.LogEvent += OnLogEvent;

        }

        void timeTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            ResetLogError(true);
        }

        private void OnOpenLogFile()
        {
            try
            {
                //var hostInfo = (services.Map["HOSTINFO"] as HostInfo);

                //if (hostInfo == null)
                //    return;

                //Process.Start(Directory.GetCurrentDirectory() + "\\..\\logs\\" + hostInfo.LogsDirectory + "\\" + hostInfo.LogName);         
            }
            catch (Exception)
            {
            }
                     
        }

        private void OnOpenLogsFolder()
        {
            try
            {
                //var hostInfo = (services.Map["HOSTINFO"] as HostInfo);
      
                //if (hostInfo == null)
                //    return;

                //Process.Start("explorer.exe", Directory.GetCurrentDirectory() + "\\..\\logs\\" + hostInfo.LogsDirectory);

            }
            catch (Exception)
            {                
            }
        }

        public void OnSendLogSnippet(DependencyObject source)
        {
            var logItems = from log in LogItems
                           where log.IsSelected
                           select log;

            //DragDrop.DoDragDrop(source, new LogSnippetEvent(logItems), DragDropEffects.Copy);
        }

        private void OnLogEvent(object sender, ProjectLoggingEvent e)
        {
            lock(logLock)
            {
                while(LogItems.Count >= MAX_LOG_SIZE)
                {
                    LogItems.RemoveAt(LogItems.Count - 1);
                }

                var log_item = new LogItem(e);

                LogItems.Insert(0, log_item);

                if (LogManager.PreviewErrors)
                {
                    if (e.Level == "ERROR" && latestTaskbarState != TaskbarProgressBarState.Error)
                    {
                        latestTaskbarState = TaskbarProgressBarState.Error;
                        TaskbarManager.Instance.SetProgressState(TaskbarProgressBarState.Error);
                        TaskbarManager.Instance.SetProgressValue(100, 100);
                        timeTimer.Enabled = true;
                    }
                }

                //if (list_view_ex.Items.Count > 0)
                //{
                //    list_view_ex.SelectedIndex = 0;
                //}
                //{
                //    list_view_ex.SelectedItem = log_item;
                //    // In case the item is out of view. If so, the next line could cause an exception without bringing this item to view.
                //    ListViewItem lvi = (ListViewItem)list_view_ex.ItemContainerGenerator.ContainerFromIndex(list_view_ex.SelectedIndex);
                //    lvi.Focus();


                //    //list_view_ex.SelectedIndex = 0;
                //    //ListViewExItem lvi = (ListViewExItem)list_view_ex.ItemContainerGenerator.ContainerFromIndex(list_view_ex.SelectedIndex);
                //    //lvi.Focus();
                //}

            }
        }

        internal void ResetLogError(bool keepIndication = false)
        {
            if (LogManager.PreviewErrors && latestTaskbarState != TaskbarProgressBarState.NoProgress)
            {
                if (keepIndication)
                {
                    latestTaskbarState = TaskbarProgressBarState.Paused;
                    TaskbarManager.Instance.SetProgressState(TaskbarProgressBarState.Error);
                    TaskbarManager.Instance.SetProgressValue(0, 100);

                }
                else
                {
                    latestTaskbarState = TaskbarProgressBarState.NoProgress;
                    TaskbarManager.Instance.SetProgressState(TaskbarProgressBarState.NoProgress);
                }

                timeTimer.Enabled = false;

            }

        }
    }

    public class LogItem : NotifyPropertyChanged
    {
        private ProjectLoggingEvent logEvent;

        public ProjectLoggingEvent LogEvent
        {
            get { return logEvent; }
            set
            {
                if(logEvent != value)
                {
                    logEvent = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool isSelected;

        public LogItem(ProjectLoggingEvent projectLoggingEvent)
        {
            LogEvent = projectLoggingEvent;
        }

        public bool IsSelected
        {
            get { return isSelected; }
            set
            {
                if(isSelected != value)
                {
                    isSelected = value;
                    OnPropertyChanged();
                }
            }
        }

        
    }
}
