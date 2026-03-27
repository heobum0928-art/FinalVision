using Project.BaseLib.DataStructures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    /// <summary>
    /// LoadPortStatusControl.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class LoadPortStatusControl : UserControl
    {
        #region fields
        public static readonly DependencyProperty LoadPortInfoProperty =
                DependencyProperty.Register("LoadPortInfo", typeof(LoadPortInfo), typeof(LoadPortStatusControl), new PropertyMetadata(null));

        #endregion

        #region propertise
        public LoadPortInfo LoadPortInfo
        {
            get
            {
                return (LoadPortInfo)GetValue(LoadPortInfoProperty);
            }

            set
            {
                SetValue(LoadPortInfoProperty, value);
            }
        }
        #endregion

        #region methods

        #endregion

        #region constructors
        public LoadPortStatusControl()
        {
            InitializeComponent();
        }
        #endregion


    }
}
