using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
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
    /// FolderRecipeWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class FolderRecipeWindow : Window, INotifyPropertyChanged
    {
        #region fields
        public event PropertyChangedEventHandler PropertyChanged;

        protected string _CreateFolderPath;

        protected string _FolderName;
        #endregion

        #region propertise
        public MessageBoxResult Result { get; private set; }

        public string CreateFolderPath
        {
            get
            {
                return _CreateFolderPath;
            }

            //set
            //{
            //    _CreateFolderPath = value;
            //    OnPropertyChanged();
            //}
        }

        public string FolderName
        {
            get
            {
                return _FolderName;
            }

            set
            {
                _FolderName = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region methods
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }
        private void Button_OK_Click(object sender, RoutedEventArgs e)
        {
            Result = MessageBoxResult.OK;
            //this.Visibility = Visibility.Hidden;
            this.Close();
        }
        private void Button_CANCEL_Click(object sender, RoutedEventArgs e)
        {
            Result = MessageBoxResult.Cancel;
            //this.Visibility = Visibility.Hidden;
            this.Close();
        }

        #endregion

        #region constructors
        protected FolderRecipeWindow()
        {
            InitializeComponent();

            DataContext = this;
        }

        public FolderRecipeWindow(string create_folder)
        {
            InitializeComponent();

            DataContext = this;


            _CreateFolderPath = create_folder;

            _FolderName = string.Empty;
        }


        #endregion


    }
}
