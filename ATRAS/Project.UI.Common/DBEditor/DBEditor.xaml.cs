using ICSharpCode.AvalonEdit.Folding;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Search;
using Microsoft.Win32;
using Project.BaseLib.Logger;
using Project.BaseLib.Utils;
using System;
using System.Collections.Generic;
using System.IO;
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


namespace Project.UI.Common
{
    /// <summary>
    /// Interaction logic for DBEditor.xaml
    /// </summary>
    /// 
    public partial class DBEditor : UserControl
    {
        #region field
        ILogger logger;
        FoldingManager foldingManager;
        XmlFoldingStrategy foldingStrategy;
        string extention = "xml";
        #endregion

        #region constructor
        public DBEditor()
        {
            DataContext = new DBEditorViewModel();
            RegisterConfigChangedEvent(ConfigManager.Instance.OnConfigurationDataCheck);
            logger = LogManager.GetLogger("DBEditor");
            InitializeComponent();
        }
        #endregion

        #region methods

        private void textEditor_TextChanged(object sender, EventArgs e)
        {
            {
                if (textEditor.Document.Text.Equals(string.Empty))
                {
                }
                else
                {
                    if (foldingStrategy != null)
                    {
                        foldingStrategy.UpdateFoldings(foldingManager, textEditor.Document);
                    }
                }
            }
        }

        void openFileClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();

            dlg.DefaultExt = extention;
            dlg.Filter = "XML Files (*.xml)|*.xml";

            dlg.CheckFileExists = true;

            if (dlg.ShowDialog() ?? false)
            {
                string currentFileName = dlg.FileName;

                if (currentFileName != null)
                {
                    currentFileName = dlg.FileName;
                    textEditor.Load(currentFileName);
                    textEditor.SyntaxHighlighting = HighlightingManager.Instance.GetDefinitionByExtension(Path.GetExtension(currentFileName));
                }
            }
        }

        private void ConfigurationOnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var ViewModel = (DataContext as DBEditorViewModel);
            logger.Info()("ConfigurationOnSelectionChanged - Category : {0}, Configuration : {1}, Path : {2}", ViewModel.SelectedCategory, ViewModel.SelectedDB, ViewModel.GetCurrentSelectedPath());

            LoadSelectedDB();
            //if (configSelectionChanged != null)
            //    configSelectionChanged(ViewModel.SelectedCategory, ViewModel.SelectedDB);
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            foldingStrategy = new XmlFoldingStrategy();
            foldingManager = FoldingManager.Install(this.textEditor.TextArea);
            //(DataContext as DataViewModel).OnTextElementChanged += DBEditorView_OnTextElementChanged;

            textEditor.TextArea.DefaultInputHandler.NestedInputHandlers.Add(new SearchInputHandler(textEditor.TextArea));

            LoadSelectedDB();
        }

        public void RegisterConfigChangedEvent(ConfigurationSelectionChanged configSelectionChanged)
        {
            (DataContext as DBEditorViewModel).configSelectionChanged += configSelectionChanged;
        }

        private void LoadSelectedDB()
        {
            if(textEditor != null)
            {
                var ViewModel = (DataContext as DBEditorViewModel);
                var path = ViewModel.ConfigPathInfo[ViewModel.SelectedCategory][ViewModel.SelectedDB];
                textEditor.Load(path);
            }
        }

        private void SaveBtnOnClick(object sender, RoutedEventArgs e)
        {
            logger.Info()("SaveBtnOnClick()");
            var ViewModel = (DataContext as DBEditorViewModel);
            logger.Info()("textEditor.Text : {0}", textEditor.Document.Text);

            //if (configSelectionChanged != null)
            //    configSelectionChanged(ViewModel.SelectedCategory, ViewModel.SelectedDB);


        }

        #endregion
    }
}
