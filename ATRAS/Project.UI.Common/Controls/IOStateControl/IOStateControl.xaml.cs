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

using Project.BaseLib.DataStructures;
using Project.BaseLib.Utils;

namespace Project.UI.Common
{
    /// <summary>
    /// IOStatusControl.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class IOStateControl : UserControl
    {
        #region fields
        public static readonly DependencyProperty IOStateListProperty =
                DependencyProperty.Register("IOStateList", typeof(List<IOState>), typeof(IOStateControl), new PropertyMetadata(null));

        public static readonly DependencyProperty HeaderProperty =
                DependencyProperty.Register("Header", typeof(string), typeof(IOStateControl), new PropertyMetadata(null));


        public static readonly DependencyProperty SelectedIOStateProperty =
            DependencyProperty.Register("SelectedIOState", typeof(IOState), typeof(IOStateControl), new PropertyMetadata(null));


        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register("ClickCommand", typeof(ICommand), typeof(IOStateControl), new PropertyMetadata(null, null));


        public static readonly DependencyProperty DescriptWidthProperty =
                DependencyProperty.Register("DescriptWidth", typeof(int), typeof(IOStateControl), new PropertyMetadata(null));


        #endregion

        #region propertise
        public string Header
        {
            get
            {
                return string.Format("[{0}]", (string)GetValue(HeaderProperty));
            }

            set
            {
                SetValue(HeaderProperty, value);
            }
        }

        public List<IOState> IOStateList
        {
            get
            {
                return (List<IOState>)GetValue(IOStateListProperty);
            }

            set
            {
                SetValue(IOStateListProperty, value);
            }
        }

        public IOState SelectedIOState
        {
            get
            {
                return (IOState)GetValue(SelectedIOStateProperty);
            }

            set
            {
                SetValue(SelectedIOStateProperty, value);
            }
        }

        public ICommand ClickCommand
        {
            get
            {
                return (ICommand)GetValue(CommandProperty);
            }

            set
            {
                SetValue(CommandProperty, value);
            }
        }


        public int DescriptWidth
        {
            get
            {
                return (int)GetValue(DescriptWidthProperty);
            }

            set
            {
                SetValue(DescriptWidthProperty, value);
            }
        }
        #endregion

        #region methods
        private void TextBox_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (ClickCommand != null)
            {
                ClickCommand.Execute(SelectedIOState);
            }
        }
        #endregion

        #region constructors
        public IOStateControl()
        {
            InitializeComponent();
        }

        #endregion
    }
}
