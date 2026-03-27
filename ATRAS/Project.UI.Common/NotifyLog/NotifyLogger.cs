using Project.BaseLib.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows.Media;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;
using System.ComponentModel;
using Project.BaseLib.Logger;
using System.Windows.Data;

namespace Project.UI.Common
{
    public static class NotifyLogger
    {
        private const int MAX_LOG_SIZE = 5000;

        private static object logLock = new object();

        public static ObservableCollection<LogItems> LogItems { get; set; }
        public static ListBox LogListBox { get; set; }

        static NotifyLogger()
        {
            LogItems = new ObservableCollection<LogItems>();


            var incc = LogItems as INotifyCollectionChanged;
            if (incc == null) return;
            BindingOperations.EnableCollectionSynchronization(LogItems, logLock);

            LogItems.CollectionChanged += OnCollectionChanged;

            LogManager.LogEvent += OnLogEvent;
        }
        public static void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            lock (logLock)
            {
                if (e.Action == NotifyCollectionChangedAction.Add)
                {
                    if (LogListBox != null)
                    {
                        LogListBox.Dispatcher.BeginInvoke(new ThreadStart(() =>
                        {
                            int count = LogListBox.Items.Count;
                            if (count == 0)
                                return;

                            object lastItem = LogListBox.Items[LogListBox.Items.Count - 1];
                            LogListBox.Items.MoveCurrentTo(lastItem);
                            LogListBox.ScrollIntoView(lastItem);
                            LogListBox.Background = GetBrush();


                        }));
                    }
                }
            }

        }

        public static Brush Brush
        {
            get { return GetBrush(); }
        }

        public static Brush GetBrush()
        {
            foreach(var item in LogListBox.Items)
            {
                var logItem = (item as LogItems);
                if (logItem != null)
                {
                    if (logItem.LogLevel == LogLevel.Error)
                        return Brushes.Red;
                }
            }
            return Brushes.DimGray;
        }

        public static void Foramt(LogLevel level, string format, params object[] args)
        {
            LogItems.Add(new LogItems(string.Format(format, args), null, level));
        }

        public static void Clear()
        {
            try
            {
                LogListBox.Dispatcher.BeginInvoke(new ThreadStart(() =>
                {
                    LogItems.Clear();
                    LogListBox.Background = Brushes.DimGray;
                }));
            }
            catch (Exception e)
            {
                
            }

        }

        public static void OnLogEvent(object sender, ProjectLoggingEvent e)
        {
            lock (logLock)
            {

                while (LogItems.Count >= MAX_LOG_SIZE)
                {
                    LogItems.RemoveAt(LogItems.Count - 1);
                }


                LogLevel level = LogLevel.Info;

                switch (e.Level.ToUpper())
                {
                    case "DEBUG":
                        return;

                    case "WARN":
                    case "INFO":
                        {
                            level = LogLevel.Info;
                        }
                        break;

                    case "ERROR":

                        level = LogLevel.Error;

                        break;
                }

                Foramt(level, e.Message);
            }
        }
    }
}
