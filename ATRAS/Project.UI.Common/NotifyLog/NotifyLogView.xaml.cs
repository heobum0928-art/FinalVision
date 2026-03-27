using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Project.UI.Common
{
    public partial class NotifyLogView : UserControl
    {
        public NotifyLogView()
        {
            InitializeComponent();
            NotifyLogger.LogListBox = notifyLogView;
        }

        private void NotifyLogView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Console.WriteLine("NotifyLogView_SelectionChanged");
        }
    }
}
